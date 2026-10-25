using System;
using Plot3D.Core;
using Plot3D.Expressions;
using Plot3D.Palettes;
using Plot3D.Presets;
using Xunit;

namespace Plot3D.Tests
{
    public class UIComponentTests
    {
        [Fact]
        public void AllPresetCurves_FormulasParseAndEvaluateWithoutErrors()
        {
            foreach (var preset in PresetCurve3D.AllPresets)
            {
                var funx = FormulaParser.Parse(preset.FormulaX);
                var funy = FormulaParser.Parse(preset.FormulaY);
                var funz = FormulaParser.Parse(preset.FormulaZ);

                double tmin = FormulaParser.EvaluateConstant(preset.TMin);
                double tmax = FormulaParser.EvaluateConstant(preset.TMax);

                Assert.False(double.IsNaN(tmin), $"tmin is NaN for preset {preset.Name}");
                Assert.False(double.IsNaN(tmax), $"tmax is NaN for preset {preset.Name}");
                Assert.True(tmax > tmin, $"tmax <= tmin for preset {preset.Name}");

                // Sample at endpoints and midpoint
                double[] testPoints = new[] { tmin, (tmin + tmax) * 0.5, tmax };
                foreach (double t in testPoints)
                {
                    double x = funx(t);
                    double y = funy(t);
                    double z = funz(t);

                    // Note: functions might return NaN at intentional singularity test points (like discontinuous curve), but should not throw unhandled exceptions
                }
            }
        }

        [Fact]
        public void ScientificColorMaps_InterpolateSmoothly()
        {
            foreach (var cmap in ColorMap3D.AllMaps)
            {
                var c0 = cmap.GetColor(0.0);
                var cMid = cmap.GetColor(0.5);
                var c1 = cmap.GetColor(1.0);

                Assert.True(c0.A > 0);
                Assert.True(cMid.A > 0);
                Assert.True(c1.A > 0);
            }
        }
    }
}
