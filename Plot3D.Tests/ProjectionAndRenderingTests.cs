using System;
using Plot3D.Core;
using Plot3D.Export;
using Plot3D.Figure;
using Plot3D.Projection;
using Plot3D.Rendering;
using Xunit;

namespace Plot3D.Tests
{
    public class ProjectionAndRenderingTests
    {
        [Fact]
        public void Camera3D_Projects3DPointsIntoViewport()
        {
            var camera = new Camera3D(azimuth: -37.5, elevation: 30.0);
            var bounds = new Bounds3D(-1, 1, -1, 1, -1, 1);
            camera.Setup(bounds, 800, 600);

            var (x, y, depth) = camera.Project(Point3D.Zero, 800, 600);

            // Center of bounds (0,0,0) should project to center of viewport (400, 300)
            Assert.True(Math.Abs(x - 400.0) < 1.0, $"Expected x near 400, got {x}");
            Assert.True(Math.Abs(y - 300.0) < 1.0, $"Expected y near 300, got {y}");
        }

        [Fact]
        public void TickCalculator_ComputesCleanIntervals()
        {
            var ticks = TickCalculator.CalculateTicks(0.0, 10.0, 5);
            Assert.True(ticks.Length >= 4);
            Assert.Contains(0.0, ticks);
            Assert.Contains(10.0, ticks);
        }

        [Fact]
        public void SvgRenderer_ProducesValidSvgDocument()
        {
            var (figure, _) = ThreeDimensionalPlotter.PlotFigure(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                new Interval(0, 2 * Math.PI),
                LineSpec.Parse("r--o")
            );

            figure.Title("3D Helix").XLabel("X Axis").YLabel("Y Axis").ZLabel("Z Axis");

            string svg = figure.ToSvg(800, 600);

            Assert.NotNull(svg);
            Assert.Contains("<svg", svg);
            Assert.Contains("</svg>", svg);
            Assert.Contains("3D Helix", svg);
            Assert.Contains("<path", svg);
            Assert.Contains("<circle", svg);
        }

        [Fact]
        public void ConsoleRenderer_RendersNonEmptyAsciiGrid()
        {
            string ascii = ThreeDimensionalPlotter.RenderConsole(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                new Interval(0, 2 * Math.PI),
                null,
                columns: 60,
                rows: 20
            );

            Assert.NotNull(ascii);
            Assert.Contains("X:", ascii);
            Assert.Contains("Y:", ascii);
            Assert.Contains("Z:", ascii);
            Assert.True(ascii.Contains("*") || ascii.Contains("."));
        }

        [Fact]
        public void Exporters_ProduceValidObjAndCsv()
        {
            var line = ThreeDimensionalPlotter.Plot(
                t => t,
                t => t * t,
                t => t * t * t,
                -1.0,
                1.0
            );

            line.DisplayName = "CubicCurve";

            var axes = new Axes3D();
            axes.AddLine(line);

            string obj = ObjExporter3D.Export(axes);
            Assert.Contains("o CubicCurve", obj);
            Assert.Contains("v ", obj);
            Assert.Contains("l ", obj);

            string csv = DataExporter3D.ToCsv(line);
            Assert.StartsWith("t,x,y,z", csv);
            Assert.Contains("0,0,0,0", csv.Replace(" ", ""));
        }
    }
}
