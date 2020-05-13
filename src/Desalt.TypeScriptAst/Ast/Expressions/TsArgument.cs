// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArgument.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an argument in a call expression.
    /// </summary>
    internal class TsArgument : TsAstNode, ITsArgument
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArgument(ITsExpression expression, bool isSpreadArgument = false)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            IsSpreadArgument = isSpreadArgument;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Expression { get; }

        /// <summary>
        /// Indicates whether the <see cref="Expression"/> is preceded by a spread operator '...'.
        /// </summary>
        public bool IsSpreadArgument { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitArgument(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            if (IsSpreadArgument)
            {
                emitter.Write("... ");
            }

            Expression.Emit(emitter);
        }
    }
}
