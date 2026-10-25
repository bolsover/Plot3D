using System;
using System.Collections.Generic;
using Plot3D.Core;
using Plot3D.Objects;
using Plot3D.Projection;

namespace Plot3D.Figure
{
    /// <summary>
    /// Represents the 3D axes box, grid, labels, and coordinate planes for 3D visualization.
    /// </summary>
    public sealed class Axes3D
    {
        public string Title { get; set; } = string.Empty;
        public string XLabel { get; set; } = "X";
        public string YLabel { get; set; } = "Y";
        public string ZLabel { get; set; } = "Z";

        public bool GridOn { get; set; } = true;
        public bool BoxOn { get; set; } = true;

        public Color3D AxisColor { get; set; } = Color3D.DarkGray;
        public Color3D GridColor { get; set; } = new Color3D(220, 220, 220);
        public Color3D BackgroundColor { get; set; } = Color3D.White;

        public Camera3D Camera { get; } = new Camera3D();

        public Bounds3D? ManualBounds { get; set; }

        private readonly List<ParameterizedFunctionLine3D> _lines = new List<ParameterizedFunctionLine3D>();
        public IReadOnlyList<ParameterizedFunctionLine3D> Lines => _lines;

        public void AddLine(ParameterizedFunctionLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _lines.Add(line);
        }

        public void Clear()
        {
            _lines.Clear();
        }

        public Bounds3D GetEffectiveBounds()
        {
            if (ManualBounds.HasValue && !ManualBounds.Value.IsEmpty)
                return ManualBounds.Value;

            Bounds3D bounds = Bounds3D.Empty;
            foreach (var line in _lines)
            {
                if (line.Visible)
                {
                    bounds = bounds.Union(line.Bounds);
                }
            }

            if (bounds.IsEmpty)
            {
                return new Bounds3D(-1, 1, -1, 1, -1, 1);
            }

            return bounds.Pad(0.08);
        }

        public (double[] XTicks, double[] YTicks, double[] ZTicks) CalculateTicks(Bounds3D bounds)
        {
            var xTicks = TickCalculator.CalculateTicks(bounds.XMin, bounds.XMax);
            var yTicks = TickCalculator.CalculateTicks(bounds.YMin, bounds.YMax);
            var zTicks = TickCalculator.CalculateTicks(bounds.ZMin, bounds.ZMax);
            return (xTicks, yTicks, zTicks);
        }
    }
}
