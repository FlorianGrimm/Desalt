// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IAstVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Ast
{
    /// <summary>
    /// Service contract for a class that visits nodes in an abstract syntax tree (AST).
    /// </summary>
    public interface IAstVisitor
    {
        void Visit(IAstNode node);
    }
}
