// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Functions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal partial class TranslationVisitor
    {
        //// ===========================================================================================================
        //// Functions and Methods
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a InvocationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCallExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Expression).Single();
            var arguments = (ITsArgumentList)Visit(node.ArgumentList).First();

            // if the node's left side expression is a method or a constructor, then it will have
            // already been translated and the [InlineCode] would have already been applied - we
            // shouldn't do it twice because it will be wrong the second time.
            bool hasLeftSideAlreadyBeenTranslatedWithInlineCode = node.Expression.Kind()
                .IsOneOf(SyntaxKind.InvocationExpression, SyntaxKind.ObjectCreationExpression);

            // see if there's an [InlineCode] entry for the method invocation
            if (!hasLeftSideAlreadyBeenTranslatedWithInlineCode &&
                _inlineCodeTranslator.TryTranslate(
                    node.Expression,
                    leftSide,
                    arguments,
                    _diagnostics,
                    out ITsAstNode translatedNode))
            {
                yield return translatedNode;
            }
            else
            {
                ITsCallExpression translated = Factory.Call(leftSide, arguments);
                yield return translated;
            }
        }

        /// <summary>
        /// Called when the visitor visits a AnonymousMethodExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrowFunction"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            ITsCallSignature callSignature = TranslateCallSignature(node.ParameterList);
            var body = (ITsBlockStatement)Visit(node.Block).Single();
            ITsArrowFunction translated = Factory.ArrowFunction(callSignature, body.Statements.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a SimpleLambdaExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrowFunction"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            ITsIdentifier singleParameterName = Factory.Identifier(node.Parameter.Identifier.ValueText);
            var body = (ITsExpression)Visit(node.Body).Single();

            ITsArrowFunction translated = Factory.ArrowFunction(singleParameterName, body);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ParenthesizedLambdaExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrowFunction"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            ITsCallSignature callSignature = TranslateCallSignature(node.ParameterList);
            var body = Visit(node.Body).Single();

            ITsArrowFunction translated;
            switch (body)
            {
                case ITsExpression bodyExpression:
                    translated = Factory.ArrowFunction(callSignature, bodyExpression);
                    break;

                case ITsBlockStatement bodyBlock:
                    translated = Factory.ArrowFunction(callSignature, bodyBlock.Statements.ToArray());
                    break;

                default:
                    _diagnostics.Add(
                        DiagnosticFactory.InternalError(
                            $"Unknown lambda expression body type: {node.Body}",
                            node.Body.GetLocation()));
                    translated = Factory.ArrowFunction(callSignature, Factory.Identifier("TranslationError"));
                    break;
            }

            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ReturnStatementSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsReturnStatement"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitReturnStatement(ReturnStatementSyntax node)
        {
            ITsExpression expression = null;
            if (node.Expression != null)
            {
                expression = (ITsExpression)Visit(node.Expression).Single();
            }

            ITsReturnStatement translated = Factory.Return(expression);
            yield return translated;
        }

        //// ===========================================================================================================
        //// Parameter and Argument Lists
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a ParameterListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsParameterList"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitParameterList(ParameterListSyntax node)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter restParameter = null;

            foreach (ParameterSyntax parameterNode in node.Parameters)
            {
                ITsAstNode parameter = Visit(parameterNode).Single();
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
            return parameterList.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterSyntax node.
        /// </summary>
        /// <returns>Either a <see cref="ITsBoundRequiredParameter"/> or a <see cref="ITsBoundOptionalParameter"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitParameter(ParameterSyntax node)
        {
            ITsIdentifier parameterName = Factory.Identifier(node.Identifier.Text);
            ITypeSymbol parameterTypeSymbol = node.Type.GetTypeSymbol(_semanticModel);
            ITsType parameterType = _typeTranslator.TranslateSymbol(
                parameterTypeSymbol,
                _typesToImport,
                _diagnostics,
                () => node.Type.GetLocation());

            ITsAstNode parameter;

            if (node.Default == null)
            {
                parameter = Factory.BoundRequiredParameter(parameterName, parameterType);
            }
            else
            {
                var initializer = (ITsExpression)Visit(node.Default).Single();
                parameter = Factory.BoundOptionalParameter(parameterName, parameterType, initializer);
            }

            return parameter.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a TypeParameterListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTypeParameters"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeParameterList(TypeParameterListSyntax node)
        {
            var typeParameters = new List<ITsTypeParameter>();
            foreach (TypeParameterSyntax typeParameterNode in node.Parameters)
            {
                var typeParameter = (ITsTypeParameter)Visit(typeParameterNode).Single();
                typeParameters.Add(typeParameter);
            }

            ITsTypeParameters translated = Factory.TypeParameters(typeParameters.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TypeParameterSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTypeParameter"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeParameter(TypeParameterSyntax node)
        {
            ITsIdentifier typeName = Factory.Identifier(node.Identifier.Text);
            ITsTypeParameter translated = Factory.TypeParameter(typeName);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TypeArgumentListSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsType"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            var translated = from typeSyntax in node.Arguments
                             let typeSymbol = typeSyntax.GetTypeSymbol(_semanticModel)
                             where typeSymbol != null
                             select _typeTranslator.TranslateSymbol(
                                 typeSymbol,
                                 _typesToImport,
                                 _diagnostics,
                                 typeSyntax.GetLocation);
            return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ArgumentListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArgumentList"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitArgumentList(ArgumentListSyntax node)
        {
            ITsArgument[] arguments = node.Arguments.SelectMany(Visit).Cast<ITsArgument>().ToArray();
            ITsArgumentList translated = Factory.ArgumentList(arguments);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ArgumentSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArgument"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitArgument(ArgumentSyntax node)
        {
            var argumentExpression = (ITsExpression)Visit(node.Expression).Single();
            ITsArgument translated = Factory.Argument(argumentExpression);
            yield return translated;
        }
    }
}
