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
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.TypeScript.Ast.Types;
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
        /// cref="ITsClassElement"/> or an <see cref="ITsExportImplementationElement"/>. If there is
        /// a static constructor, an additional function call ( <see cref="ITsCallExpression"/>) to
        /// the initializer is added immediately after the class declaration.
        /// </returns>
        public override IEnumerable<IAstNode> VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ITsIdentifier className = TranslateDeclarationIdentifier(node);

            // translate the generic type parameters
            ITsTypeParameters typeParameters = Factory.TypeParameters();

            // translate the class heritage
            ITsClassHeritage heritage = Factory.ClassHeritage(implementsTypes: null);

            // translate the class body
            var classBody = node.Members.SelectMany(Visit).Cast<ITsClassElement>();

            ITsClassDeclaration classDeclaration = Factory.ClassDeclaration(
                className,
                typeParameters,
                heritage,
                classBody);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final = ExportAndAddDocComment(classDeclaration, node);
            yield return final;

            // if there's a static constructor, then we need to add a call to it right after the
            // class declaration
            if (classDeclaration.ClassBody.OfType<ITsFunctionMemberDeclaration>()
                .Any(method => method.FunctionName == s_staticCtorName))
            {
                ITsCallExpression staticCtorCall = Factory.Call(Factory.MemberDot(className, s_staticCtorName.Text))
                    .WithLeadingTrivia(Factory.SingleLineComment("Call the static constructor"));
                yield return Factory.ExpressionStatement(staticCtorCall);
            }
        }

        /// <summary>
        /// Called when the visitor visits a FieldDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var fieldDeclarations = new List<ITsVariableMemberDeclaration>();
            foreach (VariableDeclaratorSyntax variableDeclaration in node.Declaration.Variables)
            {
                ISymbol symbol = _semanticModel.GetDeclaredSymbol(variableDeclaration);
                var variableName = symbol.GetScriptName(_scriptNameTable, variableDeclaration.Identifier.Text);
                TsAccessibilityModifier accessibilityModifier =
                    GetAccessibilityModifier(symbol, variableDeclaration.GetLocation);

                bool isReadOnly = node.Modifiers.Any(
                    token => token.IsKind(SyntaxKind.ReadOnlyKeyword) || token.IsKind(SyntaxKind.ConstKeyword));

                var typeAnnotation = _typeTranslator.TranslateSymbol(
                    node.Declaration.Type.GetTypeSymbol(_semanticModel),
                    _typesToImport,
                    _diagnostics);

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
            IMethodSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            ITsIdentifier functionName = TranslateDeclarationIdentifier(node);

            // create the call signature
            ITsCallSignature callSignature = TranslateCallSignature(
                node.ParameterList,
                node.TypeParameterList,
                node.ReturnType);

            // see if the parameter list should be adjusted to accomodate[AlternateSignature] methods
            bool adjustedParameters = _alternateSignatureTranslator.TryAdjustParameterListTypes(
                symbol,
                callSignature.Parameters,
                out ITsParameterList translatedParameterList,
                out IEnumerable<Diagnostic> diagnostics);

            _diagnostics.AddRange(diagnostics);

            if (adjustedParameters)
            {
                callSignature = callSignature.WithParameters(translatedParameterList);
            }

            // if we're defining an interface, then we need to return a ITsMethodSignature
            if (symbol.ContainingType.IsInterfaceType())
            {
                ITsMethodSignature methodSignature = Factory.MethodSignature(
                    functionName,
                    isOptional: false,
                    callSignature: callSignature);
                methodSignature = AddDocumentationComment(methodSignature, node);
                yield return methodSignature;
                yield break;
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
            yield return methodDeclaration;
        }

        /// <summary>
        /// Called when the visitor visits a ConstructorDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsConstructorDeclaration"/> or an <see
        /// cref="ITsFunctionMemberDeclaration"/> in the case of a static constructor.
        /// </returns>
        public override IEnumerable<IAstNode> VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);
            var parameterList = (ITsParameterList)Visit(node.ParameterList).Single();
            var functionBody = (ITsBlockStatement)Visit(node.Body).Single();

            ITsClassElement translated;
            if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                translated = Factory.FunctionMemberDeclaration(
                    s_staticCtorName,
                    Factory.CallSignature(),
                    TsAccessibilityModifier.Public,
                    isStatic: true,
                    functionBody: functionBody.Statements);

                translated = translated.WithLeadingTrivia(
                    Factory.SingleLineComment(
                        "Converted from the C# static constructor - it would be good to convert this"),
                    Factory.SingleLineComment("block to inline initializations."));
            }
            else
            {
                translated = Factory.ConstructorDeclaration(
                    accessibilityModifier,
                    parameterList,
                    functionBody.Statements);
            }

            translated = AddDocumentationComment(translated, node);
            yield return translated;
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
            var propertyType = _typeTranslator.TranslateSymbol(typeSymbol, _typesToImport, _diagnostics);
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
    }
}
