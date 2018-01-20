// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompileProjectToTypeScriptStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Compiler
{
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that compiles all of the C# files in a .csproj file to TypeScript.
    /// </summary>
    internal class CompileProjectToTypeScriptStage : PipelineStage<Project, bool>
    {
        /// <summary>
        /// Executes the pipeline stage.
        /// </summary>
        /// <param name="input">The input to the stage.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        public override Task<IExtendedResult<bool>> ExecuteAsync(
            Project input,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<IExtendedResult<bool>>(new ExtendedResult<bool>(true));
        }
    }
}
