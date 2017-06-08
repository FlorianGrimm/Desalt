// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeModelVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CodeModels
{
    using System;

    /// <summary>
    /// Abstract base class for an <see cref="ICodeModelVisitor{TModel, TResult}"/> visitor that
    /// visits only the single model passed into its Visit method and produces a value of the type
    /// specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to visit.</typeparam>
    /// <typeparam name="TResult">The type of the return value this visitor's Visit method.</typeparam>
    public abstract class CodeModelVisitor<TModel, TResult> : ICodeModelVisitor<TModel, TResult>
        where TModel : ICodeModel
    {
        public abstract TResult Visit(TModel model);

        public virtual TResult DefaultVisit(TModel model)
        {
            throw new InvalidOperationException($"{GetType().Name}: Model not supported: {model.GetType().Name}");
        }
    }
}
