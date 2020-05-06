﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassTranslator.Visitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
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

    internal static partial class ClassTranslator
    {
        private sealed class Visitor : BaseTranslationVisitor<MemberDeclarationSyntax, IEnumerable<ITsClassElement>>
        {
            //// =======================================================================================================
            //// Member Variables
            //// =======================================================================================================

            public static readonly ITsIdentifier StaticCtorName = Factory.Identifier("__ctor");

            /// <summary>
            /// Keeps track of the auto-generated property names, keyed by the property symbol and containing the property name.
            /// </summary>
            private readonly IDictionary<IPropertySymbol, ITsIdentifier> _autoGeneratedPropertyNames =
                new Dictionary<IPropertySymbol, ITsIdentifier>(SymbolEqualityComparer.Default);

            //// =======================================================================================================
            //// Constructors
            //// =======================================================================================================

            public Visitor(TranslationContext context)
                : base(context)
            {
            }

            //// =======================================================================================================
            //// Properties
            //// =======================================================================================================

            /// <summary>
            /// Keeps track of additional variable declarations that need to happen in the class as a result of auto properties.
            /// </summary>
            public ICollection<ITsVariableMemberDeclaration> AutoGeneratedClassVariableDeclarations { get; } =
                new List<ITsVariableMemberDeclaration>();

            //// =======================================================================================================
            //// Methods
            //// =======================================================================================================

            public override IEnumerable<ITsClassElement> VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                var fieldDeclarations = new List<ITsVariableMemberDeclaration>();

                foreach (VariableDeclaratorSyntax variableDeclaration in node.Declaration.Variables)
                {
                    ISymbol symbol = ModelExtensions.GetDeclaredSymbol(SemanticModel, variableDeclaration);
                    ITsIdentifier variableName = symbol.GetScriptName(
                        ScriptSymbolTable,
                        variableDeclaration.Identifier.Text);
                    TsAccessibilityModifier accessibilityModifier =
                        Context.GetAccessibilityModifier(symbol, variableDeclaration.GetLocation);

                    bool isReadOnly = node.Modifiers.Any(
                        token => token.IsKind(SyntaxKind.ReadOnlyKeyword) || token.IsKind(SyntaxKind.ConstKeyword));

                    ITsType typeAnnotation = TypeTranslator.TranslateTypeSymbol(
                        Context,
                        node.Declaration.Type.GetTypeSymbol(SemanticModel),
                        node.Declaration.Type.GetLocation);

                    ITsExpression? initializer = null;
                    if (variableDeclaration.Initializer != null)
                    {
                        var initializerTranslation = ExpressionTranslator.Translate(
                            Context,
                            variableDeclaration.Initializer);

                        if (initializerTranslation.AdditionalStatementsRequiredBeforeExpression.Any())
                        {
                            Context.ReportInternalError(
                                "A field initializer cannot contain additional statements.",
                                variableDeclaration.Initializer);
                        }

                        initializer = initializerTranslation.Expression;
                    }

                    ITsVariableMemberDeclaration fieldDeclaration = Context.AddDocumentationComment(
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

            //// =======================================================================================================
            //// Methods and Constructors
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a MethodDeclarationSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="ITsFunctionMemberDeclaration"/> or an empty enumerable if the method declaration should be skipped.
            /// </returns>
            public override IEnumerable<ITsClassElement> VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                IMethodSymbol methodSymbol = SemanticModel.GetDeclaredSymbol(node);

                // If the method is decorated with [InlineCode] or [ScriptSkip] then we shouldn't output the declaration.
                if (ScriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) &&
                    (scriptMethodSymbol.InlineCode != null || scriptMethodSymbol.ScriptSkip))
                {
                    yield break;
                }

                ITsIdentifier functionName = Context.TranslateDeclarationIdentifier(node);

                // Create the call signature.
                ITsCallSignature callSignature = FunctionTranslator.TranslateCallSignature(
                    Context,
                    node.ParameterList,
                    node.TypeParameterList,
                    node.ReturnType,
                    methodSymbol);

                ITsBlockStatement? functionBody = TranslateFunctionBody(
                    node,
                    isVoidReturn: methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

                TsAccessibilityModifier accessibilityModifier = Context.GetAccessibilityModifier(node);
                ITsFunctionMemberDeclaration methodDeclaration = Factory.FunctionMemberDeclaration(
                    functionName,
                    callSignature,
                    accessibilityModifier,
                    methodSymbol.IsStatic,
                    methodSymbol.IsAbstract,
                    functionBody?.Statements);

                methodDeclaration = Context.AddDocumentationComment(methodDeclaration, node);
                yield return methodDeclaration;
            }

            /// <summary>
            /// Called when the visitor visits a ConstructorDeclarationSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="ITsConstructorDeclaration"/> or an <see
            /// cref="ITsFunctionMemberDeclaration"/> in the case of a static constructor.
            /// </returns>
            public override IEnumerable<ITsClassElement> VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                IMethodSymbol methodSymbol = SemanticModel.GetDeclaredSymbol(node);

                // If the method is decorated with [InlineCode] then we shouldn't output the declaration (for [ScriptSkip]
                // we still output the declaration, but won't output any invocations).
                if (ScriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) &&
                    scriptMethodSymbol.InlineCode != null)
                {
                    yield break;
                }

                TsAccessibilityModifier accessibilityModifier = Context.GetAccessibilityModifier(node);
                ITsParameterList parameterList = FunctionTranslator.TranslateParameterList(Context, node.ParameterList);

                ITsBlockStatement? functionBody = TranslateFunctionBody(node, isVoidReturn: true);

                ITsClassElement translated;
                if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    translated = Factory.FunctionMemberDeclaration(
                        StaticCtorName,
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

                translated = Context.AddDocumentationComment(translated, node);
                yield return translated;
            }

            private ITsBlockStatement? TranslateFunctionBody(BaseMethodDeclarationSyntax node, bool isVoidReturn)
            {
                ITsBlockStatement? functionBody = null;

                // A function body can be null in the case of an 'extern' declaration.
                if (node.Body != null)
                {
                    functionBody = StatementTranslator.TranslateBlockStatement(Context, node.Body);
                }
                // This is for arrow expressions of the form `method() => x`
                else if (node.ExpressionBody != null)
                {
                    functionBody = TranslateArrowExpressionClause(node.ExpressionBody, isVoidReturn);
                }

                return functionBody;
            }

            private ITsBlockStatement TranslateArrowExpressionClause(
                ArrowExpressionClauseSyntax node,
                bool isVoidReturn)
            {
                var bodyExpressionTranslation = ExpressionTranslator.Translate(Context, node.Expression);
                var bodyExpression = bodyExpressionTranslation.Expression;
                var bodyStatement = isVoidReturn
                    ? (ITsStatementListItem)bodyExpression.ToStatement()
                    : Factory.Return(bodyExpression);

                ITsBlockStatement functionBody = Factory.Block(
                    bodyExpressionTranslation.AdditionalStatementsRequiredBeforeExpression.Add(bodyStatement)
                        .ToArray());
                return functionBody;
            }

            //// =======================================================================================================
            //// Property Declarations
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a PropertyDeclarationSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerable{T}"/> of one or both of <see cref="ITsGetAccessorMemberDeclaration"/> for the get
            /// and <see cref="ITsSetAccessorMemberDeclaration"/> set methods.
            /// </returns>
            public override IEnumerable<ITsClassElement> VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                // If the property is marked with [IntrinsicProperty], don't write out the declaration.
                if (Context.GetExpectedDeclaredScriptSymbol<IScriptPropertySymbol>(node).IntrinsicProperty)
                {
                    yield break;
                }

                ITsIdentifier propertyName = Context.TranslateDeclarationIdentifier(node);
                ITypeSymbol typeSymbol = node.Type.GetTypeSymbol(SemanticModel);
                ITsType propertyType = TypeTranslator.TranslateTypeSymbol(Context, typeSymbol, node.Type.GetLocation);

                IPropertySymbol propertySymbol = SemanticModel.GetDeclaredSymbol(node);
                bool isStatic = propertySymbol.IsStatic;
                bool isAbstract = propertySymbol.IsAbstract;

                // Do a quick translation if the property has an arrow expression clause of the form: `Prop => value`. This
                // represents a get-only property with an explicit body.
                if (node.ExpressionBody != null)
                {
                    ITsBlockStatement functionBody = TranslateArrowExpressionClause(node.ExpressionBody, isVoidReturn: false);
                    TsAccessibilityModifier modifier = Context.GetAccessibilityModifier(
                        propertySymbol,
                        node.GetLocation);

                    yield return Factory.GetAccessorMemberDeclaration(
                        Factory.GetAccessor(propertyName, propertyType, functionBody.Statements),
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

                // If the properties differ in accessibility, use the most visible (public/private => public). TypeScript
                // doesn't support differing levels of accessibility for properties.
                var accessibilities = node.AccessorList.Accessors.Select(Context.GetAccessibilityModifier)
                    .Distinct()
                    .OrderBy(x => x, AccessibilityModifierComparer.MostVisibleToLeastVisible)
                    .ToArray();

                if (accessibilities.Length > 1)
                {
                    Diagnostics.Add(
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
                        functionBody = StatementTranslator.TranslateBlockStatement(Context, accessor.Body);
                    }
                    else if (accessor.ExpressionBody != null)
                    {
                        functionBody = TranslateArrowExpressionClause(accessor.ExpressionBody, isVoidReturn: !isGetter);
                    }
                    else
                    {
                        // If this is an auto-generated property (no body), we need to create a backing field and then
                        // create an accessor that gets or sets that field.

                        // We only need to create the backing field declaration once, so check to see if we already
                        // created it in a previous iteration (processing the get or set earlier).
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
                            AutoGeneratedClassVariableDeclarations.Add(variableMemberDeclaration);
                        }

                        // Create a field reference, which will either be `ClassName.field` or `this.field` depending on
                        // whether it's a static vs. instance field.
                        ITsMemberDotExpression fieldReference;
                        if (isStatic)
                        {
                            // In TypeScript, static references need to be fully qualified with the type name.
                            INamedTypeSymbol containingType = propertySymbol.ContainingType;
                            string containingTypeScriptName =
                                ScriptSymbolTable.GetComputedScriptNameOrDefault(containingType, containingType.Name);

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

                    accessorDeclaration = Context.AddDocumentationComment(accessorDeclaration, node);
                    yield return accessorDeclaration;
                }
            }

            //// =======================================================================================================
            //// Operator Overloads
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a OperatorDeclarationSyntax node.
            /// </summary>
            /// <returns>An <see cref="ITsFunctionMemberDeclaration"/>.</returns>
            public override IEnumerable<ITsClassElement> VisitOperatorDeclaration(OperatorDeclarationSyntax node)
            {
                // If the method is decorated with [InlineCode], then we don't need to declare it.
                if (Context.GetExpectedDeclaredScriptSymbol<IScriptMethodSymbol>(node).InlineCode != null)
                {
                    yield break;
                }

                var methodSymbol = Context.GetExpectedDeclaredSymbol<IMethodSymbol>(node);
                ITsIdentifier functionName = TranslateOperatorFunctionName(node);

                // Create the call signature.
                ITsCallSignature callSignature = FunctionTranslator.TranslateCallSignature(
                    Context,
                    node.ParameterList,
                    returnTypeNode: node.ReturnType,
                    methodSymbol: methodSymbol);

                ITsBlockStatement? functionBody = TranslateFunctionBody(
                    node,
                    methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

                TsAccessibilityModifier accessibilityModifier = Context.GetAccessibilityModifier(node);

                ITsFunctionMemberDeclaration translated = Factory.FunctionMemberDeclaration(
                    functionName,
                    callSignature,
                    accessibilityModifier,
                    methodSymbol.IsStatic,
                    methodSymbol.IsAbstract,
                    functionBody?.Statements);

                translated = Context.AddDocumentationComment(translated, node);
                yield return translated;
            }

            /// <summary>
            /// Called when the visitor visits a ConversionOperatorDeclarationSyntax node.
            /// </summary>
            /// <returns>An <see cref="ITsFunctionMemberDeclaration"/>.</returns>
            public override IEnumerable<ITsClassElement> VisitConversionOperatorDeclaration(
                ConversionOperatorDeclarationSyntax node)
            {
                // If the method is decorated with [InlineCode], then we don't need to declare it.
                if (Context.GetExpectedDeclaredScriptSymbol<IScriptMethodSymbol>(node).InlineCode != null)
                {
                    yield break;
                }

                var methodSymbol = Context.GetExpectedDeclaredSymbol<IMethodSymbol>(node);
                ITsIdentifier functionName = node.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ExplicitKeyword)
                    ? Factory.Identifier(Context.RenameRules.UserDefinedOperatorMethodNames[UserDefinedOperatorKind.Explicit])
                    : Factory.Identifier(Context.RenameRules.UserDefinedOperatorMethodNames[UserDefinedOperatorKind.Implicit]);

                // Create the call signature.
                ITsCallSignature callSignature = FunctionTranslator.TranslateCallSignature(
                    Context,
                    node.ParameterList,
                    returnTypeNode: node.Type,
                    methodSymbol: methodSymbol);

                ITsBlockStatement? functionBody = TranslateFunctionBody(
                    node,
                    methodSymbol.ReturnType.SpecialType == SpecialType.System_Void);

                TsAccessibilityModifier accessibilityModifier = Context.GetAccessibilityModifier(node);

                ITsFunctionMemberDeclaration translated = Factory.FunctionMemberDeclaration(
                    functionName,
                    callSignature,
                    accessibilityModifier,
                    methodSymbol.IsStatic,
                    methodSymbol.IsAbstract,
                    functionBody?.Statements);

                translated = Context.AddDocumentationComment(translated, node);
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
                    Diagnostics.Add(DiagnosticFactory.OperatorDeclarationNotSupported(node));
                    return Factory.Identifier("op_ERROR");
                }

                if (!Context.RenameRules.UserDefinedOperatorMethodNames.TryGetValue(
                    overloadKind.Value,
                    out string functionName))
                {
                    Context.ReportInternalError(
                        $"Operator overload function name not defined for {overloadKind}",
                        node);
                    return Factory.Identifier("op_ERROR");
                }

                return Factory.Identifier(functionName);
            }
        }
    }
}
