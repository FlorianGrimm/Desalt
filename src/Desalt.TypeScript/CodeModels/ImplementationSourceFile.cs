// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplementationSourceFile.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript implementation source file (extension '.ts'), containing statements and declarations.
    /// </summary>
    public class ImplementationSourceFile : CodeModel, ITsCodeModel
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitImplementationSourceFile(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitImplementationSourceFile(this);

        public override string ToCodeDisplay() => $"{GetType().Name}, ScriptElements.Count = {ScriptElements.Length}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(writer, ScriptElements, indent: false, itemDelimiter: Environment.NewLine);
        }
    }
}
