// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Options;

    /// <summary>
    /// Abstract base class for all compiler pipeline stages.
    /// </summary>
    internal abstract class PipelineStage<TInput, TOutput> : IPipelineStage<TInput, TOutput>
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the input type to the stage.
        /// </summary>
        public Type InputType => typeof(TInput);

        /// <summary>
        /// Gets the type that the stage outputs.
        /// </summary>
        public Type OutputType => typeof(TOutput);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="options">The compiler options to use.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        async Task<IExtendedResult> IPipelineStage.ExecuteAsync(
            object input,
            CompilerOptions options,
            CancellationToken cancellationToken)
        {
            return await ExecuteAsync((TInput)input, options, cancellationToken);
        }

        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="options">The compiler options to use.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        public abstract Task<IExtendedResult<TOutput>> ExecuteAsync(
            TInput input,
            CompilerOptions options,
            CancellationToken cancellationToken = default);
    }
}
