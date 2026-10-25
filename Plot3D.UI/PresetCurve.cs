using System.Collections.Generic;

namespace Plot3D.UI
{
    /// <summary>
    /// Represents a predefined 3D parametric curve formula preset.
    /// </summary>
    public sealed class PresetCurve
    {
        public string Name { get; }
        public string FormulaX { get; }
        public string FormulaY { get; }
        public string FormulaZ { get; }
        public string TMin { get; }
        public string TMax { get; }
        public string Description { get; }
        public ColorMap DefaultColorMap { get; }
        public double Azimuth { get; }
        public double Elevation { get; }

        public PresetCurve(
            string name,
            string formulaX,
            string formulaY,
            string formulaZ,
            string tMin,
            string tMax,
            string description,
            ColorMap defaultColorMap,
            double azimuth = -37.5,
            double elevation = 30.0)
        {
            Name = name;
            FormulaX = formulaX;
            FormulaY = formulaY;
            FormulaZ = formulaZ;
            TMin = tMin;
            TMax = tMax;
            Description = description;
            DefaultColorMap = defaultColorMap;
            Azimuth = azimuth;
            Elevation = elevation;
        }

        public static IReadOnlyList<PresetCurve> Presets { get; } = new List<PresetCurve>
        {
            new PresetCurve(
                "Circular Helix",
                "sin(t)",
                "cos(t)",
                "t",
                "0",
                "6*pi",
                "Standard 3D cylindrical circular helix winding around the Z-axis.",
                ColorMap.Turbo,
                -37.5, 30.0
            ),
            new PresetCurve(
                "Trefoil Knot",
                "sin(t) + 2*sin(2*t)",
                "cos(t) - 2*cos(2*t)",
                "-sin(3*t)",
                "0",
                "2*pi",
                "Classic simplest non-trivial mathematical knot (3_1 knot).",
                ColorMap.ElectricBlue,
                -35.0, 25.0
            ),
            new PresetCurve(
                "Torus Knot (2,3)",
                "(2 + cos(3*t)) * cos(2*t)",
                "(2 + cos(3*t)) * sin(2*t)",
                "sin(3*t)",
                "0",
                "2*pi",
                "Parametric knot lying on the surface of a torus winding 2 times around meridian and 3 times around longitude.",
                ColorMap.Parula,
                -45.0, 35.0
            ),
            new PresetCurve(
                "Viviani's Curve",
                "1 + cos(t)",
                "sin(t)",
                "2 * sin(t / 2)",
                "-2*pi",
                "2*pi",
                "Figure-eight intersection of a sphere with an intersecting cylinder.",
                ColorMap.Crimson,
                -50.0, 20.0
            ),
            new PresetCurve(
                "Conical Spiral",
                "t * cos(10*t)",
                "t * sin(10*t)",
                "t",
                "0",
                "2*pi",
                "Expanding conical spiral growing linearly with height.",
                ColorMap.Emerald,
                -30.0, 40.0
            ),
            new PresetCurve(
                "Lissajous 3D Knot",
                "sin(3*t)",
                "cos(4*t)",
                "sin(5*t)",
                "0",
                "2*pi",
                "Complex harmonic 3D Lissajous orbit with frequency ratios (3, 4, 5).",
                ColorMap.Viridis,
                -40.0, 30.0
            ),
            new PresetCurve(
                "Spherical Spiral",
                "cos(8*t) * cos(t)",
                "sin(8*t) * cos(t)",
                "sin(t)",
                "-pi/2",
                "pi/2",
                "Spiral curve constrained to the surface of a unit sphere from south to north pole.",
                ColorMap.Ocean,
                -25.0, 30.0
            ),
            new PresetCurve(
                "Damped 3D Spiral Wave",
                "exp(-0.12*t) * cos(5*t)",
                "exp(-0.12*t) * sin(5*t)",
                "t",
                "0",
                "20",
                "Exponentially decaying oscillating spiral trajectory.",
                ColorMap.CoolWarm,
                -40.0, 35.0
            ),
            new PresetCurve(
                "Butterfly 3D Curve",
                "sin(t) * (exp(cos(t)) - 2*cos(4*t) - sin(t/12)^5)",
                "cos(t) * (exp(cos(t)) - 2*cos(4*t) - sin(t/12)^5)",
                "sin(t / 12) * 3",
                "0",
                "12*pi",
                "Temple H. Fay's transcendental butterfly curve projected into 3D.",
                ColorMap.Sunset,
                -45.0, 40.0
            ),
            new PresetCurve(
                "Saddle Helix",
                "cos(t)",
                "sin(t)",
                "cos(2*t)",
                "0",
                "2*pi",
                "Periodic curve traversing along a hyperbolic paraboloid saddle surface.",
                ColorMap.Plasma,
                -35.0, 30.0
            ),
            new PresetCurve(
                "Clelia Curve",
                "cos(4*t) * cos(t)",
                "sin(4*t) * cos(t)",
                "sin(t)",
                "0",
                "2*pi",
                "Spherical Clelia trajectory parameterized by longitude and latitude.",
                ColorMap.Rainbow,
                -40.0, 35.0
            ),
            new PresetCurve(
                "Rose Curve in 3D",
                "cos(3*t) * cos(t)",
                "cos(3*t) * sin(t)",
                "0.5 * sin(6*t)",
                "0",
                "2*pi",
                "Multi-petaled 3D rhodonea rose curve with harmonic vertical oscillation.",
                ColorMap.Magma,
                -30.0, 50.0
            ),
            new PresetCurve(
                "Rational Discontinuous Curve",
                "t",
                "1 / (t - 1)",
                "t^2",
                "-2",
                "4",
                "Demonstration of adaptive singularity and asymptote detection at t = 1.",
                ColorMap.Jet,
                -40.0, 25.0
            )
        };

        public override string ToString() => Name;
    }
}
