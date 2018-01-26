// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IAstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using Desalt.Core.Emit;

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
        /// Accepts the visitor by calling into a specific method on the visitor for this type of AST node.
        /// </summary>
        /// <param name="visitor">The visitor to visit.</param>
        void Accept(TsVisitor visitor);

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        void Emit(Emitter emitter);
    }
}
