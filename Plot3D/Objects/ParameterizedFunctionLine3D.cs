using System;
using System.Collections.Generic;
using Plot3D.Core;
using Plot3D.Sampling;

namespace Plot3D.Objects
{
    /// <summary>
    /// Represents a 3D parameterized function line graphics object.
    /// </summary>
    public sealed class ParameterizedFunctionLine3D
    {
        public Func<double, double> XFunction { get; set; }
        public Func<double, double> YFunction { get; set; }
        public Func<double, double> ZFunction { get; set; }

        public Plot3DOptions Options { get; }
        public SampledCurve3D SampledData { get; private set; }

        // Data arrays
        public IReadOnlyList<double> XData => SampledData.XValues;
        public IReadOnlyList<double> YData => SampledData.YValues;
        public IReadOnlyList<double> ZData => SampledData.ZValues;
        public IReadOnlyList<double> TData => SampledData.TValues;
        public IReadOnlyList<Point3D> Points => SampledData.Points;
        public IReadOnlyList<IReadOnlyList<Point3D>> Segments => SampledData.Segments;

        // Visual Styling Properties
        public Color3D Color
        {
            get => Options.Color;
            set => Options.Color = value;
        }

        public double LineWidth
        {
            get => Options.LineWidth;
            set => Options.LineWidth = value;
        }

        public LineStyle LineStyle
        {
            get => Options.LineStyle;
            set => Options.LineStyle = value;
        }

        public MarkerStyle Marker
        {
            get => Options.Marker;
            set => Options.Marker = value;
        }

        public double MarkerSize
        {
            get => Options.MarkerSize;
            set => Options.MarkerSize = value;
        }

        public Color3D? MarkerEdgeColor
        {
            get => Options.MarkerEdgeColor;
            set => Options.MarkerEdgeColor = value;
        }

        public Color3D? MarkerFaceColor
        {
            get => Options.MarkerFaceColor;
            set => Options.MarkerFaceColor = value;
        }

        public int MeshDensity
        {
            get => Options.MeshDensity;
            set => Options.MeshDensity = value;
        }

        public Interval TRange
        {
            get => Options.TRange;
            set => Options.TRange = value;
        }

        public bool Visible { get; set; } = true;
        public string? DisplayName
        {
            get => Options.DisplayName;
            set => Options.DisplayName = value;
        }

        public Bounds3D Bounds => SampledData.Bounds;
        public double ArcLength => SampledData.ArcLength;
        public int FunctionEvaluations => SampledData.FunctionEvaluations;

        public ParameterizedFunctionLine3D(
            Func<double, double> xFunction,
            Func<double, double> yFunction,
            Func<double, double> zFunction,
            Plot3DOptions? options = null)
        {
            XFunction = xFunction ?? throw new ArgumentNullException(nameof(xFunction));
            YFunction = yFunction ?? throw new ArgumentNullException(nameof(yFunction));
            ZFunction = zFunction ?? throw new ArgumentNullException(nameof(zFunction));
            Options = options?.Clone() ?? Plot3DOptions.CreateDefault();

            SampledData = ComputeData();
        }

        public ParameterizedFunctionLine3D(
            Func<double, double> xFunction,
            Func<double, double> yFunction,
            Func<double, double> zFunction,
            Interval tRange,
            LineSpec? lineSpec = null)
        {
            XFunction = xFunction ?? throw new ArgumentNullException(nameof(xFunction));
            YFunction = yFunction ?? throw new ArgumentNullException(nameof(yFunction));
            ZFunction = zFunction ?? throw new ArgumentNullException(nameof(zFunction));

            Options = Plot3DOptions.CreateDefault();
            Options.TRange = tRange;
            if (lineSpec != null)
            {
                Options.ApplyLineSpec(lineSpec);
            }

            SampledData = ComputeData();
        }

        public void Update()
        {
            SampledData = ComputeData();
        }

        public void SetDataInterval(double tmin, double tmax)
        {
            TRange = new Interval(tmin, tmax);
            Update();
        }

        public Point3D Evaluate(double t)
        {
            try
            {
                return new Point3D(XFunction(t), YFunction(t), ZFunction(t));
            }
            catch
            {
                return Point3D.NaN;
            }
        }

        public Vector3D GetTangent(double t, double dt = 1e-5)
        {
            Point3D p1 = Evaluate(t - dt);
            Point3D p2 = Evaluate(t + dt);
            if (!p1.IsValid || !p2.IsValid) return Vector3D.Zero;
            return (p2 - p1).Normalized();
        }

        public double GetCurvature(double t, double dt = 1e-5)
        {
            Point3D p0 = Evaluate(t - dt);
            Point3D p1 = Evaluate(t);
            Point3D p2 = Evaluate(t + dt);

            if (!p0.IsValid || !p1.IsValid || !p2.IsValid) return 0.0;

            Vector3D rPrime = (p2 - p0) / (2.0 * dt);
            Vector3D rDoublePrime = ((p2 - p1) - (p1 - p0)) / (dt * dt);

            Vector3D cross = rPrime.Cross(rDoublePrime);
            double numerator = cross.Length;
            double denominator = Math.Pow(rPrime.Length, 3.0);

            if (denominator <= 1e-15) return 0.0;
            return numerator / denominator;
        }

        private SampledCurve3D ComputeData()
        {
            var sampler = new AdaptiveSampler3D(XFunction, YFunction, ZFunction, Options);
            return sampler.Sample();
        }
    }
}
