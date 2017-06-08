// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5LiteralExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Expressions
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an expression containing a literal value.
    /// </summary>
    public class Es5LiteralExpression : Es5CodeModel, IEs5Expression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly Es5LiteralExpression Null =
            new Es5LiteralExpression(Es5LiteralKind.Null, "null");

        internal static readonly Es5LiteralExpression True =
            new Es5LiteralExpression(Es5LiteralKind.True, "true");

        internal static readonly Es5LiteralExpression False =
            new Es5LiteralExpression(Es5LiteralKind.False, "false");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5LiteralExpression(Es5LiteralKind kind, string literal)
        {
            Kind = kind;
            Literal = literal;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5LiteralKind Kind { get; }

        public string Literal { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public override string ToCodeDisplay()
        {
            switch (Kind)
            {
                case Es5LiteralKind.Null:
                    return "null";

                case Es5LiteralKind.True:
                    return "true";

                case Es5LiteralKind.False:
                    return "false";

                case Es5LiteralKind.Decimal:
                case Es5LiteralKind.HexInteger:
                case Es5LiteralKind.String:
                case Es5LiteralKind.RegExp:
                    return Literal;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Kind));
            }
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write(ToCodeDisplay());
        }

        internal static Es5LiteralExpression CreateString(string literal) =>
            new Es5LiteralExpression(Es5LiteralKind.String, literal);

        internal static Es5LiteralExpression CreateDecimal(string literal) =>
            new Es5LiteralExpression(Es5LiteralKind.Decimal, literal);

        internal static Es5LiteralExpression CreateHexInteger(string literal) =>
            new Es5LiteralExpression(Es5LiteralKind.HexInteger, literal);

        internal static Es5LiteralExpression CreateRegExp(string literal) =>
            new Es5LiteralExpression(Es5LiteralKind.RegExp, literal);
    }
}
