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

        //// ===========================================================================================================
        //// Property Declaration Tests
        //// ===========================================================================================================

        [Test]
        public async Task Translate_should_accept_property_accessors_with_an_explicit_body()
        {
            await AssertTranslation(
                @"
class A
{
    private int _x;

    public int GetOnly
    {
        get { return _x; }
    }

    public int GetAndSet
    {
        get { return _x; }
        set { _x = value; }
    }
}",
                @"
class A {
  private _x: number;

  public get_getOnly(): number {
    return this._x;
  }

  public get_getAndSet(): number {
    return this._x;
  }

  public set_getAndSet(value: number): void {
    this._x = value;
  }
}
");
        }

        [Test]
        public async Task Translate_should_accept_auto_generated_property_accessor_declarations()
        {
            await AssertTranslation(
                @"
class A
{
    public static int StaticProp { get; set; }
    public string GetOnly { get; private set; }
    public string GetAndSet { get; set; }
}",
                @"
class A {
  private static _$staticPropField: number;

  private _$getOnlyField: string;

  private _$getAndSetField: string;

  public static get_staticProp(): number {
    return A._$staticPropField;
  }

  public static set_staticProp(value: number): void {
    A._$staticPropField = value;
  }

  public get_getOnly(): string {
    return this._$getOnlyField;
  }

  private set_getOnly(value: string): void {
    this._$getOnlyField = value;
  }

  public get_getAndSet(): string {
    return this._$getAndSetField;
  }

  public set_getAndSet(value: string): void {
    this._$getAndSetField = value;
  }
}
");
        }

        [Test]
        public async Task Translate_should_accept_property_declarations_in_interfaces()
        {
            await AssertTranslation(
                @"
interface I
{
    int GetOnly { get; }
    int SetOnly { set; }
    int GetAndSet { get; set; }
}
",
                @"
interface I {
  get_getOnly(): number;
  set_setOnly(value: number): void;
  get_getAndSet(): number;
  set_getAndSet(value: number): void;
}
");
        }
    }
}
