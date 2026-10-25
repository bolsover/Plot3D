using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AlibreX;
using Plot3D.Core;

namespace Plot3D.UI.Alibre
{
    /// <summary>
    /// Service for creating 3D sketches of parametric curves in the active Alibre Part environment.
    /// </summary>
    public static class Alibre3DSketchService
    {
        private const int ActiveSketchSessionExists = -2147220963;

        /// <summary>
        /// Creates a 3D sketch in the active Alibre Part session from the given 3D points.
        /// </summary>
        /// <param name="root">The Alibre automation root instance.</param>
        /// <param name="points">List of 3D points to include in the sketch.</param>
        /// <param name="options">Options controlling geometry type, units scaling, and sketch name.</param>
        /// <param name="curveTitle">Optional title/description of the curve.</param>
        /// <returns>A result object indicating success or failure with descriptive message.</returns>
        public static AlibreSketchResult Create3DSketch(
            IADRoot? root,
            IReadOnlyList<Point3D> points,
            Alibre3DSketchOptions? options = null,
            string? curveTitle = null)
        {
            if (root == null)
            {
                return AlibreSketchResult.Failed("Alibre Design automation root is not available. Please ensure Alibre Design is running.");
            }

            if (points == null || points.Count == 0)
            {
                return AlibreSketchResult.Failed("No curve points available to generate sketch.");
            }

            var validPoints = points.Where(p => p.IsValid).ToList();
            if (validPoints.Count < 2)
            {
                return AlibreSketchResult.Failed("At least 2 valid 3D points are required to generate a 3D sketch.");
            }

            options ??= Alibre3DSketchOptions.Default;

            try
            {
                var session = GetActivePartSession(root);
                if (session == null)
                {
                    return AlibreSketchResult.Failed("No active Part workspace found in Alibre Design. Please open or create a Part (.AD_PRT).");
                }

                var partSession = session as IADPartSession;
                if (partSession == null)
                {
                    return AlibreSketchResult.Failed("The active workspace is not a 3D Part workspace.");
                }

                var designSession = session as IADDesignSession;
                var modelUnits = designSession?.DesignProperties?.ModelUnits ?? ADUnits.AD_CENTIMETERS;
                var displayUnits = designSession?.DesignProperties?.LengthDisplayUnits ?? modelUnits;

                // Calculate coordinate scale factor to convert points into Alibre internal model units (cm)
                double scaleToCm = CalculateScaleFactorToCm(options, displayUnits);

                // Prepare scaled coordinates
                var scaledPoints = validPoints.Select(p => new Point3D(p.X * scaleToCm, p.Y * scaleToCm, p.Z * scaleToCm)).ToList();

                // Select or create target 3D sketch
                IAD3DSketch targetSketch;
                bool isNew = true;

                if (options.TargetMode == AlibreSketchTargetMode.ActiveOrNew3DSketch)
                {
                    var activeSketch = FindActive3DSketch(partSession);
                    if (activeSketch != null)
                    {
                        targetSketch = activeSketch;
                        isNew = false;
                    }
                    else
                    {
                        targetSketch = CreateNew3DSketch(partSession, options.SketchName, curveTitle);
                    }
                }
                else
                {
                    targetSketch = CreateNew3DSketch(partSession, options.SketchName, curveTitle);
                }

                string sketchName = targetSketch.Name;
                string partName = session.Name ?? "Part";

                // Modify sketch geometry
                bool beganChange = BeginChange(targetSketch.BeginChange);
                try
                {
                    var figures = targetSketch.Figures;
                    switch (options.GeometryType)
                    {
                        case AlibreGeometryType.BSplineInterpolation:
                            AddBSplineInterpolation(figures, scaledPoints);
                            break;

                        case AlibreGeometryType.Polyline:
                            AddPolyline(figures, scaledPoints);
                            break;

                        case AlibreGeometryType.Points:
                            AddPointNodes(figures, scaledPoints);
                            break;

                        default:
                            AddBSplineInterpolation(figures, scaledPoints);
                            break;
                    }
                }
                finally
                {
                    if (beganChange)
                    {
                        targetSketch.EndChange();
                    }
                }

                string actionVerb = isNew ? "Created new" : "Updated existing";
                return AlibreSketchResult.Succeeded(
                    sketchName,
                    scaledPoints.Count,
                    partName,
                    $"{actionVerb} 3D Sketch '{sketchName}' in '{partName}' with {scaledPoints.Count} points ({options.GeometryType}).");
            }
            catch (COMException comEx)
            {
                return AlibreSketchResult.Failed($"Alibre COM Error (0x{comEx.ErrorCode:X8}): {comEx.Message}");
            }
            catch (Exception ex)
            {
                return AlibreSketchResult.Failed($"Failed to create 3D sketch: {ex.Message}");
            }
        }

        private static IADSession? GetActivePartSession(IADRoot root)
        {
            try
            {
                var top = root.TopmostSession;
                if (top is IADPartSession) return top;

                if (root.Sessions != null)
                {
                    foreach (IADSession s in root.Sessions)
                    {
                        if (s is IADPartSession) return s;
                    }
                }
            }
            catch
            {
                // Ignored
            }

            return null;
        }

        private static IAD3DSketch? FindActive3DSketch(IADPartSession part)
        {
            try
            {
                if (part.Sketches3D == null) return null;
                foreach (IAD3DSketch s in part.Sketches3D)
                {
                    if (s.IsActive) return s;
                }
            }
            catch
            {
                // Ignored
            }

            return null;
        }

        private static IAD3DSketch CreateNew3DSketch(IADPartSession part, string? preferredName, string? curveTitle)
        {
            if (part.Sketches3D == null)
            {
                throw new InvalidOperationException("Part 3D sketches collection is not available.");
            }

            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IAD3DSketch s in part.Sketches3D)
            {
                existingNames.Add(s.Name);
            }

            string baseName;
            if (!string.IsNullOrWhiteSpace(preferredName))
            {
                baseName = preferredName!.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(curveTitle))
            {
                baseName = $"Plot3D - {curveTitle!.Trim()}";
            }
            else
            {
                baseName = "Plot3D 3D Sketch";
            }

            string candidate = baseName;
            int counter = 2;
            while (existingNames.Contains(candidate))
            {
                candidate = $"{baseName} ({counter++})";
            }

            return part.Sketches3D.Add3DSketch(candidate);
        }

        private static double CalculateScaleFactorToCm(Alibre3DSketchOptions options, ADUnits displayUnits)
        {
            switch (options.UnitScale)
            {
                case AlibreUnitScale.PartDisplayUnits:
                    // Convert 1 display unit to cm
                    return AlibreUnits.Convert(1.0, displayUnits, ADUnits.AD_CENTIMETERS);

                case AlibreUnitScale.Millimeters:
                    return 0.1; // 1 mm = 0.1 cm

                case AlibreUnitScale.Centimeters:
                    return 1.0; // 1 cm = 1.0 cm

                case AlibreUnitScale.Inches:
                    return 2.54; // 1 in = 2.54 cm

                case AlibreUnitScale.Meters:
                    return 100.0; // 1 m = 100.0 cm

                case AlibreUnitScale.Custom:
                    return options.CustomScaleFactor > 0 ? options.CustomScaleFactor : 1.0;

                default:
                    return AlibreUnits.Convert(1.0, displayUnits, ADUnits.AD_CENTIMETERS);
            }
        }

        private static void AddBSplineInterpolation(IAD3DSketchFigures figures, List<Point3D> points)
        {
            try
            {
                var flat = FlattenPoints(points);
                Array pPoints = flat;
                var bspline = figures.AddBsplineByInterpolation(ref pPoints);
                if (bspline != null) return;
            }
            catch
            {
                // Fall back to polyline if BSpline interpolation fails on dense/collinear segments
            }

            AddPolyline(figures, points);
        }

        private static void AddPolyline(IAD3DSketchFigures figures, List<Point3D> points)
        {
            var flat = FlattenPoints(points);
            Array pPoints = flat;
            figures.AddPolyline(ref pPoints);
        }

        private static void AddPointNodes(IAD3DSketchFigures figures, List<Point3D> points)
        {
            foreach (var p in points)
            {
                figures.AddPoint(p.X, p.Y, p.Z);
            }
        }

        private static double[] FlattenPoints(List<Point3D> points)
        {
            var array = new double[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                array[i * 3 + 0] = points[i].X;
                array[i * 3 + 1] = points[i].Y;
                array[i * 3 + 2] = points[i].Z;
            }
            return array;
        }

        private static bool BeginChange(Action beginChange)
        {
            try
            {
                beginChange();
                return true;
            }
            catch (COMException e) when (e.ErrorCode == ActiveSketchSessionExists)
            {
                return false;
            }
        }
    }
}
