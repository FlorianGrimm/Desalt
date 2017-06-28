// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IAstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Ast
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Root interface for all abstract syntax tree (AST) node types.
    /// </summary>
    public interface IAstNode
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        string CodeDisplay { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a string representation of the full AST node, which is useful for debugging and
        /// printing to logs. This should not be used to actually emit generated code.
        /// </summary>
        /// <returns>A string representation of the full AST node.</returns>
        string ToFullCodeDisplay();

        /// <summary>
        /// Writes a string representation of this AST node to the specified <see
        /// cref="IndentedTextWriter"/>, which is useful for debugging and printing to logs. This
        /// should not be used to actually emit generated code.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        void WriteFullCodeDisplay(IndentedTextWriter writer);
    }
}
