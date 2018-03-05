// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslateProjectStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CompilerStages
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Emit;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that compiles all of the C# files in a .csproj file to TypeScript.
    /// </summary>
    internal class TranslateProjectStage : PipelineStage<IEnumerable<DocumentTranslationContextWithSymbolTable>,
        IEnumerable<string>>
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
        public override async Task<IExtendedResult<IEnumerable<string>>> ExecuteAsync(
            IEnumerable<DocumentTranslationContextWithSymbolTable> input,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Yield();

            Directory.CreateDirectory(options.OutputPath);

            ImmutableArray<IExtendedResult<string>> results = input
                .Where(context => context.Document.Name == "ILogAppender.cs")
                .AsParallel()
                .Select(context => TranslateDocument(context, cancellationToken))
                .ToImmutableArray();

            IEnumerable<string> translatedFilePaths = results.Select(result => result.Result);
            IEnumerable<Diagnostic> mergedDiagnostics = results.SelectMany(result => result.Diagnostics);

            return new ExtendedResult<IEnumerable<string>>(translatedFilePaths, mergedDiagnostics);
        }

        /// <summary>
        /// Translates a single C# document into TypeScript.
        /// </summary>
        /// <param name="context">The document to translate.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The file path to the translated TypeScript file.</returns>
        private static IExtendedResult<string> TranslateDocument(
            DocumentTranslationContextWithSymbolTable context,
            CancellationToken cancellationToken)
        {
            // TEMP - copy the original .cs file for easy comparing
            // ReSharper disable once AssignNullToNotNullAttribute
            File.Copy(
                context.Document.FilePath,
                Path.Combine(context.Options.OutputPath, Path.GetFileName(context.Document.FilePath)),
                overwrite: true);

            // translate the C# syntax tree to TypeScript
            var translator = new CSharpToTypeScriptTranslator();
            IExtendedResult<ITsImplementationSourceFile> translation =
                translator.TranslateDocument(context, cancellationToken);
            ImmutableArray<Diagnostic> diagnostics = translation.Diagnostics;

            using (var stream = new FileStream(
                context.TypeScriptFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read))
            using (var emitter = new Emitter(stream, options: EmitOptions.UnixSpaces))
            {
                ITsImplementationSourceFile typeScriptImplementationFile = translation.Result;
                typeScriptImplementationFile?.Emit(emitter);
            }

            return new ExtendedResult<string>(context.TypeScriptFilePath, diagnostics);
        }
    }
}
