// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsWithStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'with' statement of the form, 'with (expression) statement'.
    /// </summary>
    internal class TsWithStatement : AstNode<TsVisitor>, ITsWithStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsWithStatement(ITsExpression expression, ITsStatement statement)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }
        public ITsStatement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitWithStatement(this);

        public override string CodeDisplay => $"with ({Expression}) {Statement}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("with (");
            Expression.Emit(emitter);
            Statement.EmitIndentedOrInBlock(emitter);
        }
    }
}
