// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsUnaryOperator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    /// <summary>
    /// Represents the different types of TypeScript unary expression operators.
    /// </summary>
    public enum TsUnaryOperator
    {
        /// <summary>
        /// The 'delete' operator.
        /// </summary>
        Delete,

        /// <summary>
        /// The 'void' operator, which returns 'undefined'.
        /// </summary>
        Void,

        /// <summary>
        /// The 'typeof' operator.
        /// </summary>
        Typeof,

        /// <summary>
        /// The prefix increment (++) operator.
        /// </summary>
        PrefixIncrement,

        /// <summary>
        /// The prefix decrement (--) operator.
        /// </summary>
        PrefixDecrement,

        /// <summary>
        /// The postfix increment (++) operator.
        /// </summary>
        PostfixIncrement,

        /// <summary>
        /// The postfix decrement (--) operator.
        /// </summary>
        PostfixDecrement,

        /// <summary>
        /// Converts its operand to a Number type.
        /// </summary>
        Plus,

        /// <summary>
        /// Converts its operand to a Number type and then negates it.
        /// </summary>
        Minus,

        /// <summary>
        /// The bitwise NOT (~) operator.
        /// </summary>
        BitwiseNot,

        /// <summary>
        /// The logical not (!) operator.
        /// </summary>
        LogicalNot,

        /// <summary>
        /// The cast (&lt;type&gt;) operator.
        /// </summary>
        Cast,
    }
}
