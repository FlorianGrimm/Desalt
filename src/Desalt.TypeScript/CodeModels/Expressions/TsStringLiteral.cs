// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsStringLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a string literal.
    /// </summary>
    internal class TsStringLiteral : AstNode, ITsStringLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsStringLiteral(string value, StringLiteralQuoteKind quoteKind)
        {
            Value = value;
            QuoteKind = quoteKind;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public StringLiteralQuoteKind QuoteKind { get; }
        public string Value { get; }

        private string QuoteChar => QuoteKind == StringLiteralQuoteKind.SingleQuote ? "'" : "\"";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitStringLiteral(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitStringLiteral(this);

        public override string ToCodeDisplay() => $"{QuoteChar}{Value.Replace(QuoteChar, "\\" + QuoteChar)}{QuoteChar}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
