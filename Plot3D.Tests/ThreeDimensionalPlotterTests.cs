using System;
using Plot3D.Core;
using Plot3D.Objects;
using Xunit;

namespace Plot3D.Tests
{
    public class ThreeDimensionalPlotterTests
    {
        [Fact]
        public void DefaultOverload_UsesDefaultInterval()
        {
            var line = ThreeDimensionalPlotter.Plot(
                t => t,
                t => t * 2,
                t => t * 3
            );

            Assert.Equal(-5.0, line.TRange.Min);
            Assert.Equal(5.0, line.TRange.Max);
        }

        [Fact]
        public void TupleIntervalAndLineSpec_AppliedCorrectly()
        {
            var line = ThreeDimensionalPlotter.Plot(
                t => Math.Cos(t),
                t => Math.Sin(t),
                t => t,
                (-2.0, 2.0),
                "r:"
            );

            Assert.Equal(-2.0, line.TRange.Min);
            Assert.Equal(2.0, line.TRange.Max);
            Assert.Equal(LineStyle.Dotted, line.LineStyle);
            Assert.Equal(Color3D.PureRed, line.Color);
        }

        [Fact]
        public void Calculate_ReturnsArrays()
        {
            var (x, y, z, t) = ThreeDimensionalPlotter.Calculate(
                tau => tau,
                tau => tau * tau,
                tau => tau * tau * tau,
                0.0,
                1.0,
                meshDensity: 25
            );

            Assert.NotNull(x);
            Assert.NotNull(y);
            Assert.NotNull(z);
            Assert.NotNull(t);
            Assert.True(x.Length >= 25);
            Assert.Equal(x.Length, y.Length);
            Assert.Equal(x.Length, z.Length);
            Assert.Equal(x.Length, t.Length);
        }

        [Fact]
        public void FPlot3_CalculatePoints_ReturnsPoint3DArray()
        {
            var points = ThreeDimensionalPlotter.CalculatePoints(
                tau => Math.Sin(tau),
                tau => Math.Cos(tau),
                tau => tau,
                new Interval(0, Math.PI)
            );

            Assert.NotNull(points);
            Assert.True(points.Length > 20);
            Assert.All(points, pt => Assert.True(pt.IsValid));
        }

        [Fact]
        public void ParameterizedFunctionLine3D_SetDataInterval_UpdatesPoints()
        {
            var line = ThreeDimensionalPlotter.Plot(
                t => t,
                t => t,
                t => t,
                0.0,
                1.0
            );

            int initialCount = line.Points.Count;
            Assert.Equal(0.0, line.Points[0].X);
            Assert.Equal(1.0, line.Points[initialCount - 1].X);

            line.SetDataInterval(2.0, 5.0);
            Assert.Equal(2.0, line.Points[0].X);
            Assert.Equal(5.0, line.Points[line.Points.Count - 1].X);
        }
    }
}
