// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArgument.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an argument in a call expression.
    /// </summary>
    internal class TsArgument : AstNode, ITsArgument
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArgument(ITsExpression argument, bool isSpreadArgument = false)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
            IsSpreadArgument = isSpreadArgument;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Argument { get; }

        /// <summary>
        /// Indicates whether the <see cref="Argument"/> is preceded by a spread operator '...'.
        /// </summary>
        public bool IsSpreadArgument { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitArgument(this);

        public override string CodeDisplay => (IsSpreadArgument ? "... " : "") + Argument;

        protected override void EmitInternal(Emitter emitter)
        {
            if (IsSpreadArgument)
            {
                emitter.Write("... ");
            }

            Argument.Emit(emitter);
        }
    }
}
