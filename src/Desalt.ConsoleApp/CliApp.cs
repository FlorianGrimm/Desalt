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

    internal static class CliApp
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static async Task<int> RunAsync(IEnumerable<string> args, TextWriter? outWriter = null)
        {
            outWriter ??= Console.Out;

            CliOptions? cliOptions = ParseAndValidateArguments(args, outWriter);
            if (cliOptions == null)
            {
                return 1;
            }

            if (cliOptions.ShouldShowHelp)
            {
                DisplayHelp(outWriter);
                return 0;
            }

            if (cliOptions.ShouldShowVersion)
            {
                DisplayLogoAndVersion(outWriter);
                return 0;
            }

            IExtendedResult<bool> compileResult = await CompileAsync(cliOptions);
            WriteDiagnostics(compileResult.Diagnostics, outWriter);

            return compileResult.Success ? 0 : 1;
        }

        private static CliOptions? ParseAndValidateArguments(IEnumerable<string> args, TextWriter outWriter)
        {
            IExtendedResult<CliOptions> parseResult = CliOptionParser.Parse(args);
            if (parseResult.HasErrors)
            {
                WriteDiagnostics(parseResult.Diagnostics, outWriter);
                return null;
            }

            IExtendedResult<bool> validateResult = CliOptionsValidator.Validate(parseResult.Result);
            if (validateResult.HasErrors)
            {
                WriteDiagnostics(validateResult.Diagnostics, outWriter);
                return null;
            }

            // Write any warnings or info messages (errors would have gotten logged above).
            static bool IsWarningOrInfo(Diagnostic diagnostic) =>
                diagnostic.Severity == DiagnosticSeverity.Warning || diagnostic.Severity == DiagnosticSeverity.Info;

            WriteDiagnostics(
                parseResult.Diagnostics.Where(IsWarningOrInfo)
                    .Concat(validateResult.Diagnostics.Where(IsWarningOrInfo)),
                outWriter);

            return parseResult.Result;
        }

        private static void DisplayHelp(TextWriter outWriter)
        {
            string helpText = CliOptions.HelpText.Replace("\r\n", "\n");
            string[] lines = helpText.Split('\n');

            foreach (string line in lines)
            {
                outWriter.WriteLine(line);
            }
        }

        private static void DisplayLogoAndVersion(TextWriter outWriter)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            string? productName = thisAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            string? version = thisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            string? copyright = thisAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()
                ?.Copyright.Replace("©", "(C)");

            outWriter.WriteLine($"{productName} version {version}");
            outWriter.WriteLine(copyright);
        }

        private static void WriteDiagnostics(IEnumerable<Diagnostic> diagnostics, TextWriter outWriter)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                string message = diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => diagnostic.ToString().Pastel(Color.Yellow),
                    DiagnosticSeverity.Error => diagnostic.ToString().Pastel(Color.Red),
                    _ => diagnostic.ToString(),
                };

                outWriter.WriteLine(message);
            }
        }

        private static async Task<IExtendedResult<bool>> CompileAsync(CliOptions cliOptions)
        {
            CompilerOptions compilerOptions = CreateCompilerOptions(cliOptions);
            string projectFile = cliOptions.ProjectFile ??
                throw new InvalidOperationException("Project file should have been validated earlier.");

            var request = new CompilationRequest(projectFile, compilerOptions);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);
            return result;
        }

        private static CompilerOptions CreateCompilerOptions(CliOptions cliOptions)
        {
            var builder = CompilerOptions.Default.ToBuilder();

            if (cliOptions.OutDirectory != null)
            {
                builder.OutputPath = cliOptions.OutDirectory;
            }

            return builder.ToImmutable();
        }
    }
}
