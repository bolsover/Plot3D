using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plot3D.Core
{
    /// <summary>
    /// Represents an axis-aligned 3D bounding box [XMin, XMax] x [YMin, YMax] x [ZMin, ZMax].
    /// </summary>
    public readonly struct Bounds3D : IEquatable<Bounds3D>
    {
        public double XMin { get; }
        public double XMax { get; }
        public double YMin { get; }
        public double YMax { get; }
        public double ZMin { get; }
        public double ZMax { get; }

        public static Bounds3D Empty => new Bounds3D(
            double.PositiveInfinity, double.NegativeInfinity,
            double.PositiveInfinity, double.NegativeInfinity,
            double.PositiveInfinity, double.NegativeInfinity
        );

        public Bounds3D(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            XMin = Math.Min(xmin, xmax);
            XMax = Math.Max(xmin, xmax);
            YMin = Math.Min(ymin, ymax);
            YMax = Math.Max(ymin, ymax);
            ZMin = Math.Min(zmin, zmax);
            ZMax = Math.Max(zmin, zmax);
        }

        public bool IsEmpty => XMin > XMax || YMin > YMax || ZMin > ZMax ||
                               double.IsInfinity(XMin) || double.IsInfinity(XMax);

        public double DeltaX => IsEmpty ? 0.0 : XMax - XMin;
        public double DeltaY => IsEmpty ? 0.0 : YMax - YMin;
        public double DeltaZ => IsEmpty ? 0.0 : ZMax - ZMin;

        public Point3D Center => IsEmpty ? Point3D.Zero : new Point3D(
            0.5 * (XMin + XMax),
            0.5 * (YMin + YMax),
            0.5 * (ZMin + ZMax)
        );

        public double MaxDimension => Math.Max(DeltaX, Math.Max(DeltaY, DeltaZ));

        public Bounds3D Include(in Point3D pt)
        {
            if (!pt.IsValid) return this;
            if (IsEmpty)
            {
                return new Bounds3D(pt.X, pt.X, pt.Y, pt.Y, pt.Z, pt.Z);
            }
            return new Bounds3D(
                Math.Min(XMin, pt.X), Math.Max(XMax, pt.X),
                Math.Min(YMin, pt.Y), Math.Max(YMax, pt.Y),
                Math.Min(ZMin, pt.Z), Math.Max(ZMax, pt.Z)
            );
        }

        public Bounds3D Union(in Bounds3D other)
        {
            if (IsEmpty) return other;
            if (other.IsEmpty) return this;
            return new Bounds3D(
                Math.Min(XMin, other.XMin), Math.Max(XMax, other.XMax),
                Math.Min(YMin, other.YMin), Math.Max(YMax, other.YMax),
                Math.Min(ZMin, other.ZMin), Math.Max(ZMax, other.ZMax)
            );
        }

        public Bounds3D Pad(double factor = 0.05)
        {
            if (IsEmpty) return this;

            double dx = Math.Max(DeltaX * factor, 1e-6);
            double dy = Math.Max(DeltaY * factor, 1e-6);
            double dz = Math.Max(DeltaZ * factor, 1e-6);

            return new Bounds3D(
                XMin - dx, XMax + dx,
                YMin - dy, YMax + dy,
                ZMin - dz, ZMax + dz
            );
        }

        public static Bounds3D FromPoints(IEnumerable<Point3D> points)
        {
            if (points == null) return Empty;
            Bounds3D bounds = Empty;
            foreach (var pt in points)
            {
                bounds = bounds.Include(pt);
            }
            return bounds;
        }

        public bool Contains(in Point3D pt)
        {
            if (IsEmpty || !pt.IsValid) return false;
            return pt.X >= XMin && pt.X <= XMax &&
                   pt.Y >= YMin && pt.Y <= YMax &&
                   pt.Z >= ZMin && pt.Z <= ZMax;
        }

        public bool Equals(Bounds3D other)
        {
            return XMin.Equals(other.XMin) && XMax.Equals(other.XMax) &&
                   YMin.Equals(other.YMin) && YMax.Equals(other.YMax) &&
                   ZMin.Equals(other.ZMin) && ZMax.Equals(other.ZMax);
        }

        public override bool Equals(object? obj) => obj is Bounds3D other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = XMin.GetHashCode();
                hash = (hash * 397) ^ XMax.GetHashCode();
                hash = (hash * 397) ^ YMin.GetHashCode();
                hash = (hash * 397) ^ YMax.GetHashCode();
                hash = (hash * 397) ^ ZMin.GetHashCode();
                hash = (hash * 397) ^ ZMax.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Bounds3D left, Bounds3D right) => left.Equals(right);
        public static bool operator !=(Bounds3D left, Bounds3D right) => !left.Equals(right);

        public override string ToString()
        {
            if (IsEmpty) return "[Empty Bounds]";
            return string.Format(CultureInfo.InvariantCulture,
                "X:[{0:G4}, {1:G4}] Y:[{2:G4}, {3:G4}] Z:[{4:G4}, {5:G4}]",
                XMin, XMax, YMin, YMax, ZMin, ZMax);
        }
    }
}
