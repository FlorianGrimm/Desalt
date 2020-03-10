// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Emit;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class TranslationVisitorTests
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
        {codeSnippet}
    }}
}}",
                $@"
class C {{
  private method(): void {{
    {expectedTypeScriptCode.Replace("\r\n", "\n").Trim()}
  }}
}}
",
                discoveryKind);
        }

        private static async Task AssertTranslation(
            string codeSnippet,
            string expectedTypeScriptCode,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes,
            Func<CompilerOptions, CompilerOptions> populateOptionsFunc = null)
        {
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;

{codeSnippet}
";

            // get rid of \r\n sequences in the expected output
            expectedTypeScriptCode = expectedTypeScriptCode.Replace("\r\n", "\n").TrimStart();

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                CompilerOptions options = populateOptionsFunc?.Invoke(tempProject.Options);
                DocumentTranslationContextWithSymbolTables context = await tempProject.CreateContextWithSymbolTablesForFileAsync(
                    discoveryKind: discoveryKind,
                    options: options);

                var throwingDiagnosticList = DiagnosticList.Create(tempProject.Options);
                throwingDiagnosticList.ThrowOnErrors = true;

                var visitor = new TranslationVisitor(context, diagnostics: throwingDiagnosticList);
                ITsAstNode result = visitor.Visit(context.RootSyntax).Single();

                visitor.Diagnostics.Should().BeEmpty();

                // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
                string translated = result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
                translated.Should().Be(expectedTypeScriptCode);
            }
        }
    }
}
