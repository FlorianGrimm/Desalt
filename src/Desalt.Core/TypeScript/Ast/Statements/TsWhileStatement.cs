// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsWhileStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a while loop.
    /// </summary>
    internal class TsWhileStatement : AstNode, ITsWhileStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsWhileStatement(ITsExpression whileCondition, ITsStatement whileStatement)
        {
            WhileCondition = whileCondition ?? throw new ArgumentNullException(nameof(whileCondition));
            WhileStatement = whileStatement ?? throw new ArgumentNullException(nameof(whileStatement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression WhileCondition { get; }
        public ITsStatement WhileStatement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitWhileStatement(this);

        public override string CodeDisplay => $"while ({WhileCondition}) {WhileStatement}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("while (");
            WhileCondition.Emit(emitter);
            WhileStatement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }
    }
}
