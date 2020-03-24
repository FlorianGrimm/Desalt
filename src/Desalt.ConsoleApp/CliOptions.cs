// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System.Collections.Immutable;
    using Desalt.Core;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains all of the command line interface options.
    /// </summary>
    internal sealed class CliOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public const string HelpText = @"
Desalt Compiler Options

                       - OUTPUT FILES -
--out <dir>                    Specify directory of compiled TypeScript files

                       - INPUT FILES -
--project <.csproj file>       Specify Saltarelle C# project to compile (Short form: -p)

                       - ERRORS AND WARNINGS -
--warnaserror[+|-]             Report all warnings as errors
--warnaserror[+|-] <warn list> Report specific warnings as errors. Can be an error code like CS2008,
                               or just a number.
--warn <n>                     Set warning level (0-4) (Short form: -w)
                               0=Off, 1=Severe, 2=Important, 3=Minor, 4=Informational
--nowarn <warn list>           Disable specific warning messages. Can be an error code like CS2008,
                               or just a number.

                       - LANGUAGE -
--define <symbol list>         Define conditional compilation symbol(s) (Short form: -d)
--inlinecode <symbol> <code>   Define an inline code symbol table override to use when compiling the
                               symbol to TypeScript. For example, --inlinecode Tableau.JavaScript.
                               Vql.Core.ScriptEx.Value<T>(T a, T b) ""({a}) || ({b})"".
--scriptname <symbol> <code>   Define an override in the symbol table for the symbol's script name.
                               For example, --scriptname System.Text.StringBuilder ""sb"".

                       - MISCELLANEOUS -
@<file>                        Read response file for more options
--help                         Display this usage message (Short form: -?)
--nologo                       Suppress compiler copyright message
--version                      Display the compiler version number and exit
";

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string? OutDirectory { get; set; }
        public string? ProjectFile { get; set; }

        public ReportDiagnostic GeneralDiagnosticOption { get; set; } = ReportDiagnostic.Default;

        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; set; } =
            ImmutableDictionary<string, ReportDiagnostic>.Empty;

        public int WarningLevel { get; set; } = (int)CompilerOptions.DefaultWarningLevel;

        public bool ShouldShowHelp { get; set; }
        public bool NoLogo { get; set; }
        public bool ShouldShowVersion { get; set; }

        public SymbolTableOverrides SymbolTableOverrides { get; set; } = SymbolTableOverrides.Empty;
    }
}
