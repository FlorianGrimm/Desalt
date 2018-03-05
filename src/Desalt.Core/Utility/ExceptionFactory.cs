// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;

    /// <summary>
    /// Factory class for creating common exceptions.
    /// </summary>
    /// <remarks>Some content is borrowed from Roslyn source code at <see href="http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/InternalUtilities/ExceptionUtilities.cs"/>.</remarks>
    internal static class ExceptionFactory
    {
        /// <summary>
        /// Returns a new <see cref="InvalidOperationException"/> with the message "This program
        /// location is thought to be unreachable."
        /// </summary>
        public static Exception Unreachable =>
            new InvalidOperationException("This program location is thought to be unreachable.");
    }
}
