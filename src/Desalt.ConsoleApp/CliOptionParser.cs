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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Microsoft.CodeAnalysis;

    internal sealed class CliOptionParser
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly StringComparer s_warningComparer = StringComparer.Ordinal;

        private readonly ArgReader _argReader;
        private readonly IFileContentFetcher _fileContentFetcher;
        private readonly IList<Diagnostic> _diagnostics = new List<Diagnostic>();

        private readonly CliOptions _options = new CliOptions();

        private readonly ISet<string> _warnAsErrors = new HashSet<string>(s_warningComparer);
        private readonly ISet<string> _noWarns = new HashSet<string>(s_warningComparer);

        private readonly IDictionary<string, SymbolTableOverride> _symbolTableOverrides =
            new Dictionary<string, SymbolTableOverride>();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private CliOptionParser(IEnumerable<string> rawArguments, IFileContentFetcher? fileContentFetcher)
        {
            _fileContentFetcher = fileContentFetcher ?? new OsFileContentFetcher();
            _argReader = new ArgReader(rawArguments, _fileContentFetcher, _diagnostics);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static IExtendedResult<CliOptions> Parse(
            IEnumerable<string> rawArguments,
            IFileContentFetcher? fileContentFetcher = null)
        {
            var parser = new CliOptionParser(rawArguments, fileContentFetcher);
            var parseResult = parser.ParseImpl();
            return parseResult;
        }

        private IExtendedResult<CliOptions> ParseImpl()
        {
            while (!_argReader.IsAtEnd)
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

            // Set the symbol table overrides.
            _options.SymbolTableOverrides = new SymbolTableOverrides(_symbolTableOverrides.ToArray());

            return new ExtendedResult<CliOptions>(_options, _diagnostics);
        }

        private void ParseArg()
        {
            string arg = _argReader.Read() ?? throw new InvalidOperationException();
            switch (arg)
            {
                case "--help":
                case "-?":
                    _options.ShouldShowHelp = true;
                    break;

                case "--nologo":
                    _options.NoLogo = true;
                    break;

                case "--options":
                    string? optionsFileArg = ParseFileArg(arg);
                    if (!string.IsNullOrEmpty(optionsFileArg))
                    {
                        ParseOptionsFile(optionsFileArg);
                    }
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

                case "--inlinecode":
                    if (TryParseSymbolTableOverrideValues(arg, out string? inlineCodeSymbol, out string? code))
                    {
                        if (_symbolTableOverrides.TryGetValue(inlineCodeSymbol, out SymbolTableOverride? symbolOverride))
                        {
                            _symbolTableOverrides[inlineCodeSymbol] = symbolOverride.WithInlineCode(code);
                        }
                        else
                        {
                            _symbolTableOverrides.Add(inlineCodeSymbol, new SymbolTableOverride(inlineCode: code));
                        }
                    }
                    break;

                case "--scriptname":
                    if (TryParseSymbolTableOverrideValues(arg, out string? scriptNameSymbol, out string? scriptName))
                    {
                        if (_symbolTableOverrides.TryGetValue(scriptNameSymbol, out SymbolTableOverride? symbolOverride))
                        {
                            _symbolTableOverrides[scriptNameSymbol] = symbolOverride.WithScriptName(scriptName);
                        }
                        else
                        {
                            _symbolTableOverrides.Add(
                                scriptNameSymbol,
                                new SymbolTableOverride(scriptName: scriptName));
                        }
                    }
                    break;

                case "--enum-rename-rule":
                    if (TryParseMappedValue(
                        arg,
                        new Dictionary<string, EnumRenameRule>
                        {
                            ["lower-first"] = EnumRenameRule.LowerCaseFirstChar,
                            ["match-csharp"] = EnumRenameRule.MatchCSharpName
                        },
                        out EnumRenameRule enumRenameRule))
                    {
                        _options.RenameRules = _options.RenameRules.WithEnumRule(enumRenameRule);
                    }
                    break;

                case "--field-rename-rule":
                    if (TryParseMappedValue(
                        arg,
                        new Dictionary<string, FieldRenameRule>
                        {
                            ["lower-first"] = FieldRenameRule.LowerCaseFirstChar,
                            ["private-dollar-prefix"] = FieldRenameRule.PrivateDollarPrefix,
                            ["dollar-prefix-for-duplicates"] = FieldRenameRule.DollarPrefixOnlyForDuplicateName,
                        },
                        out FieldRenameRule fieldRenameRule))
                    {
                        _options.RenameRules = _options.RenameRules.WithFieldRule(fieldRenameRule);
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

        private string? ParseFileArg(string optionName)
        {
            string? value = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingFileSpecification(optionName));
                return null;
            }

            return _argReader.Read();
        }

        private int ParseIntValueArg(string optionName)
        {
            string? value = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingNumberForOption(optionName));
                return -1;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                _diagnostics.Add(DiagnosticFactory.MissingNumberForOption(optionName));
            }

            _argReader.Read();
            return result;
        }

        private ImmutableArray<string> ParseStringListArg(string optionName)
        {
            string? rawValue = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(rawValue) || IsOption(rawValue))
            {
                _diagnostics.Add(DiagnosticFactory.MissingValueForOption(optionName));
                return ImmutableArray<string>.Empty;
            }

            _argReader.Read();
            string[] values = rawValue.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return values.ToImmutableArray();
        }

        private bool TryParseOptionalStringList(out ImmutableArray<string> list)
        {
            string? rawValue = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(rawValue) || IsOption(rawValue))
            {
                list = ImmutableArray<string>.Empty;
                return false;
            }

            list = ParseStringListArg(string.Empty);
            return true;
        }

        private bool TryParseSymbolTableOverrideValues(
            string optionName,
            [NotNullWhen(true)] out string? symbol,
            [NotNullWhen(true)] out string? value)
        {
            symbol = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(symbol) || IsOption(symbol))
            {
                _diagnostics.Add(DiagnosticFactory.MissingSymbolForOption(optionName));
                value = null;
                return false;
            }

            symbol = _argReader.Read();
            value = _argReader.Peek();

            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingValueForOption($"{optionName} {symbol}"));
                return false;
            }

            value = _argReader.Read();
            return symbol != null && value != null;
        }

        private bool TryParseMappedValue<T>(string optionName, IReadOnlyDictionary<string, T> mappings, out T enumValue)
            where T : struct
        {
            string? value = _argReader.Peek();
            if (string.IsNullOrWhiteSpace(value) || IsOption(value))
            {
                _diagnostics.Add(DiagnosticFactory.MissingValueForOption(optionName));
                enumValue = default;
                return false;
            }
            _argReader.Read();

            if (!mappings.ContainsKey(value))
            {
                _diagnostics.Add(DiagnosticFactory.InvalidValueForOption(optionName, value));
                enumValue = default;
                return false;
            }

            enumValue = mappings[value];
            return true;
        }

        private void ParseOptionsFile(string optionsFile)
        {
            using Stream optionsFileStream = _fileContentFetcher.OpenRead(optionsFile);
            IExtendedResult<CompilerOptions?> result = CompilerOptions.Deserialize(optionsFileStream);
            _diagnostics.AddRange(result.Diagnostics);
            if (result.HasErrors)
            {
                return;
            }

            CompilerOptions loadedOptions = result.Result ?? throw new InvalidOperationException();

            // Merge the parsed options file with the current options, overriding any conflicts.
            _options.WarningLevel = (int)loadedOptions.DiagnosticOptions.WarningLevel;
            _options.GeneralDiagnosticOption = loadedOptions.DiagnosticOptions.GeneralDiagnosticOption;

            _warnAsErrors.UnionWith(
                loadedOptions.DiagnosticOptions.SpecificDiagnosticOptions.Where(x => x.Value == ReportDiagnostic.Error)
                    .Select(x => x.Key));

            _noWarns.UnionWith(
                loadedOptions.DiagnosticOptions.SpecificDiagnosticOptions
                    .Where(x => x.Value == ReportDiagnostic.Suppress)
                    .Select(x => x.Key));

            foreach ((string key, SymbolTableOverride overrideValue) in loadedOptions.SymbolTableOverrides)
            {
                if (_symbolTableOverrides.TryGetValue(key, out SymbolTableOverride? existingOverride))
                {
                    string? inlineCode = overrideValue.InlineCode ?? existingOverride.InlineCode;
                    string? scriptName = overrideValue.ScriptName ?? existingOverride.ScriptName;
                    _symbolTableOverrides[key] = new SymbolTableOverride(inlineCode, scriptName);
                }
                else
                {
                    _symbolTableOverrides.Add(key, overrideValue);
                }
            }

            _options.RenameRules = loadedOptions.RenameRules;
        }
    }
}
