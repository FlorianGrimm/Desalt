// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTable.Walker.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal partial class ImportSymbolTable
    {
        private sealed class Walker : CSharpSyntaxWalker
        {
            private readonly Func<ISymbol, ImportSymbolInfo, ImportSymbolInfo> _addFunc;
            private readonly IAssemblySymbol _assemblyBeingTranslated;
            private readonly SemanticModel _semanticModel;

            public Walker(SemanticModel semanticModel, Func<ISymbol, ImportSymbolInfo, ImportSymbolInfo> addFunc)
            {
                _semanticModel = semanticModel;
                _addFunc = addFunc;
                _assemblyBeingTranslated = semanticModel.Compilation.Assembly;
            }

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
                // don't add the type if it's a native JS type or if it's defined in our assembly
                if (typeSymbol == null ||
                    TypeTranslator.IsNativeJavaScriptType(typeSymbol) ||
                    Equals(typeSymbol.ContainingAssembly, _assemblyBeingTranslated))
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
                    var containingAssembly = typeSymbol.ContainingAssembly;
                    Debug.Assert(containingAssembly != null, $"{typeSymbol.Name}.ContainingAssembly is null");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (containingAssembly != null)
                    {
                        string moduleName = containingAssembly.Name;
                        ImportSymbolInfo symbolInfo = ImportSymbolInfo.CreateExternalReference(moduleName);
                        _addFunc(typeSymbol, symbolInfo);
                    }
                }
            }
        }
    }
}
