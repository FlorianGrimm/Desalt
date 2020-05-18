// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates function calls from C# to TypeScript.
    /// </summary>
    internal static class FunctionTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Translates a C# call signature into a TypeScript equivalent.
        /// </summary>
        /// <param name="context">The <see cref="TranslationContext"/> to use.</param>
        /// <param name="parameterListNode">The C# parameter list.</param>
        /// <param name="typeParameterListNode">The C# type parameter list.</param>
        /// <param name="returnTypeNode">The C# return type.</param>
        /// <param name="methodSymbol">The method symbol, which is used for [AlternateSignature] methods.</param>
        /// <returns></returns>
        public static ITsCallSignature TranslateCallSignature(
            TranslationContext context,
            ParameterListSyntax? parameterListNode,
            TypeParameterListSyntax? typeParameterListNode = null,
            TypeSyntax? returnTypeNode = null,
            IMethodSymbol? methodSymbol = null)
        {
            ITsTypeParameters typeParameters = typeParameterListNode == null
                ? Factory.TypeParameters()
                : TranslateTypeParameterList(typeParameterListNode);

            ITsParameterList parameters = parameterListNode == null
                ? Factory.ParameterList()
                : TranslateParameterList(context, parameterListNode);

            ITsType? returnType = null;
            ITypeSymbol? returnTypeSymbol = returnTypeNode != null
                ? context.GetExpectedTypeSymbol(returnTypeNode)
                : null;
            if (returnTypeNode != null && returnTypeSymbol != null)
            {
                returnType = TypeTranslator.TranslateTypeSymbol(context, returnTypeSymbol, returnTypeNode.GetLocation);
            }

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);

            // See if the parameter list should be adjusted to accomodate [AlternateSignature] methods.
            if (methodSymbol != null)
            {
                bool adjustedParameters = AlternateSignatureTranslator.TryAdjustParameterListTypes(
                    context,
                    methodSymbol,
                    callSignature.Parameters ?? Factory.ParameterList(),
                    out ITsParameterList translatedParameterList);

                if (adjustedParameters)
                {
                    callSignature = callSignature.WithParameters(translatedParameterList);
                }
            }

            return callSignature;
        }

        //// ===========================================================================================================
        //// Parameter and Argument Lists
        //// ===========================================================================================================

        public static ITsParameterList TranslateParameterList(TranslationContext context, ParameterListSyntax node)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter? restParameter = null;

            foreach (ParameterSyntax parameterNode in node.Parameters)
            {
                ITsAstNode parameter = TranslateParameter(context, parameterNode);
                if (parameter is ITsRequiredParameter requiredParameter)
                {
                    requiredParameters.Add(requiredParameter);
                }
                else if (parameter is ITsOptionalParameter optionalParameter)
                {
                    optionalParameters.Add(optionalParameter);
                }
                else if (parameter is ITsRestParameter restParam)
                {
                    restParameter = restParam;
                }
                else
                {
                    context.ReportInternalError(
                        $"Unknown translated parameter type: {parameter.GetType().Name}.",
                        parameterNode);
                }
            }

            ITsParameterList parameterList = Factory.ParameterList(
                requiredParameters,
                optionalParameters,
                restParameter);
            return parameterList;
        }

        /// <summary>
        /// Called when the visitor visits a ParameterSyntax node.
        /// </summary>
        /// <returns>
        /// One of the following: <see cref="ITsBoundRequiredParameter"/> for required parameters, <see
        /// cref="ITsBoundOptionalParameter"/> for optional parameters, or <see cref="ITsRestParameter"/> for a rest parameter.
        /// </returns>
        private static ITsAstNode TranslateParameter(TranslationContext context, ParameterSyntax node)
        {
            ITsIdentifier parameterName = Factory.Identifier(node.Identifier.Text);
            ITsType? parameterType;

            // Anonymous delegates don't always have a TypeSyntax, for example `(x, y) => ...`.
            if (node.Type == null)
            {
                parameterType = null;
            }
            else
            {
                ITypeSymbol parameterTypeSymbol = context.GetExpectedTypeSymbol(node.Type);
                parameterType = TypeTranslator.TranslateTypeSymbol(
                    context,
                    parameterTypeSymbol,
                    () => node.Type.GetLocation());
            }

            // Check for default parameter values.
            if (node.Default != null)
            {
                IExpressionTranslation initializer = ExpressionTranslator.Translate(context, node.Default.Value);
                if (initializer.AdditionalStatementsRequiredBeforeExpression.Any())
                {
                    context.ReportInternalError(
                        "A parameter must have a constant initializer, so cannot contain additional statements.",
                        node.Default);
                }

                ITsBoundOptionalParameter optionalParam = Factory.BoundOptionalParameter(
                    parameterName,
                    parameterType,
                    initializer.Expression);
                return optionalParam;
            }

            // For 'params' arguments with [ExpandParams], use a TypeScript 'rest' operator:
            // `Method(params int[] args)` => `method(...args: number[])`.
            bool isParamsArg = node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ParamsKeyword));
            if (isParamsArg)
            {
                // node.Parent is the ParameterSyntax node; node.Parent.Parent is the method/delegate/operator
                // declaration that contains the parameter list.
                SyntaxNode methodOrDelegateNode = node.Parent.Parent;
                ISymbol methodOrDelegateSymbol = context.SemanticModel.GetDeclaredSymbol(methodOrDelegateNode);
                bool expandParams = false;

                if (methodOrDelegateSymbol is INamedTypeSymbol delegateSymbol &&
                    delegateSymbol.DelegateInvokeMethod != null)
                {
                    expandParams = context.GetExpectedDeclaredScriptSymbol<IScriptDelegateSymbol>(methodOrDelegateNode)
                        .ExpandParams;
                }
                else if (methodOrDelegateSymbol is IMethodSymbol methodSymbol)
                {
                    // If this method implements an interface, we use the interface's [ExpandParams] (or lack of)
                    // instead of the method's.
                    IScriptMethodSymbol? scriptMethodSymbol =
                        context.GetExpectedDeclaredScriptSymbol<IScriptMethodSymbol>(
                            methodSymbol.TryFindInterfaceMethodOfImplementingMethod(
                                out IMethodSymbol? interfaceMethodSymbol)
                                ? interfaceMethodSymbol
                                : methodSymbol,
                            methodOrDelegateNode);

                    expandParams = scriptMethodSymbol?.ExpandParams == true;
                }
                else
                {
                    context.ReportInternalError(
                        $"Unknown symbol type '{methodOrDelegateSymbol.Kind}' for parameter node '{node}'.",
                        node);
                }

                if (expandParams)
                {
                    ITsRestParameter restParameter = Factory.RestParameter(parameterName, parameterType);
                    return restParameter;
                }
            }

            ITsBoundRequiredParameter requiredParam = Factory.BoundRequiredParameter(parameterName, parameterType);
            return requiredParam;
        }

        public static ITsTypeParameters TranslateTypeParameterList(TypeParameterListSyntax node)
        {
            ITsTypeParameter[] typeParameters = node.Parameters.Select(TranslateTypeParameter).ToArray();
            ITsTypeParameters translated = Factory.TypeParameters(typeParameters);
            return translated;
        }

        private static ITsTypeParameter TranslateTypeParameter(TypeParameterSyntax node)
        {
            ITsIdentifier typeName = Factory.Identifier(node.Identifier.Text);
            ITsTypeParameter translated = Factory.TypeParameter(typeName);
            return translated;
        }

        private static ITsType[] VisitTypeArgumentList(TranslationContext context, TypeArgumentListSyntax node)
        {
            var translated = from typeSyntax in node.Arguments
                             let typeSymbol = context.GetExpectedTypeSymbol(typeSyntax)
                             where typeSymbol != null
                             select TypeTranslator.TranslateTypeSymbol(context, typeSymbol, typeSyntax.GetLocation);
            return translated.ToArray();
        }
    }
}
