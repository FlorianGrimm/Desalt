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
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
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

        public async Task<IExtendedResult<ITsImplementationSourceFile>> TranslateDocumentAsync(
            Document document,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var diagnostics = new List<DiagnosticMessage>();

            // try to get the syntax tree
            SyntaxTree rawSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            if (rawSyntaxTree == null || !(rawSyntaxTree is CSharpSyntaxTree syntaxTree))
            {
                return new ExtendedResult<ITsImplementationSourceFile>(
                    null,
                    DiagnosticMessage.Error($"File does not contain a syntax tree: {document.FilePath}")
                        .ToSingleEnumerable());
            }

            // try to get the semantic model
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel == null)
            {
                diagnostics.AddRange(syntaxTree.GetDiagnostics().ToDiagnosticMessages());
                diagnostics.Add(
                    DiagnosticMessage.Error($"File does not contain a semantic model: {document.FilePath}"));
                return new ExtendedResult<ITsImplementationSourceFile>(null, diagnostics);
            }

            // add any diagnostic messages that may have happened when getting the syntax tree or the semantic model
            diagnostics.AddRange(semanticModel.GetDiagnostics(null, cancellationToken).ToDiagnosticMessages());

            var walker = new TranslationVisitor(semanticModel);
            CompilationUnitSyntax rootSyntaxNode = syntaxTree.GetCompilationUnitRoot(cancellationToken);
            var typeScriptSourceFile = (ITsImplementationSourceFile)walker.Visit(rootSyntaxNode).Single();

            return new ExtendedResult<ITsImplementationSourceFile>(typeScriptSourceFile, walker.Messages);
        }
    }
}
