// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplementationSourceFile.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript implementation source file (extension '.ts'), containing statements and declarations.
    /// </summary>
    public class ImplementationSourceFile : AstNode<TsVisitor>, IAstNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal ImplementationSourceFile(IEnumerable<IImplementationScriptElement> scriptElements)
        {
            ScriptElements = scriptElements?.ToImmutableArray() ?? ImmutableArray<IImplementationScriptElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<IImplementationScriptElement> ScriptElements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitImplementationSourceFile(this);

        public override string CodeDisplay => $"{GetType().Name}, ScriptElements.Count = {ScriptElements.Length}";

        public override void Emit(Emitter emitter) =>
            emitter.WriteList(ScriptElements, indent: false, itemDelimiter: emitter.Options.Newline);
    }
}
