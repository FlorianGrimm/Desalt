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
    using Desalt.JavaScript.CodeModels;
    using Desalt.JavaScript.CodeModels.Expressions;

    public partial class Es5Emitter
    {
        public override void VisitIdentifier(Es5Identifier model)
        {
            _emitter.Write(model.Text);
        }

        public override void VisitThisExpresssion(Es5ThisExpression model)
        {
            _emitter.Write("this");
        }

        public override void VisitLiteralExpression(Es5LiteralExpression model)
        {
            switch (model.Kind)
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
                    _emitter.Write(model.Literal);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(model));
            }
        }

        /// <summary>
        /// Writes expressions of the form 'x = y', where the assignment operator can be any of the
        /// standard JavaScript assignment operators.
        /// </summary>
        public override void VisitAssignmentExpression(Es5AssignmentExpression model)
        {
            Visit(model.LeftSide);

            if (_options.SurroundOperatorsWithSpaces)
            {
                _emitter.Write(" ");
            }
            _emitter.Write(model.Operator.ToCodeDisplay());
            if (_options.SurroundOperatorsWithSpaces)
            {
                _emitter.Write(" ");
            }

            Visit(model.RightSide);
        }

        /// <summary>
        /// Writes array literals of the form '[a, b, c]'.
        /// </summary>
        public override void VisitArrayLiteralExpression(Es5ArrayLiteralExpression model)
        {
            if (model.Elements.Length == 0)
            {
                _emitter.Write(_options.SpaceWithinEmptyArrayBrackets ? "[ ]" : "[]");
                return;
            }

            _emitter.Write("[");
            WriteCommaList(model.Elements);
            _emitter.Write("]");
        }

        /// <summary>
        /// Writes object literals of the form '{ prop: value, get prop() {}, set prop(value) {} }'.
        /// </summary>
        public override void VisitObjectLiteralExpression(Es5ObjectLiteralExpression model)
        {
            int propCount = model.PropertyAssignments.Length;
            if (propCount == 0)
            {
                _emitter.Write(_options.SpaceWithinEmptyObjectInitializers ? "{ }" : "{}");
                return;
            }

            _emitter.WriteBlock(isSimpleBlock: propCount == 1, writeBodyAction: () =>
            {
                for (int i = 0; i < propCount; i++)
                {
                    IEs5PropertyAssignment assignment = model.PropertyAssignments[i];
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
        public override void VisitPropertyGetAssignment(Es5PropertyGetAssignment model)
        {
            WriteFunction(
                "get",
                model.PropertyName,
                parameters: null,
                functionBody: model.FunctionBody);
        }

        /// <summary>
        /// Visits a property set assignment within an object literal of the form 'set property(value) {}'.
        /// </summary>
        public override void VisitPropertySetAssignment(Es5PropertySetAssignment model)
        {
            WriteFunction(
                "set",
                model.PropertyName,
                model.SetParameter.ToSafeArray(),
                model.FunctionBody);
        }

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public override void VisitPropertyValueAssignment(Es5PropertyValueAssignment model)
        {
            _emitter.Write(model.PropertyName);
            _emitter.Write(_options.SpaceAfterColon ? ": " : ":");
            Visit(model.Value);
        }

        /// <summary>
        /// Writes an expression surrounded by parentheses.
        /// </summary>
        public override void VisitParenthesizedExpression(Es5ParenthesizedExpression model)
        {
            _emitter.Write("(");
            Visit(model.Expression);
            _emitter.Write(")");
        }

        /// <summary>
        /// Writes a function declaration expression of the form 'function name?(args) {}'.
        /// </summary>
        public override void VisitFunctionExpression(Es5FunctionExpression model)
        {
            WriteFunction("function", model.FunctionName, model.Parameters, model.FunctionBody);
        }

        /// <summary>
        /// Writes a member access expression of the form 'obj.member' or 'obj[member]'.
        /// </summary>
        public override void VisitMemberExpression(Es5MemberExpression model)
        {
            Visit(model.MemberExpression);
            if (model.IsBracketNotation)
            {
                _emitter.Write("[");
                Visit(model.BracketExpression);
                _emitter.Write("]");
            }
            else if (model.IsDotNotation)
            {
                _emitter.Write(".");
                Visit(model.DotName);
            }
        }

        /// <summary>
        /// Writes a function call expression of the form 'func(args)' or 'new func(args)'.
        /// </summary>
        public override void VisitCallExpression(Es5CallExpression model)
        {
            if (model.IsNewCall)
            {
                _emitter.Write("new ");
            }

            Visit(model.CallExpression);
            _emitter.Write("(");
            WriteCommaList(model.Arguments);
            _emitter.Write(")");
        }

        /// <summary>
        /// Writes a unary expression.
        /// </summary>
        public override void VisitUnaryExpression(Es5UnaryExpression model)
        {
            bool isPostfix = model.Operator.IsOneOf(
                Es5UnaryOperator.PostfixIncrement, Es5UnaryOperator.PostfixDecrement);

            if (!isPostfix)
            {
                _emitter.Write(model.Operator.ToCodeDisplay());
            }

            // some operators require a space after them
            if (model.Operator.IsOneOf(Es5UnaryOperator.Delete, Es5UnaryOperator.Void, Es5UnaryOperator.Typeof))
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
        public override void VisitBinaryExpression(Es5BinaryExpression model)
        {
            Visit(model.LeftSide);

            string operatorString = model.Operator.ToCodeDisplay();
            bool surround = _options.SurroundOperatorsWithSpaces ||
                model.Operator.IsOneOf(Es5BinaryOperator.InstanceOf, Es5BinaryOperator.In);
            _emitter.Write(surround ? $" {operatorString} " : operatorString);

            Visit(model.RightSide);
        }

        /// <summary>
        /// Writes a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public override void VisitConditionalExpression(Es5ConditionalExpression model)
        {
            Visit(model.Condition);
            _emitter.Write(_options.SurroundOperatorsWithSpaces ? " ? " : "?");

            Visit(model.WhenTrue);
            _emitter.Write(_options.SurroundOperatorsWithSpaces ? " : " : ":");

            Visit(model.WhenFalse);
        }
    }
}
