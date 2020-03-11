// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenProjectStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CompilerStages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Buildalyzer;
    using Buildalyzer.Workspaces;
    using Desalt.Core.Pipeline;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Pipeline stage that opens a .csproj file using Roslyn.
    /// </summary>
    internal class OpenProjectStage : PipelineStage<CompilationRequest, Project>
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override async Task<IExtendedResult<Project>> ExecuteAsync(
            CompilationRequest input,
            CompilerOptions options,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await Task.Run(() => OpenProject(input.ProjectFilePath), cancellationToken);
        }

        private static IExtendedResult<Project> OpenProject(string projectFilePath)
        {
            // try to open the project
            var manager = new AnalyzerManager();
            ProjectAnalyzer projectAnalyzer = manager.GetProject(projectFilePath);
            AdhocWorkspace workspace = projectAnalyzer.GetWorkspace();

            var projectId = ProjectId.CreateFromSerialized(projectAnalyzer.ProjectGuid);
            Project project = workspace.CurrentSolution.GetProject(projectId);

            return new ExtendedResult<Project>(project);
        }
    }
}
