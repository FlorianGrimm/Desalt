// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentTranslationContext.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Contains information about how to validate or translate a C# document into TypeScript.
    /// </summary>
    internal sealed class DocumentTranslationContext
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DocumentTranslationContext(
            Document document,
            CompilerOptions options,
            CSharpSyntaxTree syntaxTree,
            SemanticModel semanticModel)
        {
            Document = document;
            Options = options;
            SyntaxTree = syntaxTree;
            SemanticModel = semanticModel;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Document Document { get; }
        public CompilerOptions Options { get; }

        public CSharpSyntaxTree SyntaxTree { get; }
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets the output path for the translated TypeScript file.
        /// </summary>
        public string TypeScriptFilePath =>
            Path.Combine(Options.OutputPath, Path.GetFileNameWithoutExtension(Document.FilePath) + ".ts");

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static async Task<IExtendedResult<DocumentTranslationContext>> TryCreateAsync(
            Document document,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // try to get the syntax tree
            SyntaxTree rawSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            if (rawSyntaxTree == null || !(rawSyntaxTree is CSharpSyntaxTree syntaxTree))
            {
                return new ExtendedResult<DocumentTranslationContext>(
                    null,
                    DiagnosticList.Create(options, DiagnosticFactory.DocumentContainsNoSyntaxTree(document)));
            }

            // try to get the semantic model
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel == null)
            {
                DiagnosticList syntaxDiagnostics = DiagnosticList.From(options, syntaxTree.GetDiagnostics());
                syntaxDiagnostics.Add(DiagnosticFactory.DocumentContainsNoSemanticModel(document));
                return new ExtendedResult<DocumentTranslationContext>(null, syntaxDiagnostics);
            }

            // add any diagnostic messages that may have happened when getting the syntax tree or the semantic model
            DiagnosticList diagnostics = DiagnosticList.From(options, semanticModel.GetDiagnostics(null, cancellationToken));

            var context = new DocumentTranslationContext(document, options, syntaxTree, semanticModel);
            return new ExtendedResult<DocumentTranslationContext>(context, diagnostics);
        }
    }
}
