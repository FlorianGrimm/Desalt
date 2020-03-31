// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="OperatorOverloadKind.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
{
    /// <summary>
    /// Represents the different kinds of operator overloaded methods that C# supports.
    /// </summary>
    public enum OperatorOverloadKind
    {
        /// <summary>
        /// Represents a unary <c>+</c> operator of the form <c>public static object operator +(object x)</c>.
        /// </summary>
        UnaryPlus,

        /// <summary>
        /// Represents a unary <c>-</c> operator of the form <c>public static object operator -(object x)</c>.
        /// </summary>
        UnaryNegation,

        /// <summary>
        /// Represents a logical not (<c>!</c>) operator of the form <c>public static object operator !(object x)</c>.
        /// </summary>
        LogicalNot,

        /// <summary>
        /// Represents a bitwise not (<c>~</c>) operator of the form <c>public static object operator ~(object x)</c>.
        /// </summary>
        OnesComplement,

        /// <summary>
        /// Represents a unary increment (<c>++</c>) operator of the form <c>public static object
        /// operator ++(object x)</c>.
        /// </summary>
        Increment,

        /// <summary>
        /// Represents a unary decrement (<c>--</c>) operator of the form <c>public static object
        /// operator --(object x)</c>.
        /// </summary>
        Decrement,

        /// <summary>
        /// Represents a unary <c>true</c> operator of the form <c>public static bool operator true(object x)</c>.
        /// </summary>
        True,

        /// <summary>
        /// Represents a unary <c>false</c> operator of the form <c>public static bool operator false(object x)</c>.
        /// </summary>
        False,

        /// <summary>
        /// Represents an addition <c>+</c> operator of the form <c>public static object operator
        /// +(object x, object y)</c>.
        /// </summary>
        Addition,

        /// <summary>
        /// Represents a subtraction <c>-</c> operator of the form <c>public static object operator
        /// -(object x, object y)</c>.
        /// </summary>
        Subtraction,

        /// <summary>
        /// Represents a multiplication <c>*</c> operator of the form <c>public static object operator
        /// *(object x, object y)</c>.
        /// </summary>
        Multiplication,

        /// <summary>
        /// Represents a division <c>/</c> operator of the form <c>public static object operator
        /// /(object x, object y)</c>.
        /// </summary>
        Division,

        /// <summary>
        /// Represents a remainder <c>%</c> operator of the form <c>public static object operator
        /// %(object x, object y)</c>.
        /// </summary>
        Modulus,

        /// <summary>
        /// Represents a bitwise AND <c>&amp;</c> operator of the form <c>public static object operator
        /// &amp;(object x, object y)</c>.
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// Represents a bitwise OR <c>|</c> operator of the form <c>public static object operator
        /// |(object x, object y)</c>.
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// Represents a bitwise XOR <c>^</c> operator of the form <c>public static object operator
        /// ^(object x, object y)</c>.
        /// </summary>
        BitwiseXor,

        /// <summary>
        /// Represents a left shift <c>&lt;&lt;</c> operator of the form <c>public static object operator
        /// &lt;&lt;(object x, int y)</c>.
        /// </summary>
        LeftShift,

        /// <summary>
        /// Represents a right shift <c>&gt;&gt;</c> operator of the form <c>public static object operator
        /// &gt;&gt;(object x, int y)</c>.
        /// </summary>
        RightShift,

        /// <summary>
        /// Represents an equality <c>==</c> operator of the form <c>public static bool operator
        /// ==(object x, object y)</c>.
        /// </summary>
        Equality,

        /// <summary>
        /// Represents an inequality <c>!=</c> operator of the form <c>public static bool operator
        /// !=(object x, object y)</c>.
        /// </summary>
        Inequality,

        /// <summary>
        /// Represents a less than <c>&lt;</c> operator of the form <c>public static bool operator
        /// &lt;(object x, object y)</c>.
        /// </summary>
        LessThan,

        /// <summary>
        /// Represents a less than or equal <c>&lt;=</c> operator of the form <c>public static bool operator
        /// &lt;=(object x, object y)</c>.
        /// </summary>
        LessThanEquals,

        /// <summary>
        /// Represents a greater than <c>&gt;</c> operator of the form <c>public static bool operator
        /// &gt;(object x, object y)</c>.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Represents a greater than or equal <c>&gt;=</c> operator of the form <c>public static bool operator
        /// &gt;=(object x, object y)</c>.
        /// </summary>
        GreaterThanEquals,

        /// <summary>
        /// Represents an explicit conversion operator of the form <c>public static explicit operator
        /// Type(object x)</c>.
        /// </summary>
        Explicit,

        /// <summary>
        /// Represents an implicit conversion operator of the form <c>public static implicit operator
        /// Type(object x)</c>.
        /// </summary>
        Implicit,
    }
}
