// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImmutableDiagnosticArray.Descriptors.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    internal partial class DiagnosticFactory
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private const string IdPrefix = "DSC";
        private const string TranslationCategory = "Desalt.Translation";

        private enum DiagnosticId
        {
            [Error(
                1,
                "Internal Desalt compiler error",
                "Internal error: {0}",
                customTags: new[] { WellKnownDiagnosticTags.Compiler, WellKnownDiagnosticTags.NotConfigurable })]
            InternalError,

            [Warning(1000, "Document has no C# syntax tree")]
            DocumentContainsNoSyntaxTree,

            [Warning(1001, "Document has no C# semantic model")]
            DocumentContainsNoSemanticModel,

            [Error(
                1002,
                "TypeScript translation does not understand a C# syntax node",
                "C# syntax node of type '{0}' not supported: {1}")]
            TranslationNotSupported
        }

        //// ===========================================================================================================
        //// Diagnostic Factory Creation Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a diagnostic of the form "Internal error: {0}"
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> that represents the error.</param>
        public static Diagnostic InternalError(Exception e) => Create(DiagnosticId.InternalError, Location.None, e.Message);

        /// <summary>
        /// Returns a diagnostic of the form "Document has no C# syntax tree".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSyntaxTree(Document document) =>
            Create(
                DiagnosticId.DocumentContainsNoSyntaxTree,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));

        /// <summary>
        /// Returns a diagnostic of the form "Document has no C# semantic model".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSemanticModel(Document document) =>
            Create(
                DiagnosticId.DocumentContainsNoSemanticModel,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));

        /// <summary>
        /// Returns a diagnostic of the form "C# syntax node of type '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic TranslationNotSupported(SyntaxNode node) =>
            Create(DiagnosticId.TranslationNotSupported, node.GetLocation(), node.GetType().Name, node);
    }
}
