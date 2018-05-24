// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImportSpecifier.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an import specifier, which is either an identifier or 'identifier as identifier'.
    /// </summary>
    internal class TsImportSpecifier : TsAstNode, ITsImportSpecifier
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsImportSpecifier(ITsIdentifier name, ITsIdentifier asName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AsName = asName;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Name { get; }
        public ITsIdentifier AsName { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImportSpecifier(this);

        public override string CodeDisplay => $"${Name}" + (AsName != null ? $" as {AsName}" : "");

        protected override void EmitInternal(Emitter emitter)
        {
            Name.Emit(emitter);
            if (AsName != null)
            {
                emitter.Write(" as ");
                AsName.Emit(emitter);
            }
        }
    }
}
