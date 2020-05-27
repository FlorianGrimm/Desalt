// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsTypeNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    /// <summary>
    /// Represents and immutable list of <see cref="ITsType"/> nodes separated with commas and surrounded with '&lt;'
    /// and '&gt;' tokens.
    /// </summary>
    public interface ITsTypeNodeList : ITsAstNodeList<ITsType>
    {
        /// <summary>
        /// Creates a copy of this list with the specified nodes added to the end.
        /// </summary>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes added to the end.</returns>
        ITsTypeNodeList Add(params ITsType[] nodes);

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        ITsTypeNodeList Insert(int index, params ITsType[] nodes);
    }
}
