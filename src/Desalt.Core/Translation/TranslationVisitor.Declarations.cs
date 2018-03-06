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
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
        /// <summary>
        /// Called when the visitor visits a NamespaceDeclarationSyntax node.
        /// </summary>
        /// <returns>A list of <see cref="ITsImplementationModuleElement"/> elements.</returns>
        public override IEnumerable<IAstNode> VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            return node.Members.SelectMany(Visit).Cast<ITsImplementationModuleElement>();
        }

        /// <summary>
        /// Called when the visitor visits a InterfaceDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsImplementationModuleElement"/>, which is either an <see
        /// cref="ITsInterfaceDeclaration"/> or an <see cref="ITsExportImplementationElement"/>.
        /// </returns>
        public override IEnumerable<IAstNode> VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = TranslateIdentifier(node);

            // translate the interface body
            var translatedMembers = node.Members.SelectMany(Visit).Cast<ITsTypeMember>();
            ITsObjectType body = Factory.ObjectType(translatedMembers.ToArray());

            // translate the generic type parameters
            ITsTypeParameters typeParameters = Factory.TypeParameters();

            // translate the extends clause
            IEnumerable<ITsTypeReference> extendsClause = Enumerable.Empty<ITsTypeReference>();

            // create the interface declaration
            ITsInterfaceDeclaration interfaceDeclaration = Factory.InterfaceDeclaration(
                interfaceName,
                body,
                typeParameters,
                extendsClause);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final =
                ExportAndAddDocumentationCommentIfNecessary(node, interfaceDeclaration);
            return final.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a EnumDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsImplementationModuleElement"/>, which is either an <see
        /// cref="ITsEnumDeclaration"/> or an <see cref="ITsExportImplementationElement"/>.
        /// </returns>
        public override IEnumerable<IAstNode> VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            ITsIdentifier enumName = TranslateIdentifier(node);

            // translate the enum body
            var enumMembers = node.Members.SelectMany(Visit).Cast<ITsEnumMember>();
            ITsEnumDeclaration enumDeclaration = Factory.EnumDeclaration(enumName, enumMembers);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final = ExportAndAddDocumentationCommentIfNecessary(node, enumDeclaration);
            return final.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a EnumMemberDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            ITsIdentifier scriptName = TranslateIdentifier(node);
            ITsExpression value = null;
            if (node.EqualsValue != null)
            {
                value = Visit(node.EqualsValue.Value).Cast<ITsExpression>().Single();
            }

            ITsEnumMember translatedMember = Factory.EnumMember(scriptName, value);
            return translatedMember.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsMethodSignature"/>.</returns>
        public override IEnumerable<IAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ITsIdentifier functionName = TranslateIdentifier(node);

            // create the call signature
            ITsTypeParameters typeParameters = Factory.TypeParameters();
            var parameters = (ITsParameterList)Visit(node.ParameterList).Single();
            ITsType returnType = TypeTranslator.TranslateSymbol(
                node.ReturnType.GetTypeSymbol(_context.SemanticModel),
                _typesToImport);

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);

            ITsMethodSignature functionDeclaration = Factory.MethodSignature(
                functionName,
                isOptional: false,
                callSignature: callSignature);
            functionDeclaration = AddDocumentationCommentIfNecessary(node, functionDeclaration);

            return functionDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Converts the translated declaration to an exported declaration if the C# declaration is public.
        /// </summary>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is returned.
        /// </returns>
        private ITsImplementationModuleElement ExportIfNecessary(
            BaseTypeDeclarationSyntax node,
            ITsImplementationElement translatedDeclaration)
        {
            // determine if this declaration should be exported
            INamedTypeSymbol symbol = _context.SemanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return translatedDeclaration;
            }

            ITsExportImplementationElement exportedInterfaceDeclaration =
                Factory.ExportImplementationElement(translatedDeclaration);

            exportedInterfaceDeclaration = AddDocumentationCommentIfNecessary(node, exportedInterfaceDeclaration);
            return exportedInterfaceDeclaration;
        }

        private ITsImplementationModuleElement ExportAndAddDocumentationCommentIfNecessary(
            BaseTypeDeclarationSyntax node,
            ITsImplementationElement translatedDeclaration)
        {
            ITsImplementationModuleElement exported = ExportIfNecessary(node, translatedDeclaration);
            return AddDocumentationCommentIfNecessary(node, exported);
        }
    }
}
