// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Validates a CSharp document for potential problems when translating to TypeScript.
    /// </summary>
    internal class CSharpToTypeScriptValidator
    {
        public async Task<IExtendedResult<bool>> ValidateDocumentAsync(
            DocumentTranslationContextWithSymbolTables context)
        {
            IValidator[] validators = { new NoDefaultParametersInInterfacesValidator() };

            // run all of the validators in parallel
            var tasks = validators.Select(v => Task.Run(() => v.Validate(context)));
            IExtendedResult<bool>[] results = await Task.WhenAll(tasks);

            IEnumerable<Diagnostic> diagnostics = results.SelectMany(result => result.Diagnostics);
            return new SuccessResult(diagnostics);
        }
    }
}
