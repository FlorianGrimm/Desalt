// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;
    using Desalt.Core;
    using Microsoft.CodeAnalysis;
    using Pastel;

    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            IExtendedResult<CliOptions> parseResult = CliOptionParser.Parse(args);
            if (parseResult.Success)
            {
                CliOptions options = parseResult.Result;
                if (options.ShouldShowHelp)
                {
                    PrintHelp();
                    return 0;
                }
                else if (options.ShouldShowVersion)
                {
                    PrintVersion();
                    return 0;
                }

                return await RunAsync(parseResult.Result);
            }

            Console.WriteLine("Failed".Pastel(Color.Red));
            return -1;
        }

        private static void PrintHelp()
        {
            Console.WriteLine(CliOptions.HelpText);
        }

        private static void PrintVersion()
        {
            Console.WriteLine("TODO: Version");
        }

        private static int ReportResult(IExtendedResult<bool> result)
        {
            if (result.Success)
            {
                Console.WriteLine();
                Console.WriteLine("Success".Pastel(Color.Green));
                return 0;
            }

            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                string message = diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => diagnostic.ToString().Pastel(Color.Yellow),
                    DiagnosticSeverity.Error => diagnostic.ToString().Pastel(Color.Red),
                    _ => diagnostic.ToString(),
                };

                Console.WriteLine(message);
            }

            Console.WriteLine();
            Console.WriteLine("Failed".Pastel(Color.Red));
            return -1;
        }

        private static async Task<int> RunAsync(CliOptions cliOptions)
        {
            CompilerOptions compilerOptions = CreateCompilerOptions(cliOptions);

            var request = new CompilationRequest(cliOptions.ProjectFile!, compilerOptions);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);
            return ReportResult(result);
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
