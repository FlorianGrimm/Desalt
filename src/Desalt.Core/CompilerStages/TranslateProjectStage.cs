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
    using Desalt.Core.Options;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Emit;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that compiles all of the C# files in a .csproj file to TypeScript.
    /// </summary>
    internal class TranslateProjectStage
        : PipelineStage<ImmutableArray<DocumentTranslationContextWithSymbolTables>, ImmutableArray<string>>
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
        public override async Task<IExtendedResult<ImmutableArray<string>>> ExecuteAsync(
            ImmutableArray<DocumentTranslationContextWithSymbolTables> input,
            CompilerOptions options,
            CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            var results = input.AsParallel()
                .WithCancellation(cancellationToken)
                .Select(context => TranslateDocument(context, cancellationToken))
                .ToImmutableArray();

            var translatedFilePaths = results.Select(result => result.Result).ToImmutableArray();
            IEnumerable<Diagnostic> mergedDiagnostics = results.SelectMany(result => result.Diagnostics);

            return new ExtendedResult<ImmutableArray<string>>(translatedFilePaths, mergedDiagnostics);
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
            DocumentTranslationContextWithSymbolTables context,
            CancellationToken cancellationToken)
        {
            // create the output directory tree if it's not already present
            Directory.CreateDirectory(context.OutputPath);

            // ----------------------------------------------------
            // TEMP - copy the original .cs file for easy comparing
            // get the relative path of the file
            string relativePath = PathUtil.MakeRelativePath(
                context.Document.Project.FilePath!,
                context.Document.FilePath!);

            string destinationPath = Path.Combine(context.Options.OutputPath, relativePath);

            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            File.Copy(context.Document.FilePath, destinationPath, overwrite: true);
            // ----------------------------------------------------

            // translate the C# syntax tree to TypeScript
            IExtendedResult<ITsImplementationSourceFile> translation =
                Translator.TranslateDocument(context, cancellationToken: cancellationToken);
            ImmutableArray<Diagnostic> diagnostics = translation.Diagnostics;

            using (var stream = new FileStream(
                context.TypeScriptFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read))
            {
                using var emitter = new Emitter(stream, options: EmitOptions.UnixSpaces);
                ITsImplementationSourceFile typeScriptImplementationFile = translation.Result;
                typeScriptImplementationFile?.Emit(emitter);
            }

            return new ExtendedResult<string>(context.TypeScriptFilePath, diagnostics);
        }
    }
}
