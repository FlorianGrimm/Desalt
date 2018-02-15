// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenProjectStage.cs" company="Justin Rockwood">
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
    using Microsoft.CodeAnalysis.MSBuild;

    /// <summary>
    /// Pipeline stage that opens a .csproj file using Roslyn.
    /// </summary>
    internal class OpenProjectStage : PipelineStage<CompilationRequest, Project>
    {
        public override async Task<IExtendedResult<Project>> ExecuteAsync(
            CompilationRequest input,
            CompilerOptions options,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // try to open the project
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = await workspace.OpenProjectAsync(input.ProjectFilePath, cancellationToken);

            return new ExtendedResult<Project>(project);
        }
    }
}
