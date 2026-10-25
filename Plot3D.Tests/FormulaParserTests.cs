using System;
using Plot3D.Core;
using Plot3D.Expressions;
using Xunit;

namespace Plot3D.Tests
{
    public class FormulaParserTests
    {
        private const double Tolerance = 1e-10;

        [Theory]
        [InlineData("t", 5.0, 5.0)]
        [InlineData("2 * t + 3", 4.0, 11.0)]
        [InlineData("t^2", 3.0, 9.0)]
        [InlineData("2^3^2", 0.0, 512.0)] // Right-associative exponentiation: 2^(3^2) = 2^9 = 512
        [InlineData("(2^3)^2", 0.0, 64.0)]
        [InlineData("10 - 3 - 2", 0.0, 5.0)] // Left-associative subtraction: (10-3)-2 = 5
        [InlineData("12 / 3 / 2", 0.0, 2.0)] // Left-associative division: (12/3)/2 = 2
        [InlineData("7 % 4", 0.0, 3.0)]
        public void BasicArithmeticAndPrecedence_EvaluatesCorrectly(string formula, double tVal, double expected)
        {
            var func = FormulaParser.Parse(formula);
            double actual = func(tVal);
            Assert.Equal(expected, actual, precision: 6);
        }

        [Theory]
        [InlineData("-t", 5.0, -5.0)]
        [InlineData("-t^2", 3.0, -9.0)]
        [InlineData("(-t)^2", 3.0, 9.0)]
        [InlineData("+t", 4.0, 4.0)]
        [InlineData("-(-t)", 7.0, 7.0)]
        public void UnaryOperators_EvaluatesCorrectly(string formula, double tVal, double expected)
        {
            var func = FormulaParser.Parse(formula);
            double actual = func(tVal);
            Assert.Equal(expected, actual, precision: 6);
        }

        [Theory]
        [InlineData("2t", 4.0, 8.0)]
        [InlineData("3sin(t)", Math.PI / 2.0, 3.0)]
        [InlineData("2(t + 1)", 3.0, 8.0)]
        [InlineData("(t + 1)(t - 1)", 5.0, 24.0)]
        [InlineData("t cos(t)", 0.0, 0.0)]
        [InlineData("2pi", 0.0, 2.0 * Math.PI)]
        [InlineData("2.5t", 2.0, 5.0)]
        public void ImplicitMultiplication_EvaluatesCorrectly(string formula, double tVal, double expected)
        {
            var func = FormulaParser.Parse(formula);
            double actual = func(tVal);
            Assert.Equal(expected, actual, precision: 6);
        }

        [Fact]
        public void MathematicalConstants_EvaluateAccurately()
        {
            Assert.Equal(Math.PI, FormulaParser.EvaluateConstant("pi"), precision: 10);
            Assert.Equal(Math.PI, FormulaParser.EvaluateConstant("PI"), precision: 10);
            Assert.Equal(Math.E, FormulaParser.EvaluateConstant("e"), precision: 10);
            Assert.Equal(2.0 * Math.PI, FormulaParser.EvaluateConstant("tau"), precision: 10);
            Assert.Equal((1.0 + Math.Sqrt(5.0)) * 0.5, FormulaParser.EvaluateConstant("phi"), precision: 10);
            Assert.Equal(6.0 * Math.PI, FormulaParser.EvaluateConstant("6*pi"), precision: 10);
        }

        [Theory]
        [InlineData("sin(t)", Math.PI / 6.0, 0.5)]
        [InlineData("cos(t)", Math.PI / 3.0, 0.5)]
        [InlineData("tan(t)", Math.PI / 4.0, 1.0)]
        [InlineData("asin(0.5)", 0.0, Math.PI / 6.0)]
        [InlineData("acos(0.5)", 0.0, Math.PI / 3.0)]
        [InlineData("atan(1.0)", 0.0, Math.PI / 4.0)]
        [InlineData("atan2(1.0, 1.0)", 0.0, Math.PI / 4.0)]
        [InlineData("sinh(0)", 0.0, 0.0)]
        [InlineData("cosh(0)", 0.0, 1.0)]
        [InlineData("tanh(0)", 0.0, 0.0)]
        [InlineData("exp(0)", 0.0, 1.0)]
        [InlineData("log(e)", 0.0, 1.0)]
        [InlineData("ln(e)", 0.0, 1.0)]
        [InlineData("log10(100)", 0.0, 2.0)]
        [InlineData("log2(8)", 0.0, 3.0)]
        [InlineData("sqrt(16)", 0.0, 4.0)]
        [InlineData("cbrt(-27)", 0.0, -3.0)]
        [InlineData("abs(-42)", 0.0, 42.0)]
        [InlineData("sign(-15)", 0.0, -1.0)]
        [InlineData("floor(3.7)", 0.0, 3.0)]
        [InlineData("ceil(3.2)", 0.0, 4.0)]
        [InlineData("round(3.6)", 0.0, 4.0)]
        [InlineData("sec(0)", 0.0, 1.0)]
        [InlineData("csc(pi/2)", 0.0, 1.0)]
        [InlineData("cot(pi/4)", 0.0, 1.0)]
        [InlineData("sinc(0)", 0.0, 1.0)]
        [InlineData("hypot(3, 4)", 0.0, 5.0)]
        [InlineData("min(10, 20)", 0.0, 10.0)]
        [InlineData("max(10, 20)", 0.0, 20.0)]
        [InlineData("clamp(15, 0, 10)", 0.0, 10.0)]
        [InlineData("rad2deg(pi)", 0.0, 180.0)]
        [InlineData("deg2rad(180)", 0.0, Math.PI)]
        public void StandardFunctions_EvaluateCorrectly(string formula, double tVal, double expected)
        {
            var func = FormulaParser.Parse(formula);
            double actual = func(tVal);
            Assert.Equal(expected, actual, precision: 6);
        }

        [Theory]
        [InlineData("sin(")]
        [InlineData("2 +")]
        [InlineData("(t + 1")]
        [InlineData("t + 1)")]
        [InlineData("unknownFunction(t)")]
        [InlineData("sin(1, 2)")] // Wrong argument count for 1-arg func
        [InlineData("atan2(1)")]   // Wrong argument count for 2-arg func
        [InlineData("")]
        [InlineData("   ")]
        public void InvalidFormulas_ThrowFormulaParseException(string invalidFormula)
        {
            Assert.ThrowsAny<Exception>(() => FormulaParser.Parse(invalidFormula));
            bool success = FormulaParser.TryParse(invalidFormula, out var func, out string? error);
            Assert.False(success);
            Assert.Null(func);
            Assert.NotNull(error);
        }

        [Theory]
        [InlineData("[0, 2*pi]", 0.0, 2.0 * Math.PI)]
        [InlineData("0, 6*pi", 0.0, 6.0 * Math.PI)]
        [InlineData("[-5, 5]", -5.0, 5.0)]
        [InlineData("0:20", 0.0, 20.0)]
        [InlineData("10", 0.0, 10.0)]
        public void ParseInterval_ParsesIntervalFormats(string intervalText, double expectedMin, double expectedMax)
        {
            var interval = FormulaParser.ParseInterval(intervalText);
            Assert.Equal(expectedMin, interval.Min, precision: 6);
            Assert.Equal(expectedMax, interval.Max, precision: 6);
        }

        [Fact]
        public void ComplexParametricKnotFormulas_MatchAnalyticalValues()
        {
            // Trefoil Knot at t = pi/4
            string xExpr = "sin(t) + 2*sin(2*t)";
            string yExpr = "cos(t) - 2*cos(2*t)";
            string zExpr = "-sin(3*t)";

            var funx = FormulaParser.Parse(xExpr);
            var funy = FormulaParser.Parse(yExpr);
            var funz = FormulaParser.Parse(zExpr);

            double t = Math.PI / 4.0;
            double expectedX = Math.Sin(t) + 2.0 * Math.Sin(2.0 * t);
            double expectedY = Math.Cos(t) - 2.0 * Math.Cos(2.0 * t);
            double expectedZ = -Math.Sin(3.0 * t);

            Assert.Equal(expectedX, funx(t), precision: 10);
            Assert.Equal(expectedY, funy(t), precision: 10);
            Assert.Equal(expectedZ, funz(t), precision: 10);
        }
    }
}
