// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Expressions.cs" company="Justin Rockwood">
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
        /// Called when the visitor visits a ThisExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsThis"/>.</returns>
        public override IEnumerable<IAstNode> VisitThisExpression(ThisExpressionSyntax node)
        {
            return Factory.This.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a LiteralExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    return Factory.String(node.Token.Text.Trim('"')).ToSingleEnumerable();

                case SyntaxKind.CharacterLiteralExpression:
                    return Factory.String(node.Token.Text).ToSingleEnumerable();

                case SyntaxKind.NumericLiteralExpression:
                    return node.Token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? Factory.HexInteger(Convert.ToInt64(node.Token.Value)).ToSingleEnumerable()
                        : Factory.Number(Convert.ToDouble(node.Token.Value)).ToSingleEnumerable();
            }

            var diagnostic = DiagnosticFactory.LiteralExpressionTranslationNotSupported(node);
            return ReportUnsupportedTranslataion(diagnostic);
        }

        /// <summary>
        /// Called when the visitor visits a CastExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCastExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitCastExpression(CastExpressionSyntax node)
        {
            ITsType castType = TypeTranslator.TranslateSymbol(node.Type.GetTypeSymbol(_semanticModel), _typesToImport);
            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsCastExpression translated = Factory.Cast(castType, expression);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a TypeOfExpressionSyntax node.
        /// </summary>
        /// <remarks>An <see cref="ITsIdentifier"/>.</remarks>
        public override IEnumerable<IAstNode> VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            ITsType type = TypeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_semanticModel),
                _typesToImport);

            ITsIdentifier translated = Factory.Identifier(type.EmitAsString());
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a IdentifierNameSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIdentifier"/>.</returns>
        public override IEnumerable<IAstNode> VisitIdentifierName(IdentifierNameSyntax node)
        {
            return Factory.Identifier(node.Identifier.Text).ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a GenericNameSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsGenericTypeName"/>.</returns>
        public override IEnumerable<IAstNode> VisitGenericName(GenericNameSyntax node)
        {
            ITsType[] typeArguments = node.TypeArgumentList.Arguments
                .Select(typeSyntax => typeSyntax.GetTypeSymbol(_semanticModel))
                .Where(typeSymbol => typeSymbol != null)
                .Select(typeSymbol => TypeTranslator.TranslateSymbol(typeSymbol, _typesToImport))
                .ToArray();

            ITsGenericTypeName translated = Factory.GenericTypeName(node.Identifier.Text, typeArguments);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a EqualsValueClauseSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return Visit(node.Value);
        }

        /// <summary>
        /// Called when the visitor visits a InvocationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCallExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ITsExpression leftSide = TranslateExpressionWithScriptName(node.Expression);
            var arguments = (ITsArgumentList)Visit(node.ArgumentList).First();
            ITsCallExpression translated = Factory.Call(leftSide, arguments);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ObjectCreationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsNewCallExpression"/>.</returns>
        public override IEnumerable<IAstNode> VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Type).Single();
            var arguments = (ITsArgumentList)Visit(node.ArgumentList).First();
            ITsNewCallExpression translated = Factory.NewCall(leftSide, arguments);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ArgumentListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArgumentList"/>.</returns>
        public override IEnumerable<IAstNode> VisitArgumentList(ArgumentListSyntax node)
        {
            ITsArgument[] arguments = node.Arguments.SelectMany(Visit).Cast<ITsArgument>().ToArray();
            ITsArgumentList translated = Factory.ArgumentList(arguments);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ArgumentSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArgument"/>.</returns>
        public override IEnumerable<IAstNode> VisitArgument(ArgumentSyntax node)
        {
            var argumentExpression = (ITsExpression)Visit(node.Expression).Single();
            ITsArgument translated = Factory.Argument(argumentExpression);
            return translated.ToSingleEnumerable();
        }

        /// <summary>
        /// Translates the specified expression node using translated script names.
        /// </summary>
        private ITsExpression TranslateExpressionWithScriptName(ExpressionSyntax node)
        {
            ITsExpression expression;

            // try to get the script name of the expression
            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol != null && _scriptNameTable.TryGetValue(symbol, out string scriptName))
            {
                // in TypeScript, static references need to be fully qualified with the type name
                if (symbol.IsStatic && symbol.ContainingType != null)
                {
                    string containingTypeScriptName = _scriptNameTable.GetValueOrDefault(
                        symbol.ContainingType,
                        symbol.ContainingType.Name);
                    expression = Factory.MemberDot(Factory.Identifier(containingTypeScriptName), scriptName);
                }
                else if (!symbol.IsStatic)
                {
                    expression = Factory.MemberDot(Factory.This, scriptName);
                }
                else
                {
                    expression = Factory.Identifier(scriptName);
                }
            }
            else
            {
                expression = (ITsExpression)Visit(node).Single();
            }

            return expression;
        }
    }
}
