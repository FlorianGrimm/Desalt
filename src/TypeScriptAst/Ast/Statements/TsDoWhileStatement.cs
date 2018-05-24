// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsDoWhileStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a do/while statement.
    /// </summary>
    internal class TsDoWhileStatement : TsAstNode, ITsDoWhileStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsDoWhileStatement(ITsStatement doStatement, ITsExpression whileCondition)
        {
            DoStatement = doStatement ?? throw new ArgumentNullException(nameof(doStatement));
            WhileCondition = whileCondition ?? throw new ArgumentNullException(nameof(whileCondition));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsStatement DoStatement { get; }
        public ITsExpression WhileCondition { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitDoWhileStatement(this);

        public override string CodeDisplay => $"do {DoStatement} while ({WhileCondition});";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("do");
            DoStatement.EmitIndentedOrInBlock(emitter, prefixForIndentedStatement: "", prefixForBlock: " ");

            emitter.Write(DoStatement is ITsBlockStatement ? " while (" : "while (");
            WhileCondition.Emit(emitter);
            emitter.WriteLine(");");
        }
    }
}
