using AlibreX;

namespace Plot3D.UI.Alibre
{
    /// <summary>
    /// Length unit conversions for Alibre Design.
    /// Internal Alibre geometry coordinates are in Centimeters (AD_CENTIMETERS).
    /// </summary>
    internal static class AlibreUnits
    {
        public static double ToMm(ADUnits units)
        {
            switch (units)
            {
                case ADUnits.AD_MILLIMETERS: return 1.0;
                case ADUnits.AD_CENTIMETERS: return 10.0;
                case ADUnits.AD_METERS: return 1000.0;
                case ADUnits.AD_INCHES: return 25.4;
                case ADUnits.AD_FEET: return 304.8;
                case ADUnits.AD_FEET_INCHES: return 25.4;
                default: return 10.0; // Model default is cm
            }
        }

        public static double Convert(double value, ADUnits from, ADUnits to)
        {
            return value * ToMm(from) / ToMm(to);
        }

        public static string Suffix(ADUnits units)
        {
            switch (units)
            {
                case ADUnits.AD_MILLIMETERS: return "mm";
                case ADUnits.AD_CENTIMETERS: return "cm";
                case ADUnits.AD_METERS: return "m";
                case ADUnits.AD_INCHES:
                case ADUnits.AD_FEET_INCHES: return "in";
                case ADUnits.AD_FEET: return "ft";
                default: return "cm";
            }
        }
    }
}
