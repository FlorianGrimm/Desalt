// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFromClause.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a from clause in an import or export statement, of the form 'from moduleName'.
    /// </summary>
    internal class TsFromClause : TsAstNode, ITsFromClause
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFromClause(ITsStringLiteral module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsStringLiteral Module { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitFromClause(this);
        }

        public override string CodeDisplay => $"from {Module}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("from ");
            Module.Emit(emitter);
        }
    }
}
