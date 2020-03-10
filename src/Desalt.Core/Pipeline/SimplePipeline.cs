// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePipeline.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;

    /// <summary>
    /// Represents a simple pipeline that keeps track of stages to be run in order.
    /// </summary>
    /// <typeparam name="TInput">The type that the starting stage requires.</typeparam>
    /// <typeparam name="TOutput">The type that the last stage outputs.</typeparam>
    internal class SimplePipeline<TInput, TOutput>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the stages of the pipeline.
        /// </summary>
        public IEnumerable<IPipelineStage> Stages => StagesInner.AsEnumerable();

        /// <summary>
        /// Gets the internal implementation of <see cref="Stages"/> as an <see cref="IList{T}"/> for
        /// derived classes to use.
        /// </summary>
        protected IList<IPipelineStage> StagesInner { get; } = new List<IPipelineStage>();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adds a stage to the pipeline. The stage's input need to match an output from a previous stage.
        /// </summary>
        /// <param name="stage">The stage to add.</param>
        public void AddStage(IPipelineStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            // check the first stage to make sure it accepts TInput
            if (StagesInner.Count == 0 && !typeof(TInput).IsAssignableFrom(stage.InputType))
            {
                throw new ArgumentException(
                    $"The first stage in the pipeline must accept an input type of {typeof(TInput).Name}",
                    nameof(stage));
            }

            // Make sure a previous stage outputs something that can be accepted as input to this stage.
            // Note: This is O(n^2) in building the list, but that's acceptable because the list of
            // stages are typically very short.
            if (StagesInner.Count > 0 && !StagesInner.Any(s => stage.InputType.IsAssignableFrom(s.OutputType)))
            {
                throw new ArgumentException(
                    $"There are no previous stages that output a type of {stage.InputType.Name}",
                    nameof(stage));
            }

            StagesInner.Add(stage);
        }

        /// <summary>
        /// Executes the pipeline, starting with the specified input.
        /// </summary>
        /// <param name="input">The starting value to the pipeline.</param>
        /// <param name="options">The compiler options to use.</param>
        /// <returns>
        /// The result of running all of the stages of the pipeline in order. If there is a failure
        /// in one of the stages, the pipeline ends early.
        /// </returns>
        public async Task<IExtendedResult<TOutput>> ExecuteAsync(TInput input, CompilerOptions options)
        {
            ValidateStages();

            var previousOutputs = new List<object> { input };

            var diagnostics = DiagnosticList.Create(options);
            foreach (IPipelineStage stage in StagesInner)
            {
                // find the next input, which is the latest previous output of a compatible type
                object nextInput = previousOutputs.FindLast(item => stage.InputType.IsInstanceOfType(item));
                if (nextInput == null)
                {
                    throw new InvalidOperationException(
                        $"No previous outputs were found for input type '{stage.InputType.Name}'");
                }

                IExtendedResult stageResult = await stage.ExecuteAsync(nextInput, options);
                previousOutputs.Add(stageResult.Result);
                diagnostics.AddRange(stageResult.Diagnostics);

                // don't continue the pipeline if there are errors
                if (diagnostics.HasErrors)
                {
                    previousOutputs.Add(default(TOutput));
                    break;
                }
            }

            return new ExtendedResult<TOutput>((TOutput)previousOutputs.Last(), diagnostics);
        }

        /// <summary>
        /// Validates the stages by making sure the last output matches the output of the pipeline.
        /// </summary>
        private void ValidateStages()
        {
            IPipelineStage lastStage = StagesInner.Last();
            if (!typeof(TOutput).IsAssignableFrom(lastStage.OutputType))
            {
                throw new InvalidOperationException(
                    $"The last stage outputs type '{lastStage.OutputType.Name}' but it should " +
                    $"output type '{typeof(TOutput).Name}'.");
            }
        }
    }
}
