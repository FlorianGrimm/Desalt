// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptableTypesSymbolVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Visits a symbol and gathes all types that don't have a [NonScriptable] attribute on them.
    /// </summary>
    internal sealed class ScriptableTypesSymbolVisitor : SymbolVisitor
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly CancellationToken _cancellationToken;
        private readonly List<INamedTypeSymbol> _typeSymbols = new List<INamedTypeSymbol>();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptableTypesSymbolVisitor(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<INamedTypeSymbol> ScriptableTypeSymbols => _typeSymbols.ToImmutableArray();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            foreach (IModuleSymbol moduleSymbol in symbol.Modules)
            {
                Visit(moduleSymbol);
            }
        }

        public override void VisitModule(IModuleSymbol symbol)
        {
            Visit(symbol.GlobalNamespace);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (INamespaceOrTypeSymbol namespaceOrTypeSymbol in symbol.GetMembers())
            {
                Visit(namespaceOrTypeSymbol);
            }
        }

        public override void VisitArrayType(IArrayTypeSymbol symbol)
        {
            Visit(symbol.ElementType);
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // skip namespaces, delegates, modules, and things that have a [NonScriptable] attribute
            if (!symbol.IsNamespace &&
                symbol.DelegateInvokeMethod == null &&
                symbol.MetadataName != "<Module>" &&
                SymbolTableUtils.FindSaltarelleAttribute(symbol, "NonScriptable") == null)
            {
                _typeSymbols.Add(symbol);
            }
        }
    }
}
