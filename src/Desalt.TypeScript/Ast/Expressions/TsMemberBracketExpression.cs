// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberBracketExpression.cs" company="Justin Rockwood">
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
    /// Represents a member expression of the form 'expression[expression]'.
    /// </summary>
    internal class TsMemberBracketExpression : AstNode, ITsMemberBracketExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMemberBracketExpression(ITsExpression leftSide, ITsExpression bracketContents)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            BracketContents = bracketContents ?? throw new ArgumentNullException(nameof(bracketContents));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public ITsExpression BracketContents { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitMemberBracketExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitMemberBracketExpression(this);

        public override string CodeDisplay => $"{LeftSide}[{BracketContents}]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            LeftSide.WriteFullCodeDisplay(writer);
            writer.Write("[");
            BracketContents.WriteFullCodeDisplay(writer);
            writer.Write("]");
        }
    }
}
