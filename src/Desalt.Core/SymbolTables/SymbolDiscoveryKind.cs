// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolDiscoveryKind.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    /// <summary>
    /// Controls how a <see cref="ScriptSymbolTable"/> discovers types to add to the symbol table.
    /// </summary>
    internal enum SymbolDiscoveryKind
    {
        /// <summary>
        /// Only types defined in the document are discovered.
        /// </summary>
        OnlyDocumentTypes,

        /// <summary>
        /// Types defined in the document and any types referenced by code in the document are discovered.
        /// </summary>
        DocumentAndReferencedTypes,

        /// <summary>
        /// Types defined in the document and all types in referenced assemblies are discovered.
        /// </summary>
        DocumentAndAllAssemblyTypes
    }
}
