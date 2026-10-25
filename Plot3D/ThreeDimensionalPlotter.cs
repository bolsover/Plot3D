using System;
using Plot3D.Core;
using Plot3D.Expressions;
using Plot3D.Figure;
using Plot3D.Objects;

namespace Plot3D
{
    /// <summary>
    /// Plots 3D parametric curves x = funx(t), y = funy(t), z = funz(t) using adaptive mesh generation.
    /// </summary>
    public static class ThreeDimensionalPlotter
    {
        /// <summary>
        /// Plots the 3D parametric curve over the default interval [-5, 5].
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz)
        {
            return Plot(funx, funy, funz, Interval.Default, (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve over the specified interval [tmin, tmax].
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            double tmin,
            double tmax)
        {
            return Plot(funx, funy, funz, new Interval(tmin, tmax), (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve over the specified interval tuple (tmin, tmax).
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            (double Min, double Max) interval)
        {
            return Plot(funx, funy, funz, new Interval(interval.Min, interval.Max), (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve over the specified interval.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval interval)
        {
            return Plot(funx, funy, funz, interval, (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve using a LineSpec string (e.g. 'r--o', 'b:', 'g-.*') over [-5, 5].
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            string lineSpec)
        {
            return Plot(funx, funy, funz, Interval.Default, LineSpec.Parse(lineSpec));
        }

        /// <summary>
        /// Plots the 3D parametric curve over [tmin, tmax] using a LineSpec string.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            double tmin,
            double tmax,
            string lineSpec)
        {
            return Plot(funx, funy, funz, new Interval(tmin, tmax), LineSpec.Parse(lineSpec));
        }

        /// <summary>
        /// Plots the 3D parametric curve over (tmin, tmax) using a LineSpec string.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            (double Min, double Max) interval,
            string lineSpec)
        {
            return Plot(funx, funy, funz, new Interval(interval.Min, interval.Max), LineSpec.Parse(lineSpec));
        }

        /// <summary>
        /// Plots the 3D parametric curve over the specified interval using a LineSpec object.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval interval,
            LineSpec? lineSpec)
        {
            var options = Plot3DOptions.CreateDefault();
            options.TRange = interval;
            if (lineSpec != null)
            {
                options.ApplyLineSpec(lineSpec);
            }
            return Plot(funx, funy, funz, options);
        }

        /// <summary>
        /// Plots the 3D parametric curve using custom Plot3DOptions.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Plot3DOptions options)
        {
            if (funx == null) throw new ArgumentNullException(nameof(funx));
            if (funy == null) throw new ArgumentNullException(nameof(funy));
            if (funz == null) throw new ArgumentNullException(nameof(funz));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return new ParameterizedFunctionLine3D(funx, funy, funz, options);
        }

        /// <summary>
        /// Creates a new figure containing the 3D parametric plot.
        /// </summary>
        public static (Plot3DFigure Figure, ParameterizedFunctionLine3D Line) PlotFigure(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? interval = null,
            LineSpec? lineSpec = null)
        {
            var figure = new Plot3DFigure();
            var line = figure.AddPlot(funx, funy, funz, interval, lineSpec);
            return (figure, line);
        }

        /// <summary>
        /// Computes discrete coordinate arrays (X, Y, Z, T) .
        /// </summary>
        public static (double[] X, double[] Y, double[] Z, double[] T) Calculate(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? interval = null,
            int? meshDensity = null)
        {
            var options = Plot3DOptions.CreateDefault();
            if (interval.HasValue) options.TRange = interval.Value;
            if (meshDensity.HasValue) options.MeshDensity = meshDensity.Value;

            var line = new ParameterizedFunctionLine3D(funx, funy, funz, options);
            return line.SampledData.ToArrays();
        }

        /// <summary>
        /// Computes discrete coordinate arrays (X, Y, Z, T) over [tmin, tmax].
        /// </summary>
        public static (double[] X, double[] Y, double[] Z, double[] T) Calculate(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            double tmin,
            double tmax,
            int? meshDensity = null)
        {
            return Calculate(funx, funy, funz, new Interval(tmin, tmax), meshDensity);
        }

        /// <summary>
        /// Computes discrete Point3D curve points over the specified interval.
        /// </summary>
        public static Point3D[] CalculatePoints(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? interval = null,
            int? meshDensity = null)
        {
            var options = Plot3DOptions.CreateDefault();
            if (interval.HasValue) options.TRange = interval.Value;
            if (meshDensity.HasValue) options.MeshDensity = meshDensity.Value;

            var line = new ParameterizedFunctionLine3D(funx, funy, funz, options);
            var pts = line.Points;
            var result = new Point3D[pts.Count];
            for (int i = 0; i < pts.Count; i++)
            {
                result[i] = pts[i];
            }
            return result;
        }

        /// <summary>
        /// Saves the 3D parametric plot as an SVG vector image file.
        /// </summary>
        public static void SaveSvg(
            string filePath,
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? interval = null,
            LineSpec? lineSpec = null,
            int width = 800,
            int height = 600)
        {
            var (fig, _) = PlotFigure(funx, funy, funz, interval, lineSpec);
            fig.SaveSvg(filePath, width, height);
        }

        /// <summary>
        /// Generates a terminal/console ASCII representation of the 3D parametric plot.
        /// </summary>
        public static string RenderConsole(
            Func<double, double> funx,
            Func<double, double> funy,
            Func<double, double> funz,
            Interval? interval = null,
            LineSpec? lineSpec = null,
            int columns = 78,
            int rows = 24)
        {
            var (fig, _) = PlotFigure(funx, funy, funz, interval, lineSpec);
            return fig.ToConsoleString(columns, rows);
        }

        #region String Formula Overloads

        /// <summary>
        /// Plots the 3D parametric curve from mathematical formula strings x(t), y(t), z(t) over [-5, 5].
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            string funxExpr,
            string funyExpr,
            string funzExpr)
        {
            return Plot(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), Interval.Default, (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve from mathematical formula strings x(t), y(t), z(t) over [tmin, tmax].
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            double tmin,
            double tmax)
        {
            return Plot(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), new Interval(tmin, tmax), (LineSpec?)null);
        }

        /// <summary>
        /// Plots the 3D parametric curve from mathematical formula strings x(t), y(t), z(t) over the specified interval.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            Interval interval,
            LineSpec? lineSpec = null)
        {
            return Plot(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), interval, lineSpec);
        }

        /// <summary>
        /// Plots the 3D parametric curve from mathematical formula strings with a LineSpec string.
        /// </summary>
        public static ParameterizedFunctionLine3D Plot(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            double tmin,
            double tmax,
            string lineSpec)
        {
            return Plot(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), new Interval(tmin, tmax), LineSpec.Parse(lineSpec));
        }

        /// <summary>
        /// Creates a new figure containing the 3D parametric plot evaluated from mathematical formula strings.
        /// </summary>
        public static (Plot3DFigure Figure, ParameterizedFunctionLine3D Line) PlotFigure(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            Interval? interval = null,
            LineSpec? lineSpec = null)
        {
            return PlotFigure(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), interval, lineSpec);
        }

        /// <summary>
        /// Computes discrete coordinate arrays from mathematical formula strings.
        /// </summary>
        public static (double[] X, double[] Y, double[] Z, double[] T) Calculate(
            string funxExpr,
            string funyExpr,
            string funzExpr,
            Interval? interval = null,
            int? meshDensity = null)
        {
            return Calculate(FormulaParser.Parse(funxExpr), FormulaParser.Parse(funyExpr), FormulaParser.Parse(funzExpr), interval, meshDensity);
        }

        #endregion
    }
}
