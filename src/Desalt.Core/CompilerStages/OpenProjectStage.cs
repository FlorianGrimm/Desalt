// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenProjectStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CompilerStages
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    /// <summary>
    /// Pipeline stage that opens a .csproj file using Roslyn.
    /// </summary>
    internal class OpenProjectStage : PipelineStage<CompilationRequest, Project>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly Regex s_workspaceErrorMessagePattern = new Regex(
            @"(?<id>[^:]*): (?<message>.*)",
            RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override async Task<IExtendedResult<Project>> ExecuteAsync(
            CompilationRequest input,
            CompilerOptions options,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                // register the MSBuild locator so that it can find the right assemblies for MSBuild (see
                // https://github.com/Microsoft/msbuild/issues/1889 and
                // https://docs.microsoft.com/en-us/visualstudio/msbuild/updating-an-existing-application?view=vs-2017)
                MSBuildLocator.RegisterDefaults();

                // try to open the project
                var workspace = MSBuildWorkspace.Create();
                Project project = await workspace.OpenProjectAsync(
                    input.ProjectFilePath,
                    progress: null,
                    cancellationToken: cancellationToken);

                var diagnostics = workspace.Diagnostics.Where(IsValidError).Select(WorkspaceDiagnosticToDiagnostic);
                return new ExtendedResult<Project>(project, diagnostics);
            }
            catch (ReflectionTypeLoadException e)
            {
                string subMessages = e.LoaderExceptions.Aggregate(
                    new StringBuilder().AppendLine().AppendLine("LoaderExceptions:"),
                    (builder, ex) => builder.AppendLine(ex.Message),
                    builder => builder.ToString());
                throw new Exception($"{e.Message}{subMessages}");
            }
        }

        /// <summary>
        /// Determines if the specified <see cref="WorkspaceDiagnostic"/> is "valid", meaning that
        /// it's not one of the messages that we want to ignore.
        /// </summary>
        /// <param name="diagnostic">The <see cref="WorkspaceDiagnostic"/> to check.</param>
        /// <returns>True if the error is "valid" and should be reported; otherwise, false.</returns>
        private static bool IsValidError(WorkspaceDiagnostic diagnostic)
        {
            // This error occurs because of a dynamic binding issue. We don't actually need to run
            // the SCTask anyway, so just ignore it.
            return !diagnostic.Message.Contains("The \"SCTask\" task failed unexpectedly");
        }

        /// <summary>
        /// Converts a <see cref="WorkspaceDiagnostic"/> to a <see cref="Diagnostic"/> to use in the
        /// compiler pipeline.
        /// </summary>
        /// <param name="workspaceDiagnostic">
        /// A diagnostic issued when opening or compiling the project.
        /// </param>
        /// <returns>A compatible <see cref="Diagnostic"/> for use in the compiler pipeline.</returns>
        private static Diagnostic WorkspaceDiagnosticToDiagnostic(WorkspaceDiagnostic workspaceDiagnostic)
        {
            // parse the message to get the id and the message parts
            Match match = s_workspaceErrorMessagePattern.Match(workspaceDiagnostic.Message);
            string id = match.Success ? match.Groups["id"].Value : "MSBuildWorkspace001";
            string message = match.Success ? match.Groups["message"].Value : workspaceDiagnostic.Message;

            DiagnosticSeverity severity = workspaceDiagnostic.Kind == WorkspaceDiagnosticKind.Failure
                ? DiagnosticSeverity.Error
                : DiagnosticSeverity.Warning;
            int warningLevel = severity == DiagnosticSeverity.Error ? 0 : (int)WarningLevel.Important;

            return Diagnostic.Create(
                id,
                category: "Workspace",
                message: message,
                severity: severity,
                defaultSeverity: severity,
                isEnabledByDefault: true,
                warningLevel: warningLevel);
        }
    }
}
