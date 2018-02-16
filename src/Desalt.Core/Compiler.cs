// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Compiler.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Desalt.Core.CompilerStages;
    using Desalt.Core.Pipeline;
    using Microsoft.CodeAnalysis;

    public class Compiler
    {
        public async Task<IExtendedResult<bool>> ExecuteAsync(CompilationRequest compilationRequest)
        {
            var pipeline = new SimplePipeline<CompilationRequest, IEnumerable<string>>();
            pipeline.AddStage(new OpenProjectStage());
            pipeline.AddStage(new DetermineTranslatableDocumentsStage());
            pipeline.AddStage(new ValidateProjectStage());
            pipeline.AddStage(new TranslateProjectStage());

            IExtendedResult<IEnumerable<string>> result = await pipeline.ExecuteAsync(compilationRequest, compilationRequest.Options);

            ImmutableArray<Diagnostic> diagnostics = result.Messages.ToImmutableArray();
            return new SuccessResult(compilationRequest.Options, diagnostics);
        }
    }
}
