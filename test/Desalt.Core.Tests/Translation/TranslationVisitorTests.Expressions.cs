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
");
            }

            [TestMethod]
            public async Task Assignment_expressions()
            {
                foreach (string op in new[] { "=", "*=", "/=", "%=", "+=", "-=", "<<=", ">>=", "&=", "^=", "|=" })
                {
                    string code = $"class C {{ void Method() {{ int x = 1; x {op} 123; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 1;\n    x {op} 123;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Prefix_unary_expressions()
            {
                foreach (string op in new[] { "++", "--", "+", "-", "~", "!" })
                {
                    string code = $"class C {{ void Method() {{ int x = 123; {op}x; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 123;\n    {op}x;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Postfix_unary_expressions()
            {
                foreach (string op in new[] { "++", "--" })
                {
                    string code = $"class C {{ void Method() {{ int x = 123; x{op}; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 123;\n    x{op};\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Binary_expressions_on_numbers()
            {
                foreach (string op in new[] { "*", "/", "%", "+", "-", "<<", ">>", "&", "^", "|" })
                {
                    string code = $"class C {{ void Method() {{ int x = 1; int y = 2; x = x {op} y; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n" +
                        $"    let x: number = 1;\n    let y: number = 2;\n    x = x {op} y;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Binary_expressions_on_comparisons()
            {
                foreach (string op in new[] { "<", ">", "<=", ">=", "==", "!=" })
                {
                    string code = $"class C {{ void Method() {{ int x = 1; int y = 2; bool z = x {op} y; }} }}";

                    string expectedOp = op == "==" ? "===" : op == "!=" ? "!==" : op;
                    string expected = $"class C {{\n  private method(): void {{\n" +
                        $"    let x: number = 1;\n    let y: number = 2;\n" +
                        $"    let z: boolean = x {expectedOp} y;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Binary_expressions_on_booleans()
            {
                foreach (string op in new[] { "&&", "||" })
                {
                    string code = $"class C {{ void Method() {{ bool x = true; bool y = false; x = x {op} y; }} }}";
                    string expected = $"class C {{\n  private method(): void {{\n" +
                        $"    let x: boolean = true;\n    let y: boolean = false;\n    x = x {op} y;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }
        }
    }
}
