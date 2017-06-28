﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5WithStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'with (Expression) Statement' statement.
    /// </summary>
    public sealed class Es5WithStatement : Es5AstNode, IEs5Statement
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

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitWithStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitWithStatement(this);
        }

        public override string CodeDisplay => $"with ({Expression}) {Statement}";

        public override void Emit(IndentedTextWriter writer)
        {
            writer.Write("with (");
            Expression.Emit(writer);
            writer.WriteLine(")");
            Statement.Emit(writer);
        }
    }
}
