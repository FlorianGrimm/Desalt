// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilationRequest.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Compiler
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an immutable request to compile a set of C# inputs to generated TypeScript.
    /// </summary>
    public class CompilationRequest
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new instance of the <see cref="CompilationRequest"/> class with the specified options.
        /// </summary>
        public CompilationRequest(
            string projectFilePath,
            CompilerOptions options = null)
        {
            Param.VerifyString(projectFilePath, nameof(projectFilePath));

            ProjectFilePath = projectFilePath;
            Options = options ?? new CompilerOptions();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the options used for compiling.
        /// </summary>
        public CompilerOptions Options { get; }

        /// <summary>
        /// Gets a .csproj file path containing C# files to compile.
        /// </summary>
        public string ProjectFilePath { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================
    }
}
