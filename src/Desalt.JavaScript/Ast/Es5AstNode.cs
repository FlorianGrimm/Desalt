// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5AstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Abstract base class for all ES5 abstract syntax tree (AST) nodes.
    /// </summary>
    public abstract class Es5AstNode : AstNode<Es5Visitor>, IEs5AstNode
    {
    }
}
