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
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
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
                    // Use the raw text since C# strings are escaped the same as JavaScript strings.
                    string str = node.Token.Text;
                    bool isVerbatim = str.StartsWith("@", StringComparison.Ordinal);

                    // Trim the leading @ and surrounding quotes.
                    str = isVerbatim ? str.Substring(1) : str;
                    str = str.StartsWith("\"", StringComparison.Ordinal) ? str.Substring(1) : str;
                    str = str.EndsWith("\"", StringComparison.Ordinal) ? str.Substring(0, str.Length - 1) : str;

                    // For verbatim strings, we need to add the escape characters back in.
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

            Diagnostic diagnostic = DiagnosticFactory.LiteralExpressionTranslationNotSupported(node);
            ReportUnsupportedTranslation(diagnostic);
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
            // Try to get the script name of the expression.
            ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // If there's no symbol then just return an identifier.
            string scriptName = node.Keyword.ValueText;
            if (symbol != null && _scriptSymbolTable.TryGetValue(symbol, out IScriptSymbol? scriptSymbol))
            {
                scriptName = scriptSymbol.ComputedScriptName;
            }

            yield return Factory.Identifier(scriptName);
        }

        /// <summary>
        /// Called when the visitor visits a IdentifierNameSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsIdentifier"/> or <see cref="ITsMemberDotExpression"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitIdentifierName(IdentifierNameSyntax node)
        {
            // Try to get the script name of the expression.
            ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // If there's no symbol then just return an identifier.
            if (symbol == null)
            {
                yield return Factory.Identifier(node.Identifier.Text);
                yield break;
            }

            ITsExpression translated = TranslateIdentifierName(symbol, node.GetLocation());
            yield return translated;
        }

        /// <summary>
        /// Translates an identifier name represented by the symbol, taking into account static vs. instance references.
        /// </summary>
        /// <param name="symbol">The symbol to translate.</param>
        /// <param name="nodeLocation">The start of the syntax node where this symbol was located.</param>
        /// <returns>An <see cref="ITsIdentifier"/> or <see cref="ITsMemberDotExpression"/>.</returns>
        private ITsExpression TranslateIdentifierName(ISymbol symbol, Location nodeLocation)
        {
            string scriptName = _scriptSymbolTable.GetComputedScriptNameOrDefault(symbol, symbol.Name);

            // Get the containing type of the symbol.
            INamedTypeSymbol? containingType = symbol.ContainingType;

            // Get the containing type of the syntax node (usually an identifier).
            INamedTypeSymbol? containingTypeOfSyntaxNode = _semanticModel
                .GetEnclosingSymbol(nodeLocation.SourceSpan.Start, _cancellationToken)
                ?.ContainingType;

            // See if the identifier is declared within this type.
            bool belongsToThisType = containingType != null &&
                containingTypeOfSyntaxNode != null &&
                SymbolEqualityComparer.Default.Equals(containingType, containingTypeOfSyntaxNode);

            ITsExpression expression;

            // In TypeScript, static references need to be fully qualified with the type name.
            if (symbol.IsStatic && containingType != null)
            {
                string containingTypeScriptName =
                    _scriptSymbolTable.GetComputedScriptNameOrDefault(containingType, containingType.Name);

                expression = Factory.MemberDot(Factory.Identifier(containingTypeScriptName), scriptName);
            }

            // Add a "this." prefix if it's an instance symbol within our same type.
            else if (!symbol.IsStatic &&
                belongsToThisType &&
                !symbol.Kind.IsOneOf(SymbolKind.Parameter, SymbolKind.Local, SymbolKind.Label))
            {
                expression = Factory.MemberDot(Factory.This, scriptName);
            }
            else
            {
                expression = Factory.Identifier(scriptName);
            }

            // Add this type to the import list if it doesn't belong to us.
            if (!belongsToThisType)
            {
                ITypeSymbol? typeToImport = symbol as ITypeSymbol ?? containingType;
                if (typeToImport == null)
                {
                    ReportUnsupportedTranslation(
                        DiagnosticFactory.InternalError(
                            $"Cannot find the type to import for symbol '{symbol.ToHashDisplay()}'",
                            nodeLocation));
                }
                else
                {
                    _typesToImport.Add(typeToImport);
                }
            }

            return expression;
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

            ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            // Get the script name - the symbol can be null if we're inside a dynamic scope since all
            // bets are off with the type checking.
            string scriptName = symbol == null
                ? node.Name.Identifier.Text
                : _scriptSymbolTable.GetComputedScriptNameOrDefault(symbol, node.Name.Identifier.Text);

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

            // node.ArgumentList can be null in the case of the following pattern:
            // var x = new Thing { Prop = value; }
            ITsArgumentList arguments = Factory.ArgumentList();
            if (node.ArgumentList != null)
            {
                arguments = (ITsArgumentList)Visit(node.ArgumentList).First();
            }

            if (node.Initializer != null)
            {
                // TODO - need to support object creation of the form:
                // var x = new Thing { Prop = value; }
            }

            // See if there's an [InlineCode] entry for the ctor invocation.
            if (_semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol ctorAsMethodSymbol &&
                _inlineCodeTranslator.TryTranslate(
                    ctorAsMethodSymbol,
                    node.GetLocation(),
                    leftSide,
                    arguments,
                    _diagnostics,
                    out ITsAstNode? translatedNode))
            {
                yield return translatedNode;
            }
            else if (JsDictionaryTranslator.TryTranslateObjectCreationSyntax(
                node,
                arguments,
                _semanticModel,
                _diagnostics,
                out ITsObjectLiteral? translatedObjectLiteral))
            {
                yield return translatedObjectLiteral;
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
            TsAssignmentOperator? op = operatorToken.Kind() switch
            {
                SyntaxKind.EqualsToken => TsAssignmentOperator.SimpleAssign,
                SyntaxKind.AsteriskEqualsToken => TsAssignmentOperator.MultiplyAssign,
                SyntaxKind.SlashEqualsToken => TsAssignmentOperator.DivideAssign,
                SyntaxKind.PercentEqualsToken => TsAssignmentOperator.ModuloAssign,
                SyntaxKind.PlusEqualsToken => TsAssignmentOperator.AddAssign,
                SyntaxKind.MinusEqualsToken => TsAssignmentOperator.SubtractAssign,
                SyntaxKind.LessThanLessThanEqualsToken => TsAssignmentOperator.LeftShiftAssign,
                SyntaxKind.GreaterThanGreaterThanEqualsToken => TsAssignmentOperator.SignedRightShiftAssign,
                SyntaxKind.AmpersandEqualsToken => TsAssignmentOperator.BitwiseAndAssign,
                SyntaxKind.CaretEqualsToken => TsAssignmentOperator.BitwiseXorAssign,
                SyntaxKind.BarEqualsToken => TsAssignmentOperator.BitwiseOrAssign,
                _ => null,
            };

            if (op == null)
            {
                ReportUnsupportedTranslation(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                op = TsAssignmentOperator.SimpleAssign;
            }

            return op.Value;
        }

        private TsUnaryOperator TranslateUnaryOperator(SyntaxToken operatorToken, bool asPrefix)
        {
            TsUnaryOperator? op = operatorToken.Kind() switch
            {
                SyntaxKind.PlusPlusToken => asPrefix
                    ? TsUnaryOperator.PrefixIncrement
                    : TsUnaryOperator.PostfixIncrement,
                SyntaxKind.MinusMinusToken => asPrefix
                    ? TsUnaryOperator.PrefixDecrement
                    : TsUnaryOperator.PostfixDecrement,
                SyntaxKind.PlusToken => TsUnaryOperator.Plus,
                SyntaxKind.MinusToken => TsUnaryOperator.Minus,
                SyntaxKind.TildeToken => TsUnaryOperator.BitwiseNot,
                SyntaxKind.ExclamationToken => TsUnaryOperator.LogicalNot,
                _ => null,
            };

            if (op == null)
            {
                ReportUnsupportedTranslation(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                op = TsUnaryOperator.Plus;
            }

            return op.Value;
        }

        private TsBinaryOperator TranslateBinaryOperator(SyntaxToken operatorToken)
        {
            TsBinaryOperator? op = operatorToken.Kind() switch
            {
                SyntaxKind.AsteriskToken => TsBinaryOperator.Multiply,
                SyntaxKind.SlashToken => TsBinaryOperator.Divide,
                SyntaxKind.PercentToken => TsBinaryOperator.Modulo,
                SyntaxKind.PlusToken => TsBinaryOperator.Add,
                SyntaxKind.MinusToken => TsBinaryOperator.Subtract,
                SyntaxKind.LessThanLessThanToken => TsBinaryOperator.LeftShift,
                SyntaxKind.GreaterThanGreaterThanToken => TsBinaryOperator.SignedRightShift,
                SyntaxKind.LessThanToken => TsBinaryOperator.LessThan,
                SyntaxKind.GreaterThanToken => TsBinaryOperator.GreaterThan,
                SyntaxKind.LessThanEqualsToken => TsBinaryOperator.LessThanEqual,
                SyntaxKind.GreaterThanEqualsToken => TsBinaryOperator.GreaterThanEqual,
                SyntaxKind.EqualsEqualsToken => TsBinaryOperator.StrictEquals,
                SyntaxKind.ExclamationEqualsToken => TsBinaryOperator.StrictNotEquals,
                SyntaxKind.AmpersandToken => TsBinaryOperator.BitwiseAnd,
                SyntaxKind.CaretToken => TsBinaryOperator.BitwiseXor,
                SyntaxKind.BarToken => TsBinaryOperator.BitwiseOr,
                SyntaxKind.AmpersandAmpersandToken => TsBinaryOperator.LogicalAnd,
                SyntaxKind.BarBarToken => TsBinaryOperator.LogicalOr,
                SyntaxKind.QuestionQuestionToken => TsBinaryOperator.LogicalOr,
                _ => null,
            };

            if (op == null)
            {
                ReportUnsupportedTranslation(DiagnosticFactory.OperatorKindNotSupported(operatorToken));
                op = TsBinaryOperator.Add;
            }

            return op.Value;
            }
        }
    }
}
