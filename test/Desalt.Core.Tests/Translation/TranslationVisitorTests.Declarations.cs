// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class TranslationVisitorTests
    {
        //// ===========================================================================================================
        //// Interface Declaration Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Bare_interface_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("interface ITest {}", "interface ITest {\n}\n");
        }

        [TestMethod]
        public async Task Public_interface_declaration_should_be_exported()
        {
            await AssertTranslation("public interface ITest {}", "export interface ITest {\n}\n");
        }

        [TestMethod]
        public async Task A_method_declaration_with_no_parameters_and_a_void_return_type_should_be_translated()
        {
            await AssertTranslation("interface ITest { void Do(); }", "interface ITest {\n  do(): void;\n}\n");
        }

        [TestMethod]
        public async Task A_method_declaration_with_simple_parameters_and_a_void_return_type_should_be_translated()
        {
            await AssertTranslation(
                "interface ITest { void Do(string x, string y); }",
                "interface ITest {\n  do(x: string, y: string): void;\n}\n");
        }

        //// ===========================================================================================================
        //// Enum Declaration Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Bare_enum_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("enum LoggerLevel { All }", "enum LoggerLevel {\n  all,\n}\n");
        }

        [TestMethod]
        public async Task Public_enum_declaration_should_be_exported()
        {
            await AssertTranslation("public enum LoggerLevel { All }", "export enum LoggerLevel {\n  all,\n}\n");
        }

        [TestMethod]
        public async Task Enum_declarations_with_literal_values_should_be_translated()
        {
            await AssertTranslation(
                "enum LoggerLevel { All = 123 }",
                "enum LoggerLevel {\n  all = 123,\n}\n");
        }

        [TestMethod]
        public async Task Enum_declarations_with_hex_values_should_be_translated_as_hex()
        {
            await AssertTranslation(
                "enum LoggerLevel { Hex = 0x100 }",
                "enum LoggerLevel {\n  hex = 0x100,\n}\n");
        }

        //// ===========================================================================================================
        //// Class Declaration Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Bare_class_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("class C { }", "class C {\n}\n");
        }

        [TestMethod]
        public async Task Public_class_declaration_should_be_exported()
        {
            await AssertTranslation("public class C { }", "export class C {\n}\n");
        }

        [TestMethod]
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

        [TestMethod]
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
    }
}
