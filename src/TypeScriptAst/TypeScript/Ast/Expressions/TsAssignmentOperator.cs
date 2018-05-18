// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAssignmentOperator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Expressions
{
    /// <summary>
    /// Contains all of the assignment operators.
    /// </summary>
    public enum TsAssignmentOperator
    {
        /// <summary>
        /// The simple assign operator (=).
        /// </summary>
        SimpleAssign,

        /// <summary>
        /// The multiply then assign operator (*=).
        /// </summary>
        MultiplyAssign,

        /// <summary>
        /// The divide then assign operator (/=).
        /// </summary>
        DivideAssign,

        /// <summary>
        /// The modulo then assign operator (%=).
        /// </summary>
        ModuloAssign,

        /// <summary>
        /// The add then assign operator (+=).
        /// </summary>
        AddAssign,

        /// <summary>
        /// The subtract then assign operator (-=).
        /// </summary>
        SubtractAssign,

        /// <summary>
        /// The left shift then assign operator (&lt;&lt;=).
        /// </summary>
        LeftShiftAssign,

        /// <summary>
        /// The sign-filling bitwise right shift then assign operator (&gt;&gt;=).
        /// </summary>
        SignedRightShiftAssign,

        /// <summary>
        /// The zero-filling bitwise right shift then assign operator (&gt;&gt;&gt;=).
        /// </summary>
        UnsignedRightShiftAssign,

        /// <summary>
        /// The bitwise AND then assign operator (&amp;=).
        /// </summary>
        BitwiseAndAssign,

        /// <summary>
        /// The bitwise XOR then assign operator (^=).
        /// </summary>
        BitwiseXorAssign,

        /// <summary>
        /// The bitwise OR then assign operator (|=).
        /// </summary>
        BitwiseOrAssign,
    }
}
