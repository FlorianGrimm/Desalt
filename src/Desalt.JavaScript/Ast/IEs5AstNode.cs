// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IEs5AstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Root interface for all ES5 abstract syntax tree (AST) nodes.
    /// </summary>
    public interface IEs5AstNode : IAstNode
    {
        void Accept(Es5Visitor visitor);

        T Accept<T>(Es5Visitor<T> visitor);
    }
}
