// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeEmitExtensions.cs" company="Justin Rockwood">
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

    internal static class TsAstNodeEmitExtensions
    {
        public static Emitter Write(this Emitter emitter, ITsAstNode node)
        {
            node.Emit(emitter);
            return emitter;
        }

        public static Emitter WriteIdentifier(this Emitter emitter, string text)
        {
            return emitter.Write(text);
        }

        public static Emitter WriteThis(this Emitter emitter)
        {
            return emitter.Write("this");
        }

        public static Emitter WriteParenthesizedExpression(this Emitter emitter, ITsExpression expression)
        {
            return emitter.Write("(").Write(expression).Write(")");
        }

        public static Emitter WriteNullLiteral(this Emitter emitter)
        {
            return emitter.Write("null");
        }

        public static Emitter WriteBooleanLiteral(this Emitter emitter, bool value)
        {
            return emitter.Write(value ? "true" : "false");
        }

        public static Emitter WriteNumericLiteral(this Emitter emitter, double value, TsNumericLiteralKind kind)
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

        public static Emitter WriteStringLiteral(this Emitter emitter, string value, StringLiteralQuoteKind quoteKind)
        {
            string quoteChar = quoteKind == StringLiteralQuoteKind.SingleQuote ? "'" : "\"";
            emitter.Write($"{quoteChar}{value.Replace(quoteChar, "\\" + quoteChar)}{quoteChar}");
            return emitter;
        }

        public static Emitter WriteRegularExpressionLiteral(this Emitter emitter, string body, string? flags)
        {
            return emitter.Write($"/{body}/{flags ?? string.Empty}");
        }

        public static Emitter WriteArrayLiteral(this Emitter emitter, ImmutableArray<ITsArrayElement?> elements)
        {
            return emitter.WriteList(elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }

        public static Emitter WriteArrayElement(this Emitter emitter, ITsExpression expression, bool isSpreadElement)
        {
            return emitter.WriteIf(isSpreadElement, "...").Write(expression);
        }

        public static Emitter WriteObjectLiteral(
            this Emitter emitter,
            ImmutableArray<ITsPropertyDefinition> propertyDefinitions)
        {
            return emitter.WriteCommaNewlineSeparatedBlock(propertyDefinitions);
        }

        public static Emitter WriteCoverInitializedName(
            this Emitter emitter,
            ITsIdentifier identifier,
            ITsExpression initializer)
        {
            identifier.Emit(emitter);
            initializer.EmitOptionalAssignment(emitter);
            return emitter;
        }

        public static Emitter WritePropertyAssignment(
            this Emitter emitter,
            ITsPropertyName propertyName,
            ITsExpression initializer)
        {
            return emitter.Write(propertyName).Write(": ").Write(initializer);
        }

        public static Emitter WriteComputedPropertyName(this Emitter emitter, ITsExpression expression)
        {
            return emitter.Write("[").Write(expression).Write("]");
        }

        public static Emitter WriteTemplatePart(this Emitter emitter, string template, ITsExpression? expression)
        {
            emitter.Write(template);

            if (expression != null)
            {
                emitter.Write("${").Write(expression).Write("}");
            }

            return emitter;
        }

        public static Emitter WriteTemplateLiteral(this Emitter emitter, ImmutableArray<ITsTemplatePart> parts)
        {
            return emitter.Write("`").WriteList(parts, indent: false).Write("`");
        }

        public static Emitter WriteMemberBracketExpression(
            this Emitter emitter,
            ITsExpression leftSide,
            ITsExpression bracketContents)
        {
            return emitter.Write(leftSide).Write("[").Write(bracketContents).Write("]");
        }

        public static Emitter WriteMemberDotExpression(this Emitter emitter, ITsExpression leftSide, string dotName)
        {
            return emitter.Write(leftSide).Write($".{dotName}");
        }

        public static Emitter WriteSuperBracketExpression(this Emitter emitter, ITsExpression bracketContents)
        {
            return emitter.Write("super[").Write(bracketContents).Write("]");
        }

        public static Emitter WriteSuperDotExpression(this Emitter emitter, string dotName)
        {
            return emitter.Write($"super.{dotName}");
        }

        public static Emitter WriteNewTargetExpression(this Emitter emitter)
        {
            return emitter.Write("new.target");
        }

        public static Emitter WriteCallExpression(
            this Emitter emitter,
            ITsExpression leftSide,
            ITsArgumentList argumentList)
        {
            return emitter.Write(leftSide).Write(argumentList);
        }

        public static Emitter WriteNewCallExpression(
            this Emitter emitter,
            ITsExpression leftSide,
            ITsArgumentList argumentList)
        {
            return emitter.Write("new ").Write(leftSide).Write(argumentList);
        }

        public static Emitter WriteSuperCallExpression(this Emitter emitter, ITsArgumentList argumentList)
        {
            return emitter.Write("super").Write(argumentList);
        }

        public static Emitter WriteArgumentList(
            this Emitter emitter,
            ImmutableArray<ITsType> typeArguments,
            ImmutableArray<ITsArgument> arguments)
        {
            if (!typeArguments.IsEmpty)
            {
                emitter.WriteList(typeArguments, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            return emitter.WriteParameterList(arguments);
        }

        public static Emitter WriteArgument(this Emitter emitter, ITsExpression expression, bool isSpreadArgument)
        {
            return emitter.WriteIf(isSpreadArgument, "... ").Write(expression);
        }

        public static Emitter WriteUnaryExpression(
            this Emitter emitter,
            ITsExpression operand,
            TsUnaryOperator @operator)
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

        public static Emitter WriteCastExpression(this Emitter emitter, ITsType castType, ITsExpression expression)
        {
            return emitter.Write("<").Write(castType).Write(">").Write(expression);
        }

        public static Emitter WriteBinaryExpression(
            this Emitter emitter,
            ITsExpression leftSide,
            TsBinaryOperator @operator,
            ITsExpression rightSide)
        {
            return emitter.Write(leftSide).Write($" {@operator.ToCodeDisplay()} ").Write(rightSide);
        }

        public static Emitter WriteConditionalExpression(
            this Emitter emitter,
            ITsExpression condition,
            ITsExpression whenTrue,
            ITsExpression whenFalse)
        {
            return emitter.Write(condition).Write(" ? ").Write(whenTrue).Write(" : ").Write(whenFalse);
        }

        public static Emitter WriteAssignmentExpression(
            this Emitter emitter,
            ITsExpression leftSide,
            TsAssignmentOperator @operator,
            ITsExpression rightSide)
        {
            return emitter.Write(leftSide).Write($" {@operator.ToCodeDisplay()} ").Write(rightSide);
        }

        public static Emitter WriteCommaExpression(this Emitter emitter, ImmutableArray<ITsExpression> expressions)
        {
            return emitter.WriteList(expressions, indent: false, itemDelimiter: ", ");
        }

        public static Emitter WriteBlockStatement(this Emitter emitter, ImmutableArray<ITsStatementListItem> statements)
        {
            return emitter.WriteBlock(statements, skipNewlines: true);
        }

        public static Emitter WriteLexicalDeclaration(
            this Emitter emitter,
            bool isConst,
            ImmutableArray<ITsLexicalBinding> declarations)
        {
            return emitter.Write(isConst ? "const " : "let ")
                .WriteList(declarations, indent: false, itemDelimiter: ", ")
                .WriteLine(";");
        }

        public static Emitter WriteVariableStatement(
            this Emitter emitter,
            ImmutableArray<ITsVariableDeclaration> declarations)
        {
            return emitter.Write("var ").WriteList(declarations, indent: false, itemDelimiter: ", ").WriteLine(";");
        }

        public static Emitter WriteObjectBindingPattern(
            this Emitter emitter,
            ImmutableArray<ITsBindingProperty> properties)
        {
            return emitter.WriteList(
                properties,
                indent: false,
                prefix: "{",
                suffix: "}",
                itemDelimiter: ", ",
                emptyContents: "{}");
        }
    }
}
