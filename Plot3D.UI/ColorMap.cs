using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plot3D.UI
{
    /// <summary>
    /// Represents a color palette / colormap for mapping scalar values (e.g. Z coordinate, parameter t, or curvature) to RGB colors.
    /// </summary>
    public sealed class ColorMap
    {
        public string Name { get; }
        private readonly (double Pos, Color Color)[] _stops;

        public ColorMap(string name, params (double Pos, Color Color)[] stops)
        {
            Name = name ?? "Custom";
            _stops = stops ?? throw new ArgumentNullException(nameof(stops));
            if (_stops.Length == 0)
            {
                _stops = new[] { (0.0, Color.Blue), (1.0, Color.Red) };
            }
        }

        public Color GetColor(double normalizedValue)
        {
            if (double.IsNaN(normalizedValue)) return Color.Gray;
            double t = Math.Max(0.0, Math.Min(1.0, normalizedValue));

            if (t <= _stops[0].Pos) return _stops[0].Color;
            if (t >= _stops[_stops.Length - 1].Pos) return _stops[_stops.Length - 1].Color;

            for (int i = 0; i < _stops.Length - 1; i++)
            {
                if (t >= _stops[i].Pos && t <= _stops[i + 1].Pos)
                {
                    double range = _stops[i + 1].Pos - _stops[i].Pos;
                    double factor = range > 1e-12 ? (t - _stops[i].Pos) / range : 0.0;
                    return InterpolateColor(_stops[i].Color, _stops[i + 1].Color, factor);
                }
            }

            return _stops[_stops.Length - 1].Color;
        }

        private static Color InterpolateColor(Color c1, Color c2, double fraction)
        {
            int r = (int)Math.Round(c1.R + (c2.R - c1.R) * fraction);
            int g = (int)Math.Round(c1.G + (c2.G - c1.G) * fraction);
            int b = (int)Math.Round(c1.B + (c2.B - c1.B) * fraction);
            int a = (int)Math.Round(c1.A + (c2.A - c1.A) * fraction);

            return Color.FromArgb(
                Math.Max(0, Math.Min(255, a)),
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b))
            );
        }

        #region Predefined Scientific Color Maps

        public static ColorMap Parula { get; } = new ColorMap("Parula",
            (0.00, Color.FromArgb(53, 42, 135)),
            (0.20, Color.FromArgb(15, 92, 221)),
            (0.40, Color.FromArgb(18, 156, 203)),
            (0.60, Color.FromArgb(51, 187, 118)),
            (0.80, Color.FromArgb(161, 198, 41)),
            (1.00, Color.FromArgb(249, 251, 14))
        );

        public static ColorMap Turbo { get; } = new ColorMap("Turbo",
            (0.00, Color.FromArgb(48, 18, 59)),
            (0.15, Color.FromArgb(70, 134, 251)),
            (0.35, Color.FromArgb(27, 229, 181)),
            (0.55, Color.FromArgb(164, 252, 60)),
            (0.75, Color.FromArgb(251, 185, 56)),
            (0.90, Color.FromArgb(227, 68, 10)),
            (1.00, Color.FromArgb(122, 4, 3))
        );

        public static ColorMap Viridis { get; } = new ColorMap("Viridis",
            (0.00, Color.FromArgb(68, 1, 84)),
            (0.25, Color.FromArgb(59, 82, 139)),
            (0.50, Color.FromArgb(33, 145, 140)),
            (0.75, Color.FromArgb(94, 201, 98)),
            (1.00, Color.FromArgb(253, 231, 37))
        );

        public static ColorMap Jet { get; } = new ColorMap("Jet",
            (0.00, Color.FromArgb(0, 0, 143)),
            (0.12, Color.FromArgb(0, 0, 255)),
            (0.37, Color.FromArgb(0, 255, 255)),
            (0.62, Color.FromArgb(255, 255, 0)),
            (0.87, Color.FromArgb(255, 0, 0)),
            (1.00, Color.FromArgb(128, 0, 0))
        );

        public static ColorMap Plasma { get; } = new ColorMap("Plasma",
            (0.00, Color.FromArgb(13, 8, 135)),
            (0.25, Color.FromArgb(126, 3, 168)),
            (0.50, Color.FromArgb(204, 71, 120)),
            (0.75, Color.FromArgb(248, 149, 64)),
            (1.00, Color.FromArgb(240, 249, 33))
        );

        public static ColorMap CoolWarm { get; } = new ColorMap("CoolWarm",
            (0.00, Color.FromArgb(59, 76, 192)),
            (0.50, Color.FromArgb(221, 221, 221)),
            (1.00, Color.FromArgb(180, 4, 38))
        );

        public static ColorMap Rainbow { get; } = new ColorMap("Rainbow",
            (0.00, Color.FromArgb(148, 0, 211)),
            (0.20, Color.FromArgb(0, 0, 255)),
            (0.40, Color.FromArgb(0, 255, 0)),
            (0.60, Color.FromArgb(255, 255, 0)),
            (0.80, Color.FromArgb(255, 127, 0)),
            (1.00, Color.FromArgb(255, 0, 0))
        );

        public static ColorMap Magma { get; } = new ColorMap("Magma",
            (0.00, Color.FromArgb(0, 0, 4)),
            (0.25, Color.FromArgb(81, 18, 124)),
            (0.50, Color.FromArgb(182, 54, 121)),
            (0.75, Color.FromArgb(251, 136, 97)),
            (1.00, Color.FromArgb(252, 253, 191))
        );

        public static ColorMap Ocean { get; } = new ColorMap("Ocean",
            (0.00, Color.FromArgb(10, 25, 47)),
            (0.33, Color.FromArgb(23, 70, 162)),
            (0.66, Color.FromArgb(72, 191, 227)),
            (1.00, Color.FromArgb(224, 251, 252))
        );

        public static ColorMap Sunset { get; } = new ColorMap("Sunset",
            (0.00, Color.FromArgb(15, 32, 67)),
            (0.35, Color.FromArgb(133, 44, 114)),
            (0.70, Color.FromArgb(229, 90, 77)),
            (1.00, Color.FromArgb(255, 218, 121))
        );

        public static ColorMap Emerald { get; } = new ColorMap("Emerald",
            (0.00, Color.FromArgb(4, 47, 46)),
            (0.35, Color.FromArgb(13, 148, 136)),
            (0.70, Color.FromArgb(52, 211, 153)),
            (1.00, Color.FromArgb(209, 250, 229))
        );

        public static ColorMap Crimson { get; } = new ColorMap("Crimson",
            (0.00, Color.FromArgb(69, 10, 10)),
            (0.40, Color.FromArgb(185, 28, 28)),
            (0.75, Color.FromArgb(248, 113, 113)),
            (1.00, Color.FromArgb(254, 226, 226))
        );

        public static ColorMap ElectricBlue { get; } = new ColorMap("Electric Blue",
            (0.00, Color.FromArgb(3, 7, 30)),
            (0.35, Color.FromArgb(0, 119, 182)),
            (0.70, Color.FromArgb(0, 180, 216)),
            (1.00, Color.FromArgb(144, 224, 239))
        );

        public static IReadOnlyList<ColorMap> AllMaps { get; } = new List<ColorMap>
        {
            Parula,
            Turbo,
            Viridis,
            Jet,
            Plasma,
            CoolWarm,
            Rainbow,
            Magma,
            Ocean,
            Sunset,
            Emerald,
            Crimson,
            ElectricBlue
        };

        #endregion

        public override string ToString() => Name;
    }
}
