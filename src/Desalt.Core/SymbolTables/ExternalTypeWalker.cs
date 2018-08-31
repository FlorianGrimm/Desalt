// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalTypeWalker.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System.Collections.Generic;
    using System.Threading;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Walks the C# syntax tree and gathers all of the type symbols that are declared in
    /// externally-referenced assemblies.
    /// </summary>
    internal sealed class ExternalTypeWalker : CSharpSyntaxWalker
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly IAssemblySymbol _assemblyBeingTranslated;
        private readonly CancellationToken _cancellationToken;
        private readonly SemanticModel _semanticModel;
        private readonly HashSet<ITypeSymbol> _typeSymbols = new HashSet<ITypeSymbol>(SymbolTableUtils.KeyComparer);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ExternalTypeWalker(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _cancellationToken = cancellationToken;
            _assemblyBeingTranslated = semanticModel.Compilation.Assembly;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ISet<ITypeSymbol> ExternalTypeSymbols => _typeSymbols;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            AddTypeIfNecessary(node.Declaration.Type);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddTypeIfNecessary(node.Type);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddTypeIfNecessary(node.ReturnType);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            AddTypeIfNecessary(node.Type);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            AddTypeIfNecessary(node.Type);
            base.VisitVariableDeclaration(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            TypeInfo typeInfo = _semanticModel.GetTypeInfo(node);
            AddTypeIfNecessary(typeInfo.Type);
            AddTypeIfNecessary(typeInfo.ConvertedType);

            base.VisitIdentifierName(node);
        }

        private void AddTypeIfNecessary(TypeSyntax node)
        {
            // sometimes the node won't have a type, for example in anonymous delegates
            if (node == null)
            {
                return;
            }

            ITypeSymbol typeSymbol = node.GetTypeSymbol(_semanticModel);
            AddTypeIfNecessary(typeSymbol);
        }

        private void AddTypeIfNecessary(ITypeSymbol typeSymbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // don't add the type if it's defined in our assembly
            if (typeSymbol == null || Equals(typeSymbol.ContainingAssembly, _assemblyBeingTranslated))
            {
                return;
            }

            // if we're an array, then recurse and add the type elements
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                AddTypeIfNecessary(arrayTypeSymbol.ElementType);
            }
            else if (typeSymbol.Kind == SymbolKind.DynamicType)
            {
                // don't add 'dynamic' types
            }
            else
            {
                _typeSymbols.Add(typeSymbol);
            }
        }
    }
}
