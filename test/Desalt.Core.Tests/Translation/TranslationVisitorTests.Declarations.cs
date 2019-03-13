// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    public partial class TranslationVisitorTests
    {
        //// ===========================================================================================================
        //// Interface Declaration Tests
        //// ===========================================================================================================

        [Test]
        public async Task Bare_interface_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("interface ITest {}", "interface ITest {\n}\n");
        }

        [Test]
        public async Task Public_interface_declaration_should_be_exported()
        {
            await AssertTranslation("public interface ITest {}", "export interface ITest {\n}\n");
        }

        [Test]
        public async Task A_method_declaration_with_no_parameters_and_a_void_return_type_should_be_translated()
        {
            await AssertTranslation("interface ITest { void Do(); }", "interface ITest {\n  do(): void;\n}\n");
        }

        [Test]
        public async Task A_method_declaration_with_simple_parameters_and_a_void_return_type_should_be_translated()
        {
            await AssertTranslation(
                "interface ITest { void Do(string x, string y); }",
                "interface ITest {\n  do(x: string, y: string): void;\n}\n");
        }

        //// ===========================================================================================================
        //// Enum Declaration Tests
        //// ===========================================================================================================

        [Test]
        public async Task Bare_enum_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("enum LoggerLevel { All }", "enum LoggerLevel {\n  All,\n}\n");
        }

        [Test]
        public async Task Public_enum_declaration_should_be_exported()
        {
            await AssertTranslation("public enum LoggerLevel { All }", "export enum LoggerLevel {\n  All,\n}\n");
        }

        [Test]
        public async Task Enum_declarations_with_literal_values_should_be_translated()
        {
            await AssertTranslation(
                "enum LoggerLevel { All = 123 }",
                "enum LoggerLevel {\n  All = 123,\n}\n");
        }

        [Test]
        public async Task Enum_declarations_with_hex_values_should_be_translated_as_hex()
        {
            await AssertTranslation(
                "enum LoggerLevel { Hex = 0x100 }",
                "enum LoggerLevel {\n  Hex = 0x100,\n}\n");
        }

        [Test]
        public async Task Enum_declarations_should_respect_EnumRenameRule_LowerCaseFirstChar()
        {
            await AssertTranslation(
                "enum E { [ScriptName(\"uno\")] One, Two }",
                "enum E {\n  uno,\n  two,\n}\n",
                populateOptionsFunc: options =>
                    options.WithRenameRules(options.RenameRules.WithEnumRule(EnumRenameRule.LowerCaseFirstChar)));
        }

        [Test]
        public async Task Enum_declarations_with_NamedValues_should_be_a_const_enum()
        {
            await AssertTranslation(
                "[NamedValues] enum E { One, Two }",
                "const enum E {\n  One = 'one',\n  Two = 'two',\n}\n");
        }

        [Test]
        public async Task Enum_declarations_with_NamedValues_should_be_a_const_enum_even_when_a_value_has_been_defined()
        {
            await AssertTranslation(
                "[NamedValues] enum E { One = 1, Two = 2 }",
                "const enum E {\n  One = 'one',\n  Two = 'two',\n}\n");
        }

        [Test]
        public async Task
            Enum_declarations_with_NamedValues_should_use_script_name_for_the_value_and_not_the_declaration()
        {
            await AssertTranslation(
                "[NamedValues] enum E { [ScriptName(\"uno\")] One, Two }",
                "const enum E {\n  One = 'uno',\n  Two = 'two',\n}\n");
        }

        [Test]
        public async Task
            Enum_declarations_with_NamedValues_should_use_script_name_and_respect_EnumRenameRule_LowerCaseFirstChar()
        {
            await AssertTranslation(
                "[NamedValues] enum E { [ScriptName(\"uno\")] One, Two }",
                "const enum E {\n  one = 'uno',\n  two = 'two',\n}\n",
                populateOptionsFunc: options =>
                    options.WithRenameRules(options.RenameRules.WithEnumRule(EnumRenameRule.LowerCaseFirstChar)));
        }

        //// ===========================================================================================================
        //// Class Declaration Tests
        //// ===========================================================================================================

        [Test]
        public async Task Bare_class_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("class C { }", "class C {\n}\n");
        }

        [Test]
        public async Task Public_class_declaration_should_be_exported()
        {
            await AssertTranslation("public class C { }", "export class C {\n}\n");
        }

        [Test]
        public async Task Translate_class_declarations_with_class_heritage()
        {
            await AssertTranslation(
                "interface IA{} interface IC{} class A : IA{} class B : A{} class C : B, IA, IC{}",
                @"
interface IA {
}

interface IC {
}

class A implements IA {
}

class B extends A {
}

class C extends B implements IA, IC {
}
");
        }

        [Test]
        public async Task Translate_should_rename_overloaded_method_declarations()
        {
            await AssertTranslation(
                @"
class A
{
    public void Method()
    {
    }

    public void Method(int x)
    {
    }
}",
                @"
class A {
  public method(): void { }

  public method$1(x: number): void { }
}
");
        }

        [Test]
        public async Task Translate_should_skip_methods_and_ctors_marked_with_InlineCode_in_declarations()
        {
            await AssertTranslation(
                @"
class A
{
    [InlineCode(""new object()"")]
    public A() { }

    [InlineCode(""ss.contains()"")]
    public static bool Contains()
    {
        return true;
    }
}
",
                @"
class A {
}
");
        }

        [Test]
        public async Task Translate_should_skip_methods_but_not_ctors_marked_with_ScriptSkip_in_declarations()
        {
            await AssertTranslation(
                @"
class A
{
    [ScriptSkip]
    public A() { }

    [ScriptSkip]
    public void Method() { }
}",
                @"
class A {
  public constructor() { }
}
");
        }
    }
}
