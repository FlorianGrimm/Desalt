// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAssignmentExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an expression that assigns one value to another.
    /// </summary>
    internal class TsAssignmentExpression : TsAstNode, ITsAssignmentExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAssignmentExpression(
            ITsExpression leftSide,
            TsAssignmentOperator @operator,
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
        public ITsExpression RightSide { get; }
        public TsAssignmentOperator Operator { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitAssignmentExpression(this);

        public override string CodeDisplay => $"{LeftSide} {Operator.ToCodeDisplay()} {RightSide}";

        protected override void EmitInternal(Emitter emitter)
        {
            LeftSide.Emit(emitter);
            emitter.Write($" {Operator.ToCodeDisplay()} ");
            RightSide.Emit(emitter);
        }
    }
}
