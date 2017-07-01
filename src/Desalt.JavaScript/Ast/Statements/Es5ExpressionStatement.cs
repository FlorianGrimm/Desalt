// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ExpressionStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an expression that is wrapped as a statement.
    /// </summary>
    public class Es5ExpressionStatement : AstNode<Es5Visitor>, IEs5Statement
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

        public override void Accept(Es5Visitor visitor) => visitor.VisitExpressionStatement(this);

        public override string CodeDisplay => Expression + ";";

        public override void Emit(Emitter emitter)
        {
            Expression.Emit(emitter);
            emitter.Write(";");
        }
    }
}
