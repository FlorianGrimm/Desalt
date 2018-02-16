// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Contains all of the diagnostics that the program can emit.
    /// </summary>
    internal static class DiagnosticFactory
    {
        public const string TranslationCategory = "Desalt.Translation";

        private static readonly DiagnosticDescriptor s_internalError = new DiagnosticDescriptor(
            id: "DSC0001",
            title: "Internal Desalt compiler error",
            messageFormat: "Internal error: {0}",
            category: "Desalt.Internal",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Compiler);

        /// <summary>
        /// Returns a diagnostic of the form "Internal error: {0}"
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> that represents the error.</param>
        public static Diagnostic InternalError(Exception e)
        {
            return Diagnostic.Create(s_internalError, Location.None, e.Message);
        }

        private static readonly DiagnosticDescriptor s_documentContainsNoSyntaxTree = new DiagnosticDescriptor(
            id: "DSC1000",
            title: "Document has no C# syntax tree",
            messageFormat: "Document does not contain a syntax tree",
            category: TranslationCategory,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Build);

        /// <summary>
        /// Returns a diagnostic of the form "Document does not contain a syntax tree".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSyntaxTree(Document document)
        {
            return Diagnostic.Create(
                s_documentContainsNoSyntaxTree,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));
        }

        private static readonly DiagnosticDescriptor s_documentContainsNoSemanticModel = new DiagnosticDescriptor(
            id: "DSC1001",
            title: "Document has no C# semantic model",
            messageFormat: "Document does not contain a semantic model",
            category: TranslationCategory,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Build);

        /// <summary>
        /// Returns a diagnostic of the form "Document does not contain a semantic model".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSemanticModel(Document document)
        {
            return Diagnostic.Create(
                s_documentContainsNoSemanticModel,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));
        }

        private static readonly DiagnosticDescriptor s_translationNodeNotSupported = new DiagnosticDescriptor(
            id: "DSC1002",
            title: "TypeScript translation does not understand a C# syntax node",
            messageFormat: "Node of type '{0}' not supported: {1}",
            category: TranslationCategory,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Build);

        /// <summary>
        /// Returns a diagnostic of the form "Node of type '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic TranslationNotSupported(SyntaxNode node)
        {
            return Diagnostic.Create(s_translationNodeNotSupported, node.GetLocation(), node.GetType().Name, node);
        }
    }
}
