// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientVariableDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an ambient variable declaration of the form, 'var|let|const x, y: type;'.
    /// </summary>
    internal class TsAmbientVariableDeclaration : TsAstNode, ITsAmbientVariableDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientVariableDeclaration(
            VariableDeclarationKind declarationKind,
            IEnumerable<ITsAmbientBinding> declarations)
        {
            DeclarationKind = declarationKind;
            Declarations = declarations?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(declarations));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public VariableDeclarationKind DeclarationKind { get; }
        public ImmutableArray<ITsAmbientBinding> Declarations { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitAmbientVariableDeclaration(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            DeclarationKind.Emit(emitter);
            Declarations.EmitCommaList(emitter);
            emitter.WriteLine(";");
        }
    }
}
