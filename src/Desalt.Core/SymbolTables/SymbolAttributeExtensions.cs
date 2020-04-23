// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolAttributeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System.Diagnostics.CodeAnalysis;
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
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>
        /// The found attribute or null if the symbol does not have an attached attribute of the
        /// given name.
        /// </returns>
        public static AttributeData? FindAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName)
        {
            var format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullAttributeName = $"{FullyQualifiedName(attributeName)}Attribute";
            AttributeData? attributeData = symbol?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.ToDisplayString(format) == fullAttributeName);

            return attributeData;
        }

        /// <summary>
        /// Returns a value indicating whether the specified symbol has the specified attribute.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>
        /// True if the attribute was found; false if the symbol does not have an attached attribute
        /// of the given name.
        /// </returns>
        public static bool HasAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName)
        {
            return FindAttribute(symbol, attributeName) != null;
        }

        /// <summary>
        /// Tries to gets the value of the first constructor argument of the attribute attached to
        /// the specified symbol. For example, reading [ScriptName("x")] will return "x".
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
        private static bool TryGetAttributeValue(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            SaltarelleAttributeArgumentName? propertyName,
            [NotNullWhen(true)] out object? value)
        {
            AttributeData? attributeData = FindAttribute(symbol, attributeName);
            value = propertyName == null
                ? attributeData?.ConstructorArguments.FirstOrDefault().Value
                : attributeData?.NamedArguments.FirstOrDefault(pair => pair.Key == propertyName.ToString()).Value.Value;

            return value != null;
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
        public static bool TryGetAttributeValue(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            [NotNullWhen(true)] out string? value)
        {
            if (TryGetAttributeValue(symbol, attributeName, propertyName: null, out object? rawValue))
            {
                value = (string)rawValue;
                return true;
            }

            value = default;
            return false;
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
            where T : struct
        {
            if (TryGetAttributeValue(symbol, attributeName, propertyName, out object? rawValue))
            {
                value = (T)rawValue;
                return true;
            }

            value = default;
            return false;
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
        public static string? GetAttributeValueOrDefault(
            this ISymbol symbol,
            SaltarelleAttributeName attributeName,
            string? defaultValue = null,
            SaltarelleAttributeArgumentName? propertyName = null)
        {
            if (TryGetAttributeValue(symbol, attributeName, propertyName, out object? rawValue))
            {
                return (string)rawValue;
            }

            return defaultValue;
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
            T defaultValue = default,
            SaltarelleAttributeArgumentName? propertyName = null)
            where T : struct
        {
            if (TryGetAttributeValue(symbol, attributeName, propertyName, out object? rawValue))
            {
                return (T)rawValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the value of a flag-type attribute according to the following table:<![CDATA[
        /// * No attribute => false
        /// * [Attr] => true
        /// * [Attr(value)] => value
        /// ]]>
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeName">The Saltarelle attribute to find.</param>
        /// <returns>The value of the flag attribute.</returns>
        public static bool GetFlagAttribute(this ISymbol symbol, SaltarelleAttributeName attributeName)
        {
            AttributeData? attributeData = FindAttribute(symbol, attributeName);
            if (attributeData == null)
            {
                return false;
            }

            // Return true if the attribute value is missing.
            object? value = attributeData.ConstructorArguments.IsEmpty
                ? null
                : attributeData.ConstructorArguments[0].Value;

            return (bool?)value != false;
        }

        private static string FullyQualifiedName(SaltarelleAttributeName attributeName)
        {
            return $"System.Runtime.CompilerServices.{attributeName}";
        }
    }
}
