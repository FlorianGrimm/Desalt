// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a symbol that can be used in the translation process.
    /// </summary>
    internal interface IScriptSymbol
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates that the type should not be emitted into generated script, as it represents
        /// existing script or native types. All members without another naming attribute are
        /// considered to use <see cref="PreserveName"/>.
        /// </summary>
        bool Imported { get; }

        /// <summary>
        /// Indicates whether to allow suppressing the default behavior of converting member names to
        /// camel-cased equivalents in the generated JavaScript.
        /// </summary>
        bool PreserveCase { get; }

        /// <summary>
        /// Indicates whether to allow suppressing the default behavior of minimizing private type
        /// names and member names in the generated JavaScript.
        /// </summary>
        bool PreserveName { get; }

        /// <summary>
        /// Can be applied to a member to indicate that metadata for the member should (or should
        /// not) be included in the compiled script. By default members are reflectable if they have
        /// at least one scriptable attribute. The default reflectability can be changed with the
        /// [DefaultMemberReflectability] attribute.
        /// </summary>
        bool Reflectable { get; }

        /// <summary>
        /// The name to use for a type or member in the generated script. Property and event
        /// accessors can use the placeholder {owner} to denote the name of their owning entity.
        /// </summary>
        string ScriptName { get; }

        /// <summary>
        /// Gets the associated C# <see cref="ISymbol"/>.
        /// </summary>
        ISymbol Symbol { get; }
    }
}
