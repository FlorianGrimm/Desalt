// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Visitor.IterationStatements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.JavaScript.Ast.Statements;

    public abstract partial class Es5Visitor
    {
        /// <summary>
        /// Visits a 'do-while' iteration statement.
        /// </summary>
        public virtual void VisitDoStatement(Es5DoStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'while' iteration statement.
        /// </summary>
        public virtual void VisitWhileStatement(Es5WhileStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'for' iteration statement.
        /// </summary>
        public virtual void VisitForStatement(Es5ForStatement node) => DefaultVisit(node);

        /// <summary>
        /// Visits a 'for-in' iteration statement.
        /// </summary>
        public virtual void VisitForInStatement(Es5ForInStatement node) => DefaultVisit(node);
    }
}
