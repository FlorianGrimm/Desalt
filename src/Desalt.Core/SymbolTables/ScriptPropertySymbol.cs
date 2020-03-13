// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptPropertySymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a property symbol that can be used in the translation process.
    /// </summary>
    internal class ScriptPropertySymbol : ScriptSymbol, IScriptPropertySymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptPropertySymbol(IPropertySymbol propertySymbol, string computedScriptName)
            : base(propertySymbol, computedScriptName)
        {
            BackingFieldName = propertySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.BackingFieldName);
            CustomInitializationCode =
                propertySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.CustomInitialization);
            IntrinsicProperty = propertySymbol.GetFlagAttribute(SaltarelleAttributeName.IntrinsicProperty);
            PropertySymbol = propertySymbol;
            ScriptAlias = propertySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptAlias);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Can be specified on an automatically implemented event or property to denote the name of
        /// the backing field. The presence of this attribute will also cause the backing field to be
        /// initialized even if no code is generated for the accessors (eg. if they are [InlineCode]).
        /// </summary>
        public string? BackingFieldName { get; }

        /// <summary>
        /// Can be applied to a (non-const) field or an automatically implemented property to specify
        /// custom code to create the value with which the member is being initialized. For events
        /// and properties, this attribute applies to the compiler-generated backing field.
        ///
        /// JS code to initialize the field. Can use the placeholder {value} to represent the value
        /// with which the member is being initialized (as well as all other placeholders from <see
        /// cref="IScriptMethodSymbol.InlineCode"/>). If null, the member will not be initialized.
        /// </summary>
        public string? CustomInitializationCode { get; }

        /// <summary>
        /// Indicates whether a C# property manifests like a field in the generated JavaScript (i.e.
        /// is not accessed via get/set methods). This is really meant only for use when defining OM
        /// corresponding to native objects exposed to script.
        /// </summary>
        public bool IntrinsicProperty { get; }

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        public IPropertySymbol PropertySymbol { get; }

        /// <summary>
        /// Specifies a script name for an imported method. The method is interpreted as a global
        /// method. As a result it this attribute only applies to static methods.
        /// </summary>
        public string? ScriptAlias { get; }
    }
}
