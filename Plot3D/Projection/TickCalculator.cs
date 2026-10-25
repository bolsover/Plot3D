using System;
using System.Collections.Generic;

namespace Plot3D.Projection
{
    /// <summary>
    /// Computes readable, nicely spaced axis tick marks using the Heckbert / Wilkinson algorithm.
    /// </summary>
    public static class TickCalculator
    {
        public static double[] CalculateTicks(double min, double max, int targetTicks = 5)
        {
            if (double.IsNaN(min) || double.IsNaN(max) || double.IsInfinity(min) || double.IsInfinity(max))
                return new[] { 0.0 };

            if (Math.Abs(max - min) < 1e-12)
            {
                return new[] { min };
            }

            if (min > max)
            {
                double temp = min;
                min = max;
                max = temp;
            }

            double span = max - min;
            double roughStep = span / Math.Max(targetTicks - 1, 1);
            double step = NiceNum(roughStep, round: true);

            double niceMin = Math.Floor(min / step) * step;
            double niceMax = Math.Ceiling(max / step) * step;

            List<double> ticks = new List<double>();
            for (double t = niceMin; t <= niceMax + 0.5 * step; t += step)
            {
                // Clean floating point rounding issues
                double rounded = Math.Round(t, 10);
                if (rounded >= min - 1e-9 && rounded <= max + 1e-9)
                {
                    ticks.Add(rounded);
                }
            }

            if (ticks.Count == 0)
            {
                ticks.Add(min);
                ticks.Add(max);
            }

            return ticks.ToArray();
        }

        private static double NiceNum(double range, bool round)
        {
            double exponent = Math.Floor(Math.Log10(range));
            double fraction = range / Math.Pow(10, exponent);
            double niceFraction;

            if (round)
            {
                if (fraction < 1.5) niceFraction = 1.0;
                else if (fraction < 3.0) niceFraction = 2.0;
                else if (fraction < 7.0) niceFraction = 5.0;
                else niceFraction = 10.0;
            }
            else
            {
                if (fraction <= 1.0) niceFraction = 1.0;
                else if (fraction <= 2.0) niceFraction = 2.0;
                else if (fraction <= 5.0) niceFraction = 5.0;
                else niceFraction = 10.0;
            }

            return niceFraction * Math.Pow(10, exponent);
        }
    }
}
