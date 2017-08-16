// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Pipeline
{
    using System;
    using System.Threading;

    /// <summary>
    /// Service contract for a class participating in a <see cref="SimplePipeline{TInput,TOutput}"/>.
    /// </summary>
    internal interface IPipelineStage
    {
        /// <summary>
        /// Gets the input type to the stage.
        /// </summary>
        Type InputType { get; }

        /// <summary>
        /// Gets the type that the stage outputs.
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        IExtendedResult Execute(object input, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Service contract for a class participating in a <see cref="SimplePipeline{TInput,TOutput}"/>.
    /// </summary>
    /// <typeparam name="TInput">The type of the stage's input.</typeparam>
    /// <typeparam name="TOutput">The type of the stage's output.</typeparam>
    internal interface IPipelineStage<in TInput, out TOutput> : IPipelineStage
    {
        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        IExtendedResult<TOutput> Execute(TInput input, CancellationToken cancellationToken = default(CancellationToken));
    }
}
