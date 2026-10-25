using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Plot3D.Expressions
{
    /// <summary>
    /// High-performance mathematical formula parser and compiler for parametric equations x(t), y(t), z(t).
    /// Supports arithmetic operators, implicit multiplication, standard constants (pi, e, tau, phi),
    /// and comprehensive elementary and special trigonometric, hyperbolic, exponential, and rounding functions.
    /// </summary>
    public static class FormulaParser
    {
        private static readonly ParameterExpression TParam = Expression.Parameter(typeof(double), "t");

        private static readonly Dictionary<string, double> Constants = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["pi"] = Math.PI,
            ["e"] = Math.E,
            ["tau"] = 2.0 * Math.PI,
            ["phi"] = (1.0 + Math.Sqrt(5.0)) * 0.5
        };

        private static readonly Dictionary<string, MethodInfo> SingleArgFunctions = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["sin"] = typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!,
            ["cos"] = typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!,
            ["tan"] = typeof(Math).GetMethod(nameof(Math.Tan), new[] { typeof(double) })!,
            ["asin"] = typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!,
            ["arcsin"] = typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!,
            ["acos"] = typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!,
            ["arccos"] = typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!,
            ["atan"] = typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!,
            ["arctan"] = typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!,
            ["sinh"] = typeof(Math).GetMethod(nameof(Math.Sinh), new[] { typeof(double) })!,
            ["cosh"] = typeof(Math).GetMethod(nameof(Math.Cosh), new[] { typeof(double) })!,
            ["tanh"] = typeof(Math).GetMethod(nameof(Math.Tanh), new[] { typeof(double) })!,
            ["exp"] = typeof(Math).GetMethod(nameof(Math.Exp), new[] { typeof(double) })!,
            ["log"] = typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!,
            ["ln"] = typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!,
            ["log10"] = typeof(Math).GetMethod(nameof(Math.Log10), new[] { typeof(double) })!,
            ["sqrt"] = typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!,
            ["abs"] = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!,
            ["sign"] = typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(double) })!,
            ["floor"] = typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(double) })!,
            ["ceil"] = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!,
            ["ceiling"] = typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!,
            ["round"] = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!,
            ["cbrt"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Cbrt), new[] { typeof(double) })!,
            ["log2"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Log2), new[] { typeof(double) })!,
            ["sec"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Sec), new[] { typeof(double) })!,
            ["csc"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Csc), new[] { typeof(double) })!,
            ["cot"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Cot), new[] { typeof(double) })!,
            ["asinh"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Asinh), new[] { typeof(double) })!,
            ["acosh"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Acosh), new[] { typeof(double) })!,
            ["atanh"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Atanh), new[] { typeof(double) })!,
            ["sinc"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Sinc), new[] { typeof(double) })!,
            ["heaviside"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Heaviside), new[] { typeof(double) })!,
            ["step"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Heaviside), new[] { typeof(double) })!,
            ["rad2deg"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Rad2Deg), new[] { typeof(double) })!,
            ["deg2rad"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Deg2Rad), new[] { typeof(double) })!
        };

        private static readonly Dictionary<string, MethodInfo> TwoArgFunctions = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["atan2"] = typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!,
            ["arctan2"] = typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!,
            ["pow"] = typeof(Math).GetMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!,
            ["min"] = typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!,
            ["max"] = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!,
            ["hypot"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Hypot), new[] { typeof(double), typeof(double) })!
        };

        private static readonly Dictionary<string, MethodInfo> ThreeArgFunctions = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["clamp"] = typeof(MathFunctionHelper).GetMethod(nameof(MathFunctionHelper.Clamp), new[] { typeof(double), typeof(double), typeof(double) })!
        };

        private static readonly MethodInfo MathPowMethod = typeof(Math).GetMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!;

        /// <summary>
        /// Parses a mathematical formula into a compiled high-speed delegate f(t) -> double.
        /// </summary>
        /// <param name="formula">Formula string, e.g. "sin(t) + 2*cos(2*t)", "exp(-0.1*t)*cos(t)", "(2+cos(3t))cos(2t)".</param>
        /// <returns>Compiled function delegate.</returns>
        public static Func<double, double> Parse(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new ArgumentException("Formula expression cannot be null or empty.", nameof(formula));

            var tokens = Tokenize(formula);
            var parser = new ParserState(tokens, formula);
            var expr = parser.ParseExpression();
            if (expr.Type != typeof(double))
            {
                expr = Expression.Convert(expr, typeof(double));
            }

            if (!parser.IsAtEnd)
            {
                var cur = parser.CurrentToken;
                throw new FormulaParseException($"Unexpected token '{cur.Text}' after valid expression", formula, cur.Position);
            }

            var lambda = Expression.Lambda<Func<double, double>>(expr, TParam);
            return lambda.Compile();
        }

        /// <summary>
        /// Attempts to parse a formula, returning true if successful or false with an error message if invalid.
        /// </summary>
        public static bool TryParse(string formula, out Func<double, double>? func, out string? errorMessage)
        {
            func = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(formula))
            {
                errorMessage = "Formula expression cannot be empty.";
                return false;
            }

            try
            {
                func = Parse(formula);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Evaluates a constant mathematical expression (e.g. "2*pi", "6*pi", "-2*pi", "10", "sqrt(2)").
        /// </summary>
        public static double EvaluateConstant(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

            var func = Parse(expression);
            return func(0.0);
        }

        /// <summary>
        /// Parses an interval from text (e.g. "[0, 2*pi]", "0, 2*pi", "0:6*pi", "[-5, 5]").
        /// </summary>
        public static Core.Interval ParseInterval(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Core.Interval.Default;

            string cleaned = text.Trim().Trim('[', ']', '(', ')');
            char[] separators = new[] { ',', ';', ':', ' ' };
            string[] parts = cleaned.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                double min = EvaluateConstant(parts[0]);
                double max = EvaluateConstant(parts[1]);
                return new Core.Interval(min, max);
            }

            if (parts.Length == 1)
            {
                double max = EvaluateConstant(parts[0]);
                return new Core.Interval(0.0, max);
            }

            throw new FormatException($"Invalid interval format: '{text}'. Expected format: [min, max] or min, max.");
        }

        #region Tokenizer

        private enum TokenType
        {
            Number,
            Identifier,
            Plus,
            Minus,
            Multiply,
            Divide,
            Modulo,
            Power,
            LeftParen,
            RightParen,
            Comma,
            End
        }

        private readonly struct Token
        {
            public TokenType Type { get; }
            public string Text { get; }
            public double NumberValue { get; }
            public int Position { get; }

            public Token(TokenType type, string text, int position, double numVal = 0.0)
            {
                Type = type;
                Text = text;
                Position = position;
                NumberValue = numVal;
            }

            public override string ToString() => $"{Type}: '{Text}' at {Position}";
        }

        private static List<Token> Tokenize(string formula)
        {
            var rawTokens = new List<Token>();
            int i = 0;
            int len = formula.Length;

            while (i < len)
            {
                char c = formula[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                int startPos = i;

                // Numbers (including .5, 123, 123.456, 1e-4)
                if (char.IsDigit(c) || (c == '.' && i + 1 < len && char.IsDigit(formula[i + 1])))
                {
                    int numStart = i;
                    bool hasDot = false;
                    while (i < len && (char.IsDigit(formula[i]) || (formula[i] == '.' && !hasDot)))
                    {
                        if (formula[i] == '.') hasDot = true;
                        i++;
                    }

                    // Optional scientific notation (e.g. 1e-5, 2E+3)
                    if (i < len && (formula[i] == 'e' || formula[i] == 'E'))
                    {
                        if (i + 1 < len && (char.IsDigit(formula[i + 1]) ||
                            ((formula[i + 1] == '+' || formula[i + 1] == '-') && i + 2 < len && char.IsDigit(formula[i + 2]))))
                        {
                            i++; // consume 'e'/'E'
                            if (formula[i] == '+' || formula[i] == '-') i++;
                            while (i < len && char.IsDigit(formula[i])) i++;
                        }
                    }

                    string numStr = formula.Substring(numStart, i - numStart);
                    if (!double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                    {
                        throw new FormulaParseException($"Invalid number format '{numStr}'", formula, startPos);
                    }

                    rawTokens.Add(new Token(TokenType.Number, numStr, startPos, val));
                    continue;
                }

                // Identifiers (variable t, constants pi, e, function names)
                if (char.IsLetter(c) || c == '_')
                {
                    int idStart = i;
                    while (i < len && (char.IsLetterOrDigit(formula[i]) || formula[i] == '_'))
                    {
                        i++;
                    }

                    string id = formula.Substring(idStart, i - idStart);
                    rawTokens.Add(new Token(TokenType.Identifier, id, startPos));
                    continue;
                }

                // Operators and punctuation
                switch (c)
                {
                    case '+':
                        rawTokens.Add(new Token(TokenType.Plus, "+", startPos));
                        i++;
                        break;
                    case '-':
                        rawTokens.Add(new Token(TokenType.Minus, "-", startPos));
                        i++;
                        break;
                    case '*':
                        if (i + 1 < len && formula[i + 1] == '*')
                        {
                            rawTokens.Add(new Token(TokenType.Power, "**", startPos));
                            i += 2;
                        }
                        else
                        {
                            rawTokens.Add(new Token(TokenType.Multiply, "*", startPos));
                            i++;
                        }
                        break;
                    case '/':
                        rawTokens.Add(new Token(TokenType.Divide, "/", startPos));
                        i++;
                        break;
                    case '%':
                        rawTokens.Add(new Token(TokenType.Modulo, "%", startPos));
                        i++;
                        break;
                    case '^':
                        rawTokens.Add(new Token(TokenType.Power, "^", startPos));
                        i++;
                        break;
                    case '(':
                        rawTokens.Add(new Token(TokenType.LeftParen, "(", startPos));
                        i++;
                        break;
                    case ')':
                        rawTokens.Add(new Token(TokenType.RightParen, ")", startPos));
                        i++;
                        break;
                    case ',':
                        rawTokens.Add(new Token(TokenType.Comma, ",", startPos));
                        i++;
                        break;
                    default:
                        throw new FormulaParseException($"Unexpected character '{c}'", formula, startPos);
                }
            }

            // Insert implicit multiplication tokens where appropriate:
            // e.g. 2t -> 2 * t, 2(t+1) -> 2 * (t+1), (t+1)(t-1) -> (t+1) * (t-1), t sin(t) -> t * sin(t)
            var tokensWithImplicitMult = new List<Token>(rawTokens.Count + 4);
            for (int k = 0; k < rawTokens.Count; k++)
            {
                tokensWithImplicitMult.Add(rawTokens[k]);

                if (k + 1 < rawTokens.Count)
                {
                    var curr = rawTokens[k];
                    var next = rawTokens[k + 1];

                    bool insertMultiply = false;

                    // Number followed by Identifier or '(' (e.g. 2t, 2sin(t), 2(t+1))
                    if (curr.Type == TokenType.Number && (next.Type == TokenType.Identifier || next.Type == TokenType.LeftParen))
                    {
                        insertMultiply = true;
                    }
                    // ')' followed by Number, Identifier, or '(' (e.g. (t+1)2, (t+1)t, (t+1)(t-1))
                    else if (curr.Type == TokenType.RightParen && (next.Type == TokenType.Number || next.Type == TokenType.Identifier || next.Type == TokenType.LeftParen))
                    {
                        insertMultiply = true;
                    }
                    // Variable 't' or constant followed by '(' or Number or Identifier (not when curr is a function name)
                    else if (curr.Type == TokenType.Identifier && IsVariableOrConstant(curr.Text))
                    {
                        if (next.Type == TokenType.Number || next.Type == TokenType.LeftParen || next.Type == TokenType.Identifier)
                        {
                            insertMultiply = true;
                        }
                    }

                    if (insertMultiply)
                    {
                        tokensWithImplicitMult.Add(new Token(TokenType.Multiply, "*", next.Position));
                    }
                }
            }

            tokensWithImplicitMult.Add(new Token(TokenType.End, string.Empty, len));
            return tokensWithImplicitMult;
        }

        private static bool IsVariableOrConstant(string name)
        {
            if (string.Equals(name, "t", StringComparison.OrdinalIgnoreCase)) return true;
            return Constants.ContainsKey(name);
        }

        #endregion

        #region Recursive Descent Parser

        private sealed class ParserState
        {
            private readonly List<Token> _tokens;
            private readonly string _formula;
            private int _index;

            public ParserState(List<Token> tokens, string formula)
            {
                _tokens = tokens;
                _formula = formula;
                _index = 0;
            }

            public Token CurrentToken => _tokens[_index];
            public bool IsAtEnd => CurrentToken.Type == TokenType.End;

            private Token Advance()
            {
                var cur = CurrentToken;
                if (!IsAtEnd) _index++;
                return cur;
            }

            private bool Match(TokenType type)
            {
                if (CurrentToken.Type == type)
                {
                    Advance();
                    return true;
                }
                return false;
            }

            private Token Expect(TokenType type, string message)
            {
                if (CurrentToken.Type == type)
                {
                    return Advance();
                }
                throw new FormulaParseException(message, _formula, CurrentToken.Position);
            }

            public Expression ParseExpression()
            {
                return ParseAdditive();
            }

            // Level 1: Additive (+, -)
            private Expression ParseAdditive()
            {
                var left = ParseMultiplicative();

                while (!IsAtEnd && (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus))
                {
                    var op = Advance();
                    var right = ParseMultiplicative();

                    if (op.Type == TokenType.Plus)
                    {
                        left = Expression.Add(left, right);
                    }
                    else
                    {
                        left = Expression.Subtract(left, right);
                    }
                }

                return left;
            }

            // Level 2: Multiplicative (*, /, %)
            private Expression ParseMultiplicative()
            {
                var left = ParseUnary();

                while (!IsAtEnd && (CurrentToken.Type == TokenType.Multiply || CurrentToken.Type == TokenType.Divide || CurrentToken.Type == TokenType.Modulo))
                {
                    var op = Advance();
                    var right = ParseUnary();

                    if (op.Type == TokenType.Multiply)
                    {
                        left = Expression.Multiply(left, right);
                    }
                    else if (op.Type == TokenType.Divide)
                    {
                        left = Expression.Divide(left, right);
                    }
                    else
                    {
                        left = Expression.Modulo(left, right);
                    }
                }

                return left;
            }

            // Level 3: Unary (+, -)
            private Expression ParseUnary()
            {
                if (CurrentToken.Type == TokenType.Minus)
                {
                    Advance();
                    var operand = ParsePower();
                    return Expression.Negate(operand);
                }

                if (CurrentToken.Type == TokenType.Plus)
                {
                    Advance();
                    return ParsePower();
                }

                return ParsePower();
            }

            // Level 4: Exponentiation (^, **), right-associative
            private Expression ParsePower()
            {
                var left = ParsePrimary();

                if (CurrentToken.Type == TokenType.Power)
                {
                    Advance();
                    // Right-associative: recursive call to ParseUnary or ParsePower
                    var right = ParseUnary();
                    return Expression.Call(MathPowMethod, left, right);
                }

                return left;
            }

            // Level 5: Primary (Numbers, Variables, Constants, Function calls, Parentheses)
            private Expression ParsePrimary()
            {
                var token = CurrentToken;

                // Number
                if (token.Type == TokenType.Number)
                {
                    Advance();
                    return Expression.Constant(token.NumberValue, typeof(double));
                }

                // Identifier: Variable 't', Constant ('pi', 'e', ...), or Function ('sin', 'cos', ...)
                if (token.Type == TokenType.Identifier)
                {
                    Advance();
                    string name = token.Text;

                    // Variable 't'
                    if (string.Equals(name, "t", StringComparison.OrdinalIgnoreCase))
                    {
                        return TParam;
                    }

                    // Known Constants
                    if (Constants.TryGetValue(name, out double constVal))
                    {
                        return Expression.Constant(constVal, typeof(double));
                    }

                    // Function call with parentheses: name(...)
                    if (CurrentToken.Type == TokenType.LeftParen)
                    {
                        Advance(); // Consume '('
                        var args = new List<Expression>();

                        if (CurrentToken.Type != TokenType.RightParen)
                        {
                            while (true)
                            {
                                args.Add(ParseExpression());
                                if (CurrentToken.Type == TokenType.Comma)
                                {
                                    Advance();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        Expect(TokenType.RightParen, $"Expected closing ')' after arguments for function '{name}'");

                        if (args.Count == 1 && SingleArgFunctions.TryGetValue(name, out var singleMethod))
                        {
                            var call = Expression.Call(singleMethod, args[0]);
                            return call.Type == typeof(double) ? (Expression)call : Expression.Convert(call, typeof(double));
                        }

                        if (args.Count == 2 && TwoArgFunctions.TryGetValue(name, out var twoMethod))
                        {
                            var call = Expression.Call(twoMethod, args[0], args[1]);
                            return call.Type == typeof(double) ? (Expression)call : Expression.Convert(call, typeof(double));
                        }

                        if (args.Count == 3 && ThreeArgFunctions.TryGetValue(name, out var threeMethod))
                        {
                            var call = Expression.Call(threeMethod, args[0], args[1], args[2]);
                            return call.Type == typeof(double) ? (Expression)call : Expression.Convert(call, typeof(double));
                        }

                        // Check if function name exists with different arg count
                        if (SingleArgFunctions.ContainsKey(name))
                        {
                            throw new FormulaParseException($"Function '{name}' expects 1 argument, but {args.Count} were provided", _formula, token.Position);
                        }
                        if (TwoArgFunctions.ContainsKey(name))
                        {
                            throw new FormulaParseException($"Function '{name}' expects 2 arguments, but {args.Count} were provided", _formula, token.Position);
                        }
                        if (ThreeArgFunctions.ContainsKey(name))
                        {
                            throw new FormulaParseException($"Function '{name}' expects 3 arguments, but {args.Count} were provided", _formula, token.Position);
                        }

                        throw new FormulaParseException($"Unknown function '{name}'", _formula, token.Position);
                    }

                    throw new FormulaParseException($"Unknown variable or identifier '{name}'. Did you mean 't' or a known function like sin(t)?", _formula, token.Position);
                }

                // Parentheses '(' expr ')'
                if (token.Type == TokenType.LeftParen)
                {
                    Advance();
                    var inner = ParseExpression();
                    Expect(TokenType.RightParen, "Expected matching closing ')'");
                    return inner;
                }

                throw new FormulaParseException($"Expected number, variable, constant, function, or '(', but found '{token.Text}'", _formula, token.Position);
            }
        }

        #endregion
    }
}
