// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLiteralExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an expression containing a literal value.
    /// </summary>
    internal class TsLiteralExpression : CodeModel, ITsLiteralExpression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly TsLiteralExpression Null =
            new TsLiteralExpression(TsLiteralKind.Null, "null");

        internal static readonly TsLiteralExpression True =
            new TsLiteralExpression(TsLiteralKind.True, "true");

        internal static readonly TsLiteralExpression False =
            new TsLiteralExpression(TsLiteralKind.False, "false");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsLiteralExpression(TsLiteralKind kind, string literal)
        {
            Kind = kind;
            Literal = literal;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsLiteralKind Kind { get; }

        public string Literal { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor)
        {
            visitor.VisitLiteralExpression(this);
        }

        public T Accept<T>(TypeScriptVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public override string ToCodeDisplay()
        {
            switch (Kind)
            {
                case TsLiteralKind.Null:
                    return "null";

                case TsLiteralKind.True:
                    return "true";

                case TsLiteralKind.False:
                    return "false";

                case TsLiteralKind.Decimal:
                case TsLiteralKind.BinaryInteger:
                case TsLiteralKind.OctalInteger:
                case TsLiteralKind.HexInteger:
                case TsLiteralKind.String:
                case TsLiteralKind.RegExp:
                    return Literal;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Kind));
            }
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());

        internal static TsLiteralExpression CreateString(string literal) =>
            new TsLiteralExpression(TsLiteralKind.String, literal);

        internal static TsLiteralExpression CreateDecimal(string literal) =>
            new TsLiteralExpression(TsLiteralKind.Decimal, literal);

        internal static TsLiteralExpression CreateBinaryInteger(string literal) =>
            new TsLiteralExpression(TsLiteralKind.BinaryInteger, literal);

        internal static TsLiteralExpression CreateOctalInteger(string literal) =>
            new TsLiteralExpression(TsLiteralKind.OctalInteger, literal);

        internal static TsLiteralExpression CreateHexInteger(string literal) =>
            new TsLiteralExpression(TsLiteralKind.HexInteger, literal);

        internal static TsLiteralExpression CreateRegExp(string literal) =>
            new TsLiteralExpression(TsLiteralKind.RegExp, literal);
    }
}
