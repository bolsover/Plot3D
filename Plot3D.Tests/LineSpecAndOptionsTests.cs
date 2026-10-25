using Plot3D.Core;
using Xunit;

namespace Plot3D.Tests
{
    public class LineSpecAndOptionsTests
    {
        [Theory]
        [InlineData("r--o", LineStyle.Dashed, MarkerStyle.Circle, 255, 0, 0)]
        [InlineData("b:", LineStyle.Dotted, MarkerStyle.None, 0, 0, 255)]
        [InlineData("g-.*", LineStyle.DashDot, MarkerStyle.Asterisk, 0, 255, 0)]
        [InlineData("m^", LineStyle.None, MarkerStyle.TriangleUp, 255, 0, 255)]
        [InlineData("k", LineStyle.Solid, MarkerStyle.None, 0, 0, 0)]
        [InlineData("--", LineStyle.Dashed, MarkerStyle.None, 0, 114, 189)] // default blue
        [InlineData("none", LineStyle.None, MarkerStyle.None, 0, 114, 189)]
        public void LineSpec_ParsesStringSpecsAccurately(
            string specStr,
            LineStyle expectedStyle,
            MarkerStyle expectedMarker,
            byte expectedR,
            byte expectedG,
            byte expectedB)
        {
            var spec = LineSpec.Parse(specStr);

            Assert.Equal(expectedStyle, spec.LineStyle ?? LineStyle.Solid);
            Assert.Equal(expectedMarker, spec.Marker ?? MarkerStyle.None);

            if (spec.Color.HasValue)
            {
                Assert.Equal(expectedR, spec.Color.Value.R);
                Assert.Equal(expectedG, spec.Color.Value.G);
                Assert.Equal(expectedB, spec.Color.Value.B);
            }
        }

        [Fact]
        public void Color3D_HexAndRgbConversion()
        {
            var color = Color3D.FromHex("#FF5733");
            Assert.Equal(255, color.R);
            Assert.Equal(87, color.G);
            Assert.Equal(51, color.B);
            Assert.Equal(255, color.A);

            string hex = color.ToHex();
            Assert.Equal("#FF5733", hex);
        }

        [Fact]
        public void Plot3DOptions_CloneAndApplyLineSpec()
        {
            var options = new Plot3DOptions();
            var spec = LineSpec.Parse("r--o");
            options.ApplyLineSpec(spec);

            Assert.Equal(LineStyle.Dashed, options.LineStyle);
            Assert.Equal(MarkerStyle.Circle, options.Marker);
            Assert.Equal(Color3D.PureRed, options.Color);

            var cloned = options.Clone();
            Assert.Equal(options.LineStyle, cloned.LineStyle);
            Assert.Equal(options.Marker, cloned.Marker);
            Assert.Equal(options.Color, cloned.Color);
        }
    }
}
