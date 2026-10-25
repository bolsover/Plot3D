using System;
using System.IO;
using Plot3D;
using Plot3D.Core;
using Plot3D.Export;
using Plot3D.Figure;

namespace Plot3D.Sample
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("===================================================================");
            Console.WriteLine(" Plot3D - C# Library Demo");
            Console.WriteLine("===================================================================\n");

            // Example 1: Circular Helix
            Console.WriteLine("Example 1: Circular Helix: x = sin(t), y = cos(t), z = t on [0, 6*pi]");
            var (helixFig, helixLine) = ThreeDimensionalPlotter.PlotFigure(
                t => Math.Sin(t),
                t => Math.Cos(t),
                t => t,
                new Interval(0.0, 6.0 * Math.PI),
                LineSpec.Parse("r--")
            );
            helixFig.Title("3D Circular Helix").XLabel("X = sin(t)").YLabel("Y = cos(t)").ZLabel("Z = t");

            Console.WriteLine(helixFig.ToConsoleString(columns: 74, rows: 20));
            Console.WriteLine($"Calculated Arc Length: {helixLine.ArcLength:F4} (Theoretical: {Math.Sqrt(2.0) * 6.0 * Math.PI:F4})");
            Console.WriteLine($"Sampled Points: {helixLine.Points.Count}, Function Evaluations: {helixLine.FunctionEvaluations}\n");

            // Example 2: Trefoil Knot
            Console.WriteLine("Example 2: Trefoil Knot on [0, 2*pi]");
            var (knotFig, knotLine) = ThreeDimensionalPlotter.PlotFigure(
                t => Math.Sin(t) + 2.0 * Math.Sin(2.0 * t),
                t => Math.Cos(t) - 2.0 * Math.Cos(2.0 * t),
                t => -Math.Sin(3.0 * t),
                new Interval(0.0, 2.0 * Math.PI),
                LineSpec.Parse("b-o")
            );
            knotFig.Title("Trefoil Knot");
            Console.WriteLine(knotFig.ToConsoleString(columns: 74, rows: 20));
            Console.WriteLine($"Sampled Points: {knotLine.Points.Count}, Arc Length: {knotLine.ArcLength:F4}\n");

            // Example 3: Torus Knot (p=2, q=3)
            Console.WriteLine("Example 3: Torus Knot (p=2, q=3) on [0, 2*pi]");
            int p = 2, q = 3;
            var (torusFig, torusLine) = ThreeDimensionalPlotter.PlotFigure(
                t => (2.0 + Math.Cos(q * t)) * Math.Cos(p * t),
                t => (2.0 + Math.Cos(q * t)) * Math.Sin(p * t),
                t => Math.Sin(q * t),
                new Interval(0.0, 2.0 * Math.PI),
                LineSpec.Parse("g-")
            );
            torusFig.Title("Torus Knot (2,3)");
            Console.WriteLine(torusFig.ToConsoleString(columns: 74, rows: 20));
            Console.WriteLine($"Sampled Points: {torusLine.Points.Count}\n");

            // Example 4: Exporting files
            string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            Directory.CreateDirectory(outDir);

            string svgPath = Path.Combine(outDir, "Helix.svg");
            helixFig.SaveSvg(svgPath, 900, 650);
            Console.WriteLine($"Exported SVG to: {svgPath}");

            string objPath = Path.Combine(outDir, "TrefoilKnot.obj");
            ObjExporter3D.SaveToFile(knotFig.CurrentAxes, objPath);
            Console.WriteLine($"Exported Wavefront OBJ to: {objPath}");

            string csvPath = Path.Combine(outDir, "TorusKnot.csv");
            DataExporter3D.SaveCsv(torusLine, csvPath);
            Console.WriteLine($"Exported CSV data to: {csvPath}\n");

            // Example 5: Dynamic Formula String Parsing & Plotting
            Console.WriteLine("Example 5: Dynamic String Formula Plotting: x = 'sin(3*t)', y = 'cos(4*t)', z = 'sin(5*t)' on [0, 2*pi]");
            var (lissajousFig, lissajousLine) = ThreeDimensionalPlotter.PlotFigure(
                "sin(3*t)",
                "cos(4*t)",
                "sin(5*t)",
                new Interval(0.0, 2.0 * Math.PI),
                LineSpec.Parse("m-")
            );
            lissajousFig.Title("Lissajous 3D Curve");
            Console.WriteLine(lissajousFig.ToConsoleString(columns: 74, rows: 20));
            Console.WriteLine($"Sampled Points: {lissajousLine.Points.Count}, Arc Length: {lissajousLine.ArcLength:F4}\n");

            Console.WriteLine("Interactive Desktop UI is available by running:");
            Console.WriteLine("  dotnet run --project Plot3D.UI\\Plot3D.UI.csproj\n");

            Console.WriteLine("All demonstrations completed successfully.");
        }
    }
}
