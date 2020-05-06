// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentTranslationContext.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains information about how to validate or translate a C# document into TypeScript.
    /// </summary>
    internal class DocumentTranslationContext
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected DocumentTranslationContext(
            Document document,
            CompilerOptions options,
            CSharpSyntaxTree syntaxTree,
            CompilationUnitSyntax rootSyntax,
            SemanticModel semanticModel)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            RootSyntax = rootSyntax ?? throw new ArgumentNullException(nameof(rootSyntax));
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Document Document { get; }
        public CompilerOptions Options { get; }

        public CSharpSyntaxTree SyntaxTree { get; }
        public CompilationUnitSyntax RootSyntax { get; }
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets the output path for this document translation context.
        /// </summary>
        public string OutputPath =>
            Options.OutputPath ??
            Path.GetDirectoryName(Document.Project.OutputFilePath) ??
            Path.GetDirectoryName(Document.Project.FilePath) ??
            throw new InvalidOperationException(
                $"Cannot determine the output path for the TypeScript file: {Document.Name}");

        /// <summary>
        /// Gets the output path for the translated TypeScript file.
        /// </summary>
        public string TypeScriptFilePath
        {
            get
            {
                string? docPath = Document.FilePath;
                string? projectFilePath = Document.Project.FilePath;

                if (docPath == null || projectFilePath == null)
                {
                    return Document.Name + ".ts";
                }

                string relativeDir = PathUtil.MakeRelativePath(projectFilePath, docPath);
                relativeDir = Path.GetDirectoryName(relativeDir) ?? ".";
                string tsFileName = Path.GetFileNameWithoutExtension(docPath) + ".ts";

                return Path.GetFullPath(Path.Combine(OutputPath, relativeDir, tsFileName));
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static async Task<IExtendedResult<DocumentTranslationContext?>> TryCreateAsync(
            Document document,
            CompilerOptions options,
            CancellationToken cancellationToken = default)
        {
            // try to get the syntax tree
            SyntaxTree? rawSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            if (rawSyntaxTree == null || !(rawSyntaxTree is CSharpSyntaxTree syntaxTree))
            {
                return new ExtendedResult<DocumentTranslationContext?>(
                    null,
                    DiagnosticFactory.DocumentContainsNoSyntaxTree(document).ToSingleEnumerable());
            }

            CompilationUnitSyntax rootSyntax = syntaxTree.GetCompilationUnitRoot(cancellationToken);

            // try to get the semantic model
            SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel == null)
            {
                var syntaxDiagnostics = new List<Diagnostic>(syntaxTree.GetDiagnostics())
                {
                    DiagnosticFactory.DocumentContainsNoSemanticModel(document)
                };
                return new ExtendedResult<DocumentTranslationContext?>(null, syntaxDiagnostics);
            }

            // add any diagnostic messages that may have happened when getting the syntax tree or the semantic model
            var diagnostics = new List<Diagnostic>(semanticModel.GetDiagnostics(null, cancellationToken));
            var context = new DocumentTranslationContext(document, options, syntaxTree, rootSyntax, semanticModel);
            return new ExtendedResult<DocumentTranslationContext>(context, diagnostics);
        }
    }

    /// <summary>
    /// Contains information about how to validate or translate a C# document into TypeScript, with a
    /// valid <see cref="ScriptSymbolTable"/>.
    /// </summary>
    internal sealed class DocumentTranslationContextWithSymbolTables : DocumentTranslationContext
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public DocumentTranslationContextWithSymbolTables(
            DocumentTranslationContext context,
            ScriptSymbolTable scriptSymbolTable,
            AlternateSignatureSymbolTable alternateSignatureSymbolTable) : base(
            context.Document,
            context.Options,
            context.SyntaxTree,
            context.RootSyntax,
            context.SemanticModel)
        {
            ScriptSymbolTable =
                scriptSymbolTable ?? throw new ArgumentNullException(nameof(scriptSymbolTable));

            AlternateSignatureSymbolTable = alternateSignatureSymbolTable ??
                throw new ArgumentNullException(nameof(alternateSignatureSymbolTable));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ScriptSymbolTable ScriptSymbolTable { get; }

        public AlternateSignatureSymbolTable AlternateSignatureSymbolTable { get; }
    }
}
