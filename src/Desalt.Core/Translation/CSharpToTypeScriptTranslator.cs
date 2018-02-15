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
    using System.Threading.Tasks;
    using Desalt.Core.TypeScript.Ast;
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

        public async Task<IExtendedResult<ITsImplementationSourceFile>> TranslateSyntaxTreeAsync(
            CSharpSyntaxTree syntaxTree,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var walker = new TranslationVisitor();
            CompilationUnitSyntax rootSyntaxNode = syntaxTree.GetCompilationUnitRoot(cancellationToken);
            var typeScriptSourceFile = walker.Visit(rootSyntaxNode).Single() as ITsImplementationSourceFile;

            return await Task.FromResult<IExtendedResult<ITsImplementationSourceFile>>(
                new ExtendedResult<ITsImplementationSourceFile>(typeScriptSourceFile, walker.Messages));
        }
    }
}
