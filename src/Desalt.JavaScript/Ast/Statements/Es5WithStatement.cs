// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5WithStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'with (Expression) Statement' statement.
    /// </summary>
    public sealed class Es5WithStatement : AstNode<Es5Visitor>, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5WithStatement(IEs5Expression expression, IEs5Statement statement)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Expression { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor) => visitor.VisitWithStatement(this);

        public override string CodeDisplay => $"with ({Expression}) {Statement}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("with (");
            Expression.Emit(emitter);
            emitter.WriteStatementIndentedOrInBlock(Statement, Statement is Es5BlockStatement, ")", ") ");
        }
    }
}
