// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliApp.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Desalt.Core;
    using Microsoft.CodeAnalysis;
    using Pastel;

    internal sealed class CliApp
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly TextWriter _writer;
        private bool _printedLogo;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public CliApp(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public async Task<int> RunAsync(IEnumerable<string> args)
        {
            CliOptions? cliOptions = ParseAndValidateArguments(args);
            if (cliOptions == null)
            {
                return 1;
            }

            if (cliOptions.ShouldShowHelp)
            {
                PrintHelp();
                return 0;
            }

            if (cliOptions.ShouldShowVersion)
            {
                PrintVersion();
                return 0;
            }

            IExtendedResult<bool> compileResult = await CompileAsync(cliOptions);
            WriteDiagnostics(compileResult.Diagnostics);

            return compileResult.Success ? 0 : 1;
        }

        private CliOptions? ParseAndValidateArguments(IEnumerable<string> args)
        {
            IExtendedResult<CliOptions> parseResult = CliOptionParser.Parse(args);
            if (parseResult.Result.NoLogo)
            {
                _printedLogo = true;
            }

            if (parseResult.HasErrors)
            {
                WriteDiagnostics(parseResult.Diagnostics);
                return null;
            }

            IExtendedResult<bool> validateResult = CliOptionsValidator.Validate(parseResult.Result);
            if (validateResult.HasErrors)
            {
                WriteDiagnostics(validateResult.Diagnostics);
                return null;
            }

            // Write any warnings or info messages (errors would have gotten logged above).
            static bool IsWarningOrInfo(Diagnostic diagnostic) =>
                diagnostic.Severity == DiagnosticSeverity.Warning || diagnostic.Severity == DiagnosticSeverity.Info;

            WriteDiagnostics(
                parseResult.Diagnostics.Where(IsWarningOrInfo)
                    .Concat(validateResult.Diagnostics.Where(IsWarningOrInfo)));

            return parseResult.Result;
        }

        private void PrintLogoIfNeeded()
        {
            if (_printedLogo)
            {
                return;
            }

            var thisAssembly = Assembly.GetExecutingAssembly();
            string? productName = thisAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            string? version = thisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            string? copyright = thisAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()
                ?.Copyright.Replace("©", "(C)");

            _writer.WriteLine($"{productName} version {version}");
            _writer.WriteLine(copyright);

            _printedLogo = true;
        }

        private void PrintVersion()
        {
            string? version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            _writer.WriteLine(version);
        }

        private void PrintHelp()
        {
            string helpText = CliOptions.HelpText.Replace("\r\n", "\n");
            string[] lines = helpText.Split('\n');

            PrintLogoIfNeeded();

            foreach (string line in lines)
            {
                _writer.WriteLine(line);
            }
        }

        private void WriteDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            var diagnosticsAsArray = diagnostics.ToArray();

            if (diagnosticsAsArray.Length > 0)
            {
                PrintLogoIfNeeded();
            }

            foreach (Diagnostic diagnostic in diagnosticsAsArray)
            {
                string message = diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => diagnostic.ToString().Pastel(Color.Yellow),
                    DiagnosticSeverity.Error => diagnostic.ToString().Pastel(Color.Red),
                    _ => diagnostic.ToString(),
                };

                _writer.WriteLine(message);
            }
        }

        private static async Task<IExtendedResult<bool>> CompileAsync(CliOptions cliOptions)
        {
            var compilerOptions = cliOptions.ToCompilerOptions();
            string projectFile = cliOptions.ProjectFile ??
                throw new InvalidOperationException("Project file should have been validated earlier.");

            var request = new CompilationRequest(projectFile, compilerOptions);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);
            return result;
        }
    }
}
