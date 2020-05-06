// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationContext.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Threading;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains data structures and helper methods needed for translation of C# to TypeScript.
    /// </summary>
    /// <remarks>
    /// Many translation functions are split between multiple sub-translators, but they all might participate in a larger
    /// translation context. Having this shared information in a single class allows the sub-translators to add
    /// information during a translation, for example the diagnostics or types to import.
    ///
    /// NOTE: This class is mutable and not thread-safe. This is by design since translation of a single document can't
    ///       really be done in parallel.
    /// </remarks>
    internal class TranslationContext
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TranslationContext(
            DocumentTranslationContextWithSymbolTables documentContext,
            DiagnosticList? diagnostics = null,
            ISet<ITypeSymbol>? typesToImport = null,
            CancellationToken cancellationToken = default)
            : this(
                documentContext.SemanticModel,
                documentContext.ScriptSymbolTable,
                documentContext.AlternateSignatureSymbolTable,
                documentContext.Options.RenameRules,
                diagnostics ?? new DiagnosticList(documentContext.Options.DiagnosticOptions),
                cancellationToken,
                typesToImport)
        {
        }

        public TranslationContext(
            SemanticModel semanticModel,
            ScriptSymbolTable scriptSymbolTable,
            AlternateSignatureSymbolTable alternateSignatureSymbolTable,
            RenameRules renameRules,
            DiagnosticList diagnostics,
            CancellationToken cancellationToken,
            ISet<ITypeSymbol>? typesToImport = null)
        {
            SemanticModel = semanticModel;
            ScriptSymbolTable = scriptSymbolTable;
            AlternateSignatureSymbolTable = alternateSignatureSymbolTable;
            RenameRules = renameRules;
            Diagnostics = diagnostics;
            CancellationToken = cancellationToken;
            TypesToImport = typesToImport ?? new HashSet<ITypeSymbol>();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the <see cref="AlternateSignatureSymbolTable"/> used during translation.
        /// </summary>
        public AlternateSignatureSymbolTable AlternateSignatureSymbolTable { get; }

        /// <summary>
        /// Gets the cancellation token that controls the whole translation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a <see cref="DiagnosticList"/> to use for logging errors, warnings, and other diagnostics while translating.
        /// </summary>
        public DiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the <see cref="RenameRules"/> to use when translating C# that may override what is specified in the
        /// original C# source code.
        /// </summary>
        public RenameRules RenameRules { get; }

        /// <summary>
        /// Gets the <see cref="ScriptSymbolTable"/> containing information about how all of the C# symbols should be
        /// translated to TypeScript.
        /// </summary>
        public ScriptSymbolTable ScriptSymbolTable { get; }

        /// <summary>
        /// Gets the <see cref="SemanticModel"/> containing information about all of the C# symbols.
        /// </summary>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets a <see cref="TemporaryVariableAllocator"/> to use when the translation needs to allocate temporary
        /// variables to hold transient calculations or objects.
        /// </summary>
        public TemporaryVariableAllocator TemporaryVariableAllocator { get; } = new TemporaryVariableAllocator();

        /// <summary>
        /// Gets a set of <see cref="ITypeSymbol"/> that are directly referenced in the translated TypeScript code and
        /// need to be imported at the top of the translated TypeScript file.
        /// </summary>
        public ISet<ITypeSymbol> TypesToImport { get; }
    }
}
