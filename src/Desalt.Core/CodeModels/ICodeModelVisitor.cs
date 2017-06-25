// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ICodeModelVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CodeModels
{
    /// <summary>
    /// Service contract for a class that visits nodes in a code model tree.
    /// </summary>
    /// <typeparam name="TModel">The type of nodes in the tree.</typeparam>
    public interface ICodeModelVisitor<in TModel> where TModel : IAstNode
    {
        void DefaultVisit(TModel model);

        void Visit(TModel model);
    }

    /// <summary>
    /// Service contract for a class that visits nodes in a code model tree and returns a result
    /// after visiting the node.
    /// </summary>
    /// <typeparam name="TModel">The type of nodes in the tree.</typeparam>
    /// <typeparam name="TResult">The type of the result after visiting the code model node.</typeparam>
    public interface ICodeModelVisitor<in TModel, out TResult> where TModel : IAstNode
    {
        TResult DefaultVisit(TModel model);

        TResult Visit(TModel model);
    }
}
