// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.TypeScriptAst.Emit;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;

    public partial class TranslatorTests
    {
        private static Task AssertTranslationWithClassCAndMethod(
            string codeSnippet,
            string expectedTypeScriptCode,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes)
        {
            return AssertTranslation(
                $@"
class C
{{
    void Method()
    {{
        int startHere = 0;
        {codeSnippet}
        int endHere = 0;
    }}
}}",
                $@"
class C {{
  private method(): void {{
    let startHere: number = 0;
{expectedTypeScriptCode.Replace("\r\n", "\n").Trim()}
    let endHere: number = 0;
  }}
}}
",
                discoveryKind,
                extractApplicableTypeScriptSnippet: true);
        }

        /// <summary>
        /// Runs the translation against the specified C# code and compares the result with the expected TypeScript code.
        /// </summary>
        /// <param name="codeSnippet">The C# to translate.</param>
        /// <param name="expectedTypeScriptCode">The expected TypeScript code.</param>
        /// <param name="discoveryKind">
        /// How symbols should be discovered. By default only the document symbols are used, which means that any
        /// external references won't be pulled in. This makes it much, much faster, but sometimes will produce wrong
        /// results in the translated code. For example, [InlineCode] won't be used for the system symbols.
        /// </param>
        /// <param name="populateOptionsFunc">If provided, allows callers to adjust the options used for the translation.</param>
        /// <param name="extractApplicableTypeScriptSnippet">
        /// If true, only the snippet part of the translated TypeScript code be examined. This is helpful when using a
        /// boilerplate class/method and you only care about the statements inside of the function.
        /// </param>
        /// <param name="includeImports">
        /// Determines whether an imports section is emitted at the top of the TypeScript file.
        /// </param>
        private static async Task AssertTranslation(
            string codeSnippet,
            string expectedTypeScriptCode,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes,
            Func<CompilerOptions, CompilerOptions>? populateOptionsFunc = null,
            bool extractApplicableTypeScriptSnippet = false,
            bool includeImports = false)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;

{codeSnippet}
";

            // Get rid of \r\n sequences in the expected output.
            expectedTypeScriptCode = expectedTypeScriptCode.Replace("\r\n", "\n").TrimStart();

            using TempProject tempProject = await TempProject.CreateAsync(code);
            CompilerOptions? options = populateOptionsFunc?.Invoke(tempProject.Options) ??
                tempProject.Options.WithDiagnosticOptions(
                    tempProject.Options.DiagnosticOptions.WithThrowOnErrors(true));

            DocumentTranslationContextWithSymbolTables context = await tempProject.CreateContextWithSymbolTablesForFileAsync(
                discoveryKind: discoveryKind,
                options: options);

            var result = Translator.TranslateDocument(context, skipImports: !includeImports);
            result.Diagnostics.Should().BeEmpty();

            static string ExtractApplicableLines(string code, bool isActual)
            {
                string[] lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries)

                    // Read until we see the block name.
                    .SkipWhile(line => !line.Contains("startHere", StringComparison.Ordinal))
                    .Skip(1)

                    // Read all of the contents until the next brace.
                    .TakeWhile(line => !line.Contains("endHere", StringComparison.Ordinal))

                    // Get rid of the extraneous whitespace.
                    .Select(line => isActual ? line.StartsWith("    ", StringComparison.Ordinal) ? line[4..] : line : line)
                    .ToArray();

                return string.Join('\n', lines);
            }

            // Rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings.
            string translated = result.Result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);

            string applicableTranslated = extractApplicableTypeScriptSnippet
                ? ExtractApplicableLines(translated, isActual: true)
                : translated;

            string applicableExpected = extractApplicableTypeScriptSnippet
                ? ExtractApplicableLines(expectedTypeScriptCode, isActual: false)
                : expectedTypeScriptCode;

            applicableTranslated.Should().Be(applicableExpected);
        }

        private static async Task AssertTranslationHasDiagnostics(
            string codeSnippet,
            string expectedTypeScriptCode,
            Action<IReadOnlyCollection<Diagnostic>> diagnosticsAssertionAction,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes,
            Func<CompilerOptions, CompilerOptions>? populateOptionsFunc = null,
            bool includeImports = false)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;

{codeSnippet}
";

            // Get rid of \r\n sequences in the expected output.
            expectedTypeScriptCode = expectedTypeScriptCode.Replace("\r\n", "\n").TrimStart();

            using TempProject tempProject = await TempProject.CreateAsync(code);

            CompilerOptions options = populateOptionsFunc?.Invoke(tempProject.Options) ?? tempProject.Options;
            options = options.WithDiagnosticOptions(options.DiagnosticOptions.WithThrowOnErrors(false));

            DocumentTranslationContextWithSymbolTables context = await tempProject.CreateContextWithSymbolTablesForFileAsync(
                discoveryKind: discoveryKind,
                options: options);

            var result = Translator.TranslateDocument(context, skipImports: !includeImports);

            // Rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings.
            string translated = result.Result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
            translated.Should().Be(expectedTypeScriptCode);

            diagnosticsAssertionAction(result.Diagnostics);
        }
    }
}
