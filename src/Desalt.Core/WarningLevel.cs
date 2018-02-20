// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticCollection.Descriptors.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    //// ===========================================================================================================
    //// Enums
    //// ===========================================================================================================

    /// <summary>
    /// Taken from the MSDN page at <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/warn-compiler-option"/>.
    /// </summary>
    public enum WarningLevel
    {
        /// <summary>
        /// Turns off emission of all warning messages.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Displays severe warning messages.
        /// </summary>
        Severe = 1,

        /// <summary>
        /// Displays level 1 warnings plus certain, less-severe warnings, such as warnings about hiding class members.
        /// </summary>
        Important = 2,

        /// <summary>
        /// Displays level 2 warnings plus certain, less-severe warnings, such as warnings about
        /// expressions that always evaluate to true or false.
        /// </summary>
        Minor = 3,

        /// <summary>
        /// Displays all level 3 warnings plus informational warnings.
        /// </summary>
        Informational = 4,
    }
}
