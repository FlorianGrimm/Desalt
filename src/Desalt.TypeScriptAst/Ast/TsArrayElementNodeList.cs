// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayElementList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;
    using Factory = TsAstFactory;

    /// <summary>
    /// Represents an immutable list of <see cref="ITsArrayElement"/> nodes separated with commas and surrounded by brackets.
    /// </summary>
    internal sealed class TsArrayElementNodeList : TsAstNodeList<ITsArrayElement>, ITsArrayElementNodeList
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents an empty list using a comma as the separator.
        /// </summary>
        public static readonly TsArrayElementNodeList Empty =
            new TsArrayElementNodeList(ImmutableArray<ITsArrayElement>.Empty);

        private static readonly ITsTokenNode s_separatorToken = Factory.CommaSpaceToken;
        private static readonly ITsTokenNode s_openToken = Factory.Token("[");
        private static readonly ITsTokenNode s_closeToken = Factory.Token("]");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayElementNodeList(ImmutableArray<ITsArrayElement> nodes)
            : this(CreateList(nodes, s_separatorToken))
        {
        }

        private TsArrayElementNodeList(ImmutableArray<ITsNode> nodesAndSeparators)
            : base(nodesAndSeparators, s_separatorToken, s_openToken, s_closeToken)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a copy of this list with the specified nodes added to the end.
        /// </summary>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes added to the end.</returns>
        public ITsArrayElementNodeList Add(params ITsArrayElement[] nodes)
        {
            return Add(Create, nodes);
        }

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        public ITsArrayElementNodeList Insert(int index, params ITsArrayElement[] nodes)
        {
            return Insert(Create, index, nodes);
        }

        private static TsArrayElementNodeList Create(ImmutableArray<ITsNode> nodesAndSeparators)
        {
            return new TsArrayElementNodeList(nodesAndSeparators);
        }
    }
}
