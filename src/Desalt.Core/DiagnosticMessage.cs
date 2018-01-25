// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticMessage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents an error, warning or other compiler diagnostic message.
    /// </summary>
    public class DiagnosticMessage
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly DiagnosticSeverity _severity;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DiagnosticMessage(DiagnosticSeverity severity, string message)
        {
            if (severity == DiagnosticSeverity.Hidden)
            {
                throw new ArgumentException("Hidden diagnostics should not be created.", nameof(severity));
            }

            _severity = severity;
            Message = message;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsError => _severity == DiagnosticSeverity.Error;

        public bool IsWarning => _severity == DiagnosticSeverity.Warning;

        public bool IsInfo => _severity == DiagnosticSeverity.Info;

        public string Message { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Create a new <see cref="DiagnosticMessage"/> instance that represents an error.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <returns>A new <see cref="DiagnosticMessage"/> instance.</returns>
        public static DiagnosticMessage Error(string message)
        {
            return new DiagnosticMessage(DiagnosticSeverity.Error, message);
        }

        /// <summary>
        /// Create a new <see cref="DiagnosticMessage"/> instance that represents a warning.
        /// </summary>
        /// <param name="message">A warning message.</param>
        /// <returns>A new <see cref="DiagnosticMessage"/> instance.</returns>
        public static DiagnosticMessage Warning(string message)
        {
            return new DiagnosticMessage(DiagnosticSeverity.Warning, message);
        }

        /// <summary>
        /// Create a new <see cref="DiagnosticMessage"/> instance that represents an informational message.
        /// </summary>
        /// <param name="message">An informational message.</param>
        /// <returns>A new <see cref="DiagnosticMessage"/> instance.</returns>
        public static DiagnosticMessage Info(string message)
        {
            return new DiagnosticMessage(DiagnosticSeverity.Info, message);
        }

        /// <summary>
        /// Creates a new <see cref="DiagnosticMessage"/> instance using the specified exception as
        /// the message and severity.
        /// </summary>
        /// <param name="exception">The exception to convert to a <see cref="DiagnosticMessage"/>.</param>
        /// <param name="prefixMessage">An optional message to prefix to the exception's message.</param>
        /// <returns>A new <see cref="DiagnosticMessage"/> instance.</returns>
        public static DiagnosticMessage FromException(Exception exception, string prefixMessage = null)
        {
            string message = string.IsNullOrEmpty(prefixMessage)
                ? exception.Message
                : $"{prefixMessage} ({exception.Message})";

            return new DiagnosticMessage(DiagnosticSeverity.Error, message);
        }

        internal static DiagnosticMessage FromDiagnostic(Diagnostic diagnostic)
        {
            return new DiagnosticMessage(diagnostic.Severity, diagnostic.ToString());
        }
    }
}
