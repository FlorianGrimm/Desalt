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
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        IExtendedResult IPipelineStage.Execute(object input, CancellationToken cancellationToken)
        {
            return Execute((TInput)input, cancellationToken);
        }

        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        public abstract IExtendedResult<TOutput> Execute(
            TInput input,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
