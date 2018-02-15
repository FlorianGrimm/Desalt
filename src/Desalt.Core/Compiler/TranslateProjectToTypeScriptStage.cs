// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslateProjectToTypeScriptStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Compiler
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Pipeline stage that compiles all of the C# files in a .csproj file to TypeScript.
    /// </summary>
    internal class TranslateProjectToTypeScriptStage : PipelineStage<Project, bool>
    {
        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="options">The compiler options to use.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        public override async Task<IExtendedResult<bool>> ExecuteAsync(
            Project input,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Directory.CreateDirectory(options.OutputPath);
            IEnumerable<Task<IExtendedResult<bool>>> tasks = input.Documents
                .Where(document => document.Name == "ILogAppender.cs")
                .Select(document => CompileToTypeScript(document, options, cancellationToken));
            IExtendedResult<bool>[] results = await Task.WhenAll(tasks);

            return new ExtendedResult<bool>(
                results.All(result => result.Result),
                results.SelectMany(result => result.Messages));
        }

        private async Task<IExtendedResult<bool>> CompileToTypeScript(
            Document document,
            CompilerOptions options,
            CancellationToken cancellationToken)
        {
            string typeScriptFilePath = Path.Combine(
                options.OutputPath,
                Path.GetFileNameWithoutExtension(document.FilePath) + ".ts");

            // TEMP - copy the original .cs file for easy comparing
            // ReSharper disable once AssignNullToNotNullAttribute
            File.Copy(document.FilePath, Path.Combine(options.OutputPath, Path.GetFileName(document.FilePath)), overwrite: true);

            var diagnostics = new List<DiagnosticMessage>();

            // try to get the syntax tree
            SyntaxTree rawSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            if (rawSyntaxTree == null || !(rawSyntaxTree is CSharpSyntaxTree syntaxTree))
            {
                return new ExtendedResult<bool>(
                    false,
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
                return new ExtendedResult<bool>(false, diagnostics);
            }

            // add any diagnostic messages that may have happened when getting the syntax tree or the semantic model
            diagnostics.AddRange(semanticModel.GetDiagnostics(null, cancellationToken).ToDiagnosticMessages());

            // translate the C# syntax tree to TypeScript
            var translator = new CSharpToTypeScriptTranslator();
            IExtendedResult<ITsImplementationSourceFile> translation =
                translator.TranslateSyntaxTreeAsync(syntaxTree, semanticModel, cancellationToken);
            diagnostics.AddRange(translation.Messages);

            using (var stream = new FileStream(
                typeScriptFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read))
            using (var emitter = new Emitter(stream))
            {
                ITsImplementationSourceFile typeScriptImplementationFile = translation.Result;
                typeScriptImplementationFile?.Emit(emitter);
            }

            return new ExtendedResult<bool>(diagnostics.IsSuccess(options), diagnostics);
        }
    }
}
