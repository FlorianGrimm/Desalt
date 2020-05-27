// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArgumentNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;
    using Factory = TsAstFactory;

    /// <summary>
    /// Represents an immutable list of <see cref="ITsArgument"/> nodes separated with commas and surrounded with parentheses.
    /// </summary>
    internal sealed class TsArgumentNodeList : TsAstNodeList<ITsArgument>, ITsArgumentNodeList
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents an empty list using a comma as the separator.
        /// </summary>
        public static readonly TsArgumentNodeList Empty = new TsArgumentNodeList(ImmutableArray<ITsArgument>.Empty);

        private static readonly ITsTokenNode s_separatorToken = Factory.CommaSpaceToken;
        private static readonly ITsTokenNode s_openToken = Factory.Token("(");
        private static readonly ITsTokenNode s_closeToken = Factory.Token(")");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArgumentNodeList(ImmutableArray<ITsArgument> nodes)
            : this(CreateList(nodes, s_separatorToken))
        {
        }

        private TsArgumentNodeList(ImmutableArray<ITsNode> nodesAndSeparators)
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
        public ITsArgumentNodeList Add(params ITsArgument[] nodes)
        {
            return Add(Create, nodes);
        }

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        public ITsArgumentNodeList Insert(int index, params ITsArgument[] nodes)
        {
            return Insert(Create, index, nodes);
        }

        private static TsArgumentNodeList Create(ImmutableArray<ITsNode> nodesAndSeparators)
        {
            return new TsArgumentNodeList(nodesAndSeparators);
        }
    }
}
