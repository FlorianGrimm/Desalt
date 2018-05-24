// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIfStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an 'if' statement of the form 'if (expression) statement else statement'.
    /// </summary>
    internal class TsIfStatement : TsAstNode, ITsIfStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsIfStatement(ITsExpression ifCondition, ITsStatement ifStatement, ITsStatement elseStatement = null)
        {
            IfCondition = ifCondition ?? throw new ArgumentNullException(nameof(ifCondition));
            IfStatement = ifStatement ?? throw new ArgumentNullException(nameof(ifStatement));
            ElseStatement = elseStatement;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression IfCondition { get; }
        public ITsStatement IfStatement { get; }
        public ITsStatement ElseStatement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitIfStatement(this);

        public override string CodeDisplay =>
            $"if ({IfCondition}) {IfStatement}" + (ElseStatement != null ? $" else {ElseStatement}" : "");

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("if (");
            IfCondition.Emit(emitter);
            IfStatement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: ElseStatement == null);

            if (ElseStatement != null)
            {
                if (IfStatement is ITsBlockStatement)
                {
                    emitter.Write(" ");
                }

                emitter.Write("else");
                ElseStatement.EmitIndentedOrInBlock(
                    emitter,
                    prefixForIndentedStatement: "",
                    prefixForBlock: " ",
                    newlineAfterBlock: true);
            }
        }
    }
}
