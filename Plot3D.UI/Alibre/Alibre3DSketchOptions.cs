namespace Plot3D.UI.Alibre
{
    /// <summary>
    /// Supported 3D sketch geometry primitives for Alibre Design.
    /// </summary>
    public enum AlibreGeometryType
    {
        BSplineInterpolation = 0,
        Polyline = 1,
        Points = 2
    }

    /// <summary>
    /// Scaling mode for converting Plot3D formula coordinates into Alibre sketch coordinates.
    /// </summary>
    public enum AlibreUnitScale
    {
        PartDisplayUnits = 0,
        Millimeters = 1,
        Centimeters = 2,
        Inches = 3,
        Meters = 4,
        Custom = 5
    }

    /// <summary>
    /// Target mode for 3D sketch insertion.
    /// </summary>
    public enum AlibreSketchTargetMode
    {
        New3DSketch = 0,
        ActiveOrNew3DSketch = 1
    }

    /// <summary>
    /// Options for creating a 3D sketch in Alibre Design.
    /// </summary>
    public sealed class Alibre3DSketchOptions
    {
        public AlibreGeometryType GeometryType { get; set; } = AlibreGeometryType.BSplineInterpolation;
        public AlibreUnitScale UnitScale { get; set; } = AlibreUnitScale.PartDisplayUnits;
        public double CustomScaleFactor { get; set; } = 1.0;
        public string? SketchName { get; set; }
        public AlibreSketchTargetMode TargetMode { get; set; } = AlibreSketchTargetMode.New3DSketch;

        public static Alibre3DSketchOptions Default => new Alibre3DSketchOptions();
    }

    /// <summary>
    /// Outcome of a 3D sketch creation operation.
    /// </summary>
    public sealed class AlibreSketchResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string? SketchName { get; }
        public int PointCount { get; }
        public string? PartName { get; }

        private AlibreSketchResult(bool success, string message, string? sketchName, int pointCount, string? partName)
        {
            Success = success;
            Message = message;
            SketchName = sketchName;
            PointCount = pointCount;
            PartName = partName;
        }

        public static AlibreSketchResult Succeeded(string sketchName, int pointCount, string partName, string message) =>
            new AlibreSketchResult(true, message, sketchName, pointCount, partName);

        public static AlibreSketchResult Failed(string message) =>
            new AlibreSketchResult(false, message, null, 0, null);
    }
}
