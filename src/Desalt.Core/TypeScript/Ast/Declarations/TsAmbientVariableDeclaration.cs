// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientVariableDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an ambient variable declaration of the form, 'var|let|const x, y: type;'.
    /// </summary>
    internal class TsAmbientVariableDeclaration : AstNode<TsVisitor>, ITsAmbientVariableDeclaration
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

        public override void Accept(TsVisitor visitor) => visitor.VisitAmbientVariableDeclaration(this);

        public override string CodeDisplay => $"{DeclarationKind.CodeDisplay()}{Declarations.ToElidedList()};";

        public override void Emit(Emitter emitter)
        {
            DeclarationKind.Emit(emitter);
            Declarations.EmitCommaList(emitter);
            emitter.WriteLine(";");
        }
    }
}
