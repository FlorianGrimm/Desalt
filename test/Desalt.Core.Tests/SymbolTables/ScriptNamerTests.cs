// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNamerTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class ScriptNamerTests
    {
        private static async Task AssertScriptNamesInDocument(string code, params string[] expectedScriptNames)
        {
            await AssertScriptNamesInDocument(code, RenameRules.Default, expectedScriptNames);
        }

        private static async Task AssertScriptNamesInDocument(
            string code,
            RenameRules renameRules,
            params string[] expectedScriptNames)
        {
            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                IAssemblySymbol mscorlibAssemblySymbol =
                    SymbolDiscoverer.GetMscorlibAssemblySymbol(context.SemanticModel.Compilation);

                var scriptNamer = new ScriptNamer(mscorlibAssemblySymbol, renameRules);

                var actualScriptNames = new List<string>();
                IEnumerable<INamedTypeSymbol> allTypes = context.RootSyntax.GetAllDeclaredTypes(context.SemanticModel, CancellationToken.None);
                foreach (INamedTypeSymbol type in allTypes)
                {
                    actualScriptNames.Add(scriptNamer.DetermineScriptNameForSymbol(type));
                    actualScriptNames.AddRange(
                        type.GetMembers().Select(member => scriptNamer.DetermineScriptNameForSymbol(member)));
                }

                actualScriptNames.Should().ContainInOrder(expectedScriptNames);
            }
        }

        [Test]
        public async Task ScriptNamer_should_preserve_the_case_of_interfaces_classes_structs_and_enums()
        {
            await AssertScriptNamesInDocument(
                "interface MyInterface {} class MyClass {} struct MyStruct {}",
                "MyInterface",
                "MyClass",
                "MyStruct");
        }

        [Test]
        public async Task ScriptNameSymbolTable_should_make_members_camelCase_by_default()
        {
            await AssertScriptNamesInDocument(
                "class C { int MyInt; } interface I { void MyMethod(); }",
                "C",
                "myInt",
                "I",
                "myMethod");
        }

        [Test]
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

            await AssertScriptNamesInDocument(
                code,
                "ScriptClass",
                "ScriptField",
                "ScriptEvent",
                "ScriptInterface",
                "ScriptMethod",
                "ScriptStruct",
                "ScriptProperty");
        }

        [Test]
        public async Task ScriptNameSymbolTable_should_rename_private_fields_if_specified_by_the_options()
        {
            await AssertScriptNamesInDocument(
                "class C { private string name; }",
                RenameRules.Default.WithFieldRule(FieldRenameRule.PrivateDollarPrefix),
                "C",
                "$name");
        }

        [Test]
        public async Task ScriptNameSymbolTable_should_only_rename_fields_if_there_is_a_duplicate_name()
        {
            await AssertScriptNamesInDocument(
                "class C { private int x; private string name; public string Name { get; } }",
                RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName),
                "C",
                "x",
                "$name",
                "name");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "Field", "Event", "I", "Method", "S", "Property");
        }

        [Test]
        public async Task
            ScriptNameSymbolTable_should_respect_the_PreserveMemberCase_attribute_on_the_parent_declaration()
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

            await AssertScriptNamesInDocument(code, "C", "Field", "Method");
        }

        [Test]
        public async Task ScriptNameSymbolTable_should_respect_the_PreserveMemberCase_attribute_on_the_assembly()
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

            await AssertScriptNamesInDocument(code, "C", "Field", "Method");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "trumpedField", "trumpedMethod");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "trumpedField", "aliasedMethod");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "method", "method$1", "method$2");
        }

        [Test]
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

            await AssertScriptNamesInDocument(
                code,
                "C",
                "method",
                "static_1",
                "method",
                "instance_1",
                "Method",
                "method$1");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "method", "method$1", "method", "method$1");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "method", "method", "method", "method");
        }

        [Test]
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

            await AssertScriptNamesInDocument(code, "C", "method", "method");
        }

        [Test]
        public async Task
            ScriptNameSymbolTable_should_use_script_name_of_implementation_in_alternate_signature_overloads()
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

            await AssertScriptNamesInDocument(code, "C", "overloaded", "overloaded");
        }

        [Test]
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
            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();

                // get the StringBuilder symbol
                ITypeSymbol stringBuilderSymbol = context.SemanticModel.GetTypeInfo(
                        context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().Single().Declaration.Type)
                    .Type;

                var scriptNamer = new ScriptNamer(
                    SymbolDiscoverer.GetMscorlibAssemblySymbol(context.SemanticModel.Compilation));

                scriptNamer.DetermineScriptNameForSymbol(stringBuilderSymbol).Should().Be("ss.StringBuilder");
            }
        }
    }
}
