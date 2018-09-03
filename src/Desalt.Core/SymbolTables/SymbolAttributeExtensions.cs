// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolAttributeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains extension methods for reading attribute values from an <see cref="ISymbol"/>.
    /// </summary>
    internal static class SymbolAttributeExtensions
    {
        /// <summary>
        /// Finds a Saltarelle attribute attached to a specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <returns>
        /// The found attribute or null if the symbol does not have an attached attribute of the
        /// given name.
        /// </returns>
        public static AttributeData FindAttribute(this ISymbol symbol, string attributeFullyQualifiedNameMinusSuffix)
        {
            var format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullAttributeName = $"{attributeFullyQualifiedNameMinusSuffix}Attribute";
            AttributeData attributeData = symbol?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.ToDisplayString(format) == fullAttributeName);

            return attributeData;
        }

        /// <summary>
        /// Finds a Saltarelle attribute attached to a specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>
        /// The found attribute or null if the symbol does not have an attached attribute of the
        /// given name.
        /// </returns>
        public static AttributeData FindAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName)
        {
            string fullAttributeName = FullyQualifiedName(attributeName);
            return FindAttribute(symbol, fullAttributeName);
        }

        /// <summary>
        /// Returns a value indicating whether the specified symbol has the specified attribute.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <returns>
        /// True if the attribute was found; false if the symbol does not have an attached attribute
        /// of the given name.
        /// </returns>
        public static bool HasAttribute(this ISymbol symbol, string attributeFullyQualifiedNameMinusSuffix) =>
            FindAttribute(symbol, attributeFullyQualifiedNameMinusSuffix) != null;

        /// <summary>
        /// Returns a value indicating whether the specified symbol has the specified attribute.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>
        /// True if the attribute was found; false if the symbol does not have an attached attribute
        /// of the given name.
        /// </returns>
        public static bool HasAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName) =>
            HasAttribute(symbol, FullyQualifiedName(attributeName));

        /// <summary>
        /// Tries to gets the value of the first constructor argument of the attribute attached to
        /// the specified symbol. For example, reading [ScriptName("x")] will return "x".
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <param name="value">The value of the attribute if present; otherwise, null.</param>
        /// <returns>
        /// True if the attribute is present and has a value in the constructor; otherwise false.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
            this ISymbol symbol,
            string attributeFullyQualifiedNameMinusSuffix,
            out T value)
        {
            return TryGetAttributeValue(
                symbol,
                attributeFullyQualifiedNameMinusSuffix,
                propertyName: null,
                value: out value);
        }

        /// <summary>
        /// Tries to gets the value of the first constructor argument of the attribute attached to
        /// the specified symbol. For example, reading [ScriptName("x")] will return "x".
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <param name="propertyName">
        /// An optional property value to retrieve. For example, the [Imported(ObeysTypeSystem=true)]
        /// returns true if "ObeysTypeSystem" is pass as this parameter value. A null value indicates
        /// that the first constructor argument will be queried instead of a named argument.
        /// </param>
        /// <param name="value">The value of the attribute if present; otherwise, null.</param>
        /// <returns>
        /// True if the attribute is present and has a value in the constructor; otherwise false.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
            this ISymbol symbol,
            string attributeFullyQualifiedNameMinusSuffix,
            string propertyName,
            out T value)
        {
            AttributeData attributeData = FindAttribute(symbol, attributeFullyQualifiedNameMinusSuffix);
            object rawValue = propertyName == null
                ? attributeData?.ConstructorArguments.FirstOrDefault().Value
                : attributeData?.NamedArguments.FirstOrDefault(pair => pair.Key == propertyName).Value.Value;

            value = rawValue == null ? default(T) : (T)rawValue;
            return rawValue != null;
        }

        /// <summary>
        /// Gets the value of the attribute or the default value if it's not found.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <param name="propertyName">
        /// An optional property value to retrieve. For example, the [Imported(ObeysTypeSystem=true)]
        /// returns true if "ObeysTypeSystem" is pass as this parameter value. A null value indicates
        /// that the first constructor argument will be queried instead of a named argument.
        /// </param>
        /// <param name="defaultValue">The value to return if not present.</param>
        /// <returns>
        /// Either the value or the default value, depending on whether the attribute was found.
        /// </returns>
        public static T GetAttributeValueOrDefault<T>(
            this ISymbol symbol,
            string attributeFullyQualifiedNameMinusSuffix,
            T defaultValue = default(T),
            string propertyName = null)
        {
            return TryGetAttributeValue(symbol, attributeFullyQualifiedNameMinusSuffix, propertyName, out T value)
                ? value
                : defaultValue;
        }

        /// <summary>
        /// Tries to gets the value of the first argument to the constructor of the attribute attached
        /// to the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <param name="value">The value of the attribute if present; otherwise, null.</param>
        /// <returns>
        /// True if the attribute is present and has a value in the constructor; otherwise false.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            out T value)
        {
            return TryGetAttributeValue(symbol, FullyQualifiedName(attributeName), out value);
        }

        /// <summary>
        /// Tries to gets the value of the first argument to the constructor of the attribute attached
        /// to the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <param name="propertyName">
        /// An optional property value to retrieve. For example, the [Imported(ObeysTypeSystem=true)]
        /// returns true if "ObeysTypeSystem" is pass as this parameter value. A null value indicates
        /// that the first constructor argument will be queried instead of a named argument.
        /// </param>
        /// <param name="value">The value of the attribute if present; otherwise, null.</param>
        /// <returns>
        /// True if the attribute is present and has a value in the constructor; otherwise false.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            SaltarelleAttributeArgumentName propertyName,
            out T value)
        {
            return TryGetAttributeValue(symbol, FullyQualifiedName(attributeName), propertyName.ToString(), out value);
        }

        /// <summary>
        /// Gets the value of the attribute or the default value if it's not found.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <param name="propertyName">
        /// An optional property value to retrieve. For example, the [Imported(ObeysTypeSystem=true)]
        /// returns true if "ObeysTypeSystem" is pass as this parameter value. A null value indicates
        /// that the first constructor argument will be queried instead of a named argument.
        /// </param>
        /// <param name="defaultValue">The value to return if not present.</param>
        /// <returns>
        /// Either the value or the default value, depending on whether the attribute was found.
        /// </returns>
        public static string GetAttributeValueOrDefault(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            string defaultValue = null,
            SaltarelleAttributeArgumentName? propertyName = null)
        {
            return GetAttributeValueOrDefault<string>(
                symbol,
                FullyQualifiedName(attributeName),
                propertyName: propertyName?.ToString(),
                defaultValue: defaultValue);
        }

        /// <summary>
        /// Gets the value of the attribute or the default value if it's not found.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <param name="propertyName">
        /// An optional property value to retrieve. For example, the [Imported(ObeysTypeSystem=true)]
        /// returns true if "ObeysTypeSystem" is pass as this parameter value. A null value indicates
        /// that the first constructor argument will be queried instead of a named argument.
        /// </param>
        /// <param name="defaultValue">The value to return if not present.</param>
        /// <returns>
        /// Either the value or the default value, depending on whether the attribute was found.
        /// </returns>
        public static T GetAttributeValueOrDefault<T>(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            T defaultValue = default(T),
            SaltarelleAttributeArgumentName? propertyName = null)
        {
            return GetAttributeValueOrDefault(
                symbol,
                FullyQualifiedName(attributeName),
                propertyName: propertyName?.ToString(),
                defaultValue: defaultValue);
        }

        /// <summary>
        /// Gets the value of a flag-type attribute according to the following table:
        /// * No attribute =&gt; false
        /// * [Attr] =&gt; true
        /// * [Attr(value)] =&gt; value
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeFullyQualifiedNameMinusSuffix">
        /// The fully qualified name of the attribute to find, minus the "Attribute" suffix. For
        /// example, "System.Runtime.CompilerServices.InlineCode".
        /// </param>
        /// <returns>The value of the flag attribute.</returns>
        public static bool GetFlagAttribute(this ISymbol symbol, string attributeFullyQualifiedNameMinusSuffix)
        {
            AttributeData attributeData = FindAttribute(symbol, attributeFullyQualifiedNameMinusSuffix);

            if (attributeData == null)
            {
                return false;
            }

            if (attributeData.ConstructorArguments.IsEmpty)
            {
                return true;
            }

            return (bool)attributeData.ConstructorArguments[0].Value;
        }

        /// <summary>
        /// Gets the value of a Saltarelle flag-type attribute according to the following table:
        /// * No attribute =&gt; false
        /// * [Attr] =&gt; true
        /// * [Attr(value)] =&gt; value
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>The value of the flag attribute.</returns>
        public static bool GetFlagAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName) =>
            GetFlagAttribute(symbol, FullyQualifiedName(attributeName));

        private static string FullyQualifiedName(SaltarelleAttributeName attributeName) =>
            $"System.Runtime.CompilerServices.{attributeName}";
    }
}
