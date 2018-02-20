// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DetermineTranslatableDocumentsStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CompilerStages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that takes a C# project and determines which documents are translatable to TypeScript.
    /// </summary>
    internal class DetermineTranslatableDocumentsStage : PipelineStage<Project, IEnumerable<DocumentTranslationContext>>
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
        public override async Task<IExtendedResult<IEnumerable<DocumentTranslationContext>>> ExecuteAsync(
            Project input,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = input.Documents.Select(
                document => DocumentTranslationContext.TryCreateAsync(document, options, cancellationToken));
            IExtendedResult<DocumentTranslationContext>[] results = await Task.WhenAll(tasks);

            IEnumerable<DocumentTranslationContext> onlyValidDocuments =
                results.Where(result => result.Result != null).Select(result => result.Result);
            DiagnosticList mergedDiagnostics = DiagnosticList.From(
                options,
                results.SelectMany(result => result.Diagnostics));

            return new ExtendedResult<IEnumerable<DocumentTranslationContext>>(onlyValidDocuments, mergedDiagnostics);
        }
    }
}
