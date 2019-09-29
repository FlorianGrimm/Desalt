// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolInfo.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    /// <summary>
    /// Represents an imported symbol and whether it is a project-relative import or an external
    /// module import.
    /// </summary>
    internal class ImportSymbolInfo
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ImportSymbolInfo(string relativeTypeScriptFilePathOrModuleName, bool isInternalReference)
        {
            RelativeTypeScriptFilePathOrModuleName = relativeTypeScriptFilePathOrModuleName;
            IsInternalReference = isInternalReference;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string RelativeTypeScriptFilePathOrModuleName { get; }

        /// <summary>
        /// Returns a value indicating whether this is an internal reference, meaning that the type
        /// is defined within this project. An external reference is something from another assembly.
        /// </summary>
        public bool IsInternalReference { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ImportSymbolInfo CreateInternalReference(string typeScriptFilePath)
        {
            return new ImportSymbolInfo(typeScriptFilePath, isInternalReference: true);
        }

        public static ImportSymbolInfo CreateExternalReference(string moduleName)
        {
            return new ImportSymbolInfo(moduleName, isInternalReference: false);
        }
    }
}
