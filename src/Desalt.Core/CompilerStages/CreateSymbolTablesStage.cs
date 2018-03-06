// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSymbolTablesStage.cs" company="Justin Rockwood">
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
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;

    /// <summary>
    /// Pipeline stage that takes all of the documents to be compiled and extracts all of the defined
    /// symbols and which file they live in so that each file can correctly add <c>import</c>
    /// statements at the top of the translated file.
    /// </summary>
    internal class CreateSymbolTablesStage : PipelineStage<IEnumerable<DocumentTranslationContext>,
        IEnumerable<DocumentTranslationContextWithSymbolTable>>
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
        public override async Task<IExtendedResult<IEnumerable<DocumentTranslationContextWithSymbolTable>>>
            ExecuteAsync(
                IEnumerable<DocumentTranslationContext> input,
                CompilerOptions options,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            var symbolTable = new ImportSymbolTable();
            var contexts = input as DocumentTranslationContext[] ?? input.ToArray();

            // add all of the symbols in parallel (the symbol table is thread-safe)
            var tasks = contexts.Select(
                context => Task.Run(() => symbolTable.AddDefinedTypesInDocument(context), cancellationToken));
            await Task.WhenAll(tasks);

            // create new context objects with the symbol table
            var newContexts = contexts.Select(
                context => new DocumentTranslationContextWithSymbolTable(context, symbolTable));

            return new ExtendedResult<IEnumerable<DocumentTranslationContextWithSymbolTable>>(newContexts);
        }
    }
}
