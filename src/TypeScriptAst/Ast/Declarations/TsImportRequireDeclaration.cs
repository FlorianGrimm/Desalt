// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImportRequireDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an import declaration using 'require', of the form 'import name = require(string);'.
    /// </summary>
    internal class TsImportRequireDeclaration : AstNode, ITsImportRequireDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsImportRequireDeclaration(ITsIdentifier name, ITsStringLiteral require)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Require = require ?? throw new ArgumentNullException(nameof(require));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Name { get; }
        public ITsStringLiteral Require { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImportRequireDeclaration(this);

        public override string CodeDisplay => $"import {Name} = require({Require});";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("import ");
            Name.Emit(emitter);
            emitter.Write(" = require(");
            Require.Emit(emitter);
            emitter.WriteLine(");");
        }
    }
}
