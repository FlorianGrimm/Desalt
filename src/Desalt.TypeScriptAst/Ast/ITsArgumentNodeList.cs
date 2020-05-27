// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsArgumentNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    /// <summary>
    /// Represents an immutable list of <see cref="ITsArgument"/> nodes separated with commas and surrounded with parentheses.
    /// </summary>
    public interface ITsArgumentNodeList : ITsAstNodeList<ITsArgument>
    {
        /// <summary>
        /// Creates a copy of this list with the specified nodes added to the end.
        /// </summary>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes added to the end.</returns>
        ITsArgumentNodeList Add(params ITsArgument[] nodes);

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        ITsArgumentNodeList Insert(int index, params ITsArgument[] nodes);
    }
}
