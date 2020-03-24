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
--enum-rename-rule <value>     Controls how enum members are translated from C# to TypeScript. This
                               rule only controls the name of the enum field; values always use
                               [ScriptName] if present, or the default naming rule (camelCase).
                               <value> can be one of the following:
                                   lower-first - first letter as lower case 'One' -> 'one'
                                   match-csharp (default) - original name is used 'One' -> 'One'
--field-rename-rule <value>    Controls how private fields are translated from C# to TypeScript.
                               <value> can be one of the following:
                                   lower-first (default) - first letter as lower case 'Field' -> 'field'
                                   private-dollar-prefix - private fields prefixed with '$'
                                   dollar-prefix-for-duplicates - fields prefixed with a '$' sign,
                                       but only when there is a duplicate name in the compiled code.
                                       For example, if there is a field named 'name' and a property
                                       'Name', 'name' will become '$name'.

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
        public RenameRules RenameRules { get; set; } = RenameRules.Default;
    }
}
