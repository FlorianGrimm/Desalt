// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionParser.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;

    internal static class CliOptionParser
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static IExtendedResult<CliOptions> Parse(IEnumerable<string> rawArguments)
        {
            var flattenedArgs = FlattenArgs(rawArguments).ToImmutableArray();
            var options = new CliOptions();
            var diagnostics = new List<Diagnostic>();
            var argPeeker = new ArgPeeker(flattenedArgs);

            while (!argPeeker.IsAtEnd)
            {
                ParseArg(argPeeker, options, diagnostics);
            }

            // If --version or --help are specified, ignore any other errors and just succeed.
            if (options.ShouldShowVersion || options.ShouldShowHelp)
            {
                return new ExtendedResult<CliOptions>(options);
            }

            return new ExtendedResult<CliOptions>(options, diagnostics);
        }

        /// <summary>
        /// Flattens the arguments by reading in any response files and enumerating the options as if they were
        /// specified on the command line.
        /// </summary>
        /// <param name="rawArguments"></param>
        /// <returns></returns>
        private static IEnumerable<string> FlattenArgs(IEnumerable<string> rawArguments)
        {
            return rawArguments;
        }

        private static void ParseArg(ArgPeeker argPeeker, CliOptions options, ICollection<Diagnostic> diagnostics)
        {
            string arg = argPeeker.Read();
            switch (arg)
            {
                case "--help":
                case "-?":
                    options.ShouldShowHelp = true;
                    break;

                case "--nologo":
                    options.NoLogo = true;
                    break;

                case "--out":
                    options.OutDirectory = ParseFileArg(arg, argPeeker, diagnostics);
                    break;

                case "--project":
                    options.ProjectFile = ParseFileArg(arg, argPeeker, diagnostics);
                    break;

                case "--version":
                case "-v":
                    options.ShouldShowVersion = true;
                    break;

                case "--warn":
                case "-w":
                    options.WarningLevel = ParseIntValueArg(arg, argPeeker, diagnostics);
                    break;

                default:
                    diagnostics.Add(DiagnosticFactory.UnrecognizedOption(arg));
                    break;
            };
        }

        private static bool IsOption(string? arg)
        {
            return !string.IsNullOrWhiteSpace(arg) && arg[0] == '-';
        }

        private static string ParseFileArg(string name, ArgPeeker argPeeker, ICollection<Diagnostic> diagnostics)
        {
            string? value = argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                diagnostics.Add(DiagnosticFactory.MissingFileSpecification(name));
                return string.Empty;
            }

            return argPeeker.Read();
        }

        private static int ParseIntValueArg(string name, ArgPeeker argPeeker, ICollection<Diagnostic> diagnostics)
        {
            string? value = argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                diagnostics.Add(DiagnosticFactory.MissingNumberForOption(name));
                return -1;
            }

            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result))
            {
                diagnostics.Add(DiagnosticFactory.MissingNumberForOption(name));
            }

            argPeeker.Read();
            return result;
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class ArgPeeker
        {
            private ImmutableArray<string> _args;
            private int _currentIndex;

            public ArgPeeker(IEnumerable<string> args)
            {
                _args = args.Select(x => x.Trim()).ToImmutableArray();
            }

            public bool IsAtEnd => _currentIndex >= _args.Length;

            public string Read()
            {
                string current = _args[_currentIndex];
                _currentIndex++;
                return current;
            }

            public string? Peek()
            {
                return _currentIndex < _args.Length ? _args[_currentIndex] : null;
            }
        }
    }
}
