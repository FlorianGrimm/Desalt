// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitter.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Emit
{
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast;
    using Desalt.TypeScript.Ast.Expressions;

    public partial class TsEmitter
    {
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

            string operatorString = node.Operator.ToCodeDisplay();
            bool surround = _options.SurroundOperatorsWithSpaces ||
                node.Operator.IsOneOf(TsBinaryOperator.InstanceOf, TsBinaryOperator.In);
            _emitter.Write(surround ? $" {operatorString} " : operatorString);

            Visit(node.RightSide);
        }

        /// <summary>
        /// Writes a conditional expression of the form 'x ? y : z'.
        /// </summary>
        public override void VisitConditionalExpression(ITsConditionalExpression node)
        {
            Visit(node.Condition);
            _emitter.Write(_options.SurroundOperatorsWithSpaces ? " ? " : "?");

            Visit(node.WhenTrue);
            _emitter.Write(_options.SurroundOperatorsWithSpaces ? " : " : ":");

            Visit(node.WhenFalse);
        }
    }
}
