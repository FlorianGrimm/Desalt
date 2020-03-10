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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;
    using Desalt.Core.SymbolTables;
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
            var diagnostics = new List<Diagnostic>();

            // get the symbol table overrides
            SymbolTableOverrides overrides = options.SymbolTableOverrides;

            // the compilation should be the same in all of the contexts, so just use the first one
            Compilation compilation = input.First().SemanticModel.Compilation;
            Debug.Assert(input.All(context => ReferenceEquals(context.SemanticModel.Compilation, compilation)));

            // since most of the symbol tables will need references to types directly referenced in
            // the documents and types in referenced assemblies, compute them once and then pass them
            // into each symbol table
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols =
                SymbolDiscoverer.DiscoverDirectlyReferencedExternalTypes(
                    input,
                    SymbolDiscoveryKind.DocumentAndAllAssemblyTypes,
                    cancellationToken);

            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols =
                SymbolDiscoverer.DiscoverTypesInReferencedAssemblies(
                    directlyReferencedExternalTypeSymbols,
                    compilation,
                    cancellationToken);

            // create a script namer
            IAssemblySymbol mscorlibAssemblySymbol = SymbolDiscoverer.GetMscorlibAssemblySymbol(compilation);
            var scriptNamer = new ScriptNamer(mscorlibAssemblySymbol, options.RenameRules);

            // construct each symbol table in parallel
            var tasks = new List<Task<object>>
            {
                // create the script symbol table
                Task.Run<object>(
                    () => ScriptSymbolTable.Create(
                        input,
                        scriptNamer,
                        SymbolDiscoveryKind.DocumentAndAllAssemblyTypes,
                        cancellationToken),
                    cancellationToken),

                // create the alternate signature symbol table
                Task.Run<object>(
                    () => AlternateSignatureSymbolTable.Create(input, cancellationToken),
                    cancellationToken),
            };

            await Task.WhenAll(tasks);

            var scriptSymbolTable = (ScriptSymbolTable)tasks[0].Result;

            var alternateSignatureTableCreateResult = (IExtendedResult<AlternateSignatureSymbolTable>)tasks[1].Result;
            diagnostics.AddRange(alternateSignatureTableCreateResult.Diagnostics);
            AlternateSignatureSymbolTable alternateSignatureSymbolTable = alternateSignatureTableCreateResult.Result;

            // create new context objects with the symbol table
            var newContexts = input.Select(
                    context => new DocumentTranslationContextWithSymbolTables(
                        context,
                        scriptSymbolTable,
                        alternateSignatureSymbolTable))
                .ToImmutableArray();

            return new ExtendedResult<ImmutableArray<DocumentTranslationContextWithSymbolTables>>(
                newContexts,
                diagnostics);
        }
    }
}
