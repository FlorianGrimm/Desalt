// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Converts a CSharp syntax tree into a TypeScript syntax tree.
    /// </summary>
    internal class CSharpToTypeScriptTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public IExtendedResult<ITsImplementationSourceFile> TranslateDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var diagnostics = new List<Diagnostic>();

            var walker = new TranslationVisitor(context.SemanticModel);
            CompilationUnitSyntax rootSyntaxNode = context.SyntaxTree.GetCompilationUnitRoot(cancellationToken);
            var typeScriptSourceFile = (ITsImplementationSourceFile)walker.Visit(rootSyntaxNode).Single();

            return new ExtendedResult<ITsImplementationSourceFile>(typeScriptSourceFile, walker.Diagnostics);
        }
    }
}
