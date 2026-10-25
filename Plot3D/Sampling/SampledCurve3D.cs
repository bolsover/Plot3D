using System;
using System.Collections.Generic;
using System.Linq;
using Plot3D.Core;

namespace Plot3D.Sampling
{
    /// <summary>
    /// Represents the discrete evaluation points, segments, and metadata resulting from sampling a 3D parametric curve.
    /// </summary>
    public sealed class SampledCurve3D
    {
        public IReadOnlyList<double> TValues { get; }
        public IReadOnlyList<Point3D> Points { get; }
        public IReadOnlyList<double> XValues { get; }
        public IReadOnlyList<double> YValues { get; }
        public IReadOnlyList<double> ZValues { get; }

        /// <summary>
        /// Continuous curve segments separated by NaN / discontinuities / asymptotes.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<Point3D>> Segments { get; }

        public Bounds3D Bounds { get; }
        public double ArcLength { get; }
        public int FunctionEvaluations { get; }
        public int PointCount => Points.Count;

        public SampledCurve3D(
            IReadOnlyList<double> tValues,
            IReadOnlyList<Point3D> points,
            int functionEvaluations)
        {
            TValues = tValues ?? throw new ArgumentNullException(nameof(tValues));
            Points = points ?? throw new ArgumentNullException(nameof(points));
            FunctionEvaluations = functionEvaluations;

            double[] x = new double[points.Count];
            double[] y = new double[points.Count];
            double[] z = new double[points.Count];

            Bounds3D bounds = Bounds3D.Empty;
            List<List<Point3D>> segments = new List<List<Point3D>>();
            List<Point3D>? currentSegment = null;
            double totalArcLength = 0.0;
            Point3D? lastValidPoint = null;

            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                x[i] = pt.X;
                y[i] = pt.Y;
                z[i] = pt.Z;

                if (pt.IsValid)
                {
                    bounds = bounds.Include(pt);

                    if (currentSegment == null)
                    {
                        currentSegment = new List<Point3D>();
                        segments.Add(currentSegment);
                    }
                    currentSegment.Add(pt);

                    if (lastValidPoint.HasValue)
                    {
                        totalArcLength += lastValidPoint.Value.DistanceTo(pt);
                    }
                    lastValidPoint = pt;
                }
                else
                {
                    // Discontinuity break
                    currentSegment = null;
                    lastValidPoint = null;
                }
            }

            XValues = x;
            YValues = y;
            ZValues = z;
            Segments = segments;
            Bounds = bounds;
            ArcLength = totalArcLength;
        }

        public (double[] X, double[] Y, double[] Z, double[] T) ToArrays()
        {
            return (XValues.ToArray(), YValues.ToArray(), ZValues.ToArray(), TValues.ToArray());
        }
    }
}
