// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SuccessResult.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a success/fail result from executing a process that can produce messages.
    /// </summary>
    public class SuccessResult : ExtendedResult<bool>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public SuccessResult(bool result, IEnumerable<Diagnostic> messages = null)
            : base(result, messages)
        {
        }

        public SuccessResult(bool result, params Diagnostic[] messages)
            : base(result, messages)
        {
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public SuccessResult(CompilerOptions options, IEnumerable<Diagnostic> messages)
            : base(IsSuccess(options.WarningsAsErrors, messages), messages)
        {
        }

        public SuccessResult(CompilerOptions options, params Diagnostic[] messages)
            : base(IsSuccess(options.WarningsAsErrors, messages), messages)
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
            return new SuccessResult(Result && other.Result, Messages.Concat(other.Messages));
        }

        private static bool IsSuccess(bool warningsAsErrors, IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();

            if (warningsAsErrors)
            {
                return diagnostics.All(
                    diagnostic => !diagnostic.Severity.IsOneOf(DiagnosticSeverity.Error, DiagnosticSeverity.Warning));
            }

            return diagnostics.All(diagnostic => diagnostic.Severity != DiagnosticSeverity.Error);
        }
    }
}
