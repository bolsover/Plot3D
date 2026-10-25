namespace Plot3D.Core
{
    /// <summary>
    /// Line style specifiers.
    /// </summary>
    public enum LineStyle
    {
        /// <summary>Solid line ('-')</summary>
        Solid,
        /// <summary>Dashed line ('--')</summary>
        Dashed,
        /// <summary>Dotted line (':')</summary>
        Dotted,
        /// <summary>Dash-dot line ('-.')</summary>
        DashDot,
        /// <summary>No line ('none')</summary>
        None
    }

    /// <summary>
    /// Marker style specifiers.
    /// </summary>
    public enum MarkerStyle
    {
        /// <summary>No marker ('none')</summary>
        None,
        /// <summary>Circle marker ('o')</summary>
        Circle,
        /// <summary>Plus sign ('+')</summary>
        Plus,
        /// <summary>Asterisk ('*')</summary>
        Asterisk,
        /// <summary>Point / Dot ('.')</summary>
        Point,
        /// <summary>Cross ('x')</summary>
        Cross,
        /// <summary>Square ('s')</summary>
        Square,
        /// <summary>Diamond ('d')</summary>
        Diamond,
        /// <summary>Upward-pointing triangle ('^')</summary>
        TriangleUp,
        /// <summary>Downward-pointing triangle ('v')</summary>
        TriangleDown,
        /// <summary>Right-pointing triangle ('>')</summary>
        TriangleRight,
        /// <summary>Left-pointing triangle ('&lt;')</summary>
        TriangleLeft,
        /// <summary>Five-pointed star / Pentagram ('p')</summary>
        Pentagram,
        /// <summary>Six-pointed star / Hexagram ('h')</summary>
        Hexagram
    }
}
