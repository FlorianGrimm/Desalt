// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
        /// <summary>
        /// Called when the visitor visits a NamespaceDeclarationSyntax node.
        /// </summary>
        /// <returns>A list of <see cref="ITsImplementationModuleElement"/> elements.</returns>
        public override IEnumerable<ITsAstNode> VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            return VisitMultipleOfType<ITsImplementationModuleElement>(node.Members);
        }

        /// <summary>
        /// Called when the visitor visits a InterfaceDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsImplementationModuleElement"/>, which is either an <see
        /// cref="ITsInterfaceDeclaration"/> or an <see cref="ITsExportImplementationElement"/>.
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = TranslateDeclarationIdentifier(node);

            // translate the interface body
            IList<ITsTypeMember> translatedMembers = VisitMultipleOfType<ITsTypeMember>(node.Members);
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
            yield return final;
        }

        /// <summary>
        /// Called when the visitor visits a EnumDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsImplementationModuleElement"/>, which is either an <see
        /// cref="ITsEnumDeclaration"/> or an <see cref="ITsExportImplementationElement"/>.
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            ITsIdentifier enumName = TranslateDeclarationIdentifier(node);

            // make the enum const if [NamedValues] is present
            bool isConst = Context.SemanticModel.GetDeclaredSymbol(node).GetFlagAttribute(SaltarelleAttributeName.NamedValues);

            // translate the enum body
            IList<ITsEnumMember> enumMembers = VisitMultipleOfType<ITsEnumMember>(node.Members);
            ITsEnumDeclaration enumDeclaration = Factory.EnumDeclaration(enumName, enumMembers, isConst);

            // export if necessary and add documentation comments
            ITsImplementationModuleElement final = ExportAndAddDocComment(enumDeclaration, node);
            yield return final;
        }

        /// <summary>
        /// Called when the visitor visits a EnumMemberDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<ITsAstNode> VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            ITsIdentifier scriptName = TranslateDeclarationIdentifier(node);

            // get the explicitly defined value if present
            ITsExpression? value = null;
            if (node.EqualsValue != null)
            {
                value = VisitSingleOfType<ITsExpression>(node.EqualsValue.Value);
            }

            // ignore the value if the enum is [NamedValues] and generate our own value
            bool isNamedValues = Context.SemanticModel.GetDeclaredSymbol(node.Parent)
                .GetFlagAttribute(SaltarelleAttributeName.NamedValues);
            if (isNamedValues)
            {
                IFieldSymbol fieldSymbol = Context.SemanticModel.GetDeclaredSymbol(node);
                string defaultFieldName = ScriptNamer.DetermineEnumFieldDefaultScriptName(fieldSymbol);
                value = Factory.String(defaultFieldName);
            }

            ITsEnumMember translatedMember = Factory.EnumMember(scriptName, value);
            yield return translatedMember;
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
        public override IEnumerable<ITsAstNode> VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ITsIdentifier className = TranslateDeclarationIdentifier(node);
            bool isAbstract = node.Modifiers.Any(SyntaxKind.AbstractKeyword);

            // translate the type parameters if there are any
            ITsTypeParameters? typeParameters = node.TypeParameterList == null
                ? null
                : VisitSingleOfType<ITsTypeParameters>(node.TypeParameterList);

            // translate the class heritage (extends and implements)
            ITsTypeReference? extendsClause = null;
            var implementsTypes = new List<ITsTypeReference>();
            if (node.BaseList != null)
            {
                var baseList = Visit(node.BaseList).Cast<ITsTypeReference>().ToImmutableArray();

                // get the type symbols so we can tell which ones are interfaces
                var typeSymbols = node.BaseList.Types
                    .Select(typeSyntax => typeSyntax.Type.GetTypeSymbol(Context.SemanticModel))
                    .ToImmutableArray();

                for (int i = 0; i < baseList.Length; i++)
                {
                    if (typeSymbols[i].IsInterfaceType())
                    {
                        implementsTypes.Add(baseList[i]);
                    }
                    else
                    {
                        if (extendsClause != null)
                        {
                            Context.Diagnostics.Add(
                                DiagnosticFactory.InternalError(
                                    "C# isn't supposed to support multiple inheritance! We already saw " +
                                    $"'{extendsClause.EmitAsString()}' but we have another type " +
                                    $"'{baseList[i].EmitAsString()}' that is claiming to be a base class.",
                                    node.BaseList.Types[i].GetLocation()));
                        }

                        extendsClause = baseList[i];
                    }
                }
            }

            ITsClassHeritage heritage = Factory.ClassHeritage(extendsClause, implementsTypes.ToArray());

            // translate the body
            var classBody = VisitMultipleOfType<ITsClassElement>(node.Members);

            // add any auto-generated class member variable declarations at the top of the class, for example when
            // adding auto-generated properties
            classBody.InsertRange(0, _autoGeneratedClassVariableDeclarations);

            ITsClassDeclaration classDeclaration = Factory.ClassDeclaration(
                className,
                typeParameters,
                heritage,
                isAbstract,
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
        /// Called when the visitor visits a BaseListSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsTypeReference"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBaseList(BaseListSyntax node)
        {
            return VisitMultipleOfType<ITsTypeReference>(node.Types);
        }

        /// <summary>
        /// Called when the visitor visits a SimpleBaseTypeSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTypeReference"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitSimpleBaseType(SimpleBaseTypeSyntax node)
        {
            ITypeSymbol typeSymbol = node.Type.GetTypeSymbol(Context.SemanticModel);
            var translated = (ITsTypeReference)TypeTranslator.TranslateTypeSymbol(
                Context,
                typeSymbol,
                node.Type.GetLocation);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a FieldDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<ITsAstNode> VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var fieldDeclarations = new List<ITsVariableMemberDeclaration>();
            foreach (VariableDeclaratorSyntax variableDeclaration in node.Declaration.Variables)
            {
                ISymbol symbol = Context.SemanticModel.GetDeclaredSymbol(variableDeclaration);
                ITsIdentifier variableName = symbol.GetScriptName(Context.ScriptSymbolTable, variableDeclaration.Identifier.Text);
                TsAccessibilityModifier accessibilityModifier =
                    GetAccessibilityModifier(symbol, variableDeclaration.GetLocation);

                bool isReadOnly = node.Modifiers.Any(
                    token => token.IsKind(SyntaxKind.ReadOnlyKeyword) || token.IsKind(SyntaxKind.ConstKeyword));

                ITsType typeAnnotation = TypeTranslator.TranslateTypeSymbol(
                    Context,
                    node.Declaration.Type.GetTypeSymbol(Context.SemanticModel),
                    node.Declaration.Type.GetLocation);

                ITsExpression? initializer = null;
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

        //// ===========================================================================================================
        //// Methods and Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsMethodSignature"/> if we're within an interface declaration; otherwise
        /// an <see cref="ITsFunctionMemberDeclaration"/>.
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            IMethodSymbol methodSymbol = Context.SemanticModel.GetDeclaredSymbol(node);

            // If the method is decorated with [InlineCode] or [ScriptSkip] then we shouldn't output the declaration.
            if (Context.ScriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) &&
                (scriptMethodSymbol.InlineCode != null || scriptMethodSymbol.ScriptSkip))
            {
                yield break;
            }

            ITsIdentifier functionName = TranslateDeclarationIdentifier(node);

            // Create the call signature.
            ITsCallSignature callSignature = TranslateCallSignature(
                node.ParameterList,
                node.TypeParameterList,
                node.ReturnType,
                methodSymbol);

            // If we're defining an interface, then we need to return a ITsMethodSignature.
            if (methodSymbol.ContainingType.IsInterfaceType())
            {
                ITsMethodSignature methodSignature = Factory.MethodSignature(
                    functionName,
                    isOptional: false,
                    callSignature: callSignature);
                methodSignature = AddDocumentationComment(methodSignature, node);
                yield return methodSignature;
                yield break;
            }

            ITsBlockStatement? functionBody = TranslateFunctionBody(
                node,
                methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

            // We're within a class, so return a method declaration.
            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);
            ITsFunctionMemberDeclaration methodDeclaration = Factory.FunctionMemberDeclaration(
                functionName,
                callSignature,
                accessibilityModifier,
                methodSymbol.IsStatic,
                methodSymbol.IsAbstract,
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
        public override IEnumerable<ITsAstNode> VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            IMethodSymbol methodSymbol = Context.SemanticModel.GetDeclaredSymbol(node);

            // If the method is decorated with [InlineCode] then we shouldn't output the declaration (for [ScriptSkip]
            // we still output the declaration, but won't output any invocations).
            if (Context.ScriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) &&
                scriptMethodSymbol.InlineCode != null)
            {
                yield break;
            }

            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);
            var parameterList = VisitSingleOfType<ITsParameterList>(node.ParameterList);

            ITsBlockStatement? functionBody = TranslateFunctionBody(node, isVoidReturn: true);

            ITsClassElement translated;
            if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                translated = Factory.FunctionMemberDeclaration(
                    s_staticCtorName,
                    Factory.CallSignature(),
                    TsAccessibilityModifier.Public,
                    isStatic: true,
                    functionBody: functionBody?.Statements);

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
                    functionBody?.Statements);
            }

            translated = AddDocumentationComment(translated, node);
            yield return translated;
        }

        private ITsBlockStatement? TranslateFunctionBody(BaseMethodDeclarationSyntax node, bool isVoidReturn)
        {
            ITsBlockStatement? functionBody = null;

            // A function body can be null in the case of an 'extern' declaration.
            if (node.Body != null)
            {
                functionBody = VisitSingleOfType<ITsBlockStatement>(node.Body);
            }
            // This is for arrow expressions of the form `method() => x`
            else if (node.ExpressionBody != null)
            {
                var bodyExpression = VisitSingleOfType<ITsExpression>(node.ExpressionBody);
                functionBody = isVoidReturn ? bodyExpression.ToBlock() : Factory.Return(bodyExpression).ToBlock();
            }

            return functionBody;
        }

        //// ===========================================================================================================
        //// Property Declarations
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a PropertyDeclarationSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of one or both of <see cref="ITsGetAccessorMemberDeclaration"/> for the get
        /// and <see cref="ITsSetAccessorMemberDeclaration"/> set methods (for classes) or a <see
        /// cref="ITsPropertySignature"/> for the get and set methods (for interfaces).
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            ITsIdentifier propertyName = TranslateDeclarationIdentifier(node);
            ITypeSymbol typeSymbol = node.Type.GetTypeSymbol(Context.SemanticModel);
            ITsType propertyType = TypeTranslator.TranslateTypeSymbol(
                Context,
                typeSymbol,
                node.Type.GetLocation);
            IPropertySymbol propertySymbol = Context.SemanticModel.GetDeclaredSymbol(node);

            // If the property is marked with [IntrinsicProperty], don't write out the declaration.
            if (!Context.ScriptSymbolTable.TryGetValue(propertySymbol, out ScriptPropertySymbol? scriptPropertySymbol))
            {
                Context.ReportInternalError($"We should have a script symbol for property '{node.Identifier.Text}'", node);
            }

            if (scriptPropertySymbol!.IntrinsicProperty)
            {
                yield break;
            }

            bool isStatic = propertySymbol.IsStatic;
            bool isAbstract = propertySymbol.IsAbstract;
            bool isWithinInterface = propertySymbol.ContainingType.IsInterfaceType();

            // Do a quick translation if the property has an arrow expression clause of the form: `Prop => value`. This
            // represents a get-only property with an explicit body.
            if (node.ExpressionBody != null)
            {
                var bodyExpression = (ITsExpression)Visit(node.ExpressionBody).Single();
                TsAccessibilityModifier modifier = GetAccessibilityModifier(propertySymbol, node.GetLocation);
                yield return Factory.GetAccessorMemberDeclaration(
                    Factory.GetAccessor(propertyName, propertyType, Factory.Return(bodyExpression)),
                    modifier,
                    isStatic,
                    isAbstract);
                yield break;
            }

            if (node.AccessorList == null)
            {
                Context.ReportInternalError(
                    "A property declaration with a null accessor list should have an expression body.",
                    node);

                yield break;
            }

            // Property declarations within interfaces don't have a body and are a different translation type
            // (ITsPropertySignature). We only need to create a single property signature for both the getter and setter.
            if (isWithinInterface)
            {
                bool hasSetter = node.AccessorList.Accessors.Any(x => x.Kind() == SyntaxKind.SetAccessorDeclaration);
                ITsPropertySignature propertySignature = Factory.PropertySignature(
                    propertyName,
                    propertyType,
                    isReadOnly: !hasSetter);
                yield return propertySignature;
                yield break;
            }

            // If the properties differ in accessibility, use the most visible (public/private => public). TypeScript
            // doesn't support differing levels of accessibility for properties.
            var accessibilities = node.AccessorList.Accessors.Select(GetAccessibilityModifier)
                .Distinct()
                .OrderBy(x => x, AccessibilityModifierComparer.MostVisibleToLeastVisible)
                .ToArray();

            if (accessibilities.Length > 1)
            {
                Context.Diagnostics.Add(
                    DiagnosticFactory.GetterAndSetterAccessorsDoNotAgreeInVisibility(
                        node.Identifier.Text,
                        node.GetLocation()));
            }

            TsAccessibilityModifier accessibilityModifier = accessibilities[0];

            foreach (AccessorDeclarationSyntax accessor in node.AccessorList.Accessors)
            {
                bool isGetter = accessor.Kind() == SyntaxKind.GetAccessorDeclaration;
                ITsBlockStatement functionBody;

                if (accessor.Body != null)
                {
                    functionBody = VisitSingleOfType<ITsBlockStatement>(accessor.Body);
                }
                else if (accessor.ExpressionBody != null)
                {
                    var bodyExpression = VisitExpression(accessor.ExpressionBody);
                    functionBody = isGetter ? Factory.Return(bodyExpression).ToBlock() : bodyExpression.ToBlock();
                }
                else
                {
                    // If this is an auto-generated property (no body), we need to create a backing field and then create an
                    // accessor that gets or sets that field.

                    // We only need to create the backing field declaration once, so check to see if we already created
                    // it in a previous iteration (processing the get or set earlier).
                    if (!_autoGeneratedPropertyNames.TryGetValue(
                        propertySymbol,
                        out ITsIdentifier? backingFieldIdentifier))
                    {
                        // Add the backing field to the dictionary so we don't add two field declarations.
                        backingFieldIdentifier = Factory.Identifier($"_${propertyName}Field");
                        _autoGeneratedPropertyNames.Add(propertySymbol, backingFieldIdentifier);

                        ITsVariableMemberDeclaration variableMemberDeclaration = Factory.VariableMemberDeclaration(
                            backingFieldIdentifier,
                            TsAccessibilityModifier.Private,
                            isStatic,
                            isReadOnly: false,
                            propertyType);

                        // Adding to this collection will add a declaration to the generated class declaration (see VisitClassDeclaration).
                        _autoGeneratedClassVariableDeclarations.Add(variableMemberDeclaration);
                    }

                    // Create a field reference, which will either be `ClassName.field` or `this.field` depending on
                    // whether it's a static vs. instance field.
                    ITsMemberDotExpression fieldReference;
                    if (isStatic)
                    {
                        // In TypeScript, static references need to be fully qualified with the type name.
                        INamedTypeSymbol containingType = propertySymbol.ContainingType;
                        string containingTypeScriptName =
                            Context.ScriptSymbolTable.GetComputedScriptNameOrDefault(containingType, containingType.Name);

                        fieldReference = Factory.MemberDot(
                            Factory.Identifier(containingTypeScriptName),
                            backingFieldIdentifier.Text);
                    }
                    else
                    {
                        fieldReference = Factory.MemberDot(Factory.This, backingFieldIdentifier.Text);
                    }

                    // Create the accessor body, which is just a return statement for get and an assignment for set.
                    ITsStatement accessorStatement = accessor.Kind() == SyntaxKind.GetAccessorDeclaration
                        ? (ITsStatement)Factory.Return(fieldReference)
                        : Factory.Assignment(fieldReference, TsAssignmentOperator.SimpleAssign, Factory.Identifier("value"))
                            .ToStatement();
                    functionBody = Factory.Block(accessorStatement);
                }

                // Create the get/set accessor declaration.
                ITsClassElement accessorDeclaration = isGetter
                    ? (ITsClassElement)Factory.GetAccessorMemberDeclaration(
                        Factory.GetAccessor(propertyName, propertyType, functionBody.Statements),
                        accessibilityModifier,
                        isStatic,
                        isAbstract)
                    : Factory.SetAccessorMemberDeclaration(
                        Factory.SetAccessor(propertyName, Factory.Identifier("value"), propertyType, functionBody.Statements),
                        accessibilityModifier,
                        isStatic,
                        isAbstract);

                accessorDeclaration = AddDocumentationComment(accessorDeclaration, node);
                yield return accessorDeclaration;
            }
        }

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a OperatorDeclarationSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsFunctionMemberDeclaration"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            (IMethodSymbol methodSymbol, IScriptMethodSymbol scriptMethodSymbol) =
                GetExpectedDeclaredScriptSymbol<IMethodSymbol, IScriptMethodSymbol>(node);

            // If the method is decorated with [InlineCode], then we don't need to declare it.
            if (scriptMethodSymbol.InlineCode != null)
            {
                yield break;
            }

            ITsIdentifier functionName = TranslateOperatorFunctionName(node);

            // Create the call signature.
            ITsCallSignature callSignature = TranslateCallSignature(
                node.ParameterList,
                returnTypeNode: node.ReturnType,
                methodSymbol: methodSymbol);

            ITsBlockStatement? functionBody = TranslateFunctionBody(
                node,
                methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);

            ITsFunctionMemberDeclaration translated = Factory.FunctionMemberDeclaration(
                functionName,
                callSignature,
                accessibilityModifier,
                methodSymbol.IsStatic,
                methodSymbol.IsAbstract,
                functionBody?.Statements);

            translated = AddDocumentationComment(translated, node);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ConversionOperatorDeclarationSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsFunctionMemberDeclaration"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            (IMethodSymbol methodSymbol, IScriptMethodSymbol scriptMethodSymbol) =
                GetExpectedDeclaredScriptSymbol<IMethodSymbol, IScriptMethodSymbol>(node);

            // If the method is decorated with [InlineCode], then we don't need to declare it.
            if (scriptMethodSymbol.InlineCode != null)
            {
                yield break;
            }

            ITsIdentifier functionName = node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ExplicitKeyword)
                ? Factory.Identifier(Context.RenameRules.UserDefinedOperatorMethodNames[UserDefinedOperatorKind.Explicit])
                : Factory.Identifier(Context.RenameRules.UserDefinedOperatorMethodNames[UserDefinedOperatorKind.Implicit]);

            // Create the call signature.
            ITsCallSignature callSignature = TranslateCallSignature(
                node.ParameterList,
                returnTypeNode: node.Type,
                methodSymbol: methodSymbol);

            ITsBlockStatement? functionBody = TranslateFunctionBody(
                node,
                methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

            TsAccessibilityModifier accessibilityModifier = GetAccessibilityModifier(node);

            ITsFunctionMemberDeclaration translated = Factory.FunctionMemberDeclaration(
                functionName,
                callSignature,
                accessibilityModifier,
                methodSymbol.IsStatic,
                methodSymbol.IsAbstract,
                functionBody?.Statements);

            translated = AddDocumentationComment(translated, node);
            yield return translated;
        }

        /// <summary>
        /// Translates an operator declaration function name.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ITsIdentifier TranslateOperatorFunctionName(OperatorDeclarationSyntax node)
        {
            bool isUnary = node.ParameterList.Parameters.Count == 1;
            UserDefinedOperatorKind? overloadKind = node.OperatorToken.Kind() switch
            {
                SyntaxKind.PlusToken when isUnary => UserDefinedOperatorKind.UnaryPlus,
                SyntaxKind.MinusToken when isUnary => UserDefinedOperatorKind.UnaryNegation,
                SyntaxKind.ExclamationToken => UserDefinedOperatorKind.LogicalNot,
                SyntaxKind.TildeToken => UserDefinedOperatorKind.OnesComplement,
                SyntaxKind.PlusPlusToken => UserDefinedOperatorKind.Increment,
                SyntaxKind.MinusMinusToken => UserDefinedOperatorKind.Decrement,
                SyntaxKind.TrueKeyword => UserDefinedOperatorKind.True,
                SyntaxKind.FalseKeyword => UserDefinedOperatorKind.False,
                SyntaxKind.PlusToken => UserDefinedOperatorKind.Addition,
                SyntaxKind.MinusToken => UserDefinedOperatorKind.Subtraction,
                SyntaxKind.AsteriskToken => UserDefinedOperatorKind.Multiplication,
                SyntaxKind.SlashToken => UserDefinedOperatorKind.Division,
                SyntaxKind.PercentToken => UserDefinedOperatorKind.Modulus,
                SyntaxKind.AmpersandToken => UserDefinedOperatorKind.BitwiseAnd,
                SyntaxKind.BarToken => UserDefinedOperatorKind.BitwiseOr,
                SyntaxKind.CaretToken => UserDefinedOperatorKind.ExclusiveOr,
                SyntaxKind.LessThanLessThanToken => UserDefinedOperatorKind.LeftShift,
                SyntaxKind.GreaterThanGreaterThanToken => UserDefinedOperatorKind.RightShift,
                SyntaxKind.EqualsEqualsToken => UserDefinedOperatorKind.Equality,
                SyntaxKind.ExclamationEqualsToken => UserDefinedOperatorKind.Inequality,
                SyntaxKind.LessThanToken => UserDefinedOperatorKind.LessThan,
                SyntaxKind.LessThanEqualsToken => UserDefinedOperatorKind.LessThanEquals,
                SyntaxKind.GreaterThanToken => UserDefinedOperatorKind.GreaterThan,
                SyntaxKind.GreaterThanEqualsToken => UserDefinedOperatorKind.GreaterThanEquals,
                _ => null,
            };

            if (overloadKind == null)
            {
                Context.Diagnostics.Add(DiagnosticFactory.OperatorDeclarationNotSupported(node));
                return Factory.Identifier("op_ERROR");
            }

            if (!Context.RenameRules.UserDefinedOperatorMethodNames.TryGetValue(overloadKind.Value, out string functionName))
            {
                Context.ReportInternalError($"Operator overload function name not defined for {overloadKind}", node);
                return Factory.Identifier("op_ERROR");
            }

            return Factory.Identifier(functionName);
        }
    }
}
