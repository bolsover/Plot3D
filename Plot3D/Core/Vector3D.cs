using System;
using System.Globalization;

namespace Plot3D.Core
{
    /// <summary>
    /// Represents a 3-dimensional vector with double-precision components and standard linear algebra operations.
    /// </summary>
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Vector3D Zero => new Vector3D(0.0, 0.0, 0.0);
        public static Vector3D UnitX => new Vector3D(1.0, 0.0, 0.0);
        public static Vector3D UnitY => new Vector3D(0.0, 1.0, 0.0);
        public static Vector3D UnitZ => new Vector3D(0.0, 0.0, 1.0);

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
        public double LengthSquared => X * X + Y * Y + Z * Z;

        public bool IsValid => !double.IsNaN(X) && !double.IsInfinity(X) &&
                               !double.IsNaN(Y) && !double.IsInfinity(Y) &&
                               !double.IsNaN(Z) && !double.IsInfinity(Z);

        public Vector3D Normalized()
        {
            double len = Length;
            if (len <= 1e-15 || double.IsNaN(len) || double.IsInfinity(len))
                return Zero;
            return new Vector3D(X / len, Y / len, Z / len);
        }

        public double Dot(in Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

        public Vector3D Cross(in Vector3D other)
        {
            return new Vector3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }

        public double AngleTo(in Vector3D other)
        {
            double dot = Dot(other);
            double lenProduct = Length * other.Length;
            if (lenProduct <= 1e-15) return 0.0;
            double cos = dot / lenProduct;
            if (cos > 1.0) cos = 1.0;
            if (cos < -1.0) cos = -1.0;
            return Math.Acos(cos);
        }

        public static Vector3D operator +(in Vector3D a, in Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3D operator -(in Vector3D a, in Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3D operator -(in Vector3D a) => new Vector3D(-a.X, -a.Y, -a.Z);
        public static Vector3D operator *(in Vector3D a, double scalar) => new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Vector3D operator *(double scalar, in Vector3D a) => new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Vector3D operator /(in Vector3D a, double scalar) => new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);

        public static bool operator ==(in Vector3D left, in Vector3D right) => left.Equals(right);
        public static bool operator !=(in Vector3D left, in Vector3D right) => !left.Equals(right);

        public bool Equals(Vector3D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object? obj) => obj is Vector3D other && Equals(other);

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
            return string.Format(CultureInfo.InvariantCulture, "<{0:G6}, {1:G6}, {2:G6}>", X, Y, Z);
        }
    }
}
