// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.Emit;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CSharpToTypeScriptTranslatorTests
    {
        private static async Task AssertTranslationAsync(string csharpCode, string expectedTypeScriptCode)
        {
            string code = $@"
using System;
using System.Collections.Generic;

{csharpCode}
";

            using (var tempProject = await TempProject.CreateAsync("TestProject", new TempProjectFile("File", code)))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync("File");
                var translator = new CSharpToTypeScriptTranslator();
                var result = translator.TranslateDocument(context);

                result.Diagnostics.Should().BeEmpty();

                // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
                string translated = result.Result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
                translated.Should().Be(expectedTypeScriptCode.TrimStart().Replace("\r\n", "\n"));
            }
        }

        [TestMethod]
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

        [TestMethod]
        public async Task TranslateDocument_should_find_types_defined_in_mscorlib()
        {
            await AssertTranslationAsync(
                "class C { private List<string> _list; }",
                "import { List } from 'mscorlib';\n\nclass C {\n  private _list: List<string>;\n}\n");
        }
    }
}
