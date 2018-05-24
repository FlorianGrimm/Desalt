// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImportAliasDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an import alias declaration of the form, 'import alias = dotted.name'.
    /// </summary>
    internal class TsImportAliasDeclaration : TsAstNode, ITsImportAliasDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsImportAliasDeclaration(ITsIdentifier alias, ITsQualifiedName importedName)
        {
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            ImportedName = importedName ?? throw new ArgumentNullException(nameof(importedName));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Alias { get; }
        public ITsQualifiedName ImportedName { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImportAliasDeclaration(this);

        public override string CodeDisplay => $"import {Alias} = {ImportedName};";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("import ");
            Alias.Emit(emitter);
            emitter.Write(" = ");
            ImportedName.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
