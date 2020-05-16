// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstEmitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using Desalt.TypeScriptAst.Emit;

    internal static class TsAstEmitter
    {
        public static Emitter Write(this Emitter emitter, ITsAstNode node)
        {
            node.Emit(emitter);
            return emitter;
        }

        public static Emitter EmitIdentifier(Emitter emitter, string text)
        {
            return emitter.Write(text);
        }

        public static Emitter EmitThis(Emitter emitter)
        {
            return emitter.Write("this");
        }

        public static Emitter EmitParenthesizedExpression(Emitter emitter, ITsExpression expression)
        {
            return emitter.Write("(").Write(expression).Write(")");
        }

        public static Emitter EmitNullLiteral(Emitter emitter)
        {
            return emitter.Write("null");
        }

        public static Emitter EmitBooleanLiteral(Emitter emitter, bool value)
        {
            return emitter.Write(value ? "true" : "false");
        }

        public static Emitter EmitNumericLiteral(Emitter emitter, double value, TsNumericLiteralKind kind)
        {
            string number = kind switch
            {
                TsNumericLiteralKind.Decimal => value.ToString(CultureInfo.InvariantCulture),
                TsNumericLiteralKind.BinaryInteger => "0b" + Convert.ToString((long)value, 2),
                TsNumericLiteralKind.OctalInteger => "0o" + Convert.ToString((long)value, 8),
                TsNumericLiteralKind.HexInteger => "0x" + Convert.ToString((long)value, 16),
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };

            return emitter.Write(number);
        }

        public static Emitter EmitStringLiteral(Emitter emitter, string value, StringLiteralQuoteKind quoteKind)
        {
            string quoteChar = quoteKind == StringLiteralQuoteKind.SingleQuote ? "'" : "\"";
            emitter.Write($"{quoteChar}{value.Replace(quoteChar, "\\" + quoteChar)}{quoteChar}");
            return emitter;
        }

        public static Emitter EmitRegularExpressionLiteral(Emitter emitter, string body, string? flags)
        {
            return emitter.Write($"/{body}/{flags ?? string.Empty}");
        }

        public static Emitter EmitArrayLiteral(Emitter emitter, ImmutableArray<ITsArrayElement?> elements)
        {
            return emitter.WriteList(elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }

        public static Emitter EmitArrayElement(Emitter emitter, ITsExpression expression, bool isSpreadElement)
        {
            return emitter.WriteIf(isSpreadElement, "...").Write(expression);
        }

        public static Emitter EmitObjectLiteral(
            Emitter emitter,
            ImmutableArray<ITsPropertyDefinition> propertyDefinitions)
        {
            return emitter.WriteCommaNewlineSeparatedBlock(propertyDefinitions);
        }

        public static Emitter EmitCoverInitializedName(
            Emitter emitter,
            ITsIdentifier identifier,
            ITsExpression initializer)
        {
            identifier.Emit(emitter);
            initializer.EmitOptionalAssignment(emitter);
            return emitter;
        }

        public static Emitter EmitPropertyAssignment(
            Emitter emitter,
            ITsPropertyName propertyName,
            ITsExpression initializer)
        {
            return emitter.Write(propertyName).Write(": ").Write(initializer);
        }

        public static Emitter EmitComputedPropertyName(Emitter emitter, ITsExpression expression)
        {
            return emitter.Write("[").Write(expression).Write("]");
        }

        public static Emitter EmitTemplatePart(Emitter emitter, string template, ITsExpression? expression)
        {
            emitter.Write(template);

            if (expression != null)
            {
                emitter.Write("${").Write(expression).Write("}");
            }

            return emitter;
        }

        public static Emitter EmitTemplateLiteral(Emitter emitter, ImmutableArray<ITsTemplatePart> parts)
        {
            return emitter.Write("`").WriteList(parts, indent: false).Write("`");
        }

        public static Emitter EmitMemberBracketExpression(
            Emitter emitter,
            ITsExpression leftSide,
            ITsExpression bracketContents)
        {
            return emitter.Write(leftSide).Write("[").Write(bracketContents).Write("]");
        }

        public static Emitter EmitMemberDotExpression(Emitter emitter, ITsExpression leftSide, string dotName)
        {
            return emitter.Write(leftSide).Write($".{dotName}");
        }

        public static Emitter EmitSuperBracketExpression(Emitter emitter, ITsExpression bracketContents)
        {
            return emitter.Write("super[").Write(bracketContents).Write("]");
        }

        public static Emitter EmitSuperDotExpression(Emitter emitter, string dotName)
        {
            return emitter.Write($"super.{dotName}");
        }

        public static Emitter EmitNewTargetExpression(Emitter emitter)
        {
            return emitter.Write("new.target");
        }

        public static Emitter EmitCallExpression(Emitter emitter, ITsExpression leftSide, ITsArgumentList argumentList)
        {
            return emitter.Write(leftSide).Write(argumentList);
        }

        public static Emitter EmitNewCallExpression(
            Emitter emitter,
            ITsExpression leftSide,
            ITsArgumentList argumentList)
        {
            return emitter.Write("new ").Write(leftSide).Write(argumentList);
        }

        public static Emitter EmitSuperCallExpression(Emitter emitter, ITsArgumentList argumentList)
        {
            return emitter.Write("super").Write(argumentList);
        }

        public static Emitter EmitArgumentList(
            Emitter emitter,
            ImmutableArray<ITsType> typeArguments,
            ImmutableArray<ITsArgument> arguments)
        {
            if (!typeArguments.IsEmpty)
            {
                emitter.WriteList(typeArguments, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            return emitter.WriteParameterList(arguments);
        }

        public static Emitter EmitArgument(Emitter emitter, ITsExpression expression, bool isSpreadArgument)
        {
            return emitter.WriteIf(isSpreadArgument, "... ").Write(expression);
        }

        public static Emitter EmitUnaryExpression(Emitter emitter, ITsExpression operand, TsUnaryOperator @operator)
        {
            switch (@operator)
            {
                case TsUnaryOperator.Delete:
                case TsUnaryOperator.Void:
                case TsUnaryOperator.Typeof:
                    emitter.Write($"{@operator.ToCodeDisplay()} ");
                    operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PrefixIncrement:
                case TsUnaryOperator.PrefixDecrement:
                case TsUnaryOperator.Plus:
                case TsUnaryOperator.Minus:
                case TsUnaryOperator.BitwiseNot:
                case TsUnaryOperator.LogicalNot:
                    emitter.Write(@operator.ToCodeDisplay());
                    operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PostfixIncrement:
                case TsUnaryOperator.PostfixDecrement:
                    operand.Emit(emitter);
                    emitter.Write(@operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator));
            }

            return emitter;
        }

        public static Emitter EmitCastExpression(Emitter emitter, ITsType castType, ITsExpression expression)
        {
            return emitter.Write("<").Write(castType).Write(">").Write(expression);
        }

        public static Emitter EmitBinaryExpression(
            Emitter emitter,
            ITsExpression leftSide,
            TsBinaryOperator @operator,
            ITsExpression rightSide)
        {
            return emitter.Write(leftSide).Write($" {@operator.ToCodeDisplay()} ").Write(rightSide);
        }

        public static Emitter EmitConditionalExpression(
            Emitter emitter,
            ITsExpression condition,
            ITsExpression whenTrue,
            ITsExpression whenFalse)
        {
            return emitter.Write(condition).Write(" ? ").Write(whenTrue).Write(" : ").Write(whenFalse);
        }

        public static Emitter EmitAssignmentExpression(
            Emitter emitter,
            ITsExpression leftSide,
            TsAssignmentOperator @operator,
            ITsExpression rightSide)
        {
            return emitter.Write(leftSide).Write($" {@operator.ToCodeDisplay()} ").Write(rightSide);
        }

        public static Emitter EmitCommaExpression(Emitter emitter, ImmutableArray<ITsExpression> expressions)
        {
            return emitter.WriteList(expressions, indent: false, itemDelimiter: ", ");
        }

        public static Emitter EmitBlockStatement(Emitter emitter, ImmutableArray<ITsStatementListItem> statements)
        {
            return emitter.WriteBlock(statements, skipNewlines: true);
        }

        public static Emitter EmitLexicalDeclaration(
            Emitter emitter,
            bool isConst,
            ImmutableArray<ITsLexicalBinding> declarations)
        {
            return emitter.Write(isConst ? "const " : "let ")
                .WriteList(declarations, indent: false, itemDelimiter: ", ")
                .WriteLine(";");
        }

        public static Emitter EmitVariableStatement(
            Emitter emitter,
            ImmutableArray<ITsVariableDeclaration> declarations)
        {
            return emitter.Write("var ").WriteList(declarations, indent: false, itemDelimiter: ", ").WriteLine(";");
        }

        public static Emitter EmitObjectBindingPattern(Emitter emitter, ImmutableArray<ITsBindingProperty> properties)
        {
            return emitter.WriteList(
                properties,
                indent: false,
                prefix: "{",
                suffix: "}",
                itemDelimiter: ", ",
                emptyContents: "{}");
        }

        public static Emitter EmitArrayBindingPattern(
            Emitter emitter,
            ImmutableArray<ITsBindingElement?> elements,
            ITsIdentifier? restElement)
        {
            if (restElement != null)
            {
                return emitter.Write("[")
                    .WriteList(elements, indent: false, itemDelimiter: ", ", delimiterAfterLastItem: true)
                    .Write("... ")
                    .Write(restElement)
                    .Write("]");
            }

            return emitter.WriteList(
                elements,
                indent: false,
                prefix: "[",
                suffix: "]",
                itemDelimiter: ", ",
                emptyContents: "[]");
        }

        public static Emitter EmitSingleNameBinding(Emitter emitter, ITsIdentifier name, ITsExpression? defaultValue)
        {
            name.Emit(emitter);
            defaultValue?.EmitOptionalAssignment(emitter);
            return emitter;
        }

        public static Emitter EmitPropertyNameBinding(
            Emitter emitter,
            ITsPropertyName propertyName,
            ITsBindingElement bindingElement)
        {
            return emitter.Write(propertyName).Write(": ").Write(bindingElement);
        }

        public static Emitter EmitPatternBinding(
            Emitter emitter,
            ITsBindingPattern bindingPattern,
            ITsExpression? initializer)
        {
            bindingPattern.Emit(emitter);
            initializer?.EmitOptionalAssignment(emitter);
            return emitter;
        }
    }
}
