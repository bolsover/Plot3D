using System;

namespace Plot3D.Expressions
{
    /// <summary>
    /// Provides mathematical helper functions used by the formula parser and evaluator.
    /// </summary>
    public static class MathFunctionHelper
    {
        public static double Cbrt(double x)
        {
            if (x >= 0.0) return Math.Pow(x, 1.0 / 3.0);
            return -Math.Pow(-x, 1.0 / 3.0);
        }

        public static double Log2(double x)
        {
            return Math.Log(x) / Math.Log(2.0);
        }

        public static double Sec(double x)
        {
            return 1.0 / Math.Cos(x);
        }

        public static double Csc(double x)
        {
            return 1.0 / Math.Sin(x);
        }

        public static double Cot(double x)
        {
            return 1.0 / Math.Tan(x);
        }

        public static double Asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1.0));
        }

        public static double Acosh(double x)
        {
            if (x < 1.0) return double.NaN;
            return Math.Log(x + Math.Sqrt(x * x - 1.0));
        }

        public static double Atanh(double x)
        {
            if (Math.Abs(x) >= 1.0) return double.NaN;
            return 0.5 * Math.Log((1.0 + x) / (1.0 - x));
        }

        public static double Sinc(double x)
        {
            if (Math.Abs(x) < 1e-12) return 1.0;
            return Math.Sin(x) / x;
        }

        public static double Heaviside(double x)
        {
            if (double.IsNaN(x)) return double.NaN;
            return x >= 0.0 ? 1.0 : 0.0;
        }

        public static double Rad2Deg(double rad)
        {
            return rad * (180.0 / Math.PI);
        }

        public static double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180.0);
        }

        public static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
    }
}
