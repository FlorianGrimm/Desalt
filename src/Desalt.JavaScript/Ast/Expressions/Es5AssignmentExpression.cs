// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5AssignmentExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an expression that assigns one value to another.
    /// </summary>
    public class Es5AssignmentExpression : AstNode<Es5Visitor>, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5AssignmentExpression(
            IEs5Expression leftSide,
            Es5AssignmentOperator @operator,
            IEs5Expression rightSide)
        {
            LeftSide = leftSide;
            Operator = @operator;
            RightSide = rightSide;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression LeftSide { get; }
        public IEs5Expression RightSide { get; }
        public Es5AssignmentOperator Operator { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitAssignmentExpression(this);
        }

        public override string CodeDisplay => $"{LeftSide} {Operator.ToCodeDisplay()} {RightSide}";

        public override void Emit(Emitter emitter)
        {
            LeftSide.Emit(emitter);
            emitter.Write($" {Operator.ToCodeDisplay()} ");
            RightSide.Emit(emitter);
        }
    }
}
