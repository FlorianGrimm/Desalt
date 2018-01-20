// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DesaltCompiler.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Threading.Tasks;
    using Desalt.Core.Compiler;
    using Desalt.Core.Pipeline;

    public class DesaltCompiler
    {
        public async Task<IExtendedResult<bool>> ExecuteAsync(CompilationRequest compilationRequest)
        {
            var pipeline = new SimplePipeline<CompilationRequest, bool>();
            pipeline.AddStage(new OpenProjectStage());
            pipeline.AddStage(new CompileProjectToTypeScriptStage());

            return await pipeline.ExecuteAsync(compilationRequest);
        }
    }
}
