// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ExpressionStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an expression that is wrapped as a statement.
    /// </summary>
    public class Es5ExpressionStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ExpressionStatement(IEs5Expression expression)
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

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }

        public override string ToCodeDisplay() => Expression + ";";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            Expression.WriteFullCodeDisplay(writer);
            writer.Write(";");
        }
    }
}
