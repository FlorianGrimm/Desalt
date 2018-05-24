// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsUnaryExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a unary expression.
    /// </summary>
    internal class TsUnaryExpression : TsAstNode, ITsUnaryExpression
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

        public override void Accept(TsVisitor visitor) => visitor.VisitUnaryExpression(this);

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

        protected override void EmitInternal(Emitter emitter)
        {
            switch (Operator)
            {
                case TsUnaryOperator.Delete:
                case TsUnaryOperator.Void:
                case TsUnaryOperator.Typeof:
                    emitter.Write($"{Operator.ToCodeDisplay()} ");
                    Operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PrefixIncrement:
                case TsUnaryOperator.PrefixDecrement:
                case TsUnaryOperator.Plus:
                case TsUnaryOperator.Minus:
                case TsUnaryOperator.BitwiseNot:
                case TsUnaryOperator.LogicalNot:
                    emitter.Write(Operator.ToCodeDisplay());
                    Operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PostfixIncrement:
                case TsUnaryOperator.PostfixDecrement:
                    Operand.Emit(emitter);
                    emitter.Write(Operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator));
            }
        }
    }
}
