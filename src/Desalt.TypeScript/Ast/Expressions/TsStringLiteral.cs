// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsStringLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

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

        public void Accept(TsVisitor visitor) => visitor.VisitStringLiteral(this);

        public override string CodeDisplay => $"{QuoteChar}{Value.Replace(QuoteChar, "\\" + QuoteChar)}{QuoteChar}";

        public override void Emit(Emitter emitter) => emitter.Write(CodeDisplay);
    }
}
