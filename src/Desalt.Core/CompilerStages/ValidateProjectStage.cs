// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateProjectStage.cs" company="Justin Rockwood">
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
    using Desalt.Core.Validation;

    /// <summary>
    /// Pipeline stage that validates a C# Project in preparation for translating to TypeScript.
    /// </summary>
    internal class ValidateProjectStage : PipelineStage<IEnumerable<DocumentTranslationContextWithSymbolTables>, bool>
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
            IEnumerable<DocumentTranslationContextWithSymbolTables> input,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // validate all of the documents in parallel
            IEnumerable<Task<IExtendedResult<bool>>> tasks = input.Select(
                context => new CSharpToTypeScriptValidator().ValidateDocumentAsync(context));

            IExtendedResult<bool>[] results = await Task.WhenAll(tasks);

            DiagnosticList diagnostics = DiagnosticList.From(options, results.SelectMany(result => result.Diagnostics));
            return new SuccessResult(diagnostics.FilteredDiagnostics);
        }
    }
}
