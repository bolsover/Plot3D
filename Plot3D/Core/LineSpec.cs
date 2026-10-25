using System;

namespace Plot3D.Core
{
    /// <summary>
    /// Line specification (combining LineStyle, Marker, and Color).
    /// Parses specifiers such as 'r--o', 'b:', 'g-.*', 'm^', etc.
    /// </summary>
    public sealed class LineSpec : IEquatable<LineSpec>
    {
        public LineStyle? LineStyle { get; set; }
        public MarkerStyle? Marker { get; set; }
        public Color3D? Color { get; set; }

        public LineSpec(LineStyle? lineStyle = null, MarkerStyle? marker = null, Color3D? color = null)
        {
            LineStyle = lineStyle;
            Marker = marker;
            Color = color;
        }

        public static LineSpec Default => new LineSpec(Core.LineStyle.Solid, MarkerStyle.None, Color3D.Blue);

        public static LineSpec Parse(string? spec)
        {
            if (string.IsNullOrWhiteSpace(spec))
                return new LineSpec();

            string cleanSpec = spec!.Trim();

            if (cleanSpec.Equals("none", StringComparison.OrdinalIgnoreCase))
                return new LineSpec(Core.LineStyle.None, MarkerStyle.None, null);

            LineStyle? lineStyle = null;
            MarkerStyle? marker = null;
            Color3D? color = null;

            int i = 0;
            while (i < cleanSpec.Length)
            {
                // Check 2-character tokens first
                if (i + 1 < cleanSpec.Length)
                {
                    string two = cleanSpec.Substring(i, 2);
                    if (two == "--")
                    {
                        lineStyle = Core.LineStyle.Dashed;
                        i += 2;
                        continue;
                    }
                    if (two == "-.")
                    {
                        lineStyle = Core.LineStyle.DashDot;
                        i += 2;
                        continue;
                    }
                }

                char c = cleanSpec[i];

                // Single-char LineStyle
                if (c == '-')
                {
                    lineStyle = Core.LineStyle.Solid;
                    i++;
                    continue;
                }
                if (c == ':')
                {
                    lineStyle = Core.LineStyle.Dotted;
                    i++;
                    continue;
                }

                // Markers
                if (c == 'o') { marker = MarkerStyle.Circle; i++; continue; }
                if (c == '+') { marker = MarkerStyle.Plus; i++; continue; }
                if (c == '*') { marker = MarkerStyle.Asterisk; i++; continue; }
                if (c == '.') { marker = MarkerStyle.Point; i++; continue; }
                if (c == 'x' || c == 'X') { marker = MarkerStyle.Cross; i++; continue; }
                if (c == 's' || c == 'S') { marker = MarkerStyle.Square; i++; continue; }
                if (c == 'd' || c == 'D') { marker = MarkerStyle.Diamond; i++; continue; }
                if (c == '^') { marker = MarkerStyle.TriangleUp; i++; continue; }
                if (c == 'v') { marker = MarkerStyle.TriangleDown; i++; continue; }
                if (c == '>') { marker = MarkerStyle.TriangleRight; i++; continue; }
                if (c == '<') { marker = MarkerStyle.TriangleLeft; i++; continue; }
                if (c == 'p' || c == 'P') { marker = MarkerStyle.Pentagram; i++; continue; }
                if (c == 'h' || c == 'H') { marker = MarkerStyle.Hexagram; i++; continue; }

                // Colors
                char lower = char.ToLowerInvariant(c);
                if (lower == 'r' || lower == 'g' || lower == 'b' || lower == 'c' ||
                    lower == 'm' || lower == 'y' || lower == 'k' || lower == 'w')
                {
                    color = Color3D.FromChar(lower);
                    i++;
                    continue;
                }

                // Unrecognized character: skip
                i++;
            }

            // If markers are specified but no line style is explicitly given in the spec,
            // line style is None unless line style tokens were present.
            if (marker.HasValue && !lineStyle.HasValue)
            {
                lineStyle = Core.LineStyle.None;
            }

            return new LineSpec(lineStyle, marker, color);
        }

        public static implicit operator LineSpec(string spec) => Parse(spec);

        public bool Equals(LineSpec? other)
        {
            if (other is null) return false;
            return LineStyle == other.LineStyle && Marker == other.Marker && Color == other.Color;
        }

        public override bool Equals(object? obj) => obj is LineSpec other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = LineStyle.GetHashCode();
                hash = (hash * 397) ^ Marker.GetHashCode();
                hash = (hash * 397) ^ Color.GetHashCode();
                return hash;
            }
        }
    }
}
