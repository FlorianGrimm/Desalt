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

    /// <summary>
    /// Contains all of the command line interface options.
    /// </summary>
    internal class CliOptions
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
--nowarn <warn list>           Disable specific warning messages. Can be an error code like CS2008,
                               or just a number.

                       - LANGUAGE -
--define <symbol list>         Define conditional compilation symbol(s) (Short form: -d)

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

        public bool AllWarningsAsErrors { get; set; }
        public IImmutableSet<string> WarningsAsErrors { get; set; } = ImmutableHashSet<string>.Empty;
        public IImmutableSet<string> WarningsNotAsErrors { get; set; } = ImmutableHashSet<string>.Empty;
        public int WarningLevel { get; set; } = (int)CompilerOptions.DefaultWarningLevel;
        public IImmutableSet<string> NoWarn { get; set; } = ImmutableHashSet<string>.Empty;

        public bool ShouldShowHelp { get; set; }
        public bool NoLogo { get; set; }
        public bool ShouldShowVersion { get; set; }
    }
}
