// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImportDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an import declaration of the form 'import ImportClause FromClause;' or 'import Module;'.
    /// </summary>
    internal class TsImportDeclaration : AstNode<TsVisitor>, ITsImportDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsImportDeclaration(ITsImportClause importClause, ITsFromClause fromClause)
        {
            ImportClause = importClause ?? throw new ArgumentNullException(nameof(importClause));
            FromClause = fromClause ?? throw new ArgumentNullException(nameof(fromClause));
        }

        public TsImportDeclaration(ITsStringLiteral module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsImportClause ImportClause { get; }
        public ITsFromClause FromClause { get; }
        public ITsStringLiteral Module { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImportDeclaration(this);

        public override string CodeDisplay =>
            $"import {Module?.CodeDisplay}{ImportClause?.CodeDisplay} {FromClause?.CodeDisplay} ;";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("import ");

            if (Module != null)
            {
                Module.Emit(emitter);
            }
            else
            {
                ImportClause.Emit(emitter);
                emitter.Write(" ");
                FromClause.Emit(emitter);
            }

            emitter.WriteLine(";");
        }
    }
}
