// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLabelledStatement.cs" company="Justin Rockwood">
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
    /// Represents a labelled statement.
    /// </summary>
    internal class TsLabelledStatement : AstNode<TsVisitor>, ITsLabelledStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsLabelledStatement(ITsIdentifier label, ITsStatement statement)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        public TsLabelledStatement(ITsIdentifier label, ITsFunctionDeclaration functionDeclaration)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            FunctionDeclaration = functionDeclaration ?? throw new ArgumentNullException(nameof(functionDeclaration));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Label { get; }
        public ITsStatement Statement { get; }
        public ITsFunctionDeclaration FunctionDeclaration { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitLabelledStatement(this);

        public override string CodeDisplay => $"{Label}: {Statement?.CodeDisplay}{FunctionDeclaration?.CodeDisplay}";

        public override void Emit(Emitter emitter)
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
