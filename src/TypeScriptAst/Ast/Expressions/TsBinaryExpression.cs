// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBinaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a binary expression.
    /// </summary>
    internal class TsBinaryExpression : AstNode, ITsBinaryExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal TsBinaryExpression(
            ITsExpression leftSide,
            TsBinaryOperator @operator,
            ITsExpression rightSide)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            Operator = @operator;
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public TsBinaryOperator Operator { get; }
        public ITsExpression RightSide { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitBinaryExpression(this);

        public override string CodeDisplay => $"{LeftSide} {Operator.ToCodeDisplay()} {RightSide}";

        protected override void EmitInternal(Emitter emitter)
        {
            LeftSide.Emit(emitter);
            emitter.Write($" {Operator.ToCodeDisplay()} ");
            RightSide.Emit(emitter);
        }
    }
}
