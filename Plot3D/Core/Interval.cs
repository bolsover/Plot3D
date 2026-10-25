using System;
using System.Globalization;

namespace Plot3D.Core
{
    /// <summary>
    /// Represents a 1-dimensional closed interval [Min, Max] for curve parametrization.
    /// </summary>
    public readonly struct Interval : IEquatable<Interval>
    {
        public double Min { get; }
        public double Max { get; }

        public static Interval Default => new Interval(-5.0, 5.0);

        public Interval(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max))
                throw new ArgumentException("Interval bounds cannot be NaN.");

            if (min <= max)
            {
                Min = min;
                Max = max;
            }
            else
            {
                Min = max;
                Max = min;
            }
        }

        public double Length => Max - Min;
        public double Midpoint => 0.5 * (Min + Max);

        public bool Contains(double value) => value >= Min && value <= Max;

        public double Clamp(double value)
        {
            if (value < Min) return Min;
            if (value > Max) return Max;
            return value;
        }

        public double Lerp(double fraction) => Min + fraction * (Max - Min);

        public double Normalize(double value)
        {
            double len = Length;
            if (len <= 1e-15) return 0.0;
            return (value - Min) / len;
        }

        public static implicit operator Interval((double Min, double Max) tuple) => new Interval(tuple.Min, tuple.Max);

        public bool Equals(Interval other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        public override bool Equals(object? obj) => obj is Interval other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        public static bool operator ==(Interval left, Interval right) => left.Equals(right);
        public static bool operator !=(Interval left, Interval right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0:G6}, {1:G6}]", Min, Max);
        }

        public void Deconstruct(out double min, out double max)
        {
            min = Min;
            max = Max;
        }
    }
}
