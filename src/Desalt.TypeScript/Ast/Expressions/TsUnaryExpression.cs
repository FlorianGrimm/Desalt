// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsUnaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a unary expression.
    /// </summary>
    internal class TsUnaryExpression : AstNode, ITsUnaryExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsUnaryExpression(ITsExpression operand, TsUnaryOperator @operator)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            Operator = @operator;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Operand { get; }
        public TsUnaryOperator Operator { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitUnaryExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitUnaryExpression(this);

        public override string CodeDisplay
        {
            get
            {
                switch (Operator)
                {
                    case TsUnaryOperator.Delete:
                    case TsUnaryOperator.Void:
                    case TsUnaryOperator.Typeof:
                        return $"{Operator.ToCodeDisplay()} {Operand}";

                    case TsUnaryOperator.PrefixIncrement:
                    case TsUnaryOperator.PrefixDecrement:
                    case TsUnaryOperator.Plus:
                    case TsUnaryOperator.Minus:
                    case TsUnaryOperator.BitwiseNot:
                    case TsUnaryOperator.LogicalNot:
                        return $"{Operator.ToCodeDisplay()}{Operand}";

                    case TsUnaryOperator.PostfixIncrement:
                    case TsUnaryOperator.PostfixDecrement:
                        return $"{Operand}{Operator.ToCodeDisplay()}";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Operator));
                }
            }
        }

        public override void Emit(IndentedTextWriter writer)
        {
            switch (Operator)
            {
                case TsUnaryOperator.Delete:
                case TsUnaryOperator.Void:
                case TsUnaryOperator.Typeof:
                    writer.Write($"{Operator.ToCodeDisplay()}");
                    Operand.Emit(writer);
                    break;

                case TsUnaryOperator.PrefixIncrement:
                case TsUnaryOperator.PrefixDecrement:
                case TsUnaryOperator.Plus:
                case TsUnaryOperator.Minus:
                case TsUnaryOperator.BitwiseNot:
                case TsUnaryOperator.LogicalNot:
                    writer.Write(Operator.ToCodeDisplay());
                    Operand.Emit(writer);
                    break;

                case TsUnaryOperator.PostfixIncrement:
                case TsUnaryOperator.PostfixDecrement:
                    Operand.Emit(writer);
                    writer.Write(Operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator));
            }
        }
    }
}
