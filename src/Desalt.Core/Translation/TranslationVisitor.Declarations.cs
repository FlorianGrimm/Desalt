// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Diagnostics;
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
            ITsIdentifier interfaceName = TranslateDeclarationIdentifier(node);

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
            ITsImplementationModuleElement final = ExportAndAddDocComment(interfaceDeclaration, node);
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
            ITsIdentifier enumName = TranslateDeclarationIdentifier(node);

            // translate the enum body
            var enumMembers = node.Members.SelectMany(Visit).Cast<ITsEnumMember>();
            ITsEnumDeclaration enumDeclaration = Factory.EnumDeclaration(enumName, enumMembers);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final = ExportAndAddDocComment(enumDeclaration, node);
            return final.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a EnumMemberDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            ITsIdentifier scriptName = TranslateDeclarationIdentifier(node);
            ITsExpression value = null;
            if (node.EqualsValue != null)
            {
                value = Visit(node.EqualsValue.Value).Cast<ITsExpression>().Single();
            }

            ITsEnumMember translatedMember = Factory.EnumMember(scriptName, value);
            return translatedMember.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ClassDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsImplementationModuleElement"/>, which is either an <see
        /// cref="ITsClassElement"/> or an <see cref="ITsExportImplementationElement"/>.
        /// </returns>
        public override IEnumerable<IAstNode> VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ITsIdentifier className = TranslateDeclarationIdentifier(node);

            // translate the generic type parameters
            ITsTypeParameters typeParameters = Factory.TypeParameters();

            // translate the class heritage
            ITsClassHeritage heritage = Factory.ClassHeritage(implementsTypes: null);

            // translate the interface body
            var classBody = node.Members.SelectMany(Visit).Cast<ITsClassElement>();

            ITsClassDeclaration classDeclaration = Factory.ClassDeclaration(
                className,
                typeParameters,
                heritage,
                classBody);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final = ExportAndAddDocComment(classDeclaration, node);
            return final.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a FieldDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var fieldDeclarations = new List<ITsVariableMemberDeclaration>();
            foreach (VariableDeclaratorSyntax variableDeclaration in node.Declaration.Variables)
            {
                var variableName = Factory.Identifier(variableDeclaration.Identifier.Text);
                ISymbol symbol = _semanticModel.GetDeclaredSymbol(variableDeclaration);
                TsAccessibilityModifier accessibilityModifier =
                    GetAccessibilityModifier(symbol, variableDeclaration.GetLocation);

                bool isReadOnly = node.Modifiers.Any(token => token.IsKind(SyntaxKind.ReadOnlyKeyword));

                var typeAnnotation = TypeTranslator.TranslateSymbol(
                    node.Declaration.Type.GetTypeSymbol(_semanticModel),
                    _typesToImport);

                ITsExpression initializer = null;
                if (variableDeclaration.Initializer != null)
                {
                    initializer = (ITsExpression)Visit(variableDeclaration.Initializer).First();
                }

                ITsVariableMemberDeclaration fieldDeclaration = AddDocumentationComment(
                    Factory.VariableMemberDeclaration(
                        variableName,
                        accessibilityModifier,
                        symbol.IsStatic,
                        isReadOnly,
                        typeAnnotation,
                        initializer),
                    node,
                    variableDeclaration);
                fieldDeclarations.Add(fieldDeclaration);
            }

            return fieldDeclarations;
        }

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsMethodSignature"/> if we're within an interface declaration; otherwise
        /// an <see cref="ITsFunctionMemberDeclaration"/>.
        /// </returns>
        public override IEnumerable<IAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            ITsIdentifier functionName = TranslateDeclarationIdentifier(node);

            // create the call signature
            ITsCallSignature callSignature = TranslateCallSignature(node.ParameterList, node.ReturnType);

            // if we're defining an interface, then we need to return a ITsMethodSignature
            if (_semanticModel.GetDeclaredSymbol(node).ContainingType.IsInterfaceType())
            {
                ITsMethodSignature methodSignature = Factory.MethodSignature(
                    functionName,
                    isOptional: false,
                    callSignature: callSignature);
                methodSignature = AddDocumentationComment(methodSignature, node);
                return methodSignature.ToSingleEnumerable();
            }

            // a function body can be null in the case of an 'extern' declaration.
            ITsBlockStatement functionBody = null;
            if (node.Body != null)
            {
                functionBody = (ITsBlockStatement)Visit(node.Body).Single();
            }

            // we're within a class, so return a method declaration
            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);
            ITsFunctionMemberDeclaration methodDeclaration = Factory.FunctionMemberDeclaration(
                functionName,
                callSignature,
                accessibilityModifier,
                symbol.IsStatic,
                functionBody?.Statements);

            methodDeclaration = AddDocumentationComment(methodDeclaration, node);
            return methodDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ConstructorDeclarationSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsConstructorDeclaration"/>.</returns>
        public override IEnumerable<IAstNode> VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);
            var parameterList = Factory.ParameterList();

            var functionBody = (ITsBlockStatement)Visit(node.Body).Single();

            ITsConstructorDeclaration constructorDeclaration = Factory.ConstructorDeclaration(
                accessibilityModifier,
                parameterList,
                functionBody.Statements);

            constructorDeclaration = AddDocumentationComment(constructorDeclaration, node);
            return constructorDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a PropertyDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of one or both of <see
        /// cref="ITsGetAccessorMemberDeclaration"/> or <see cref="ITsSetAccessorMemberDeclaration"/>.
        /// </returns>
        public override IEnumerable<IAstNode> VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            ITsIdentifier propertyName = TranslateDeclarationIdentifier(node);
            ITypeSymbol typeSymbol = node.Type.GetTypeSymbol(_semanticModel);
            var propertyType = TypeTranslator.TranslateSymbol(typeSymbol, _typesToImport);
            bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);

            foreach (AccessorDeclarationSyntax accessor in node.AccessorList.Accessors)
            {
                TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(accessor);
                var functionBody = (ITsBlockStatement)Visit(accessor.Body).Single();

                switch (accessor.Kind())
                {
                    case SyntaxKind.GetAccessorDeclaration:
                        ITsGetAccessor getAccessor = Factory.GetAccessor(
                            propertyName,
                            propertyType,
                            functionBody.Statements);
                        ITsGetAccessorMemberDeclaration getAccessorDeclaration = Factory.GetAccessorMemberDeclaration(
                            getAccessor,
                            accessibilityModifier,
                            isStatic);
                        yield return AddDocumentationComment(getAccessorDeclaration, node);
                        break;

                    case SyntaxKind.SetAccessorDeclaration:
                        ITsSetAccessor setAccessor = Factory.SetAccessor(
                            propertyName,
                            Factory.Identifier("value"),
                            propertyType,
                            functionBody.Statements);
                        ITsSetAccessorMemberDeclaration setAccessorDeclaration = Factory.SetAccessorMemberDeclaration(
                            setAccessor,
                            accessibilityModifier,
                            isStatic);
                        yield return AddDocumentationComment(setAccessorDeclaration, node);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown accessor kind '{accessor.Kind()}'");
                }
            }
        }

        /// <summary>
        /// Converts the translated declaration to an exported declaration if the C# declaration is public.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is returned.
        /// </returns>
        private ITsImplementationModuleElement ExportIfNeeded(
            ITsImplementationElement translatedDeclaration,
            BaseTypeDeclarationSyntax node)
        {
            // determine if this declaration should be exported
            INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return translatedDeclaration;
            }

            ITsExportImplementationElement exportedInterfaceDeclaration =
                Factory.ExportImplementationElement(translatedDeclaration);
            return exportedInterfaceDeclaration;
        }

        /// <summary>
        /// Calls <see cref="ExportIfNeeded"/> followed by <see cref="AddDocumentationComment{T}"/>.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is
        /// returned. Whichever element is returned, it includes any documentation comment.
        /// </returns>
        private ITsImplementationModuleElement ExportAndAddDocComment(
            ITsImplementationElement translatedDeclaration,
            BaseTypeDeclarationSyntax node)
        {
            var exportedDeclaration = ExportIfNeeded(translatedDeclaration, node);
            var withDocComment = AddDocumentationComment(exportedDeclaration, node);
            return withDocComment;
        }

        private ITsCallSignature TranslateCallSignature(
            ParameterListSyntax parameterListNode,
            TypeSyntax returnTypeNode = null)
        {
            ITsTypeParameters typeParameters = Factory.TypeParameters();

            ITsParameterList parameters = parameterListNode == null
                ? Factory.ParameterList()
                : (ITsParameterList)Visit(parameterListNode).Single();

            ITsType returnType = Factory.VoidType;
            if (returnTypeNode != null)
            {
                returnType = TypeTranslator.TranslateSymbol(
                    returnTypeNode.GetTypeSymbol(_semanticModel),
                    _typesToImport);
            }

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);
            return callSignature;
        }

        private TsAccessibilityModifier GetAccessibilityModifier(SyntaxNode node)
        {
            ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            return GetAccessibilityModifier(symbol, node.GetLocation);
        }

        private TsAccessibilityModifier GetAccessibilityModifier(ISymbol symbol, Func<Location> getLocationFunc)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    return TsAccessibilityModifier.Private;

                case Accessibility.Protected:
                    return TsAccessibilityModifier.Protected;

                case Accessibility.Public:
                    return TsAccessibilityModifier.Public;

                case Accessibility.NotApplicable:
                case Accessibility.Internal:
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                    _diagnostics.Add(
                        DiagnosticFactory.UnsupportedAccessibility(
                            symbol.DeclaredAccessibility.ToString(),
                            "public",
                            getLocationFunc()));
                    return TsAccessibilityModifier.Public;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
