// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptLiteralExpression.cs" company="Justin Rockwood">
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
    internal class TypeScriptLiteralExpression : CodeModel, ITypeScriptLiteralExpression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly TypeScriptLiteralExpression Null =
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.Null, "null");

        internal static readonly TypeScriptLiteralExpression True =
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.True, "true");

        internal static readonly TypeScriptLiteralExpression False =
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.False, "false");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TypeScriptLiteralExpression(TypeScriptLiteralKind kind, string literal)
        {
            Kind = kind;
            Literal = literal;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TypeScriptLiteralKind Kind { get; }

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
                case TypeScriptLiteralKind.Null:
                    return "null";

                case TypeScriptLiteralKind.True:
                    return "true";

                case TypeScriptLiteralKind.False:
                    return "false";

                case TypeScriptLiteralKind.Decimal:
                case TypeScriptLiteralKind.BinaryInteger:
                case TypeScriptLiteralKind.OctalInteger:
                case TypeScriptLiteralKind.HexInteger:
                case TypeScriptLiteralKind.String:
                case TypeScriptLiteralKind.RegExp:
                    return Literal;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Kind));
            }
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());

        internal static TypeScriptLiteralExpression CreateString(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.String, literal);

        internal static TypeScriptLiteralExpression CreateDecimal(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.Decimal, literal);

        internal static TypeScriptLiteralExpression CreateBinaryInteger(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.BinaryInteger, literal);

        internal static TypeScriptLiteralExpression CreateOctalInteger(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.OctalInteger, literal);

        internal static TypeScriptLiteralExpression CreateHexInteger(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.HexInteger, literal);

        internal static TypeScriptLiteralExpression CreateRegExp(string literal) =>
            new TypeScriptLiteralExpression(TypeScriptLiteralKind.RegExp, literal);
    }
}
