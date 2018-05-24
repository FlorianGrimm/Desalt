// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNumericLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using System.Globalization;
    using CompilerUtilities.Extensions;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an expression containing a numeric literal value.
    /// </summary>
    internal class TsNumericLiteral : AstNode, ITsNumericLiteral
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// The maximum safe integer in JavaScript (2 ^ 53 - 1).
        /// </summary>
        public static readonly long MaxSafeInteger = (long)(Math.Pow(2, 53) - 1);

        /// <summary>
        /// Represents the number zero.
        /// </summary>
        public static readonly TsNumericLiteral Zero = new TsNumericLiteral(TsNumericLiteralKind.Decimal, 0);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsNumericLiteral(TsNumericLiteralKind kind, double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                throw new ArgumentException("Value cannot be infinity or NaN", nameof(value));
            }

            if (value < 0)
            {
                throw new ArgumentException("Value must be positive", nameof(value));
            }

            if (kind.IsOneOf(
                TsNumericLiteralKind.BinaryInteger, TsNumericLiteralKind.OctalInteger, TsNumericLiteralKind.HexInteger) &&
                value > MaxSafeInteger)
            {
                throw new ArgumentException($"Integers must be less than {MaxSafeInteger}", nameof(value));
            }

            Kind = kind;
            Value = value;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsNumericLiteralKind Kind { get; }

        public double Value { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitNumericLiteral(this);

        public override string CodeDisplay
        {
            get
            {
                switch (Kind)
                {
                    case TsNumericLiteralKind.Decimal:
                        return Value.ToString(CultureInfo.InvariantCulture);

                    case TsNumericLiteralKind.BinaryInteger:
                        return "0b" + Convert.ToString((long)Value, 2);

                    case TsNumericLiteralKind.OctalInteger:
                        return "0o" + Convert.ToString((long)Value, 8);

                    case TsNumericLiteralKind.HexInteger:
                        return "0x" + Convert.ToString((long)Value, 16);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Kind));
                }
            }
        }

        protected override void EmitInternal(Emitter emitter) => emitter.Write(CodeDisplay);
    }
}
