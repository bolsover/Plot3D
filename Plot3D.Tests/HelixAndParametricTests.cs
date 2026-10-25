using System;
using Plot3D.Core;
using Plot3D.Objects;
using Xunit;

namespace Plot3D.Tests
{
    public class HelixAndParametricTests
    {
        [Fact]
        public void Helix_GeneratesPointsOnUnitCylinder()
        {
            // ThreeDimensionalPlotter canonical example: ThreeDimensionalPlotter(@(t) sin(t), @(t) cos(t), @(t) t, [0, 10*pi])
            double tmax = 10.0 * Math.PI;
            var line = ThreeDimensionalPlotter.Plot(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                0.0,
                tmax
            );

            Assert.NotNull(line);
            Assert.True(line.Points.Count > 100);

            // Verify all points lie on x^2 + y^2 = 1
            for (int i = 0; i < line.Points.Count; i++)
            {
                var pt = line.Points[i];
                double radius = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                Assert.True(Math.Abs(radius - 1.0) < 1e-4, $"Radius at point {i} ({radius}) should be ~1.0");

                // Verify z = t
                double t = line.TData[i];
                Assert.True(Math.Abs(pt.Z - t) < 1e-6, $"Z ({pt.Z}) should equal t ({t})");
            }
        }

        [Fact]
        public void Helix_ArcLengthMatchesTheoreticalValue()
        {
            // For x = sin(t), y = cos(t), z = t on [0, 6*pi]:
            // speed = sqrt(cos^2(t) + (-sin(t))^2 + 1^2) = sqrt(2)
            // Theoretical length = sqrt(2) * 6 * pi = ~26.6573
            double tmax = 6.0 * Math.PI;
            var line = ThreeDimensionalPlotter.Plot(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                new Interval(0.0, tmax)
            );

            double theoreticalLength = Math.Sqrt(2.0) * tmax;
            double actualLength = line.ArcLength;

            double relativeError = Math.Abs(actualLength - theoreticalLength) / theoreticalLength;
            Assert.True(relativeError < 1e-3, $"Arc length relative error {relativeError} should be < 0.1%");
        }

        [Fact]
        public void Helix_CurvatureMatchesTheoreticalValue()
        {
            // For a circular helix of radius a=1, pitch c=1:
            // Curvature kappa = a / (a^2 + c^2) = 1 / (1 + 1) = 0.5
            var line = ThreeDimensionalPlotter.Plot(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                0.0,
                2.0 * Math.PI
            );

            double kappa = line.GetCurvature(1.5);
            Assert.True(Math.Abs(kappa - 0.5) < 1e-3, $"Curvature {kappa} should be close to 0.5");
        }

        [Fact]
        public void TrefoilKnot_ProducesSmoothClosedCurve()
        {
            // Trefoil knot: x = sin(t) + 2*sin(2t), y = cos(t) - 2*cos(2t), z = -sin(3t) on [0, 2*pi]
            var line = ThreeDimensionalPlotter.Plot(
                t => Math.Sin(t) + 2.0 * Math.Sin(2.0 * t),
                t => Math.Cos(t) - 2.0 * Math.Cos(2.0 * t),
                t => -Math.Sin(3.0 * t),
                0.0,
                2.0 * Math.PI
            );

            Assert.True(line.Points.Count > 150);
            var start = line.Points[0];
            var end = line.Points[line.Points.Count - 1];

            // Should be a closed loop: start == end
            Assert.True(start.DistanceTo(end) < 1e-4, "Start and end points of trefoil knot must match");

            // Bounds should be approximately [-3, 3] x [-3, 3] x [-1, 1]
            var bounds = line.Bounds;
            Assert.True(bounds.XMin >= -3.1 && bounds.XMax <= 3.1);
            Assert.True(bounds.YMin >= -3.1 && bounds.YMax <= 3.1);
            Assert.True(bounds.ZMin >= -1.1 && bounds.ZMax <= 1.1);
        }
    }
}
