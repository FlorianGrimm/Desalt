// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBinaryOperator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    /// <summary>
    /// Represents the kinds of binary operators.
    /// </summary>
    public enum TsBinaryOperator
    {
        /// <summary>
        /// The multiplication operator (*).
        /// </summary>
        Multiply,

        /// <summary>
        /// The division operator (/).
        /// </summary>
        Divide,

        /// <summary>
        /// The modulo operator (%).
        /// </summary>
        Modulo,

        /// <summary>
        /// The addition operator (+).
        /// </summary>
        Add,

        /// <summary>
        /// The subtraction operator (-).
        /// </summary>
        Subtract,

        /// <summary>
        /// The bitwise left shift operator (&lt;&lt;).
        /// </summary>
        LeftShift,

        /// <summary>
        /// The sign-filling bitwise right shift operator (&gt;&gt;).
        /// </summary>
        SignedRightShift,

        /// <summary>
        /// The zero-filling bitwise right shift operator (&gt;&gt;&gt;).
        /// </summary>
        UnsignedRightShift,

        /// <summary>
        /// The relational less-than operator (&lt;).
        /// </summary>
        LessThan,

        /// <summary>
        /// The relational greater-than operator (&gt;).
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The relational less-than-or-equal operator (&lt;=).
        /// </summary>
        LessThanEqual,

        /// <summary>
        /// The relational greater-than-or-equal operator (&gt;=).
        /// </summary>
        GreaterThanEqual,

        /// <summary>
        /// The 'instanceof' operator.
        /// </summary>
        InstanceOf,

        /// <summary>
        /// The 'in' operator.
        /// </summary>
        In,

        /// <summary>
        /// The equals operator (==).
        /// </summary>
        Equals,

        /// <summary>
        /// The not-equals operator (!=).
        /// </summary>
        NotEquals,

        /// <summary>
        /// The strict equals operator (===).
        /// </summary>
        StrictEquals,

        /// <summary>
        /// The strict not-equals operator (!==).
        /// </summary>
        StrictNotEquals,

        /// <summary>
        /// The bitwise AND operator (&amp;).
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// The bitwise XOR operator (^).
        /// </summary>
        BitwiseXor,

        /// <summary>
        /// The bitwise OR operator (|).
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// The logical AND operator (&amp;&amp;).
        /// </summary>
        LogicalAnd,

        /// <summary>
        /// The logical OR operator (||).
        /// </summary>
        LogicalOr,
    }
}
