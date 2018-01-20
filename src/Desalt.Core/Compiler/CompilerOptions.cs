// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Compiler
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Contains options that control how to compile C# into TypeScript.
    /// </summary>
    public class CompilerOptions
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public CompilerOptions(int warningLevel = 4)
        {
            WarningLevel = warningLevel;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the global warning level (from 0 to 4).
        /// </summary>
        public int WarningLevel { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        internal CSharpCompilationOptions ToCSharpCompilationOptions()
        {
            return new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                warningLevel: WarningLevel);
        }
    }
}
