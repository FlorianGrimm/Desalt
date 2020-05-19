// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsTokenNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;

    /// <summary>
    /// Base interface for all abstract syntax tree (AST) token node types. A token node is a keyword, operator,
    /// punctuator, or other structural tokens. It can have leading or trailing trivia nodes (comments typically) that
    /// control the emitting.
    /// </summary>
    public interface ITsTokenNode : ITsNode, IEquatable<ITsTokenNode>
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the string representation of the token.
        /// </summary>
        public string Token { get; }
    }
}
