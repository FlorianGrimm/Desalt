// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.TypeScriptAst.Emit;
    using FluentAssertions;
    using NUnit.Framework;

    public class TranslatorTests
    {
        private static async Task AssertTranslationAsync(string csharpCode, string expectedTypeScriptCode)
        {
            string code = $@"
using System;
using System.Collections.Generic;

{csharpCode}
";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            var context = await tempProject.CreateContextWithSymbolTablesForFileAsync();
            IExtendedResult<TypeScriptAst.Ast.ITsImplementationModule> result = Translator.TranslateDocument(context);

            result.Diagnostics.Should().BeEmpty();

            // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
            string translated = result.Result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
            translated.Should().Be(expectedTypeScriptCode.TrimStart().Replace("\r\n", "\n"));
        }

        [Test]
        public async Task TranslateDocument_should_translate_an_interface_declaration_with_members_and_doc_comments()
        {
            await AssertTranslationAsync(
                @"
/// <summary>
/// Tests an interface declaration.
/// </summary>
public interface MyInterface
{
    void VoidMethod();
}
",
                @"
/**
 * Tests an interface declaration.
 */
export interface MyInterface {
  voidMethod(): void;
}
");
        }

        [Test]
        public async Task TranslateDocument_should_find_types_defined_in_mscorlib()
        {
            await AssertTranslationAsync(
                "using System.Text; class C { private StringBuilder _builder; }",
                "import 'mscorlib';\n\nclass C {\n  private _builder: ss.StringBuilder;\n}\n");
        }
    }
}
