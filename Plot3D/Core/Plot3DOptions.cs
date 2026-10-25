using System;

namespace Plot3D.Core
{
    /// <summary>
    /// Configuration options for 3D parametric function plotting.
    /// </summary>
    public sealed class Plot3DOptions
    {
        /// <summary>Parametrization interval [tmin, tmax]. Default is [-5, 5].</summary>
        public Interval TRange { get; set; } = Interval.Default;

        /// <summary>Color of the line. Default is blue.</summary>
        public Color3D Color { get; set; } = Color3D.Blue;

        /// <summary>Width of the curve line. Default is 1.5.</summary>
        public double LineWidth { get; set; } = 1.5;

        /// <summary>Line style (Solid, Dashed, Dotted, DashDot, None).</summary>
        public LineStyle LineStyle { get; set; } = LineStyle.Solid;

        /// <summary>Marker symbol style.</summary>
        public MarkerStyle Marker { get; set; } = MarkerStyle.None;

        /// <summary>Size of markers in points/pixels. Default is 6.0.</summary>
        public double MarkerSize { get; set; } = 6.0;

        /// <summary>Marker edge color. If null, matches Color.</summary>
        public Color3D? MarkerEdgeColor { get; set; }

        /// <summary>Marker face/fill color. If null, transparent or auto.</summary>
        public Color3D? MarkerFaceColor { get; set; }

        /// <summary>
        /// Minimum number of evaluation points over the domain.
        /// Higher values resolve rapidly changing curves more reliably.
        /// </summary>
        public int MeshDensity { get; set; } = 23;

        /// <summary>
        /// Maximum number of function evaluations allowed for adaptive sampling. Default is 25000.
        /// </summary>
        public int MaxFunctionEvaluations { get; set; } = 25000;

        /// <summary>
        /// Maximum recursion depth for adaptive subdivision. Default is 12.
        /// </summary>
        public int MaxRecursionDepth { get; set; } = 12;

        /// <summary>
        /// Relative tolerance for adaptive mesh refinement (chord error / curvature). Default is 2e-3.
        /// </summary>
        public double RelativeTolerance { get; set; } = 2e-3;

        /// <summary>
        /// Absolute tolerance for adaptive mesh refinement. Default is 1e-4.
        /// </summary>
        public double AbsoluteTolerance { get; set; } = 1e-4;

        /// <summary>
        /// Maximum angle (in radians) allowed between consecutive segments before subdividing. Default is ~0.15 rad (~8.5 deg).
        /// </summary>
        public double MaxSegmentAngle { get; set; } = 0.15;

        /// <summary>
        /// Whether to automatically detect and split lines at asymptotes and discontinuities. Default is true.
        /// </summary>
        public bool ShowDiscontinuities { get; set; } = true;

        /// <summary>
        /// Display name for legends.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Creates a default options object.
        /// </summary>
        public static Plot3DOptions CreateDefault() => new Plot3DOptions();

        /// <summary>
        /// Applies settings from a LineSpec object or string.
        /// </summary>
        public Plot3DOptions ApplyLineSpec(LineSpec lineSpec)
        {
            if (lineSpec == null) return this;
            if (lineSpec.LineStyle.HasValue) LineStyle = lineSpec.LineStyle.Value;
            if (lineSpec.Marker.HasValue) Marker = lineSpec.Marker.Value;
            if (lineSpec.Color.HasValue) Color = lineSpec.Color.Value;
            return this;
        }

        public Plot3DOptions Clone()
        {
            return new Plot3DOptions
            {
                TRange = TRange,
                Color = Color,
                LineWidth = LineWidth,
                LineStyle = LineStyle,
                Marker = Marker,
                MarkerSize = MarkerSize,
                MarkerEdgeColor = MarkerEdgeColor,
                MarkerFaceColor = MarkerFaceColor,
                MeshDensity = MeshDensity,
                MaxFunctionEvaluations = MaxFunctionEvaluations,
                MaxRecursionDepth = MaxRecursionDepth,
                RelativeTolerance = RelativeTolerance,
                AbsoluteTolerance = AbsoluteTolerance,
                MaxSegmentAngle = MaxSegmentAngle,
                ShowDiscontinuities = ShowDiscontinuities,
                DisplayName = DisplayName
            };
        }
    }
}
