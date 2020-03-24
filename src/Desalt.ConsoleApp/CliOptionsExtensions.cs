// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionsExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using Desalt.Core;

    /// <summary>
    /// Converts <see cref="CliOptions"/> to <see cref="CompilerOptions"/> and <see cref="CompilationRequest"/>.
    /// </summary>
    internal static class CliOptionsExtensions
    {
        public static CompilationRequest ToCompilationRequest(this CliOptions cliOptions)
        {
            var compilerOptions = ToCompilerOptions(cliOptions);
            string projectFilePath = cliOptions.ProjectFile ??
                throw new InvalidOperationException($"{nameof(cliOptions.ProjectFile)} should have been specified");

            var request = new CompilationRequest(projectFilePath, compilerOptions);
            return request;
        }

        private static CompilerOptions ToCompilerOptions(CliOptions cliOptions)
        {
            var builder = CompilerOptions.Default.ToBuilder();

            builder.OutputPath = cliOptions.OutDirectory;

            builder.GeneralDiagnosticOption = cliOptions.GeneralDiagnosticOption;
            builder.SpecificDiagnosticOptions = cliOptions.SpecificDiagnosticOptions;
            builder.WarningLevel = (WarningLevel)cliOptions.WarningLevel;

            builder.SymbolTableOverrides = cliOptions.SymbolTableOverrides;
            builder.RenameRules = cliOptions.RenameRules;

            return builder.ToImmutable();
        }
    }
}
