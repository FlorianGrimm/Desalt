// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SuccessResult.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Pipeline
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        public SuccessResult(bool result, IEnumerable<Diagnostic> diagnostics = null)
            : base(result, diagnostics)
        {
        }

        public SuccessResult(bool result, params Diagnostic[] diagnostics)
            : base(result, diagnostics)
        {
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public SuccessResult(IEnumerable<Diagnostic> diagnostics)
            : base(IsSuccess(diagnostics), diagnostics)
        {
        }

        public SuccessResult(params Diagnostic[] diagnostics)
            : base(IsSuccess(diagnostics), diagnostics)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Merges the results for this instance with the results of another instance. Success is
        /// determined if both instances have a success.
        /// </summary>
        /// <param name="other">The other instance to merge.</param>
        /// <returns>A new <see cref="SuccessResult"/> with the merged results.</returns>
        public SuccessResult MergeWith(IExtendedResult<bool> other)
        {
            return new SuccessResult(Result && other.Result, Diagnostics.Concat(other.Diagnostics));
        }

        private static bool IsSuccess(IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();
            return diagnostics.All(diagnostic => diagnostic.Severity != DiagnosticSeverity.Error);
        }
    }
}
