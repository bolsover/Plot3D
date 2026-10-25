using System;
using System.IO;
using Plot3D.Core;
using Plot3D.Expressions;
using Plot3D.Figure;
using Plot3D.Objects;
using Xunit;

namespace Plot3D.Tests
{
    public class FormulaPlot3DTests
    {
        [Fact]
        public void WithFormulas_GeneratesValidHelixCurve()
        {
            var line = ThreeDimensionalPlotter.Plot("sin(t)", "cos(t)", "t", 0.0, 6.0 * Math.PI);

            Assert.NotNull(line);
            Assert.True(line.Points.Count >= 50);
            Assert.True(line.ArcLength > 20.0);

            // Bounds verification
            var bounds = line.Bounds;
            Assert.InRange(bounds.XMin, -1.05, -0.95);
            Assert.InRange(bounds.XMax, 0.95, 1.05);
            Assert.InRange(bounds.YMin, -1.05, -0.95);
            Assert.InRange(bounds.YMax, 0.95, 1.05);
            Assert.InRange(bounds.ZMin, -0.05, 0.05);
            Assert.InRange(bounds.ZMax, 6.0 * Math.PI - 0.1, 6.0 * Math.PI + 0.1);
        }

        [Fact]
        public void PlotFigure_WithFormulasAndLineSpec_ConfiguresFigureCorrectly()
        {
            var (fig, line) = ThreeDimensionalPlotter.PlotFigure(
                "sin(t) + 2*sin(2*t)",
                "cos(t) - 2*cos(2*t)",
                "-sin(3*t)",
                new Interval(0.0, 2.0 * Math.PI),
                LineSpec.Parse("r--o")
            );

            fig.Title("Trefoil Knot").XLabel("X Axis").YLabel("Y Axis").ZLabel("Z Axis");

            Assert.NotNull(fig);
            Assert.NotNull(line);
            Assert.Equal("Trefoil Knot", fig.CurrentAxes.Title);
            Assert.Equal(LineStyle.Dashed, line.LineStyle);
            Assert.Equal(MarkerStyle.Circle, line.Marker);
            Assert.Equal(Color3D.PureRed, line.Color);

            string svg = fig.ToSvg(800, 600);
            Assert.Contains("<svg", svg);
            Assert.Contains("Trefoil Knot", svg);
        }

        [Fact]
        public void Calculate_WithFormulas_ReturnsMatchingCoordinateArrays()
        {
            var (x, y, z, t) = ThreeDimensionalPlotter.Calculate("sin(t)", "cos(t)", "t", new Interval(0.0, 2.0 * Math.PI));

            Assert.NotEmpty(x);
            Assert.Equal(x.Length, y.Length);
            Assert.Equal(x.Length, z.Length);
            Assert.Equal(x.Length, t.Length);

            for (int i = 0; i < t.Length; i++)
            {
                Assert.Equal(Math.Sin(t[i]), x[i], precision: 6);
                Assert.Equal(Math.Cos(t[i]), y[i], precision: 6);
                Assert.Equal(t[i], z[i], precision: 6);
            }
        }
    }
}
