// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SuccessResult.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a success/fail result from executing a process that can produce diagnostics.
    /// </summary>
    internal class SuccessResult : ExtendedResult<bool>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public SuccessResult(IEnumerable<Diagnostic> filteredDiagnostics = null) : base(
            (filteredDiagnostics ?? Enumerable.Empty<Diagnostic>()).Any(d => d.Severity == DiagnosticSeverity.Error),
            filteredDiagnostics)
        {
        }
    }
}
