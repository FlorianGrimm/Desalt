// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5UnaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a unary expression.
    /// </summary>
    public sealed class Es5UnaryExpression : Es5CodeModel, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5UnaryExpression(IEs5Expression operand, Es5UnaryOperator @operator)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            Operator = @operator;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Operand { get; }
        public Es5UnaryOperator Operator { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitUnaryExpression(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }

        public override string ToCodeDisplay()
        {
            switch (Operator)
            {
                case Es5UnaryOperator.Delete:
                case Es5UnaryOperator.Void:
                case Es5UnaryOperator.Typeof:
                    return $"{Operator.ToCodeDisplay()} {Operand}";

                case Es5UnaryOperator.PrefixIncrement:
                case Es5UnaryOperator.PrefixDecrement:
                case Es5UnaryOperator.Plus:
                case Es5UnaryOperator.Minus:
                case Es5UnaryOperator.BitwiseNot:
                case Es5UnaryOperator.LogicalNot:
                    return $"{Operator.ToCodeDisplay()}{Operand}";

                case Es5UnaryOperator.PostfixIncrement:
                case Es5UnaryOperator.PostfixDecrement:
                    return $"{Operand}{Operator.ToCodeDisplay()}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator));
            }
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            switch (Operator)
            {
                case Es5UnaryOperator.Delete:
                case Es5UnaryOperator.Void:
                case Es5UnaryOperator.Typeof:
                    writer.Write($"{Operator.ToCodeDisplay()}");
                    Operand.WriteFullCodeDisplay(writer);
                    break;

                case Es5UnaryOperator.PrefixIncrement:
                case Es5UnaryOperator.PrefixDecrement:
                case Es5UnaryOperator.Plus:
                case Es5UnaryOperator.Minus:
                case Es5UnaryOperator.BitwiseNot:
                case Es5UnaryOperator.LogicalNot:
                    writer.Write(Operator.ToCodeDisplay());
                    Operand.WriteFullCodeDisplay(writer);
                    break;

                case Es5UnaryOperator.PostfixIncrement:
                case Es5UnaryOperator.PostfixDecrement:
                    Operand.WriteFullCodeDisplay(writer);
                    writer.Write(Operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator));
            }
        }
    }
}
