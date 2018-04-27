// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSymbolTablesStage.cs" company="Justin Rockwood">
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
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;

    /// <summary>
    /// Pipeline stage that takes all of the documents to be compiled and extracts all of the defined
    /// symbols and which file they live in so that each file can correctly add <c>import</c>
    /// statements at the top of the translated file.
    /// </summary>
    internal class CreateSymbolTablesStage : PipelineStage<IEnumerable<DocumentTranslationContext>,
        IEnumerable<DocumentTranslationContextWithSymbolTables>>
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
        public override async Task<IExtendedResult<IEnumerable<DocumentTranslationContextWithSymbolTables>>>
            ExecuteAsync(
                IEnumerable<DocumentTranslationContext> input,
                CompilerOptions options,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            var contexts = input.ToImmutableArray();

            // construct each symbol table in parallel
            var tasks = new List<Task<object>>
            {
                // create the import symbol table
                Task.Run<object>(
                    () => ImportSymbolTable.Create(
                        contexts,
                        SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                        cancellationToken: cancellationToken),
                    cancellationToken),

                // create the script name symbol table
                Task.Run<object>(
                    () => ScriptNameSymbolTable.Create(
                        contexts,
                        SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                        cancellationToken: cancellationToken),
                    cancellationToken),

                // create the inline code symbol table
                Task.Run<object>(
                    () => InlineCodeSymbolTable.Create(
                        contexts,
                        SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                        cancellationToken),
                    cancellationToken),
            };

            await Task.WhenAll(tasks);

            var importSymbolTable = (ImportSymbolTable)tasks[0].Result;
            var scriptNameSymbolTable = (ScriptNameSymbolTable)tasks[1].Result;
            var inlineCodeSymbolTable = (InlineCodeSymbolTable)tasks[2].Result;

            // create new context objects with the symbol table
            var newContexts = contexts.Select(
                context => new DocumentTranslationContextWithSymbolTables(
                    context,
                    importSymbolTable,
                    scriptNameSymbolTable,
                    inlineCodeSymbolTable));

            return new ExtendedResult<IEnumerable<DocumentTranslationContextWithSymbolTables>>(newContexts);
        }
    }
}
