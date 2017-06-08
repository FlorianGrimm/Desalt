// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ExpressionExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Expressions
{
    using System;
    using Desalt.JavaScript.CodeModels.Statements;

    /// <summary>
    /// Contains static extension methods for <see cref="IEs5Expression"/> objects.
    /// </summary>
    public static class Es5ExpressionExtensions
    {
        /// <summary>
        /// Creates a new <see cref="Es5ExpressionStatement"/>, wrapping the specified expression.
        /// </summary>
        /// <param name="expression">The expression to wrap inside of a statement.</param>
        /// <returns>A new <see cref="Es5ExpressionStatement"/>, wrapping the specified expression.</returns>
        public static Es5ExpressionStatement ToStatement(this IEs5Expression expression)
        {
            return new Es5ExpressionStatement(expression);
        }

        /// <summary>
        /// Creates a new <see cref="Es5ParenthesizedExpression"/> wrapping the
        /// specified expression.
        /// </summary>
        /// <param name="expression">The expression to wrap inside of parentheses.</param>
        /// <returns>
        /// A new <see cref="Es5ParenthesizedExpression"/> wrapping the specified expression.
        /// </returns>
        public static Es5ParenthesizedExpression WithParentheses(this IEs5Expression expression)
        {
            return new Es5ParenthesizedExpression(expression);
        }

        /// <summary>
        /// Converts an assignment operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this Es5AssignmentOperator assignmentOperator)
        {
            switch (assignmentOperator)
            {
                case Es5AssignmentOperator.SimpleAssign: return "=";
                case Es5AssignmentOperator.MultiplyAssign: return "*=";
                case Es5AssignmentOperator.DivideAssign: return "/=";
                case Es5AssignmentOperator.ModuloAssign: return "%=";
                case Es5AssignmentOperator.AddAssign: return "+=";
                case Es5AssignmentOperator.SubtractAssign: return "-=";
                case Es5AssignmentOperator.LeftShiftAssign: return "<<=";
                case Es5AssignmentOperator.SignedRightShiftAssign: return ">>=";
                case Es5AssignmentOperator.UnsignedRightShiftAssign: return ">>>=";
                case Es5AssignmentOperator.BitwiseAndAssign: return "&=";
                case Es5AssignmentOperator.BitwiseXorAssign: return "^=";
                case Es5AssignmentOperator.BitwiseOrAssign: return "|=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(assignmentOperator), assignmentOperator, message: null);
            }
        }

        /// <summary>
        /// Converts a unary operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this Es5UnaryOperator unaryOperator)
        {
            switch (unaryOperator)
            {
                case Es5UnaryOperator.Delete: return "delete";
                case Es5UnaryOperator.Void: return "void";
                case Es5UnaryOperator.Typeof: return "typeof";

                case Es5UnaryOperator.PrefixIncrement:
                case Es5UnaryOperator.PostfixIncrement:
                    return "++";

                case Es5UnaryOperator.PrefixDecrement:
                case Es5UnaryOperator.PostfixDecrement:
                    return "--";

                case Es5UnaryOperator.Plus: return "+";
                case Es5UnaryOperator.Minus: return "-";
                case Es5UnaryOperator.BitwiseNot: return "~";
                case Es5UnaryOperator.LogicalNot: return "!";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryOperator), unaryOperator, message: null);
            }
        }

        /// <summary>
        /// Converts a binary operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this Es5BinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case Es5BinaryOperator.Multiply: return "*";
                case Es5BinaryOperator.Divide: return "/";
                case Es5BinaryOperator.Modulo: return "%";
                case Es5BinaryOperator.Add: return "+";
                case Es5BinaryOperator.Subtract: return "-";
                case Es5BinaryOperator.LeftShift: return "<<";
                case Es5BinaryOperator.SignedRightShift: return ">>";
                case Es5BinaryOperator.UnsignedRightShift: return ">>>";
                case Es5BinaryOperator.LessThan: return "<";
                case Es5BinaryOperator.GreaterThan: return ">";
                case Es5BinaryOperator.LessThanEqual: return "<=";
                case Es5BinaryOperator.GreaterThanEqual: return ">=";
                case Es5BinaryOperator.InstanceOf: return "instanceof";
                case Es5BinaryOperator.In: return "in";
                case Es5BinaryOperator.Equals: return "==";
                case Es5BinaryOperator.NotEquals: return "!=";
                case Es5BinaryOperator.StrictEquals: return "===";
                case Es5BinaryOperator.StrictNotEquals: return "!==";
                case Es5BinaryOperator.BitwiseAnd: return "&";
                case Es5BinaryOperator.BitwiseXor: return "^";
                case Es5BinaryOperator.BitwiseOr: return "|";
                case Es5BinaryOperator.LogicalAnd: return "&&";
                case Es5BinaryOperator.LogicalOr: return "||";
                default:
                    throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null);
            }
        }
    }
}
