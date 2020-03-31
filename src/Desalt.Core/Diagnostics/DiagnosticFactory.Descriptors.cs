// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticFactory.Descriptors.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using Desalt.Core.Options;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public partial class DiagnosticFactory
    {
        //// ===========================================================================================================
        //// Enums
        //// ===========================================================================================================

        public enum DiagnosticId
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
                "Unstructured text within XML documentation comments is not currently supported. Add it to a " +
                "<remarks> element. Text: '{0}'",
                WarningLevel.Minor)]
            UnstructuredXmlTextNotSupported,

            [Error(
                1007,
                "TypeScript translation does not understand a C# literal expression",
                "C# literal expression of kind '{0}' not supported: {1}")]
            LiteralExpressionTranslationNotSupported,

            [Error(
                1008,
                "TypeScript translation does not understand C# identifier node",
                "C# identifier node of type '{0}' not supported: {1}")]
            IdentifierNotSupported,

            [Error(
                1009,
                "TypeScript translation does not understand C# operator token",
                "C# operator token of kind '{0}' not supported: {1}")]
            OperatorKindNotSupported,

            [Error(
                1010,
                "Element access with more than one expression is not supported",
                "Element access with more than one expression is not supported: {0}")]
            ElementAccessWithMoreThanOneExpressionNotAllowed,

            [Error(
                1011,
                "Catch clauses with more than one parameter are not yet supported",
                "Catch clauses with more than one parameter are not yet supported: {0}")]
            CatchClausesWithMoreThanOneParameterNotYetSupported,

            [Warning(
                1012,
                "Class contains a field and property name that will be the same compiled name",
                "Class '{0}' contains a field and property, '{1}', that will be the same name in TypeScript",
                description:
                "Either change one of the names, add a [ScriptName] attribute to rename the compiled name, or " +
                "remove the property and expose the field directly.",
                warningLevel: WarningLevel.Important)]
            ClassWithDuplicateFieldAndPropertyName,

            [Error(
                1013,
                "Partial classes are not supported",
                "Class '{0}' is declared as partial, which is not supported",
                description: "TypeScript does not support partial classes")]
            PartialClassesNotSupported,

            [Error(1015, "Unknown type reference", "Type '{0}' is not a known type reference")]
            UnknownTypeReference,

            [Error(1016, "[InlineCode] parsing error", "Error parsing inline code '{0}' for '{1}': {2}")]
            InlineCodeParsingError,

            [Error(1017, "Invalid options file", "Error in reading the options file in '{0}': {1}")]
            InvalidOptionsFile,

            [Error(
                1028,
                "Incorrect [ScriptSkip] usage",
                "A [ScriptSkip] attribute must be placed on a static method or constructor with a single argument or " +
                "an instance method/ctor with no arguments. Method: '{0}'")]
            IncorrectScriptSkipUsage,

            [Error(1018, "Cannot open project", "Cannot open project file '{0}'.")]
            CannotOpenProject,

            [Error(1019, "Unrecognized option", "Unrecognized option: '{0}'.")]
            UnrecognizedOption,

            [Error(1020, "Missing file specification", "Missing file specification for '{0}' option.")]
            MissingFileSpecification,

            [Error(1021, "Missing number for option", "Missing number for '{0}' option.")]
            MissingNumberForOption,

            [Error(1022, "Missing value for option", "Missing value for '{0}' option.")]
            MissingValueForOption,

            [Error(1023, "Missing required option", "Missing required option '{0}'.")]
            MissingRequiredOption,

            [Error(1024, "Warning level not in range", "Warning level must be in the range 0-4.")]
            WarningLevelMustBeInRange,

            [Error(1025, "Missing symbol", "Missing symbol for '{0}' option.")]
            MissingSymbolForOption,

            [Error(1026, "Invalid value for option", "Invalid value for '{0}' option: '{1}'.")]
            InvalidValueForOption,

            [Error(1027, "Error opening response file", "Error opening response file '{0}'.")]
            ErrorOpeningResponseFile,

            [Error(
                1028,
                "Getter and setter accessors do not agree in visibility",
                "Getter and setter accessors for property '{0}' do not agree in visibility.")]
            GetterAndSetterAccessorsDoNotAgreeInVisibility,

            [Error(
                1029,
                "TypeScript translation does not understand C# operator declaration type",
                "C# operator declaration of kind '{0}' not supported: {1}")]
            OperatorDeclarationNotSupported,
        }

        //// ===========================================================================================================
        //// Diagnostic Factory Creation Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a diagnostic of the form "Internal error: {0}"
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> that represents the error.</param>
        /// <param name="location">An optional associated location in the source code.</param>
        public static Diagnostic InternalError(Exception e, Location? location = null)
        {
            return Create(DiagnosticId.InternalError, location ?? Location.None, e.Message);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Internal error: {0}"
        /// </summary>
        /// <param name="error">An error message.</param>
        /// <param name="location">An optional associated location in the source code.</param>
        public static Diagnostic InternalError(string error, Location? location = null)
        {
            return Create(DiagnosticId.InternalError, location ?? Location.None, error);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Document has no C# syntax tree".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSyntaxTree(Document document)
        {
            return Create(
                DiagnosticId.DocumentContainsNoSyntaxTree,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));
        }

        /// <summary>
        /// Returns a diagnostic of the form "Document has no C# semantic model".
        /// </summary>
        /// <param name="document">The document that does not contain a syntax tree.</param>
        public static Diagnostic DocumentContainsNoSemanticModel(Document document)
        {
            return Create(
                DiagnosticId.DocumentContainsNoSemanticModel,
                Location.Create(document.FilePath, new TextSpan(), new LinePositionSpan()));
        }

        /// <summary>
        /// Returns a diagnostic of the form "C# syntax node of type '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic TranslationNotSupported(SyntaxNode node)
        {
            return Create(DiagnosticId.TranslationNotSupported, node.GetLocation(), node.GetType().Name, node);
        }

        /// <summary>
        /// Returns a diagnostic of the form "C# literal expression of kind '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported literal expression syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic LiteralExpressionTranslationNotSupported(LiteralExpressionSyntax node)
        {
            return Create(DiagnosticId.LiteralExpressionTranslationNotSupported, node.GetLocation(), node.Kind(), node);
        }

        /// <summary>
        /// Returns a diagnostic of the form "C# identifier node of type '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic IdentifierNotSupported(SyntaxNode node)
        {
            return Create(DiagnosticId.IdentifierNotSupported, node.GetLocation(), node.GetType().Name, node);
        }

        /// <summary>
        /// Returns a diagnostic of the form "C# operator token of kind '{0}' not supported: {1}".
        /// </summary>
        /// <param name="token">The unsupported syntax token.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic OperatorKindNotSupported(SyntaxToken token)
        {
            return Create(DiagnosticId.OperatorKindNotSupported, token.GetLocation(), token.Kind(), token);
        }

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
        /// <param name="typeName">The name of the unknown type.</param>
        /// <param name="location">The location of the error.</param>
        public static Diagnostic UnknownType(string typeName, Location? location = null)
        {
            return Create(DiagnosticId.UnknownType, location ?? Location.None, typeName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "TypeScript has no equivalent to C#'s {0} accessibility.
        /// Falling back to {1} accessibility".
        /// </summary>
        /// <param name="unsupportedAccessibility">The accessibility level that is not supported.</param>
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
        public static Diagnostic UnstructuredXmlTextNotSupported(string text, Location location)
        {
            return Create(DiagnosticId.UnstructuredXmlTextNotSupported, location, text);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Element access with more than one expression is not supported: {0}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic ElementAccessWithMoreThanOneExpressionNotAllowed(BracketedArgumentListSyntax node)
        {
            return Create(DiagnosticId.ElementAccessWithMoreThanOneExpressionNotAllowed, node.GetLocation(), node);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Catch clauses with more than one parameter are not yet supported: {0}".
        /// </summary>
        /// <param name="node">The unsupported syntax node.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic CatchClausesWithMoreThanOneParameterNotYetSupported(SyntaxNode node)
        {
            return Create(DiagnosticId.CatchClausesWithMoreThanOneParameterNotYetSupported, node.GetLocation(), node);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Class '{0}' contains a field and property, '{1}', that
        /// will be the same name in TypeScript. Please change one or the other name or add a
        /// [ScriptName] attribute to rename the compiled name.".
        /// </summary>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic ClassWithDuplicateFieldAndPropertyName(
            string className,
            string duplicateName,
            Location location)
        {
            return Create(DiagnosticId.ClassWithDuplicateFieldAndPropertyName, location, className, duplicateName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Class '{0}' is declared as partial, which is not supported".
        /// </summary>
        public static Diagnostic PartialClassesNotSupported(ClassDeclarationSyntax node)
        {
            return Create(DiagnosticId.PartialClassesNotSupported, node.GetLocation(), node.Identifier.Text);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Type '{0}' is not a known type reference".
        /// </summary>
        public static Diagnostic UnknownTypeReference(string typeName, Location? location = null)
        {
            return Create(DiagnosticId.UnknownTypeReference, location ?? Location.None, typeName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Error parsing inline code '{0}' for '{1}': {2}".
        /// </summary>
        /// <param name="inlineCode">The [InlineCode] that has an error.</param>
        /// <param name="symbolName">The symbol containing the [InlineCode] attribute.</param>
        /// <param name="errorMessage">Details about the parsing error.</param>
        /// <param name="location">The associated location in the source code.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic InlineCodeParsingError(
            string inlineCode,
            string symbolName,
            string errorMessage,
            Location location)
        {
            return Create(DiagnosticId.InlineCodeParsingError, location, inlineCode, symbolName, errorMessage);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Error in reading the options file in '{0}': {1}".
        /// </summary>
        /// <param name="filePath">The file path of the invalid JSON file.</param>
        /// <param name="errorMessage">The error when reading the file.</param>
        public static Diagnostic InvalidOptionsFile(string filePath, string errorMessage)
        {
            return Create(DiagnosticId.InvalidOptionsFile, Location.None, filePath, errorMessage);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Cannot open project file '{0}'.".
        /// </summary>
        /// <param name="projectFilePath">The file path to the .csproj file that was being opened.</param>
        public static Diagnostic CannotOpenProject(string projectFilePath)
        {
            return Create(DiagnosticId.CannotOpenProject, Location.None, projectFilePath);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Unrecognized option: '{0}'."
        /// </summary>
        /// <param name="optionName">The name of the unrecognized command-line option.</param>
        public static Diagnostic UnrecognizedOption(string optionName)
        {
            return Create(DiagnosticId.UnrecognizedOption, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Missing file specification for '{0}' option."
        /// </summary>
        /// <param name="optionName">The name of the command-line option that expects a file name.</param>
        public static Diagnostic MissingFileSpecification(string optionName)
        {
            return Create(DiagnosticId.MissingFileSpecification, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Missing number for '{0}' option."
        /// </summary>
        /// <param name="optionName">The name of the command-line option that expects a number value.</param>
        public static Diagnostic MissingNumberForOption(string optionName)
        {
            return Create(DiagnosticId.MissingNumberForOption, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Missing value for '{0}' option."
        /// </summary>
        /// <param name="optionName">The name of the command-line option that expects a value.</param>
        public static Diagnostic MissingValueForOption(string optionName)
        {
            return Create(DiagnosticId.MissingValueForOption, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Missing required option '{0}'."
        /// </summary>
        /// <param name="optionName">The name of the command-line option that is missing.</param>
        public static Diagnostic MissingRequiredOption(string optionName)
        {
            return Create(DiagnosticId.MissingRequiredOption, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Warning level must be in the range 0-4."
        /// </summary>
        public static Diagnostic WarningLevelMustBeInRange()
        {
            return Create(DiagnosticId.WarningLevelMustBeInRange, Location.None);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Missing symbol for '{0}' option."
        /// </summary>
        /// <param name="optionName">The name of the command-line option that is missing.</param>
        public static Diagnostic MissingSymbolForOption(string optionName)
        {
            return Create(DiagnosticId.MissingSymbolForOption, Location.None, optionName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Invalid value for '{0}' option: '{1}'."
        /// </summary>
        /// <param name="invalidValue">The invalid value.</param>
        /// <param name="optionName">The name of the command-line option that is missing.</param>
        public static Diagnostic InvalidValueForOption(string optionName, string invalidValue)
        {
            return Create(DiagnosticId.InvalidValueForOption, Location.None, optionName, invalidValue);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Error opening response file '{0}'."
        /// </summary>
        /// <param name="responseFile">The path to the response file.</param>
        public static Diagnostic ErrorOpeningResponseFile(string responseFile)
        {
            return Create(DiagnosticId.ErrorOpeningResponseFile, Location.None, responseFile);
        }

        /// <summary>
        /// Returns a diagnostic of the form "A [ScriptSkip] attribute must be placed on a static
        /// method or constructor with a single argument or an instance method/ctor with no
        /// arguments. Method: '{0}'".
        /// </summary>
        /// <param name="methodName">The method that is incorrect.</param>
        /// <param name="location">The associated location in the source code.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic IncorrectScriptSkipUsage(string methodName, Location location)
        {
            return Create(DiagnosticId.IncorrectScriptSkipUsage, location, methodName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "Getter and setter accessors for property '{0}' do not agree in visibility."
        /// </summary>
        /// <param name="propertyName">The name of the property containing the getter and setter.</param>
        /// <param name="location">The associated location in the source code.</param>
        public static Diagnostic GetterAndSetterAccessorsDoNotAgreeInVisibility(string propertyName, Location location)
        {
            return Create(DiagnosticId.GetterAndSetterAccessorsDoNotAgreeInVisibility, location, propertyName);
        }

        /// <summary>
        /// Returns a diagnostic of the form "C# operator declaration of kind '{0}' not supported: {1}".
        /// </summary>
        /// <param name="node">The unsupported syntax token.</param>
        /// <returns>A new <see cref="Diagnostic"/>.</returns>
        public static Diagnostic OperatorDeclarationNotSupported(OperatorDeclarationSyntax node)
        {
            return Create(
                DiagnosticId.OperatorDeclarationNotSupported,
                node.GetLocation(),
                node.OperatorToken.Kind(),
                node);
        }
    }
}
