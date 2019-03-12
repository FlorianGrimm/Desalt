// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a symbol that can be used in the translation process.
    /// </summary>
    internal class ScriptSymbol : IScriptSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptSymbol(ISymbol symbol)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            ScriptName = symbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptName);
            PreserveCase = symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveCase);
            PreserveName = symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveName);
            Imported = ((symbol is INamedTypeSymbol) ? symbol : symbol.ContainingType).GetFlagAttribute(
                SaltarelleAttributeName.Imported);
            Reflectable = symbol.GetFlagAttribute(SaltarelleAttributeName.Reflectable);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates that the type should not be emitted into generated script, as it represents
        /// existing script or native types. All members without another naming attribute are
        /// considered to use <see cref="PreserveName"/>.
        /// </summary>
        public bool Imported { get; }

        /// <summary>
        /// Indicates whether to allow suppressing the default behavior of converting member names to
        /// camel-cased equivalents in the generated JavaScript.
        /// </summary>
        public bool PreserveCase { get; }

        /// <summary>
        /// Indicates whether to allow suppressing the default behavior of minimizing private type
        /// names and member names in the generated JavaScript.
        /// </summary>
        public bool PreserveName { get; }

        /// <summary>
        /// Can be applied to a member to indicate that metadata for the member should (or should
        /// not) be included in the compiled script. By default members are reflectable if they have
        /// at least one scriptable attribute. The default reflectability can be changed with the
        /// [DefaultMemberReflectability] attribute.
        /// </summary>
        public bool Reflectable { get; }

        /// <summary>
        /// The name to use for a type or member in the generated script. Property and event
        /// accessors can use the placeholder {owner} to denote the name of their owning entity.
        /// </summary>
        public string ScriptName { get; }

        /// <summary>
        /// Gets the associated C# <see cref="ISymbol"/>.
        /// </summary>
        public ISymbol Symbol { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override string ToString() => Symbol.ToHashDisplay();

        ///// <summary>
        ///// Returns a value indicating whether the specified symbol has the specified attribute.
        ///// </summary>
        ///// <param name="symbol">The symbol to query.</param>
        ///// <param name="attributeNameMinusSuffix">
        ///// The name of the attribute to find, minus the "Attribute" suffix. For example,
        ///// "InlineCode", which represents the <c>System.Runtime.CompilerServices.InlineCodeAttribute</c>.
        ///// </param>
        ///// <param name="isTypeAttribute">Indicates whether the attribute is only valid on a type.</param>
        ///// <returns>
        ///// True if the attribute was found; false if the symbol does not have an attached attribute
        ///// of the given name.
        ///// </returns>
        //protected static bool HasAttribute(
        //    ISymbol symbol,
        //    string attributeNameMinusSuffix,
        //    bool isTypeAttribute = false)
        //{
        //    if (isTypeAttribute && !(symbol is INamedTypeSymbol))
        //    {
        //        symbol = symbol.ContainingType;
        //    }

        //    return symbol != null && FindAttribute(symbol, attributeNameMinusSuffix) != null;
        //}
    }
}
