// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ThrowStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'throw' statement.
    /// </summary>
    public sealed class Es5ThrowStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ThrowStatement(IEs5Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
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
            visitor.VisitThrowStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitThrowStatement(this);
        }

        public override string CodeDisplay
        {
            get { return $"throw {Expression};"; }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("throw ");
            Expression.Emit(emitter);
            emitter.Write(";");
        }
    }
}
