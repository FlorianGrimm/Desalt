// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an element in an array.
    /// </summary>
    internal class TsArrayElement : TsAstNode, ITsArrayElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayElement(ITsExpression expression, bool isSpreadElement = false)
        {
            Expression = expression;
            IsSpreadElement = isSpreadElement;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }

        /// <summary>
        /// Indicates whether the <see cref="ITsArrayElement.Expression"/> is preceded by a spread operator '...'.
        /// </summary>
        public bool IsSpreadElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitArrayElement(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            if (IsSpreadElement)
            {
                emitter.Write("...");
            }

            Expression.Emit(emitter);
        }
    }
}
