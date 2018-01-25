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

    /// <summary>
    /// Represents a success/fail result from executing a process that can produce messages.
    /// </summary>
    public class SuccessResult : ExtendedResult<bool>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public SuccessResult(bool result, IEnumerable<DiagnosticMessage> messages = null)
            : base(result, messages)
        {
        }

        public SuccessResult(bool result, params DiagnosticMessage[] messages)
            : base(result, messages)
        {
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public SuccessResult(IEnumerable<DiagnosticMessage> messages)
            : base(messages?.All(m => !m.IsError) ?? true, messages)
        {
        }

        public SuccessResult(params DiagnosticMessage[] messages)
            : this((IEnumerable<DiagnosticMessage>)messages)
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
    }
}
