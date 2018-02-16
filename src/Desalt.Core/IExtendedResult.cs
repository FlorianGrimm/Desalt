// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedResult.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents the results from executing a process that can produce messages.
    /// </summary>
    public interface IExtendedResult
    {
        /// <summary>
        /// Gets the result of the operation.
        /// </summary>
        object Result { get; }

        /// <summary>
        /// Gets all of the messages in the order in which they were generated.
        /// </summary>
        ImmutableArray<Diagnostic> Messages { get; }

        /// <summary>
        /// Gets the count of errors.
        /// </summary>
        int ErrorCount { get; }

        /// <summary>
        /// Gets a value indicating if there are any errors.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Gets a value indicating if there are any warnings.
        /// </summary>
        bool HasWarnings { get; }

        /// <summary>
        /// Gets a value indicating if the overall result is a success, meaning that there are no
        /// errors. Warnings are allowed.
        /// </summary>
        bool Success { get; }
    }

    /// <summary>
    /// Represents the results from executing a process that can produce messages.
    /// </summary>
    public interface IExtendedResult<out T> : IExtendedResult
    {
        /// <summary>
        /// Gets the result of the operation.
        /// </summary>
        new T Result { get; }
    }
}
