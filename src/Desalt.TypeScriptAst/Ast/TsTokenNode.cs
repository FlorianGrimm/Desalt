// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTokenNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents all abstract syntax tree (AST) token node types. A token node is a keyword, operator, punctuator, or
    /// other structural tokens. It can have leading or trailing trivia nodes (comments typically) that control the emitting.
    /// </summary>
    internal sealed class TsTokenNode : TsNode, ITsTokenNode, IEquatable<ITsTokenNode>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsTokenNode CloseBracket = new TsTokenNode("]");
        public static readonly TsTokenNode CloseParen = new TsTokenNode(")");
        public static readonly TsTokenNode Comma = new TsTokenNode(",");
        public static readonly TsTokenNode Ellipsis = new TsTokenNode("...");
        public static readonly TsTokenNode OpenBracket = new TsTokenNode("[");
        public static readonly TsTokenNode OpenParen = new TsTokenNode("(");
        public static readonly TsTokenNode Semicolon = new TsTokenNode(";");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTokenNode(
            string token,
            ImmutableArray<ITsAstTriviaNode>? leadingTrivia = null,
            ImmutableArray<ITsAstTriviaNode>? trailingTrivia = null)
            : base(leadingTrivia, trailingTrivia)
        {
            Token = token;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the string representation of the token.
        /// </summary>
        public string Token { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>. This will be called after the
        /// leading trivia has been emitted and before the trailing trivia.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write(Token);
        }

        /// <summary>
        /// Creates a shallow copy of this node with the leading and trailing trivia replaced with the specified values.
        /// </summary>
        /// <param name="leadingTrivia">The new leading trivia for the node.</param>
        /// <param name="trailingTrivia">The new trailing trivia for the node.</param>
        /// <returns>A copy of this node with the trivia replaced.</returns>
        public override ITsNode ShallowCopy(
            ImmutableArray<ITsAstTriviaNode> leadingTrivia,
            ImmutableArray<ITsAstTriviaNode> trailingTrivia)
        {
            return LeadingTrivia == leadingTrivia && TrailingTrivia == trailingTrivia
                ? this
                : new TsTokenNode(Token, leadingTrivia, trailingTrivia);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        bool IEquatable<ITsTokenNode>.Equals(ITsTokenNode other)
        {
            return Equals(other as TsTokenNode);
        }
    }
}
