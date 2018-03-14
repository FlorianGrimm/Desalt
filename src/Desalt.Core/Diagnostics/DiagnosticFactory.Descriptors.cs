// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticFactory.Descriptors.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            TranslationNotSupported,

            [Error(
                1003,
                "Interfaces with default parameters are not supported in TypeScript",
                "Interface method '{0}.{1}' contains parameter '{2}' with a default value")]
            InterfaceWithDefaultParameter,

            [Error(1004, "Cannot import an unknown type", "Type '{0}' is unknown and cannot be imported.")]
            UnknownType,

            [Warning(
                1005,
                "Unsupported accessibility",
                "TypeScript has no equivalent to C#'s {0} accessibility. Falling back to {1} accessibility",
                WarningLevel.Important)]
            UnsupportedAccessibility,

            [Warning(
                1006,
                "Unstructured XML text not supported",
                "Unstructured text within XML documentation comments is not currently supported. Add it to a <remarks> element. Text: '{0}'",
                WarningLevel.Minor)]
            UnstructuredXmlTextNotSupported,
        }

        //// ===========================================================================================================
        //// Diagnostic Factory Creation Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a diagnostic of the form "Internal error: {0}"
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> that represents the error.</param>
        public static Diagnostic InternalError(Exception e) =>
            Create(DiagnosticId.InternalError, Location.None, e.Message);

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

        /// <summary>
        /// Returns a diagnostic of the form "Interface method '{0}.{1}' contains parameter '{2}' with a default value.".
        /// </summary>
        /// <param name="interfaceName">The full name of the interface.</param>
        /// <param name="methodName">The name of the method containing the default value.</param>
        /// <param name="parameterSyntax">The <see cref="SyntaxNode"/> containing the parameter with the default value.</param>
        public static Diagnostic InterfaceWithDefaultParam(
            string interfaceName,
            string methodName,
            ParameterSyntax parameterSyntax)
        {
            return Create(
                DiagnosticId.InterfaceWithDefaultParameter,
                parameterSyntax.GetLocation(),
                interfaceName,
                methodName,
                parameterSyntax.Identifier.Text);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Type '{0}' is unknown and cannot be imported."
        /// </summary>
        /// <param name="typeName">The name of the uknown type.</param>
        /// <param name="location">The location of the error.</param>
        public static Diagnostic UnknownType(string typeName, Location location = null) =>
            Create(DiagnosticId.UnknownType, location ?? Location.None, typeName);

        /// <summary>
        /// Returns a diagnostic of the form "TypeScript has no equivalent to C#'s {0} accessibility.
        /// Falling back to {1} accessibility".
        /// </summary>
        /// <param name="unsupportedAccessibility">The accessibilty level that is not supported.</param>
        /// <param name="fallbackAccessibility">The accessibility level that will be used instead.</param>
        /// <param name="location">The location of the error.</param>
        public static Diagnostic UnsupportedAccessibility(
            string unsupportedAccessibility,
            string fallbackAccessibility,
            Location location)
        {
            return Create(
                DiagnosticId.UnsupportedAccessibility,
                location,
                unsupportedAccessibility,
                fallbackAccessibility);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Unstructured text within XML documentation comments is
        /// not currently supported. Add it to a &lt;remarks&gt; element. Text: '{0}'".
        /// </summary>
        /// <param name="text">The unstructured text.</param>
        /// <param name="location">The location of the error.</param>
        public static Diagnostic UnstructuredXmlTextNotSupported(string text, Location location) =>
            Create(DiagnosticId.UnstructuredXmlTextNotSupported, location, text);
    }
}
