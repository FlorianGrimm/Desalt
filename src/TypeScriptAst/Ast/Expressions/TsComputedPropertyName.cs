// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsComputedPropertyName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a property name inside of an object of the form '[ expression ]'.
    /// </summary>
    internal class TsComputedPropertyName : TsAstNode, ITsComputedPropertyName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsComputedPropertyName(ITsExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitComputedPropertyName(this);

        public override string CodeDisplay => $"[{Expression.CodeDisplay}]";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("[");
            Expression.Emit(emitter);
            emitter.Write("]");
        }
    }
}
