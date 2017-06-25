// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsAstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Root interface for all TypeScript abstract syntax tree (AST) nodes.
    /// </summary>
    public interface ITsAstNode : IAstNode
    {
        void Accept(TypeScriptVisitor visitor);

        T Accept<T>(TypeScriptVisitor<T> visitor);
    }
}
