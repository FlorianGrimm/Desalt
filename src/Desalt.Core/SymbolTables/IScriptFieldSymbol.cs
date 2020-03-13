// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptFieldSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represent a field symbol that can be used in the translation process.
    /// </summary>
    internal interface IScriptFieldSymbol : IScriptSymbol
    {
        /// <summary>
        /// Can be applied to a (non-const) field or an automatically implemented property to specify
        /// custom code to create the value with which the member is being initialized. For events
        /// and properties, this attribute applies to the compiler-generated backing field.
        ///
        /// JS code to initialize the field. Can use the placeholder {value} to represent the value
        /// with which the member is being initialized (as well as all other placeholders from <see
        /// cref="IScriptMethodSymbol.InlineCode"/>). If null, the member will not be initialized.
        /// </summary>
        string? CustomInitialization { get; }

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        IFieldSymbol FieldSymbol { get; }

        /// <summary>
        /// Can be applied to a const field to indicate that the literal value of the constant should
        /// always be used instead of the symbolic field name.
        /// </summary>
        bool InlineConstant { get; }

        /// <summary>
        /// Can be applied to a constant field to ensure that it will never be inlined, even in
        /// minified scripts.
        /// </summary>
        bool NoInline { get; }
    }
}
