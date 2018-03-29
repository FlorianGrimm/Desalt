// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptNameSymbolTableTests
    {
        private static async Task AssertEntriesInSymbolTable(
            string code,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            await AssertEntriesInSymbolTable(code, null, expectedEntries);
        }

        private static async Task AssertEntriesInSymbolTable(
            string code,
            CompilerOptions options,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            using (var tempProject = TempProject.Create("TempProject", new TempProjectFile("File.cs", code)))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs", options);

                var symbolTable = new ScriptNameSymbolTable();
                symbolTable.AddDefinedTypesInDocument(context, CancellationToken.None);

                symbolTable.Should().BeEquivalentTo(expectedEntries);
            }
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_preserve_the_case_of_interfaces_classes_structs_and_enums()
        {
            await AssertEntriesInSymbolTable(
                "interface MyInterface {} class MyClass {} struct MyStruct {}",
                new KeyValuePair<string, string>("interface MyInterface", "MyInterface"),
                new KeyValuePair<string, string>("class MyClass", "MyClass"),
                new KeyValuePair<string, string>("struct MyStruct", "MyStruct"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_skip_instance_and_static_constructors()
        {
            await AssertEntriesInSymbolTable(
                "class C { C(int x) { } static C() { }",
                new KeyValuePair<string, string>("class C", "C"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_not_store_delegate_types()
        {
            await AssertEntriesInSymbolTable("delegate void MyDelegate();");
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_make_members_camelCase_by_default()
        {
            await AssertEntriesInSymbolTable(
                "class C { int MyInt; } interface I { void MyMethod(); } enum MyEnum { One }",
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.MyInt", "myInt"),
                new KeyValuePair<string, string>("interface I", "I"),
                new KeyValuePair<string, string>("I.MyMethod", "myMethod"),
                new KeyValuePair<string, string>("enum MyEnum", "MyEnum"),
                new KeyValuePair<string, string>("MyEnum.One", "one"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_events_but_no_add_remove_methods()
        {
            await AssertEntriesInSymbolTable(
                "class C { event System.Action MyEvent; }",
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("event C.MyEvent", "myEvent"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_properties_but_no_get_set_methods()
        {
            await AssertEntriesInSymbolTable(
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
    private void Method() { }
}

[ScriptName(""ScriptStruct"")]
struct S
{
    [ScriptName(""ScriptProperty"")]
    public bool Property { get; set; }
}";

            await AssertEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("class C", "ScriptClass"),
                new KeyValuePair<string, string>("C.field", "ScriptField"),
                new KeyValuePair<string, string>("event C.Event", "ScriptEvent"),
                new KeyValuePair<string, string>("interface I", "ScriptInterface"),
                new KeyValuePair<string, string>("I.Method", "ScriptMethod"),
                new KeyValuePair<string, string>("struct S", "ScriptStruct"),
                new KeyValuePair<string, string>("S.Property", "ScriptProperty"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_rename_private_fields_if_specified_by_the_options()
        {
            await AssertEntriesInSymbolTable(
                "class C { private string name; }",
                new CompilerOptions(
                    "outPath",
                    renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.PrivateDollarPrefix)),
                new KeyValuePair<string, string>("class C", "C"),
                new KeyValuePair<string, string>("C.name", "$name"));
        }
    }
}
