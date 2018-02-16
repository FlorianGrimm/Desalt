// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Validates a CSharp document for potential problems when translating to TypeScript.
    /// </summary>
    internal class CSharpToTypeScriptValidator
    {
        public async Task<IExtendedResult<bool>> ValidateDocumentAsync(Document document, CompilerOptions options)
        {
            var diagnostics = new List<DiagnosticMessage>();
            await Task.Yield();

            return new ExtendedResult<bool>(diagnostics.IsSuccess(options));
        }
    }
}
