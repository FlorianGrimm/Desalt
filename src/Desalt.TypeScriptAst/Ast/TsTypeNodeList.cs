// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;
    using Factory = TsAstFactory;

    /// <summary>
    /// Represents and immutable list of <see cref="ITsType"/> nodes separated with commas and surrounded with '&lt;'
    /// and '&gt;' tokens.
    /// </summary>
    internal sealed class TsTypeNodeList : TsAstNodeList<ITsType>, ITsTypeNodeList
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents an empty list using a comma as the separator.
        /// </summary>
        public static readonly TsTypeNodeList Empty = new TsTypeNodeList(ImmutableArray<ITsType>.Empty);

        private static readonly ITsTokenNode s_separatorToken = Factory.CommaSpaceToken;
        private static readonly ITsTokenNode s_openToken = Factory.Token("<");
        private static readonly ITsTokenNode s_closeToken = Factory.Token(">");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeNodeList(ImmutableArray<ITsType> nodes)
            : this(CreateList(nodes, s_separatorToken))
        {
        }

        private TsTypeNodeList(ImmutableArray<ITsNode> nodesAndSeparators)
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
        public ITsTypeNodeList Add(params ITsType[] nodes)
        {
            return Add(Create, nodes);
        }

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        public ITsTypeNodeList Insert(int index, params ITsType[] nodes)
        {
            return Insert(Create, index, nodes);
        }

        private static TsTypeNodeList Create(ImmutableArray<ITsNode> nodesAndSeparators)
        {
            return new TsTypeNodeList(nodesAndSeparators);
        }
    }
}
