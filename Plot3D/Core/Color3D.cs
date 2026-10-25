using System;
using System.Globalization;

namespace Plot3D.Core
{
    /// <summary>
    /// Represents an RGBA color with double or byte channels for cross-platform 3D visualization.
    /// </summary>
    public readonly struct Color3D : IEquatable<Color3D>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public double RDouble => R / 255.0;
        public double GDouble => G / 255.0;
        public double BDouble => B / 255.0;
        public double ADouble => A / 255.0;

        public Color3D(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color3D(double r, double g, double b, double a = 1.0)
        {
            R = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(r * 255.0)));
            G = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(g * 255.0)));
            B = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(b * 255.0)));
            A = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(a * 255.0)));
        }

        // Default colors
        public static Color3D Blue => new Color3D(0, 114, 189);        
        public static Color3D Red => new Color3D(217, 83, 25);         
        public static Color3D Yellow => new Color3D(237, 177, 32);    
        public static Color3D Purple => new Color3D(126, 47, 142);    
        public static Color3D Green => new Color3D(119, 172, 48);      
        public static Color3D Cyan => new Color3D(77, 190, 238);     
        public static Color3D Maroon => new Color3D(162, 20, 47);   

        // Basic Color Specifiers
        public static Color3D PureRed => new Color3D(255, 0, 0);
        public static Color3D PureGreen => new Color3D(0, 255, 0);
        public static Color3D PureBlue => new Color3D(0, 0, 255);
        public static Color3D PureCyan => new Color3D(0, 255, 255);
        public static Color3D PureMagenta => new Color3D(255, 0, 255);
        public static Color3D PureYellow => new Color3D(255, 255, 0);
        public static Color3D Black => new Color3D(0, 0, 0);
        public static Color3D White => new Color3D(255, 255, 255);
        public static Color3D Gray => new Color3D(128, 128, 128);
        public static Color3D DarkGray => new Color3D(64, 64, 64);
        public static Color3D LightGray => new Color3D(211, 211, 211);
        public static Color3D Transparent => new Color3D(0, 0, 0, 0);

        public static Color3D FromRgb(byte r, byte g, byte b) => new Color3D(r, g, b);
        public static Color3D FromArgb(byte a, byte r, byte g, byte b) => new Color3D(r, g, b, a);

        public static Color3D Lerp(in Color3D c1, in Color3D c2, double fraction)
        {
            double f = Math.Max(0.0, Math.Min(1.0, fraction));
            byte r = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(c1.R + (c2.R - c1.R) * f)));
            byte g = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(c1.G + (c2.G - c1.G) * f)));
            byte b = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(c1.B + (c2.B - c1.B) * f)));
            byte a = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(c1.A + (c2.A - c1.A) * f)));
            return new Color3D(r, g, b, a);
        }

        public static Color3D FromChar(char c)
        {
            switch (char.ToLowerInvariant(c))
            {
                case 'r': return PureRed;
                case 'g': return PureGreen;
                case 'b': return PureBlue;
                case 'c': return PureCyan;
                case 'm': return PureMagenta;
                case 'y': return PureYellow;
                case 'k': return Black;
                case 'w': return White;
                default: return Blue;
            }
        }

        public static Color3D FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Blue;
            hex = hex.Trim().TrimStart('#');

            if (hex.Length == 6)
            {
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return new Color3D(r, g, b);
            }
            if (hex.Length == 8)
            {
                byte a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return new Color3D(r, g, b, a);
            }
            if (hex.Length == 3)
            {
                byte r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return new Color3D(r, g, b);
            }
            return Blue;
        }

        public string ToHex(bool includeAlpha = false)
        {
            return includeAlpha
                ? string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", A, R, G, B)
                : string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", R, G, B);
        }

        public string ToRgbaString()
        {
            return string.Format(CultureInfo.InvariantCulture, "rgba({0}, {1}, {2}, {3:F3})", R, G, B, ADouble);
        }

        public bool Equals(Color3D other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is Color3D other && Equals(other);
        public override int GetHashCode() => unchecked((R << 24) | (G << 16) | (B << 8) | A);
        public static bool operator ==(Color3D left, Color3D right) => left.Equals(right);
        public static bool operator !=(Color3D left, Color3D right) => !left.Equals(right);

        public override string ToString() => ToHex(A < 255);
    }
}
