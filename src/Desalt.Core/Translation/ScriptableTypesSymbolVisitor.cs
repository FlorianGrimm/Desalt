// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptableTypesSymbolVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    internal sealed class ScriptableTypesSymbolVisitor : SymbolVisitor
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<ITypeSymbol> _typeSymbols = new HashSet<ITypeSymbol>();

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

        public ISet<ITypeSymbol> ExternalTypeSymbols => _typeSymbols;

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

            if (!symbol.IsNamespace && symbol.MetadataName != "<Module>")
            {
                _typeSymbols.Add(symbol);
            }
        }
    }
}
