// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed partial class TranslationVisitor
    {
        /// <summary>
        /// Called when the visitor visits a NamespaceDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var elements = new List<ITsImplementationModuleElement>();

            foreach (MemberDeclarationSyntax member in node.Members)
            {
                var element = (ITsImplementationModuleElement)Visit(member).SingleOrDefault();
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return elements;
        }

        /// <summary>
        /// Called when the visitor visits a InterfaceDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = TranslateIdentifier(node);

            // get the interface body
            var typeMembers = new List<ITsTypeMember>();
            foreach (MemberDeclarationSyntax member in node.Members)
            {
                var typeMember = (ITsTypeMember)Visit(member).SingleOrDefault();
                if (typeMember != null)
                {
                    typeMembers.Add(typeMember);
                }
            }

            ITsObjectType body = TsAstFactory.ObjectType(typeMembers.ToArray());
            ITsTypeParameters typeParameters = TsAstFactory.TypeParameters();
            IEnumerable<ITsTypeReference> extendsClause = Enumerable.Empty<ITsTypeReference>();

            ITsInterfaceDeclaration interfaceDeclaration = TsAstFactory.InterfaceDeclaration(
                interfaceName,
                body,
                typeParameters,
                extendsClause);

            INamedTypeSymbol symbol = _context.SemanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                ITsExportImplementationElement exportedInterfaceDeclaration =
                    TsAstFactory.ExportImplementationElement(interfaceDeclaration);

                exportedInterfaceDeclaration = AddDocumentationCommentIfNecessary(node, exportedInterfaceDeclaration);
                return exportedInterfaceDeclaration.ToSingleEnumerable();
            }

            interfaceDeclaration = AddDocumentationCommentIfNecessary(node, interfaceDeclaration);
            return interfaceDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsMethodSignature"/>.</returns>
        public override IEnumerable<IAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ITsIdentifier functionName = TranslateIdentifier(node);

            // create the call signature
            ITsTypeParameters typeParameters = TsAstFactory.TypeParameters();
            var parameters = (ITsParameterList)Visit(node.ParameterList).Single();
            ITsType returnType = TypeTranslator.TranslateSymbol(
                node.ReturnType.GetTypeSymbol(_context.SemanticModel),
                _typesToImport);

            ITsCallSignature callSignature = TsAstFactory.CallSignature(typeParameters, parameters, returnType);

            ITsMethodSignature functionDeclaration = TsAstFactory.MethodSignature(
                functionName,
                isOptional: false,
                callSignature: callSignature);
            functionDeclaration = AddDocumentationCommentIfNecessary(node, functionDeclaration);

            return functionDeclaration.ToSingleEnumerable();
        }
    }
}
