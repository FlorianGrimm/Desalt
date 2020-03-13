// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Compiler.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Desalt.Core.CompilerStages;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;

    public class Compiler
    {
        public async Task<IExtendedResult<bool>> ExecuteAsync(CompilationRequest compilationRequest)
        {
            var pipeline = new SimplePipeline<CompilationRequest, IEnumerable<string>>();
            pipeline.AddStage(new OpenProjectStage());
            pipeline.AddStage(new DetermineTranslatableDocumentsStage());
            pipeline.AddStage(new CreateSymbolTablesStage());
            pipeline.AddStage(new ValidateProjectStage());
            pipeline.AddStage(new TranslateProjectStage());

            IExtendedResult<IEnumerable<string>?> result = await pipeline.ExecuteAsync(
                compilationRequest,
                compilationRequest.Options);

            return new SuccessOnNoErrorsResult(DiagnosticList.From(compilationRequest.Options, result.Diagnostics));
        }
    }
}
