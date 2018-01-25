// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThrowStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'throw' statement.
    /// </summary>
    internal class TsThrowStatement : AstNode<TsVisitor>, ITsThrowStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsThrowStatement(ITsExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitThrowStatement(this);

        public override string CodeDisplay => $"throw {Expression};";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("throw ");
            Expression.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
