// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionsValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System.Collections.Generic;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;

    internal static class CliOptionsValidator
    {
        public static IExtendedResult<bool> Validate(CliOptions options)
        {
            var diagnostics = new List<Diagnostic>();

            // If --help or --version is specified, we don't need to validate anything else.
            if (options.ShouldShowVersion || options.ShouldShowHelp)
            {
                return new ExtendedResult<bool>(true);
            }

            if (string.IsNullOrWhiteSpace(options.ProjectFile))
            {
                diagnostics.Add(DiagnosticFactory.MissingRequiredOption("--project"));
            }

            return new ExtendedResult<bool>(diagnostics.Count == 0, diagnostics);
        }
    }
}
