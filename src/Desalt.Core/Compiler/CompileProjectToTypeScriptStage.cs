// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompileProjectToTypeScriptStage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Compiler
{
    using System.IO;
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
        /// <param name="options">The compiler options to use.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> allowing the execution to be canceled.
        /// </param>
        /// <returns>The result of the stage.</returns>
        public override async Task<IExtendedResult<bool>> ExecuteAsync(
            Project input,
            CompilerOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Directory.CreateDirectory(options.OutputPath);

            foreach (Document document in input.Documents)
            {
                string typeScriptFilePath = Path.Combine(
                    options.OutputPath,
                    Path.GetFileNameWithoutExtension(document.FilePath) + ".ts");

                using (var writer = new StreamWriter(typeScriptFilePath))
                {
                    await writer.WriteLineAsync($"// This is {typeScriptFilePath}");
                }
            }

            return new ExtendedResult<bool>(true);
        }
    }
}
