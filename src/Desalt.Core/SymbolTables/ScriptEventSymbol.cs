// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptEventSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents an event symbol that can be used in the translation process.
    /// </summary>
    internal sealed class ScriptEventSymbol : ScriptSymbol, IScriptEventSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptEventSymbol(IEventSymbol eventSymbol)
            : base(eventSymbol)
        {
            BackingFieldName = eventSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.BackingFieldName);
            CustomInitialization =
                eventSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.CustomInitialization);
            EventSymbol = eventSymbol;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Can be specified on an automatically implemented event or property to denote the name of
        /// the backing field. The presence of this attribute will also cause the backing field to be
        /// initialized even if no code is generated for the accessors (eg. if they are [InlineCode]).
        /// </summary>
        public string BackingFieldName { get; }

        /// <summary>
        /// Can be applied to a (non-const) field or an automatically implemented property to specify
        /// custom code to create the value with which the member is being initialized. For events
        /// and properties, this attribute applies to the compiler-generated backing field.
        ///
        /// JS code to initialize the field. Can use the placeholder {value} to represent the value
        /// with which the member is being initialized (as well as all other placeholders from <see
        /// cref="IScriptMethodSymbol.InlineCode"/>). If null, the member will not be initialized.
        /// </summary>
        public string CustomInitialization { get; }

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        public IEventSymbol EventSymbol { get; }
    }
}
