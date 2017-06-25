// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBinaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

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

        public void Accept(TsVisitor visitor) => visitor.VisitBinaryExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitBinaryExpression(this);

        public override string ToCodeDisplay() => $"{LeftSide} {Operator.ToCodeDisplay()} {RightSide}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            LeftSide.WriteFullCodeDisplay(writer);
            writer.Write($" {Operator.ToCodeDisplay()} ");
            RightSide.WriteFullCodeDisplay(writer);
        }
    }
}
