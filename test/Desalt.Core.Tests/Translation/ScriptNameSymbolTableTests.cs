// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptNameSymbolTableTests
    {
        private static async Task AssertDocumentEntriesInSymbolTable(
            string code,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            await AssertEntriesInSymbolTable(
                code,
                options: null,
                discoveryKind: SymbolTableDiscoveryKind.OnlyDocumentTypes,
                expectedEntries: expectedEntries);
        }

        private static async Task AssertDocumentEntriesInSymbolTable(
            string code,
            CompilerOptions options,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            await AssertEntriesInSymbolTable(
                code,
                options,
                discoveryKind: SymbolTableDiscoveryKind.OnlyDocumentTypes,
                expectedEntries: expectedEntries);
        }

        private static async Task AssertExternalEntriesInSymbolTable(
            string code,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            await AssertEntriesInSymbolTable(
                code,
                options: null,
                discoveryKind: SymbolTableDiscoveryKind.DocumentAndReferencedTypes,
                expectedEntries: expectedEntries);
        }

        private static async Task AssertEntriesInSymbolTable(
            string code,
            CompilerOptions options,
            SymbolTableDiscoveryKind discoveryKind,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            using (var tempProject = await TempProject.CreateAsync("TempProject", new TempProjectFile("File.cs", code)))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs", options);

                var symbolTable = ScriptNameSymbolTable.Create(context.ToSafeArray(), discoveryKind);

                switch (discoveryKind)
                {
                    case SymbolTableDiscoveryKind.OnlyDocumentTypes:
                        symbolTable.DocumentSymbols.Should().BeEquivalentTo(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndReferencedTypes:
                        symbolTable.DirectlyReferencedExternalSymbols.Should().Contain(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes:
                        var expectedKeys = expectedEntries.Select(pair => pair.Key).ToImmutableArray();
                        symbolTable.IndirectlyReferencedExternalSymbols.Where(pair => pair.Key.IsOneOf(expectedKeys))
                            .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.Value))
                            .Should()
                            .BeEquivalentTo(expectedEntries);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(discoveryKind), discoveryKind, null);
                }
            }
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_preserve_the_case_of_interfaces_classes_structs_and_enums()
        {
            await AssertDocumentEntriesInSymbolTable(
                "interface MyInterface {} class MyClass {} struct MyStruct {}",
                new KeyValuePair<string, string>("interface MyInterface", "MyInterface"),
                new KeyValuePair<string, string>("class MyClass", "MyClass"),
                new KeyValuePair<string, string>("struct MyStruct", "MyStruct"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_skip_instance_and_static_constructors()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { C(int x) { } static C() { } }",
                new KeyValuePair<string, string>("class C", "C"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_not_store_delegate_types()
        {
            await AssertDocumentEntriesInSymbolTable("delegate void MyDelegate();");
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_make_members_camelCase_by_default()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { int MyInt; } interface I { void MyMethod(); } enum MyEnum { One }",
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.MyInt", "myInt"),
                new KeyValuePair<string, string>("interface I", "I"),
                new KeyValuePair<string, string>("I.MyMethod()", "myMethod"),
                new KeyValuePair<string, string>("enum MyEnum", "MyEnum"),
                new KeyValuePair<string, string>("MyEnum.One", "one"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_events_but_no_add_remove_methods()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { event System.Action MyEvent; }",
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("event C.MyEvent", "myEvent"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_properties_but_no_get_set_methods()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { bool MyProperty { get; set; } }",
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.MyProperty", "myProperty"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_respect_the_ScriptName_attribute()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[ScriptName(""ScriptClass"")]
class C
{
    [ScriptName(""ScriptField"")]
    private int field;

    [ScriptName(""ScriptEvent"")]
    public event Action Event;
}

[ScriptName(""ScriptInterface"")]
interface I
{
    [ScriptName(""ScriptMethod"")]
    void Method();
}

[ScriptName(""ScriptStruct"")]
struct S
{
    [ScriptName(""ScriptProperty"")]
    public bool Property { get { return true; } }
}";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "ScriptClass"),
                new KeyValuePair<string, string>("C.field", "ScriptField"),
                new KeyValuePair<string, string>("event C.Event", "ScriptEvent"),
                new KeyValuePair<string, string>("interface I", "ScriptInterface"),
                new KeyValuePair<string, string>("I.Method()", "ScriptMethod"),
                new KeyValuePair<string, string>("struct S", "ScriptStruct"),
                new KeyValuePair<string, string>("S.Property", "ScriptProperty"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_rename_private_fields_if_specified_by_the_options()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { private string name; }",
                new CompilerOptions(
                    "outPath",
                    renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.PrivateDollarPrefix)),
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.name", "$name"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_only_rename_fields_if_there_is_a_duplicate_name()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { private int x; private string name; public string Name { get; } }",
                new CompilerOptions(
                    "outPath",
                    renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName)),
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.x", "x"),
                new KeyValuePair<string, string>("C.name", "$name"),
                new KeyValuePair<string, string>("C.Name", "name"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_respect_the_PreserveCase_attribute()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [PreserveCase]
    private int Field;

    [PreserveCase]
    public event Action Event;
}

interface I
{
    [PreserveCase]
    void Method();
}

struct S
{
    [PreserveCase]
    public bool Property { get { return true; } }
}";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.Field", "Field"),
                new KeyValuePair<string, string>("event C.Event", "Event"),
                new KeyValuePair<string, string>("interface I", "I"),
                new KeyValuePair<string, string>("I.Method()", "Method"),
                new KeyValuePair<string, string>("struct S", "S"),
                new KeyValuePair<string, string>("S.Property", "Property"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_respect_the_PreserveMemberCase_attribute_on_the_parent_declaration()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[PreserveMemberCase]
class C
{
    private int Field;
    private void Method() {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.Field", "Field"),
                new KeyValuePair<string, string>("C.Method()", "Method"));
        }

        [TestMethod]
        public async Task
            ScriptNameSymbolTable_should_respect_the_PreserveMemberCase_attribute_on_the_assembly()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[assembly: PreserveMemberCase]
class C
{
    private int Field;
    private void Method() {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.Field", "Field"),
                new KeyValuePair<string, string>("C.Method()", "Method"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_use_ScriptName_over_PreserveCase_or_PreserveMemberCase()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[PreserveMemberCase]
class C
{
    [PreserveCase]
    [ScriptName(""trumpedField"")]
    private int Field;

    [ScriptName(""trumpedMethod"")]
    private void Method(int x) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.Field", "trumpedField"),
                new KeyValuePair<string, string>("C.Method(int x)", "trumpedMethod"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_use_ScriptName_from_referenced_assemblies()
        {
            const string code = @"
using System;
using Underscore;

class C
{
    private void Method()
    {
        var value = new UnderscoreValue<int>();
    }
}
";

            await AssertExternalEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class Underscore.UnderscoreValue<int>", "UnderscoreValue"),
                new KeyValuePair<string, string>("Underscore.UnderscoreValue<int>.Value()", "value"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_respect_ScriptAlias()
        {
            const string code = @"
using System;
using jQueryApi;

class C
{
    private void Method()
    {
        var element = jQuery.FromHtml(""<div>"");
    }
}
";

            await AssertExternalEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("jQueryApi.jQuery.FromHtml(string html)", "$"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_bring_in_all_of_the_symbols_in_referenced_assemblies()
        {
            await AssertEntriesInSymbolTable(
                "using System; class C { bool x; }",
                options: null,
                discoveryKind: SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                expectedEntries: new KeyValuePair<string, string>("System.Script.Eval(string s)", "eval"));
        }
    }
}
