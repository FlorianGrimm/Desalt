// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Linq;
    using System.Threading;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Converts a CSharp syntax tree into a TypeScript syntax tree.
    /// </summary>
    public class CSharpToTypeScriptTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public IExtendedResult<ITsImplementationSourceFile> TranslateSyntaxTreeAsync(
            CSharpSyntaxTree syntaxTree,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var walker = new TranslationVisitor(semanticModel);
            CompilationUnitSyntax rootSyntaxNode = syntaxTree.GetCompilationUnitRoot(cancellationToken);
            var typeScriptSourceFile = (ITsImplementationSourceFile)walker.Visit(rootSyntaxNode).Single();

            return new ExtendedResult<ITsImplementationSourceFile>(typeScriptSourceFile, walker.Messages);
        }
    }
}
