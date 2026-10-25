using System;
using System.Globalization;

namespace Plot3D.Core
{
    /// <summary>
    /// Represents a point in 3-dimensional Cartesian space with double-precision coordinates.
    /// </summary>
    public readonly struct Point3D : IEquatable<Point3D>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Point3D Zero => new Point3D(0.0, 0.0, 0.0);
        public static Point3D NaN => new Point3D(double.NaN, double.NaN, double.NaN);

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsValid => !double.IsNaN(X) && !double.IsInfinity(X) &&
                               !double.IsNaN(Y) && !double.IsInfinity(Y) &&
                               !double.IsNaN(Z) && !double.IsInfinity(Z);

        public bool IsNaN => double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);

        public double DistanceTo(in Point3D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double DistanceSquaredTo(in Point3D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
        public double MagnitudeSquared => X * X + Y * Y + Z * Z;

        public static Point3D Midpoint(in Point3D a, in Point3D b)
        {
            return new Point3D(
                0.5 * (a.X + b.X),
                0.5 * (a.Y + b.Y),
                0.5 * (a.Z + b.Z)
            );
        }

        public static Point3D Lerp(in Point3D a, in Point3D b, double t)
        {
            return new Point3D(
                a.X + t * (b.X - a.X),
                a.Y + t * (b.Y - a.Y),
                a.Z + t * (b.Z - a.Z)
            );
        }

        public static Point3D operator +(in Point3D a, in Vector3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator +(in Point3D a, in Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3D operator -(in Point3D a, in Point3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Point3D operator *(in Point3D a, double scalar) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Point3D operator *(double scalar, in Point3D a) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Point3D operator /(in Point3D a, double scalar) => new Point3D(a.X / scalar, a.Y / scalar, a.Z / scalar);

        public static bool operator ==(in Point3D left, in Point3D right) => left.Equals(right);
        public static bool operator !=(in Point3D left, in Point3D right) => !left.Equals(right);

        public bool Equals(Point3D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object? obj) => obj is Point3D other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Z.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:G6}, {1:G6}, {2:G6})", X, Y, Z);
        }

        public void Deconstruct(out double x, out double y, out double z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
}
