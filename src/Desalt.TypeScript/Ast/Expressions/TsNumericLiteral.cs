// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNumericLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using System.Globalization;
    using Desalt.Core.Ast;
    using Desalt.Core.Extensions;
    using Desalt.Core.Utility;

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
        public const long MaxSafeInteger = (2 ^ 53) - 1;

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

        public void Accept(TsVisitor visitor) => visitor.VisitNumericLiteral(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitNumericLiteral(this);

        public override string ToCodeDisplay()
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

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
