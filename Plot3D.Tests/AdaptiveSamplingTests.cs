using System;
using System.Linq;
using Plot3D.Core;
using Plot3D.Sampling;
using Xunit;

namespace Plot3D.Tests
{
    public class AdaptiveSamplingTests
    {
        [Fact]
        public void AdaptiveSampling_ConcentratesPointsAtHighCurvature()
        {
            // Gaussian peak: sharp curve around t=0, flat elsewhere on [-4, 4]
            // x = t, y = 0, z = exp(-20 * t^2)
            var options = new Plot3DOptions
            {
                TRange = new Interval(-4.0, 4.0),
                RelativeTolerance = 1e-3,
                MeshDensity = 15
            };

            var sampler = new AdaptiveSampler3D(
                t => t,
                t => 0.0,
                t => Math.Exp(-20.0 * t * t),
                options
            );

            var sampled = sampler.Sample();
            Assert.True(sampled.PointCount > 30);

            // Count points in high-curvature central region [-0.5, 0.5] vs flat outer regions [-4, -2]
            int peakCount = sampled.TValues.Count(t => t >= -0.5 && t <= 0.5);
            int outerCount = sampled.TValues.Count(t => t >= -4.0 && t <= -2.0);

            // Interval length of [-0.5, 0.5] is 1.0; outer interval [-4, -2] is 2.0 (twice as long)
            // But point density (points per unit length) at peak should be much higher!
            double peakDensity = peakCount / 1.0;
            double outerDensity = outerCount / 2.0;

            Assert.True(peakDensity > 2.0 * outerDensity,
                $"Peak point density ({peakDensity}) must exceed outer flat density ({outerDensity})");
        }

        [Fact]
        public void AdaptiveSampling_HandlesDiscontinuitiesGracefully()
        {
            // Asymptote / Pole at t = 0: y = 1/t
            var options = new Plot3DOptions
            {
                TRange = new Interval(-2.0, 2.0),
                ShowDiscontinuities = true
            };

            var line = ThreeDimensionalPlotter.Plot(
                t => t,
                t => Math.Abs(t) < 1e-4 ? double.NaN : 1.0 / t,
                t => t,
                options
            );

            Assert.NotNull(line);
            Assert.True(line.Segments.Count >= 2, "Discontinuous curve should be split into multiple segments");
        }

        [Fact]
        public void AdaptiveSampling_HandlesNaNAndInfinityWithoutCrashing()
        {
            // Square root of negative numbers returns NaN
            var line = ThreeDimensionalPlotter.Plot(
                t => t,
                t => t >= 0 ? Math.Sqrt(t) : double.NaN,
                t => t,
                -2.0,
                2.0
            );

            Assert.NotNull(line);
            Assert.True(line.Segments.Count >= 1);
            Assert.All(line.Segments[0], pt => Assert.True(pt.IsValid));
        }

        [Fact]
        public void AdaptiveSampling_HigherMeshDensityGuaranteesMinimumPoints()
        {
            // Linear curve has zero curvature, so point count equals MeshDensity exactly
            var samplerLow = new AdaptiveSampler3D(
                t => t,
                t => 2 * t,
                t => 3 * t,
                new Plot3DOptions { MeshDensity = 10, TRange = new Interval(0, 10) }
            );

            var samplerHigh = new AdaptiveSampler3D(
                t => t,
                t => 2 * t,
                t => 3 * t,
                new Plot3DOptions { MeshDensity = 50, TRange = new Interval(0, 10) }
            );

            var dataLow = samplerLow.Sample();
            var dataHigh = samplerHigh.Sample();

            Assert.Equal(10, dataLow.PointCount);
            Assert.Equal(50, dataHigh.PointCount);
            Assert.True(dataHigh.PointCount > dataLow.PointCount);
        }

        [Fact]
        public void AdaptiveSampling_AlwaysSatisfiesMinimumMeshDensity()
        {
            int requestedDensity = 37;
            var sampler = new AdaptiveSampler3D(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                new Plot3DOptions { MeshDensity = requestedDensity, TRange = new Interval(0, 10) }
            );

            var data = sampler.Sample();
            Assert.True(data.PointCount >= requestedDensity,
                $"Point count {data.PointCount} must be at least requested MeshDensity {requestedDensity}");
        }
    }
}
