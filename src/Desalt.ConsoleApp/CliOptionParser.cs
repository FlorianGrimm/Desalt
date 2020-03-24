// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionParser.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;

    internal sealed class CliOptionParser
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly StringComparer s_warningComparer = StringComparer.Ordinal;

        private readonly ArgPeeker _argPeeker;
        private readonly IList<Diagnostic> _diagnostics = new List<Diagnostic>();

        private readonly ISet<string> _warnAsErrors = new HashSet<string>(s_warningComparer);
        private readonly ISet<string> _noWarns = new HashSet<string>(s_warningComparer);

        private readonly CliOptions _options = new CliOptions();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private CliOptionParser(ArgPeeker argPeeker)
        {
            _argPeeker = argPeeker ?? throw new ArgumentNullException(nameof(argPeeker));
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static IExtendedResult<CliOptions> Parse(IEnumerable<string> rawArguments)
        {
            var flattenedArgs = FlattenArgs(rawArguments).ToImmutableArray();
            var argPeeker = new ArgPeeker(flattenedArgs);
            var parser = new CliOptionParser(argPeeker);
            var parseResult = parser.Parse();
            return parseResult;
        }

        private IExtendedResult<CliOptions> Parse()
        {
            while (!_argPeeker.IsAtEnd)
            {
                ParseArg();
            }

            // If --version or --help are specified, ignore any other errors and just succeed.
            if (_options.ShouldShowVersion || _options.ShouldShowHelp)
            {
                return new ExtendedResult<CliOptions>(_options);
            }

            // Create the specific diagnostic options by first using the warnAsErrors then applying the noWarns on top
            // of them so that the noWarns have precedence.
            var specificDiagnostics = new Dictionary<string, ReportDiagnostic>(s_warningComparer);
            specificDiagnostics.AddRange(
                _warnAsErrors.Select(x => new KeyValuePair<string, ReportDiagnostic>(x, ReportDiagnostic.Error)));
            foreach (string noWarn in _noWarns)
            {
                specificDiagnostics[noWarn] = ReportDiagnostic.Suppress;
            }

            _options.SpecificDiagnosticOptions = specificDiagnostics.ToImmutableDictionary(s_warningComparer);

            return new ExtendedResult<CliOptions>(_options, _diagnostics);
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

        private void ParseArg()
        {
            string arg = _argPeeker.Read();
            switch (arg)
            {
                case "--help":
                case "-?":
                    _options.ShouldShowHelp = true;
                    break;

                case "--nologo":
                    _options.NoLogo = true;
                    break;

                case "--nowarn":
                    _noWarns.UnionWith(ParseStringListArg(arg));
                    break;

                case "--out":
                    _options.OutDirectory = ParseFileArg(arg);
                    break;

                case "--project":
                    _options.ProjectFile = ParseFileArg(arg);
                    break;

                case "--version":
                case "-v":
                    _options.ShouldShowVersion = true;
                    break;

                case "--warn":
                case "-w":
                    _options.WarningLevel = ParseIntValueArg(arg);
                    break;

                case "--warnaserror":
                case "--warnaserror+":
                    if (TryParseOptionalStringList(out ImmutableArray<string> warningsAsErrors))
                    {
                        _warnAsErrors.UnionWith(warningsAsErrors);
                    }
                    else
                    {
                        // If --warnaserror is used as a flag, clear the previous specific errors.
                        _warnAsErrors.Clear();
                        _options.GeneralDiagnosticOption = ReportDiagnostic.Error;
                    }

                    break;

                case "--warnaserror-":
                    if (TryParseOptionalStringList(out ImmutableArray<string> warningsNotAsErrors))
                    {
                        _warnAsErrors.ExceptWith(warningsNotAsErrors);
                    }
                    else
                    {
                        // Clear the previous warnaserror state since the last one takes precedence.
                        _warnAsErrors.Clear();
                        _options.GeneralDiagnosticOption = ReportDiagnostic.Default;
                    }

                    break;

                default:
                    _diagnostics.Add(DiagnosticFactory.UnrecognizedOption(arg));
                    break;
            }
        }

        private static bool IsOption(string? arg)
        {
            return !string.IsNullOrWhiteSpace(arg) && arg[0] == '-';
        }

        private string ParseFileArg(string name)
        {
            string? value = _argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingFileSpecification(name));
                return string.Empty;
            }

            return _argPeeker.Read();
        }

        private int ParseIntValueArg(string name)
        {
            string? value = _argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingNumberForOption(name));
                return -1;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                _diagnostics.Add(DiagnosticFactory.MissingNumberForOption(name));
            }

            _argPeeker.Read();
            return result;
        }

        private ImmutableArray<string> ParseStringListArg(string name)
        {
            string? rawValue = _argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(rawValue) || IsOption(rawValue))
            {
                _diagnostics.Add(DiagnosticFactory.MissingValueForOption(name));
                return ImmutableArray<string>.Empty;
            }

            rawValue = _argPeeker.Read();
            string[] values = rawValue.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return values.ToImmutableArray();
        }

        private bool TryParseOptionalStringList(out ImmutableArray<string> list)
        {
            string? rawValue = _argPeeker.Peek();

            if (string.IsNullOrWhiteSpace(rawValue) || IsOption(rawValue))
            {
                list = ImmutableArray<string>.Empty;
                return false;
            }

            list = ParseStringListArg(string.Empty);
            return true;
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
