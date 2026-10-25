using System;
using System.IO;
using System.Text;
using Plot3D.Core;
using Plot3D.Expressions;
using Plot3D.Objects;
using Plot3D.Projection;
using Plot3D.Rendering;

namespace Plot3D.Figure
{
    /// <summary>
    /// Represents a high-level 3D plot figure container.
    /// </summary>
    public sealed class Plot3DFigure
    {
        public Axes3D CurrentAxes { get; }

        public bool HoldState { get; set; } = true;

        public Plot3DFigure()
        {
            CurrentAxes = new Axes3D();
        }

        public Plot3DFigure Title(string title)
        {
            CurrentAxes.Title = title ?? string.Empty;
            return this;
        }

        public Plot3DFigure XLabel(string label)
        {
            CurrentAxes.XLabel = label ?? string.Empty;
            return this;
        }

        public Plot3DFigure YLabel(string label)
        {
            CurrentAxes.YLabel = label ?? string.Empty;
            return this;
        }

        public Plot3DFigure ZLabel(string label)
        {
            CurrentAxes.ZLabel = label ?? string.Empty;
            return this;
        }

        public Plot3DFigure Grid(bool enable = true)
        {
            CurrentAxes.GridOn = enable;
            return this;
        }

        public Plot3DFigure Box(bool enable = true)
        {
            CurrentAxes.BoxOn = enable;
            return this;
        }

        public Plot3DFigure View(double azimuth, double elevation)
        {
            CurrentAxes.Camera.Azimuth = azimuth;
            CurrentAxes.Camera.Elevation = elevation;
            return this;
        }

        public Plot3DFigure Projection(ProjectionMode mode)
        {
            CurrentAxes.Camera.Projection = mode;
            return this;
        }

        public ParameterizedFunctionLine3D AddPlot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? tRange = null,
            LineSpec? lineSpec = null)
        {
            if (!HoldState)
            {
                CurrentAxes.Clear();
            }

            var line = new ParameterizedFunctionLine3D(
                funx, funy, funz,
                tRange ?? Interval.Default,
                lineSpec
            );

            CurrentAxes.AddLine(line);
            return line;
        }

        public ParameterizedFunctionLine3D AddPlot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Plot3DOptions options)
        {
            if (!HoldState)
            {
                CurrentAxes.Clear();
            }

            var line = new ParameterizedFunctionLine3D(funx, funy, funz, options);
            CurrentAxes.AddLine(line);
            return line;
        }

        public ParameterizedFunctionLine3D AddPlot(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            Interval? tRange = null,
            LineSpec? lineSpec = null)
        {
            var funx = FormulaParser.Parse(funxExpr);
            var funy = FormulaParser.Parse(funyExpr);
            var funz = FormulaParser.Parse(funzExpr);
            return AddPlot(funx, funy, funz, tRange, lineSpec);
        }

        public string ToSvg(int width = 800, int height = 600)
        {
            var renderer = new SvgRenderer3D(width, height);
            return renderer.Render(CurrentAxes);
        }

        public void SaveSvg(string filePath, int width = 800, int height = 600)
        {
            File.WriteAllText(filePath, ToSvg(width, height), Encoding.UTF8);
        }

        public string ToConsoleString(int columns = 78, int rows = 24)
        {
            var renderer = new ConsoleRenderer3D(columns, rows);
            return renderer.Render(CurrentAxes);
        }

        public void PrintConsole(int columns = 78, int rows = 24)
        {
            Console.WriteLine(ToConsoleString(columns, rows));
        }
    }
}
