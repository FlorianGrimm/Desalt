// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs", options);
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var directlyReferencedExternalTypeSymbols =
                    SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(contexts, discoveryKind);

                var symbolTable = ScriptNameSymbolTable.Create(
                    contexts,
                    directlyReferencedExternalTypeSymbols,
                    indirectlyReferencedExternalTypeSymbols: SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                        directlyReferencedExternalTypeSymbols,
                        context.SemanticModel.Compilation));

                switch (discoveryKind)
                {
                    case SymbolTableDiscoveryKind.OnlyDocumentTypes:
                        symbolTable.DocumentSymbols
                            .Select(
                                pair => new KeyValuePair<string, string>(
                                    pair.Key.ToHashDisplay(),
                                    pair.Value))
                            .Should()
                            .BeEquivalentTo(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndReferencedTypes:
                        symbolTable.DirectlyReferencedExternalSymbols.Select(
                                pair => new KeyValuePair<string, string>(
                                    pair.Key.ToHashDisplay(),
                                    pair.Value))
                            .Should()
                            .Contain(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes:
                        var expectedKeys = expectedEntries.Select(pair => pair.Key).ToImmutableArray();
                        symbolTable.IndirectlyReferencedExternalSymbols
                            .Where(pair => pair.Key.ToHashDisplay().IsOneOf(expectedKeys))
                            .Select(
                                pair => new KeyValuePair<string, string>(
                                    pair.Key.ToHashDisplay(),
                                    pair.Value.Value))
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
                new KeyValuePair<string, string>("MyInterface", "MyInterface"),
                new KeyValuePair<string, string>("MyClass", "MyClass"),
                new KeyValuePair<string, string>("MyStruct", "MyStruct"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_skip_instance_and_static_constructors()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { C(int x) { } static C() { } }",
                new KeyValuePair<string, string>("C", "C"));
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
                "class C { int MyInt; } interface I { void MyMethod(); }",
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.MyInt", "myInt"),
                new KeyValuePair<string, string>("I", "I"),
                new KeyValuePair<string, string>("I.MyMethod()", "myMethod"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_events_but_no_add_remove_methods()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { event System.Action MyEvent; }",
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.MyEvent", "myEvent"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_handle_properties_but_no_get_set_methods()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { bool MyProperty { get; set; } }",
                new KeyValuePair<string, string>("C", "C"),
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
                new KeyValuePair<string, string>("C", "ScriptClass"),
                new KeyValuePair<string, string>("C.field", "ScriptField"),
                new KeyValuePair<string, string>("C.Event", "ScriptEvent"),
                new KeyValuePair<string, string>("I", "ScriptInterface"),
                new KeyValuePair<string, string>("I.Method()", "ScriptMethod"),
                new KeyValuePair<string, string>("S", "ScriptStruct"),
                new KeyValuePair<string, string>("S.Property", "ScriptProperty"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_rename_private_fields_if_specified_by_the_options()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { private string name; }",
                new CompilerOptions(
                    renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.PrivateDollarPrefix)),
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.name", "$name"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_only_rename_fields_if_there_is_a_duplicate_name()
        {
            await AssertDocumentEntriesInSymbolTable(
                "class C { private int x; private string name; public string Name { get; } }",
                new CompilerOptions(
                    renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName)),
                new KeyValuePair<string, string>("C", "C"),
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
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Field", "Field"),
                new KeyValuePair<string, string>("C.Event", "Event"),
                new KeyValuePair<string, string>("I", "I"),
                new KeyValuePair<string, string>("I.Method()", "Method"),
                new KeyValuePair<string, string>("S", "S"),
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
                new KeyValuePair<string, string>("C", "C"),
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
                new KeyValuePair<string, string>("C", "C"),
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
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Field", "trumpedField"),
                new KeyValuePair<string, string>("C.Method(int x)", "trumpedMethod"));
        }

        [TestMethod]
        public async Task
            ScriptNameSymbolTable_should_use_ScriptAlias_over_ScriptName_or_PreserveCase_or_PreserveMemberCase()
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

    [PreserveCase]
    [ScriptName(""trumpedMethod"")]
    [ScriptAlias(""aliasedMethod"")]
    private static void Method(int x) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Field", "trumpedField"),
                new KeyValuePair<string, string>("C.Method(int x)", "aliasedMethod"));
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
                new KeyValuePair<string, string>("Underscore.UnderscoreValue<int>", "UnderscoreValue"),
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
                null,
                SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes,
                new KeyValuePair<string, string>("System.Script", "ss"),
                new KeyValuePair<string, string>("System.Script.Eval(string s)", "eval"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_find_instantiated_versions_of_generic_methods()
        {
            const string code = @"
using System;

public static class ScriptEx
{
    public static T Value<T>(T a, T b)
    {
        return default(T);
    }

    public static void Main()
    {
        int x = ScriptEx.Value(1, 10);
    }
}
";
            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextForFileAsync();

                var scriptNameTable = ScriptNameSymbolTable.Create(
                    context.ToSingleEnumerable().ToImmutableArray(),
                    ImmutableArray<ITypeSymbol>.Empty,
                    ImmutableArray<INamedTypeSymbol>.Empty);

                // get the method invocation symbol
                var memberAccessSyntax = context.RootSyntax.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Single();

                var symbol = context.SemanticModel.GetSymbolInfo(memberAccessSyntax).Symbol;

                scriptNameTable.TryGetValue(symbol, out string scriptName).Should().BeTrue();
                scriptName.Should().Be("value");
            }
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_rename_overloaded_method_declarations()
        {
            const string code = @"
using System;

class C
{
    public void Method() {}
    public void Method(int x) {}
    public void Method(string y) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method()", "method"),
                new KeyValuePair<string, string>("C.Method(int x)", "method$1"),
                new KeyValuePair<string, string>("C.Method(string y)", "method$2"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_use_attributes_first_before_renaming_overloads()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    public static void Method() {}

    [ScriptAlias(""static_1"")]
    public static void Method(int i, string s) {}

    public void Method(int i) {}

    [ScriptName(""instance_1"")]
    public void Method(string s) {}

    [PreserveCase]
    public void Method(double d) {}

    public void Method(float f) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method()", "method"),
                new KeyValuePair<string, string>("C.Method(int i, string s)", "static_1"),
                new KeyValuePair<string, string>("C.Method(int i)", "method"),
                new KeyValuePair<string, string>("C.Method(string s)", "instance_1"),
                new KeyValuePair<string, string>("C.Method(double d)", "Method"),
                new KeyValuePair<string, string>("C.Method(float f)", "method$1"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_consider_static_vs_non_static_when_renaming_overloads()
        {
            const string code = @"
using System;

class C
{
    public static void Method() {}
    public static void Method(int x, string y) {}

    public void Method(int i) {}
    public void Method(string s) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method()", "method"),
                new KeyValuePair<string, string>("C.Method(int x, string y)", "method$1"),
                new KeyValuePair<string, string>("C.Method(int i)", "method"),
                new KeyValuePair<string, string>("C.Method(string s)", "method$1"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_not_rename_imported_overloads()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[Imported]
class C
{
    public static void Method() {}
    public static void Method(int x, string y) {}

    public void Method(int i) {}
    public void Method(string s) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method()", "method"),
                new KeyValuePair<string, string>("C.Method(int x, string y)", "method"),
                new KeyValuePair<string, string>("C.Method(int i)", "method"),
                new KeyValuePair<string, string>("C.Method(string s)", "method"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_not_rename_alternate_signature_overloads()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [AlternateSignature]
    public extern void Method(int i);
    public void Method(string s) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method(int i)", "method"),
                new KeyValuePair<string, string>("C.Method(string s)", "method"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_use_script_name_of_implementation_in_alternate_signature_overloads()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [AlternateSignature]
    public extern void Method(int i);
    [ScriptName(""overloaded"")]
    public void Method(string s) {}
}
";

            await AssertDocumentEntriesInSymbolTable(
                code,
                new KeyValuePair<string, string>("C", "C"),
                new KeyValuePair<string, string>("C.Method(int i)", "overloaded"),
                new KeyValuePair<string, string>("C.Method(string s)", "overloaded"));
        }

        [TestMethod]
        public async Task ScriptNameSymbolTable_should_prefix_ss_to_mscorlib_types()
        {
            const string code = @"
using System.Runtime.CompilerServices;
using System.Text;

class C
{
    private StringBuilder _builder = new StringBuilder();
}
";
            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync();
                var scriptNameTable = context.ScriptNameSymbolTable;

                // get the StringBuilder symbol
                var stringBuilderSymbol = context.SemanticModel.GetTypeInfo(
                        context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().Single().Declaration.Type)
                    .Type;

                scriptNameTable.TryGetValue(stringBuilderSymbol, out string scriptName).Should().BeTrue();
                scriptName.Should().Be("ss.StringBuilder");
            }
        }
    }
}
