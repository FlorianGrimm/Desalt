// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.Expressions.cs" company="Justin Rockwood">
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
        [TestClass]
        public class Expressions
        {
            [TestMethod]
            public async Task This_expression_assigning_numeric_literals()
            {
                await AssertTranslation(
                    "class C { private int Int32; public C() { this.Int32 = 1; } }",
                    "class C {\n  private int32: number;\n\n  public constructor() {\n    this.int32 = 1;\n  }\n}\n");
            }

            [TestMethod]
            public async Task This_expression_assigning_string_literals()
            {
                await AssertTranslation(
                    "class C { private string Str; public C() { this.Str = \"s\"; } }",
                    "class C {\n  private str: string;\n\n  public constructor() {\n    this.str = 's';\n  }\n}\n");
            }

            [TestMethod]
            public async Task Cast_of_a_field_should_use_the_script_name()
            {
                await AssertTranslation(
                    "class C { private string Str; public void Method() { var x = (string)Str; } }",
                    "class C {\n  private str: string;\n\n  public method(): void {\n    " +
                    "let x: string = <string>this.str;\n  }\n}\n");
            }

            [TestMethod]
            public async Task Cast_of_a_method_invocation_should_use_the_script_name()
            {
                await AssertTranslation(
                    "class C { public string Method() { var x = (string)Method(); } }",
                    "class C {\n  public method(): string {\n    " +
                    "let x: string = <string>this.method();\n  }\n}\n");
            }

            [TestMethod]
            public async Task Invocation_expressions_on_instance_methods_should_prefix_this()
            {
                await AssertTranslation(
                    "class C { public void Method() { Method(); } }",
                    "class C {\n  public method(): void {\n    this.method();\n  }\n}\n");
            }

            [TestMethod]
            public async Task Invocation_expression_on_static_method_should_prefix_class_name()
            {
                await AssertTranslation(
                    "class X { public static void A() { A(); }",
                    "class X {\n  public static a(): void {\n    X.a();\n  }\n}\n");
            }

            [TestMethod]
            public async Task Complex_example_involving_referencing_an_enum_value_inside_of_a_element_access_expression()
            {
                await AssertTranslation(
                    @"
enum LoggerLevel { All = 0 }
class Logger
{
    public static List<string> LoggerLevelNames = new List<string>();

    private static void Init()
    {
        LoggerLevelNames[(int)LoggerLevel.All] = ""all"";
    }
}",
                    @"enum LoggerLevel {
  all = 0,
}

class Logger {
  public static loggerLevelNames: List<string> = new List<string>();

  private static init(): void {
    Logger.loggerLevelNames[<number>LoggerLevel.all] = 'all';
  }
}
".Replace("\r\n", "\n"));
            }
        }
    }
}
