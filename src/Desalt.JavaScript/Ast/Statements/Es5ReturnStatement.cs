// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ReturnStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a JavaScript 'return' statement.
    /// </summary>
    public sealed class Es5ReturnStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ReturnStatement(IEs5Expression expression)
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
            visitor.VisitReturnStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }

        public override string ToCodeDisplay() => $"return {Expression};";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("return ");
            Expression.WriteFullCodeDisplay(writer);
            writer.Write(";");
        }
    }
}
