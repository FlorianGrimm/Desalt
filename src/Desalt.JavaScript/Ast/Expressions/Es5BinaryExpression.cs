// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5BinaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a binary expression of the form 'x ? y' where ? represents any of the enum values
    /// from <see cref="Es5BinaryOperator"/>.
    /// </summary>
    public sealed class Es5BinaryExpression : Es5AstNode, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5BinaryExpression(
            IEs5Expression leftSide,
            Es5BinaryOperator @operator,
            IEs5Expression rightSide)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            Operator = @operator;
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression LeftSide { get; }
        public Es5BinaryOperator Operator { get; }
        public IEs5Expression RightSide { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitBinaryExpression(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        public override string ToCodeDisplay() => $"{LeftSide} {Operator.ToCodeDisplay()} {RightSide}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            LeftSide.WriteFullCodeDisplay(writer);
            writer.Write($" {Operator.ToCodeDisplay()} ");
            RightSide.WriteFullCodeDisplay(writer);
        }
    }
}
