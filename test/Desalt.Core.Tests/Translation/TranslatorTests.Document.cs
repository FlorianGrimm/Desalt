// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorTests.Document.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using NUnit.Framework;

    public partial class TranslatorTests
    {
        [Test]
        public async Task TranslateDocument_should_translate_an_interface_declaration_with_members_and_doc_comments()
        {
            await AssertTranslation(
                @"
/// <summary>
/// Tests an interface declaration.
/// </summary>
public interface MyInterface
{
    void Method();
}
",
                @"
/**
 * Tests an interface declaration.
 */
export interface MyInterface {
  method(): void;
}
");
        }

        [Test]
        public async Task TranslateDocument_should_find_types_defined_in_mscorlib()
        {
            await AssertTranslation(
                @"
using System.Text;

class C
{
    private StringBuilder _builder;
}
",
                @"
import 'mscorlib';

class C {
  private _builder: ss.StringBuilder;
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes,
                includeImports: true);
        }
    }
}
