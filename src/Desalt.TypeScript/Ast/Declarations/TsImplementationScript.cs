// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImplementationScript.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript implementation source file (extension '.ts'), containing statements and declarations.
    /// </summary>
    internal class TsImplementationScript : AstNode<TsVisitor>, ITsImplementationScript
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsImplementationScript(IEnumerable<ITsImplementationScriptElement> elements = null)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<ITsImplementationScriptElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsImplementationScriptElement> Elements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImplementationScript(this);

        public override string CodeDisplay => $"{GetType().Name}, Elements.Length = {Elements.Length}";

        public override void Emit(Emitter emitter) =>
            emitter.WriteList(Elements, indent: false, itemDelimiter: emitter.Options.Newline);
    }
}
