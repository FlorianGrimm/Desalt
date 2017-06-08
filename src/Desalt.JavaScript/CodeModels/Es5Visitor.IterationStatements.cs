// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Visitor.IterationStatements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels
{
    using Desalt.JavaScript.CodeModels.Statements;

    public abstract partial class Es5Visitor
    {
        /// <summary>
        /// Visits a 'do-while' iteration statement.
        /// </summary>
        public virtual void VisitDoStatement(Es5DoStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'while' iteration statement.
        /// </summary>
        public virtual void VisitWhileStatement(Es5WhileStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'for' iteration statement.
        /// </summary>
        public virtual void VisitForStatement(Es5ForStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'for-in' iteration statement.
        /// </summary>
        public virtual void VisitForInStatement(Es5ForInStatement model) => DefaultVisit(model);
    }

    public abstract partial class Es5Visitor<TResult>
    {
        /// <summary>
        /// Visits a 'do-while' iteration statement.
        /// </summary>
        public virtual TResult VisitDoStatement(Es5DoStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'while' iteration statement.
        /// </summary>
        public virtual TResult VisitWhileStatement(Es5WhileStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'for' iteration statement.
        /// </summary>
        public virtual TResult VisitForStatement(Es5ForStatement model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'for-in' iteration statement.
        /// </summary>
        public virtual TResult VisitForInStatement(Es5ForInStatement model) => DefaultVisit(model);
    }
}
