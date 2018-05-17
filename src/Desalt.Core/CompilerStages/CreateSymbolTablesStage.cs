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
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that takes all of the documents to be compiled and extracts all of the defined
    /// symbols and which file they live in so that each file can correctly add <c>import</c>
    /// statements at the top of the translated file.
    /// </summary>
    internal class CreateSymbolTablesStage
        : PipelineStage<ImmutableArray<DocumentTranslationContext>,
            ImmutableArray<DocumentTranslationContextWithSymbolTables>>
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
        public override async Task<IExtendedResult<ImmutableArray<DocumentTranslationContextWithSymbolTables>>>
            ExecuteAsync(
                ImmutableArray<DocumentTranslationContext> input,
                CompilerOptions options,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            // since most of the symbol tables will need references to types directly referenced in
            // the documents and types in referenced assemblies, compute them once and then pass them
            // into each symbol table
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols =
                SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(
                    input,
                    SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                    cancellationToken);

            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols =
                SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                    directlyReferencedExternalTypeSymbols,
                    input.FirstOrDefault()?.SemanticModel.Compilation,
                    cancellationToken);

            // construct each symbol table in parallel
            var tasks = new List<Task<object>>
            {
                // create the import symbol table
                Task.Run<object>(
                    () => ImportSymbolTable.Create(input, directlyReferencedExternalTypeSymbols, cancellationToken),
                    cancellationToken),

                // create the script name symbol table
                Task.Run<object>(
                    () => ScriptNameSymbolTable.Create(
                        input,
                        directlyReferencedExternalTypeSymbols,
                        indirectlyReferencedExternalTypeSymbols,
                        cancellationToken),
                    cancellationToken),

                // create the inline code symbol table
                Task.Run<object>(
                    () => InlineCodeSymbolTable.Create(
                        input,
                        directlyReferencedExternalTypeSymbols,
                        indirectlyReferencedExternalTypeSymbols,
                        ImmutableArray<KeyValuePair<string, string>>.Empty,
                        cancellationToken),
                    cancellationToken),

                // create the alternate signature symbol table
                Task.Run<object>(
                    () => AlternateSignatureSymbolTable.Create(input, cancellationToken),
                    cancellationToken),
            };

            await Task.WhenAll(tasks);

            var importSymbolTable = (ImportSymbolTable)tasks[0].Result;
            var scriptNameSymbolTable = (ScriptNameSymbolTable)tasks[1].Result;
            var inlineCodeSymbolTable = (InlineCodeSymbolTable)tasks[2].Result;

            var alternateSignatureTableCreateResult = (IExtendedResult<AlternateSignatureSymbolTable>)tasks[3].Result;
            var diagnostics = alternateSignatureTableCreateResult.Diagnostics;
            var alternateSignatureSymbolTable = alternateSignatureTableCreateResult.Result;

            // create new context objects with the symbol table
            var newContexts = input.Select(
                    context => new DocumentTranslationContextWithSymbolTables(
                        context,
                        importSymbolTable,
                        scriptNameSymbolTable,
                        inlineCodeSymbolTable,
                        alternateSignatureSymbolTable))
                .ToImmutableArray();

            return new ExtendedResult<ImmutableArray<DocumentTranslationContextWithSymbolTables>>(
                newContexts,
                diagnostics);
        }
    }
}
