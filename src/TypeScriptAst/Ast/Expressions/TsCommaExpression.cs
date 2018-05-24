// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCommaExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an expression list separated by commas. Useful in for loops for the initializer or
    /// incrementor, for example.
    /// </summary>
    internal class TsCommaExpression : AstNode, ITsCommaExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCommaExpression(IEnumerable<ITsExpression> expressions)
        {
            Expressions = expressions?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(expressions));
            if (Expressions.IsEmpty)
            {
                throw new ArgumentException("Empty expression list", nameof(expressions));
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsExpression> Expressions { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitCommaExpression(this);

        public override string CodeDisplay => Expressions.ToElidedList();

        protected override void EmitInternal(Emitter emitter) =>
            emitter.WriteList(Expressions, indent: false, itemDelimiter: ", ");
    }
}
