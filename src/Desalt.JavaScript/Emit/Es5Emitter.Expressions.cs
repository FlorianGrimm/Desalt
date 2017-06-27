// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using System;
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Ast.Expressions;

    public partial class Es5Emitter
    {
        public override void VisitIdentifier(Es5Identifier node)
        {
            _emitter.Write(node.Text);
        }

        public override void VisitThisExpresssion(Es5ThisExpression node)
        {
            _emitter.Write("this");
        }

        public override void VisitLiteralExpression(Es5LiteralExpression node)
        {
            switch (node.Kind)
            {
                case Es5LiteralKind.Null:
                    _emitter.Write("null");
                    break;

                case Es5LiteralKind.True:
                    _emitter.Write("true");
                    break;

                case Es5LiteralKind.False:
                    _emitter.Write("false");
                    break;

                case Es5LiteralKind.Decimal:
                case Es5LiteralKind.HexInteger:
                case Es5LiteralKind.String:
                case Es5LiteralKind.RegExp:
                    _emitter.Write(node.Literal);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }

        /// <summary>
        /// Writes expressions of the form 'x = y', where the assignment operator can be any of the
        /// standard JavaScript assignment operators.
        /// </summary>
        public override void VisitAssignmentExpression(Es5AssignmentExpression node)
        {
            Visit(node.LeftSide);
            _emitter.Write($" {node.Operator.ToCodeDisplay()} ");
            Visit(node.RightSide);
        }

        /// <summary>
        /// Writes array literals of the form '[a, b, c]'.
        /// </summary>
        public override void VisitArrayLiteralExpression(Es5ArrayLiteralExpression node)
        {
            if (node.Elements.Length == 0)
            {
                _emitter.Write("[]");
                return;
            }

            _emitter.Write("[");
            WriteCommaList(node.Elements);
            _emitter.Write("]");
        }

        /// <summary>
        /// Writes object literals of the form '{ prop: value, get prop() {}, set prop(value) {} }'.
        /// </summary>
        public override void VisitObjectLiteralExpression(Es5ObjectLiteralExpression node)
        {
            int propCount = node.PropertyAssignments.Length;
            if (propCount == 0)
            {
                _emitter.Write(_options.SpaceWithinEmptyObjectInitializers ? "{ }" : "{}");
                return;
            }

            _emitter.WriteBlock(isSimpleBlock: propCount == 1, writeBodyAction: () =>
            {
                for (int i = 0; i < propCount; i++)
                {
                    IEs5PropertyAssignment assignment = node.PropertyAssignments[i];
                    assignment.Accept(this);

                    // write the comma if this isn't the last item in the list
                    if (i >= propCount - 1)
                    {
                        continue;
                    }

                    if (_options.NewlineBetweenPropertyAssignments)
                    {
                        _emitter.WriteLine(",");
                    }
                    else
                    {
                        _emitter.Write(",");
                    }
                }
            });
        }

        /// <summary>
        /// Visits a property get assignment within an object literal of the form 'get property() {}'.
        /// </summary>
        public override void VisitPropertyGetAssignment(Es5PropertyGetAssignment node)
        {
            WriteFunction(
                "get",
                node.PropertyName,
                parameters: null,
                functionBody: node.FunctionBody);
        }

        /// <summary>
        /// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        /// </summary>
        public override void VisitPropertySetAssignment(Es5PropertySetAssignment node)
        {
            WriteFunction(
                "set",
                node.PropertyName,
                node.SetParameter.ToSafeArray(),
                node.FunctionBody);
        }

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public override void VisitPropertyValueAssignment(Es5PropertyValueAssignment node)
        {
            _emitter.Write(node.PropertyName);
            _emitter.Write(": ");
            Visit(node.Value);
        }

        /// <summary>
        /// Writes an expression surrounded by parentheses.
        /// </summary>
        public override void VisitParenthesizedExpression(Es5ParenthesizedExpression node)
        {
            _emitter.Write("(");
            Visit(node.Expression);
            _emitter.Write(")");
        }

        /// <summary>
        /// Writes a function declaration expression of the form 'function name?(args) {}'.
        /// </summary>
        public override void VisitFunctionExpression(Es5FunctionExpression node)
        {
            WriteFunction("function", node.FunctionName, node.Parameters, node.FunctionBody);
        }

        /// <summary>
        /// Writes a member access expression of the form 'obj.member' or 'obj[member]'.
        /// </summary>
        public override void VisitMemberExpression(Es5MemberExpression node)
        {
            Visit(node.MemberExpression);
            if (node.IsBracketNotation)
            {
                _emitter.Write("[");
                Visit(node.BracketExpression);
                _emitter.Write("]");
            }
            else if (node.IsDotNotation)
            {
                _emitter.Write(".");
                Visit(node.DotName);
            }
        }

        /// <summary>
        /// Writes a function call expression of the form 'func(args)' or 'new func(args)'.
        /// </summary>
        public override void VisitCallExpression(Es5CallExpression node)
        {
            if (node.IsNewCall)
            {
                _emitter.Write("new ");
            }

            Visit(node.CallExpression);
            _emitter.Write("(");
            WriteCommaList(node.Arguments);
            _emitter.Write(")");
        }

        /// <summary>
        /// Writes a unary expression.
        /// </summary>
        public override void VisitUnaryExpression(Es5UnaryExpression node)
        {
            bool isPostfix = node.Operator.IsOneOf(
                Es5UnaryOperator.PostfixIncrement, Es5UnaryOperator.PostfixDecrement);

            if (!isPostfix)
            {
                _emitter.Write(node.Operator.ToCodeDisplay());
            }

            // some operators require a space after them
            if (node.Operator.IsOneOf(Es5UnaryOperator.Delete, Es5UnaryOperator.Void, Es5UnaryOperator.Typeof))
            {
                _emitter.Write(" ");
            }

            Visit(node.Operand);

            if (isPostfix)
            {
                _emitter.Write(node.Operator.ToCodeDisplay());
            }
        }

        /// <summary>
        /// Writes a binary expression.
        /// </summary>
        public override void VisitBinaryExpression(Es5BinaryExpression node)
        {
            Visit(node.LeftSide);
            _emitter.Write($" {node.Operator.ToCodeDisplay()} ");
            Visit(node.RightSide);
        }

        /// <summary>
        /// Writes a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public override void VisitConditionalExpression(Es5ConditionalExpression node)
        {
            Visit(node.Condition);
            _emitter.Write(" ? ");

            Visit(node.WhenTrue);
            _emitter.Write(" : ");

            Visit(node.WhenFalse);
        }
    }
}
