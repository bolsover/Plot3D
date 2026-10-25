using System;

namespace Plot3D.Expressions
{
    /// <summary>
    /// Exception thrown when a mathematical formula fails to parse.
    /// </summary>
    public sealed class FormulaParseException : Exception
    {
        public int Position { get; }
        public string Expression { get; }

        public FormulaParseException(string message, string expression, int position = -1)
            : base(position >= 0 ? $"{message} (at character {position + 1} in '{expression}')" : $"{message} in '{expression}'")
        {
            Expression = expression ?? string.Empty;
            Position = position;
        }

        public FormulaParseException(string message, string expression, int position, Exception innerException)
            : base(position >= 0 ? $"{message} (at character {position + 1} in '{expression}')" : $"{message} in '{expression}'", innerException)
        {
            Expression = expression ?? string.Empty;
            Position = position;
        }
    }
}
