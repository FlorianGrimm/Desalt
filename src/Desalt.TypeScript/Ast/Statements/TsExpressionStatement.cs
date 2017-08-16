﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExpressionStatement.cs" company="Justin Rockwood">
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
    /// Represents an expression in statement form.
    /// </summary>
    internal class TsExpressionStatement : AstNode<TsVisitor>, ITsExpressionStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsExpressionStatement(ITsExpression expression)
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

        public override void Accept(TsVisitor visitor) => visitor.VisitExpressionStatement(this);

        public override string CodeDisplay => $"{Expression};";

        public override void Emit(Emitter emitter)
        {
            Expression.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}