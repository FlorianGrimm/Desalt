// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateProjectStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CompilerStages
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Options;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Desalt.Core.Validation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that validates a C# Project in preparation for translating to TypeScript.
    /// </summary>
    internal class ValidateProjectStage
        : PipelineStage<ImmutableArray<DocumentTranslationContextWithSymbolTables>, bool>
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
            ImmutableArray<DocumentTranslationContextWithSymbolTables> input,
            CompilerOptions options,
            CancellationToken cancellationToken = default)
        {
            IValidator[] validators =
            {
                new NoDefaultParametersInInterfacesValidator(),
                new NoDuplicateFieldAndPropertyNamesValidator(),
                new NoPartialClassesValidator(),
            };

            // Run all of the validators in parallel.
            var tasks = validators.Select(v => Task.Run(() => v.Validate(input), cancellationToken));
            IExtendedResult<bool>[] results = await Task.WhenAll(tasks);

            IEnumerable<Diagnostic> diagnostics = results.SelectMany(result => result.Diagnostics);
            return new SuccessOnNoErrorsResult(diagnostics);
        }
    }
}
