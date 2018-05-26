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
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TypeScriptAst.Ast;
    using TypeScriptAst.Ast.Expressions;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal sealed partial class TranslationVisitor
    {
        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a ThisExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsThis"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitThisExpression(ThisExpressionSyntax node)
        {
            yield return Factory.This;
        }

        /// <summary>
        /// Called when the visitor visits a LiteralExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    // use the raw text since C# strings are escaped the same as JavaScript strings
                    string str = node.Token.Text;
                    bool isVerbatim = str.StartsWith("@", StringComparison.Ordinal);

                    // trim the leading @ and surrounding quotes
                    str = isVerbatim ? str.Substring(1) : str;
                    str = str.StartsWith("\"", StringComparison.Ordinal) ? str.Substring(1) : str;
                    str = str.EndsWith("\"", StringComparison.Ordinal) ? str.Substring(0, str.Length - 1) : str;

                    // for verbatim strings, we need to add the escape characters back in
                    if (isVerbatim)
                    {
                        str = str.Replace(@"\", @"\\").Replace("\"\"", @"\""");
                    }

                    return Factory.String(str).ToSingleEnumerable();

                case SyntaxKind.CharacterLiteralExpression:
                    return Factory.String(node.Token.ValueText).ToSingleEnumerable();

                case SyntaxKind.NumericLiteralExpression:
                    return node.Token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? Factory.HexInteger(Convert.ToInt64(node.Token.Value)).ToSingleEnumerable()
                        : Factory.Number(Convert.ToDouble(node.Token.Value)).ToSingleEnumerable();

                case SyntaxKind.TrueLiteralExpression:
                    return Factory.True.ToSingleEnumerable();

                case SyntaxKind.FalseLiteralExpression:
                    return Factory.False.ToSingleEnumerable();

                case SyntaxKind.NullLiteralExpression:
                    return Factory.Null.ToSingleEnumerable();
            }

            var diagnostic = DiagnosticFactory.LiteralExpressionTranslationNotSupported(node);
            ReportUnsupportedTranslataion(diagnostic);
            return Enumerable.Empty<ITsAstNode>();
        }

        //// ===========================================================================================================
        //// Parenthesized, Cast, and TypeOf Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a ParenthesizedExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsParenthesizedExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsParenthesizedExpression translated = Factory.ParenthesizedExpression(expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a CastExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsCastExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitCastExpression(CastExpressionSyntax node)
        {
            ITsType castType = _typeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_semanticModel),
                _typesToImport,
                _diagnostics,
                node.Type.GetLocation);

            var expression = (ITsExpression)Visit(node.Expression).Single();
            ITsCastExpression translated = Factory.Cast(castType, expression);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TypeOfExpressionSyntax node.
        /// </summary>
        /// <remarks>An <see cref="ITsIdentifier"/>.</remarks>
        public override IEnumerable<ITsAstNode> VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            ITsType type = _typeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_semanticModel),
                _typesToImport,
                _diagnostics,
                node.Type.GetLocation);

            ITsIdentifier translated = Factory.Identifier(type.EmitAsString());
            yield return translated;
        }

        //// ===========================================================================================================
        //// Identifiers and Member Names
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a PredefinedTypeSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIdentifier"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitPredefinedType(PredefinedTypeSyntax node)
        {
            // try to get the script name of the expression
            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // if there's no symbol then just return an identifier
            if (symbol == null || !_scriptNameTable.TryGetValue(symbol, out string scriptName))
            {
                yield return Factory.Identifier(node.Keyword.Text);
            }
            else
            {
                yield return Factory.Identifier(scriptName);
            }
        }

        /// <summary>
        /// Called when the visitor visits a IdentifierNameSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIdentifier"/> or <see cref="ITsMemberDotExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitIdentifierName(IdentifierNameSyntax node)
        {
            // try to get the script name of the expression
            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // if there's no symbol then just return an identifier
            if (symbol == null || !_scriptNameTable.TryGetValue(symbol, out string scriptName))
            {
                return Factory.Identifier(node.Identifier.Text).ToSingleEnumerable();
            }

            // get the containing type
            INamedTypeSymbol containingType = symbol.ContainingType;

            // see if the identifier is declared within this type
            bool belongsToThisType = containingType ==
                _semanticModel.GetEnclosingSymbol(node.SpanStart, _cancellationToken)?.ContainingType;

            ITsExpression expression;

            // in TypeScript, static references need to be fully qualified with the type name
            if (symbol.IsStatic && containingType != null)
            {
                string containingTypeScriptName =
                    _scriptNameTable.GetValueOrDefault(containingType, containingType.Name);

                expression = Factory.MemberDot(Factory.Identifier(containingTypeScriptName), scriptName);
            }
            // add a "this." prefix if it's an instance symbol within our same type
            else if (!symbol.IsStatic && belongsToThisType)
            {
                expression = Factory.MemberDot(Factory.This, scriptName);
            }
            else
            {
                expression = Factory.Identifier(scriptName);
            }

            // add this type to the import list if it doesn't belong to us
            if (!belongsToThisType)
            {
                _typesToImport.Add(symbol);
            }

            return expression.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a GenericNameSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsGenericTypeName"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitGenericName(GenericNameSyntax node)
        {
            ITsType[] typeArguments = Visit(node.TypeArgumentList).Cast<ITsType>().ToArray();
            ITsGenericTypeName translated = Factory.GenericTypeName(node.Identifier.Text, typeArguments);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a MemberAccessExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsMemberDotExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Expression).Single();

            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // get the script name - the symbol can be null if we're inside a dynamic scope since all
            // bets are off with the type checking
            string scriptName = symbol == null
                ? node.Name.Identifier.Text
                : _scriptNameTable.GetValueOrDefault(symbol, node.Name.Identifier.Text);

            ITsMemberDotExpression translated = Factory.MemberDot(leftSide, scriptName);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ElementAccessExpressionSyntax node.
        /// </summary>
        public override IEnumerable<ITsAstNode> VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Expression).Single();
            var bracketContents = (ITsExpression)Visit(node.ArgumentList).Single();
            ITsMemberBracketExpression translation = Factory.MemberBracket(leftSide, bracketContents);
            return translation.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a EqualsValueClauseSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return Visit(node.Value);
        }

        //// ===========================================================================================================
        //// Array and Object Creation Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a ArrayCreationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrayLiteral"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var arrayElements = Visit(node.Initializer).Cast<ITsExpression>();
            ITsArrayLiteral translated = Factory.Array(arrayElements.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a InitializerExpressionSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node.Expressions.SelectMany(Visit);
        }

        /// <summary>
        /// Called when the visitor visits a ObjectCreationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsNewCallExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Type).Single();
            var arguments = (ITsArgumentList)Visit(node.ArgumentList).First();

            // see if there's an [InlineCode] entry for the ctor invocation
            if (_inlineCodeTranslator.TryTranslate(
                node,
                leftSide,
                arguments,
                _diagnostics,
                out ITsAstNode translatedNode))
            {
                yield return translatedNode;
            }
            else
            {
                ITsNewCallExpression translated = Factory.NewCall(leftSide, arguments);
                yield return translated;
            }
        }

        /// <summary>
        /// Called when the visitor visits a ImplicitArrayCreationExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsArrayLiteral"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            var elements =
                new List<ITsExpression>(node.Initializer.Expressions.SelectMany(Visit).Cast<ITsExpression>());
            ITsArrayLiteral translated = Factory.Array(elements.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a DefaultExpressionSyntax node.
        /// </summary>
        /// <returns>
        /// An <see cref="ITsCallExpression"/>, since `default(T)` gets translated as a call to `ss.getDefaultValue(T)`
        /// </returns>
        public override IEnumerable<ITsAstNode> VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            ITsType translatedType = _typeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_semanticModel),
                _typesToImport,
                _diagnostics,
                node.Type.GetLocation);

            ITsCallExpression translated = Factory.Call(
                Factory.MemberDot(Factory.Identifier("ss"), "getDefaultValue"),
                Factory.ArgumentList(Factory.Argument(Factory.Identifier(translatedType.EmitAsString()))));
            yield return translated;
        }

        //// ===========================================================================================================
        //// Assignments, Conditional, Unary, and Binary Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Called when the visitor visits a AssignmentExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsAssignmentExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Left).Single();
            var rightSide = (ITsExpression)Visit(node.Right).Single();

            ITsAssignmentExpression translated = Factory.Assignment(
                leftSide,
                TranslateAssignmentOperator(node.OperatorToken),
                rightSide);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a ConditionalExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsConditionalExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var condition = (ITsExpression)Visit(node.Condition).Single();
            var whenTrue = (ITsExpression)Visit(node.WhenTrue).Single();
            var whenFalse = (ITsExpression)Visit(node.WhenFalse).Single();

            ITsConditionalExpression translated = Factory.Conditional(condition, whenTrue, whenFalse);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a PrefixUnaryExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsUnaryExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var operand = (ITsExpression)Visit(node.Operand).Single();
            ITsUnaryExpression translated = Factory.UnaryExpression(
                operand,
                TranslateUnaryOperator(node.OperatorToken, asPrefix: true));
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a PostfixUnaryExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsUnaryExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var operand = (ITsExpression)Visit(node.Operand).Single();
            ITsUnaryExpression translated = Factory.UnaryExpression(
                operand,
                TranslateUnaryOperator(node.OperatorToken, asPrefix: false));
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a BinaryExpressionSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsBinaryExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var leftSide = (ITsExpression)Visit(node.Left).Single();
            var rightSide = (ITsExpression)Visit(node.Right).Single();

            ITsBinaryExpression translated = Factory.BinaryExpression(
                leftSide,
                TranslateBinaryOperator(node.OperatorToken),
                rightSide);
            yield return translated;
        }

        private TsAssignmentOperator TranslateAssignmentOperator(SyntaxToken operatorToken)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (operatorToken.Kind())
            {
                case SyntaxKind.EqualsToken:
                    return TsAssignmentOperator.SimpleAssign;

                case SyntaxKind.AsteriskEqualsToken:
                    return TsAssignmentOperator.MultiplyAssign;

                case SyntaxKind.SlashEqualsToken:
                    return TsAssignmentOperator.DivideAssign;

                case SyntaxKind.PercentEqualsToken:
                    return TsAssignmentOperator.ModuloAssign;

                case SyntaxKind.PlusEqualsToken:
                    return TsAssignmentOperator.AddAssign;

                case SyntaxKind.MinusEqualsToken:
                    return TsAssignmentOperator.SubtractAssign;

                case SyntaxKind.LessThanLessThanEqualsToken:
                    return TsAssignmentOperator.LeftShiftAssign;

                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                    return TsAssignmentOperator.SignedRightShiftAssign;

                case SyntaxKind.AmpersandEqualsToken:
                    return TsAssignmentOperator.BitwiseAndAssign;

                case SyntaxKind.CaretEqualsToken:
                    return TsAssignmentOperator.BitwiseXorAssign;

                case SyntaxKind.BarEqualsToken:
                    return TsAssignmentOperator.BitwiseOrAssign;

                default:
                    ReportUnsupportedTranslataion(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                    return TsAssignmentOperator.SimpleAssign;
            }
        }

        private TsUnaryOperator TranslateUnaryOperator(SyntaxToken operatorToken, bool asPrefix)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (operatorToken.Kind())
            {
                case SyntaxKind.PlusPlusToken:
                    return asPrefix ? TsUnaryOperator.PrefixIncrement : TsUnaryOperator.PostfixIncrement;

                case SyntaxKind.MinusMinusToken:
                    return asPrefix ? TsUnaryOperator.PrefixDecrement : TsUnaryOperator.PostfixDecrement;

                case SyntaxKind.PlusToken:
                    return TsUnaryOperator.Plus;

                case SyntaxKind.MinusToken:
                    return TsUnaryOperator.Minus;

                case SyntaxKind.TildeToken:
                    return TsUnaryOperator.BitwiseNot;

                case SyntaxKind.ExclamationToken:
                    return TsUnaryOperator.LogicalNot;

                default:
                    ReportUnsupportedTranslataion(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                    return TsUnaryOperator.Plus;
            }
        }

        private TsBinaryOperator TranslateBinaryOperator(SyntaxToken operatorToken)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (operatorToken.Kind())
            {
                case SyntaxKind.AsteriskToken:
                    return TsBinaryOperator.Multiply;

                case SyntaxKind.SlashToken:
                    return TsBinaryOperator.Divide;

                case SyntaxKind.PercentToken:
                    return TsBinaryOperator.Modulo;

                case SyntaxKind.PlusToken:
                    return TsBinaryOperator.Add;

                case SyntaxKind.MinusToken:
                    return TsBinaryOperator.Subtract;

                case SyntaxKind.LessThanLessThanToken:
                    return TsBinaryOperator.LeftShift;

                case SyntaxKind.GreaterThanGreaterThanToken:
                    return TsBinaryOperator.SignedRightShift;

                case SyntaxKind.LessThanToken:
                    return TsBinaryOperator.LessThan;

                case SyntaxKind.GreaterThanToken:
                    return TsBinaryOperator.GreaterThan;

                case SyntaxKind.LessThanEqualsToken:
                    return TsBinaryOperator.LessThanEqual;

                case SyntaxKind.GreaterThanEqualsToken:
                    return TsBinaryOperator.GreaterThanEqual;

                case SyntaxKind.EqualsEqualsToken:
                    return TsBinaryOperator.StrictEquals;

                case SyntaxKind.ExclamationEqualsToken:
                    return TsBinaryOperator.StrictNotEquals;

                case SyntaxKind.AmpersandToken:
                    return TsBinaryOperator.BitwiseAnd;

                case SyntaxKind.CaretToken:
                    return TsBinaryOperator.BitwiseXor;

                case SyntaxKind.BarToken:
                    return TsBinaryOperator.BitwiseOr;

                case SyntaxKind.AmpersandAmpersandToken:
                    return TsBinaryOperator.LogicalAnd;

                case SyntaxKind.BarBarToken:
                case SyntaxKind.QuestionQuestionToken:
                    return TsBinaryOperator.LogicalOr;

                default:
                    ReportUnsupportedTranslataion(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                    return TsBinaryOperator.Add;
            }
        }
    }
}
