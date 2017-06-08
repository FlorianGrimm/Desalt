// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ParenthesizedExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Expressions
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an expression wrapped in parentheses.
    /// </summary>
    public sealed class Es5ParenthesizedExpression : Es5CodeModel, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ParenthesizedExpression(IEs5Expression expression)
        {
            Expression = expression;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitParenthesizedExpression(this);
        }

        public override string ToCodeDisplay() => $"({Expression})";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("(");
            Expression.WriteFullCodeDisplay(writer);
            writer.Write(")");
        }
    }
}
