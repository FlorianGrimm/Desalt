// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5UnaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a unary expression.
    /// </summary>
    public sealed class Es5UnaryExpression : AstNode<Es5Visitor>, IEs5Expression
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

        public override string CodeDisplay
        {
            get
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
        }

        public override void Emit(Emitter emitter)
        {
            switch (Operator)
            {
                case Es5UnaryOperator.Delete:
                case Es5UnaryOperator.Void:
                case Es5UnaryOperator.Typeof:
                    emitter.Write($"{Operator.ToCodeDisplay()}");
                    Operand.Emit(emitter);
                    break;

                case Es5UnaryOperator.PrefixIncrement:
                case Es5UnaryOperator.PrefixDecrement:
                case Es5UnaryOperator.Plus:
                case Es5UnaryOperator.Minus:
                case Es5UnaryOperator.BitwiseNot:
                case Es5UnaryOperator.LogicalNot:
                    emitter.Write(Operator.ToCodeDisplay());
                    Operand.Emit(emitter);
                    break;

                case Es5UnaryOperator.PostfixIncrement:
                case Es5UnaryOperator.PostfixDecrement:
                    Operand.Emit(emitter);
                    emitter.Write(Operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator));
            }
        }
    }
}
