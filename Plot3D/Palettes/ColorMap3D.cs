using System;
using System.Collections.Generic;
using Plot3D.Core;

namespace Plot3D.Palettes
{
    /// <summary>
    /// Represents a platform-independent scientific colormap for mapping normalized scalar values to Color3D.
    /// </summary>
    public sealed class ColorMap3D
    {
        public string Name { get; }
        private readonly (double Pos, Color3D Color)[] _stops;

        public ColorMap3D(string name, params (double Pos, Color3D Color)[] stops)
        {
            Name = name ?? "Custom";
            _stops = stops ?? throw new ArgumentNullException(nameof(stops));
            if (_stops.Length == 0)
            {
                _stops = new[] { (0.0, Color3D.PureBlue), (1.0, Color3D.PureRed) };
            }
        }

        public Color3D GetColor(double normalizedValue)
        {
            if (double.IsNaN(normalizedValue)) return Color3D.Gray;
            double t = Math.Max(0.0, Math.Min(1.0, normalizedValue));

            if (t <= _stops[0].Pos) return _stops[0].Color;
            if (t >= _stops[_stops.Length - 1].Pos) return _stops[_stops.Length - 1].Color;

            for (int i = 0; i < _stops.Length - 1; i++)
            {
                if (t >= _stops[i].Pos && t <= _stops[i + 1].Pos)
                {
                    double range = _stops[i + 1].Pos - _stops[i].Pos;
                    double factor = range > 1e-12 ? (t - _stops[i].Pos) / range : 0.0;
                    return Color3D.Lerp(_stops[i].Color, _stops[i + 1].Color, factor);
                }
            }

            return _stops[_stops.Length - 1].Color;
        }

        #region Predefined Scientific Color Maps

        public static ColorMap3D Parula { get; } = new ColorMap3D("Parula",
            (0.00, new Color3D(53, 42, 135)),
            (0.20, new Color3D(15, 92, 221)),
            (0.40, new Color3D(18, 156, 203)),
            (0.60, new Color3D(51, 187, 118)),
            (0.80, new Color3D(161, 198, 41)),
            (1.00, new Color3D(249, 251, 14))
        );

        public static ColorMap3D Turbo { get; } = new ColorMap3D("Turbo",
            (0.00, new Color3D(48, 18, 59)),
            (0.15, new Color3D(70, 134, 251)),
            (0.35, new Color3D(27, 229, 181)),
            (0.55, new Color3D(164, 252, 60)),
            (0.75, new Color3D(251, 185, 56)),
            (0.90, new Color3D(227, 68, 10)),
            (1.00, new Color3D(122, 4, 3))
        );

        public static ColorMap3D Viridis { get; } = new ColorMap3D("Viridis",
            (0.00, new Color3D(68, 1, 84)),
            (0.25, new Color3D(59, 82, 139)),
            (0.50, new Color3D(33, 145, 140)),
            (0.75, new Color3D(94, 201, 98)),
            (1.00, new Color3D(253, 231, 37))
        );

        public static ColorMap3D Jet { get; } = new ColorMap3D("Jet",
            (0.00, new Color3D(0, 0, 143)),
            (0.12, new Color3D(0, 0, 255)),
            (0.37, new Color3D(0, 255, 255)),
            (0.62, new Color3D(255, 255, 0)),
            (0.87, new Color3D(255, 0, 0)),
            (1.00, new Color3D(128, 0, 0))
        );

        public static ColorMap3D Plasma { get; } = new ColorMap3D("Plasma",
            (0.00, new Color3D(13, 8, 135)),
            (0.25, new Color3D(126, 3, 168)),
            (0.50, new Color3D(204, 71, 120)),
            (0.75, new Color3D(248, 149, 64)),
            (1.00, new Color3D(240, 249, 33))
        );

        public static ColorMap3D CoolWarm { get; } = new ColorMap3D("CoolWarm",
            (0.00, new Color3D(59, 76, 192)),
            (0.50, new Color3D(221, 221, 221)),
            (1.00, new Color3D(180, 4, 38))
        );

        public static ColorMap3D Rainbow { get; } = new ColorMap3D("Rainbow",
            (0.00, new Color3D(148, 0, 211)),
            (0.20, new Color3D(0, 0, 255)),
            (0.40, new Color3D(0, 255, 0)),
            (0.60, new Color3D(255, 255, 0)),
            (0.80, new Color3D(255, 127, 0)),
            (1.00, new Color3D(255, 0, 0))
        );

        public static ColorMap3D Magma { get; } = new ColorMap3D("Magma",
            (0.00, new Color3D(0, 0, 4)),
            (0.25, new Color3D(81, 18, 124)),
            (0.50, new Color3D(182, 54, 121)),
            (0.75, new Color3D(251, 136, 97)),
            (1.00, new Color3D(252, 253, 191))
        );

        public static ColorMap3D Ocean { get; } = new ColorMap3D("Ocean",
            (0.00, new Color3D(10, 25, 47)),
            (0.33, new Color3D(23, 70, 162)),
            (0.66, new Color3D(72, 191, 227)),
            (1.00, new Color3D(224, 251, 252))
        );

        public static ColorMap3D Sunset { get; } = new ColorMap3D("Sunset",
            (0.00, new Color3D(15, 32, 67)),
            (0.35, new Color3D(133, 44, 114)),
            (0.70, new Color3D(229, 90, 77)),
            (1.00, new Color3D(255, 218, 121))
        );

        public static ColorMap3D Emerald { get; } = new ColorMap3D("Emerald",
            (0.00, new Color3D(4, 47, 46)),
            (0.35, new Color3D(13, 148, 136)),
            (0.70, new Color3D(52, 211, 153)),
            (1.00, new Color3D(209, 250, 229))
        );

        public static ColorMap3D Crimson { get; } = new ColorMap3D("Crimson",
            (0.00, new Color3D(69, 10, 10)),
            (0.40, new Color3D(185, 28, 28)),
            (0.75, new Color3D(248, 113, 113)),
            (1.00, new Color3D(254, 226, 226))
        );

        public static ColorMap3D ElectricBlue { get; } = new ColorMap3D("Electric Blue",
            (0.00, new Color3D(3, 7, 30)),
            (0.35, new Color3D(0, 119, 182)),
            (0.70, new Color3D(0, 180, 216)),
            (1.00, new Color3D(144, 224, 239))
        );

        public static IReadOnlyList<ColorMap3D> AllMaps { get; } = new List<ColorMap3D>
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
