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
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Types;
    using Microsoft.CodeAnalysis;
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
            ITypeSymbol? returnTypeSymbol = returnTypeNode?.GetTypeSymbol(context.SemanticModel);
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
                    callSignature.Parameters,
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
                else
                {
                    optionalParameters.Add((ITsOptionalParameter)parameter);
                }
            }

            // ReSharper disable once ExpressionIsAlwaysNull
            ITsParameterList parameterList = Factory.ParameterList(
                requiredParameters,
                optionalParameters,
                restParameter);
            return parameterList;
        }

        /// <summary>
        /// Called when the visitor visits a ParameterSyntax node.
        /// </summary>
        /// <returns>Either a <see cref="ITsBoundRequiredParameter"/> or a <see cref="ITsBoundOptionalParameter"/>.</returns>
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
                ITypeSymbol parameterTypeSymbol = node.Type.GetTypeSymbol(context.SemanticModel);
                parameterType = TypeTranslator.TranslateTypeSymbol(
                    context,
                    parameterTypeSymbol,
                    () => node.Type.GetLocation());
            }

            ITsAstNode parameter;

            // Check for default parameter values.
            if (node.Default == null)
            {
                parameter = Factory.BoundRequiredParameter(parameterName, parameterType);
            }
            else
            {
                IExpressionTranslation initializer = ExpressionTranslator.Translate(context, node.Default.Value);
                if (initializer.AdditionalStatementsRequiredBeforeExpression.Any())
                {
                    context.ReportInternalError(
                        "A parameter must have a constant initializer, so cannot contain additional statements.",
                        node.Default);
                }

                parameter = Factory.BoundOptionalParameter(parameterName, parameterType, initializer.Expression);
            }

            return parameter;
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
                             let typeSymbol = typeSyntax.GetTypeSymbol(context.SemanticModel)
                             where typeSymbol != null
                             select TypeTranslator.TranslateTypeSymbol(context, typeSymbol, typeSyntax.GetLocation);
            return translated.ToArray();
        }
    }
}
