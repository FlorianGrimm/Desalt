// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitter.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Emit
{
    using System;
    using System.Globalization;
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast;
    using Desalt.TypeScript.Ast.Expressions;

    public partial class TsEmitter
    {
        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public override void VisitNullLiteral(ITsNullLiteral node)
        {
            _emitter.Write("null");
        }

        public override void VisitBooleanLiteral(ITsBooleanLiteral node)
        {
            _emitter.Write(node.Value ? "true" : "false");
        }

        public override void VisitNumericLiteral(ITsNumericLiteral node)
        {
            switch (node.Kind)
            {
                case TsNumericLiteralKind.Decimal:
                    _emitter.Write(node.Value.ToString(CultureInfo.InvariantCulture));
                    break;

                case TsNumericLiteralKind.BinaryInteger:
                    _emitter.Write("0b" + Convert.ToString((long)node.Value, 2));
                    break;

                case TsNumericLiteralKind.OctalInteger:
                    _emitter.Write("0o" + Convert.ToString((long)node.Value, 8));
                    break;

                case TsNumericLiteralKind.HexInteger:
                    _emitter.Write("0x" + Convert.ToString((long)node.Value, 16));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void VisitStringLiteral(ITsStringLiteral node)
        {
            _emitter.Write(node.ToFullCodeDisplay());
        }

        public override void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral node)
        {
            _emitter.Write("/");
            _emitter.Write(node.Body);
            _emitter.Write("/");
            _emitter.Write(node.Flags);
        }

        public override void VisitArrayLiteral(ITsArrayLiteral node)
        {
            _emitter.Write("[");
            _emitter.WriteCommaList(node.Elements, Visit);
            _emitter.Write("]");
        }

        public override void VisitArrayElement(ITsArrayElement node)
        {
            Visit(node.Element);

            if (node.IsSpreadElement)
            {
                _emitter.Write(" ...");
            }
        }

        public override void VisitTemplateLiteral(ITsTemplateLiteral node)
        {
            _emitter.Write("`");

            foreach (TsTemplatePart part in node.Parts)
            {
                if (part.Template != null)
                {
                    _emitter.Write(part.Template);
                }

                if (part.Expression != null)
                {
                    _emitter.Write("${");
                    Visit(part.Expression);
                    _emitter.Write("}");
                }
            }

            _emitter.Write("`");
        }

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public override void VisitObjectLiteral(ITsObjectLiteral node)
        {
            if (node.PropertyDefinitions.IsEmpty)
            {
                _emitter.Write("{}");
                return;
            }

            _emitter.WriteLine("{");
            _emitter.WriteList(node.PropertyDefinitions, ",", Visit);
        }

        //// ===========================================================================================================
        //// Unary and Binary Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Writes a unary expression.
        /// </summary>
        public override void VisitUnaryExpression(ITsUnaryExpression model)
        {
            bool isPostfix = model.Operator.IsOneOf(
                TsUnaryOperator.PostfixIncrement, TsUnaryOperator.PostfixDecrement);

            if (!isPostfix)
            {
                _emitter.Write(model.Operator.ToCodeDisplay());
            }

            // some operators require a space after them
            if (model.Operator.IsOneOf(TsUnaryOperator.Delete, TsUnaryOperator.Void, TsUnaryOperator.Typeof))
            {
                _emitter.Write(" ");
            }

            Visit(model.Operand);

            if (isPostfix)
            {
                _emitter.Write(model.Operator.ToCodeDisplay());
            }
        }

        /// <summary>
        /// Writes a binary expression.
        /// </summary>
        public override void VisitBinaryExpression(ITsBinaryExpression node)
        {
            Visit(node.LeftSide);
            _emitter.Write($" {node.Operator.ToCodeDisplay()} ");
            Visit(node.RightSide);
        }

        /// <summary>
        /// Writes a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public override void VisitConditionalExpression(ITsConditionalExpression node)
        {
            Visit(node.Condition);
            _emitter.Write(" ? ");

            Visit(node.WhenTrue);
            _emitter.Write(" : ");

            Visit(node.WhenFalse);
        }

        /// <summary>
        /// Writes expressions of the form 'x = y', where the assignment operator can be any of the
        /// standard JavaScript assignment operators.
        /// </summary>
        public override void VisitAssignmentExpression(ITsAssignmentExpression node)
        {
            Visit(node.LeftSide);

            _emitter.Write(" ");
            _emitter.Write(node.Operator.ToCodeDisplay());
            _emitter.Write(" ");

            Visit(node.RightSide);
        }

        //// ===========================================================================================================
        //// Member Expressions
        //// ===========================================================================================================

        /// <summary>
        /// Writes expressions of the form 'expression[expression]'.
        /// </summary>
        public override void VisitMemberBracketExpression(ITsMemberBracketExpression node)
        {
            Visit(node.LeftSide);

            _emitter.Write("[");
            Visit(node.BracketContents);
            _emitter.Write("]");
        }

        /// <summary>
        /// Writes expressions of the form 'expression.name'.
        /// </summary>
        public override void VisitMemberDotExpression(ITsMemberDotExpression node)
        {
            Visit(node.LeftSide);
            _emitter.Write(".");
            _emitter.Write(node.DotName);
        }
    }
}
