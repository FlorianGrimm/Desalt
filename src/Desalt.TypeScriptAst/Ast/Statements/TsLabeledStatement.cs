// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLabeledStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a labeled statement.
    /// </summary>
    internal class TsLabeledStatement : TsAstNode, ITsLabeledStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsLabeledStatement(ITsIdentifier label, ITsStatement statement)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        public TsLabeledStatement(ITsIdentifier label, ITsFunctionDeclaration functionDeclaration)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            FunctionDeclaration = functionDeclaration ?? throw new ArgumentNullException(nameof(functionDeclaration));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Label { get; }
        public ITsStatement? Statement { get; }
        public ITsFunctionDeclaration? FunctionDeclaration { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitLabeledStatement(this);
        }

        public override string CodeDisplay => $"{Label}: {Statement?.CodeDisplay}{FunctionDeclaration?.CodeDisplay}";

        protected override void EmitInternal(Emitter emitter)
        {
            int currentIndentLevel = emitter.IndentLevel;

            // write the label one indentation level out from the rest of the body
            emitter.IndentLevel = Math.Max(emitter.IndentLevel - 1, 0);
            Label.Emit(emitter);
            emitter.WriteLine(":");
            emitter.IndentLevel = currentIndentLevel;

            Statement?.Emit(emitter);
            FunctionDeclaration?.Emit(emitter);
        }
    }
}
