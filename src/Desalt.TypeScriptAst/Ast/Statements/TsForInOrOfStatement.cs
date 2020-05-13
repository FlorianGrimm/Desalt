// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsForInOrOfStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a for-in or for-of loop of the form 'for (const x: type in|of expression) statement'.
    /// </summary>
    internal class TsForInOrOfStatement : TsAstNode, ITsForInStatement, ITsForOfStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a for-in loop of the form, 'for (x in expression) statement'.
        /// </summary>
        public TsForInOrOfStatement(
            ITsExpression initializer,
            ITsExpression rightSide,
            ITsStatement statement,
            bool ofLoop)
        {
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            OfLoop = ofLoop;
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (const x in expression) statement'.
        /// </summary>
        public TsForInOrOfStatement(
            VariableDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide,
            ITsStatement statement,
            bool ofLoop)
        {
            DeclarationKind = declarationKind;
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            OfLoop = ofLoop;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression? Initializer { get; }
        public VariableDeclarationKind? DeclarationKind { get; }
        public ITsBindingIdentifierOrPattern? Declaration { get; }

        public ITsExpression RightSide { get; }
        public ITsStatement Statement { get; }

        public bool OfLoop { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            if (OfLoop)
            {
                visitor.VisitForOfStatement(this);
            }
            else
            {
                visitor.VisitForInStatement(this);
            }
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("for (");

            if (Initializer != null)
            {
                Initializer.Emit(emitter);
            }
            else
            {
                DeclarationKind?.Emit(emitter);
                Declaration?.Emit(emitter);
            }

            emitter.Write(OfLoop ? " of " : " in ");
            RightSide.Emit(emitter);

            Statement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }
    }
}
