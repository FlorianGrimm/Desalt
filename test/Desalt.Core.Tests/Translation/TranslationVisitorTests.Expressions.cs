// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.Translation;
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
                    "class C { public string Method() { var x = (string)Method(); return \"\"; } }",
                    "class C {\n  public method(): string {\n    " +
                    "let x: string = <string>this.method();\n    return '';\n  }\n}\n");
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
                    "class X { public static void A() { A(); } }",
                    "class X {\n  public static a(): void {\n    X.a();\n  }\n}\n");
            }

            [TestMethod]
            public async Task IdentifierName_as_a_parameter()
            {
                await AssertTranslation(
                    "class C { string name; C(string name) { this.name = name; } }",
                    "class C {\n  private name: string;\n\n  private constructor(name: string) {\n    this.name = name;\n  }\n}\n");
            }

            [TestMethod]
            public async Task Complex_example_involving_referencing_an_enum_value_inside_of_a_element_access_expression()
            {
                await AssertTranslation(
                    @"
using System.Collections.Generic;

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
  public static loggerLevelNames: string[] = [];

  private static init(): void {
    Logger.loggerLevelNames[<number>LoggerLevel.all] = 'all';
  }
}
",
                    SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
            }

            [TestMethod]
            public async Task Assignment_expressions()
            {
                foreach (string op in new[] { "=", "*=", "/=", "%=", "+=", "-=", "<<=", ">>=", "&=", "^=", "|=" })
                {
                    string code = $"class C {{ void Method() {{ int x = 1; x = x {op} 123; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 1;\n    x = x {op} 123;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }
            }

            [TestMethod]
            public async Task Prefix_unary_expressions()
            {
                string code, expected;

                foreach (string op in new[] { "++", "--", "+", "-", "~" })
                {
                    code = $"class C {{ void Method() {{ int x = 123; x = {op}x; }} }}";
                    expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 123;\n    x = {op}x;\n  }}\n}}\n";

                    await AssertTranslation(code, expected);
                }

                code = $"class C {{ void Method() {{ bool x = !true; }} }}";
                expected =
                    $"class C {{\n  private method(): void {{\n    let x: boolean = !true;\n  }}\n}}\n";

                await AssertTranslation(code, expected);
            }

            [TestMethod]
            public async Task Postfix_unary_expressions()
            {
                foreach (string op in new[] { "++", "--" })
                {
                    string code = $"class C {{ void Method() {{ int x = 123; x = x{op}; }} }}";
                    string expected =
                        $"class C {{\n  private method(): void {{\n    let x: number = 123;\n    x = x{op};\n  }}\n}}\n";

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

            [TestMethod]
            public async Task ObjectCreationExpression_should_use_InlineCode_for_ListT_creation()
            {
                await AssertTranslation(
                    @"
class C
{
    List<int> list1 = new List<int>();
    List<int> list2 = new List<int>(capacity: 10);
    List<int> list3 = new List<int>(1, 2, 3);
    List<int> list4 = new List<int>(new [] { 1, 2, 3 });
}",
                    @"
class C {
  private list1: number[] = [];

  private list2: number[] = [];

  private list3: number[] = [1, 2, 3];

  private list4: number[] = ss.arrayClone([1, 2, 3]);
}
",
                    SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes);
            }

            [TestMethod]
            public async Task InvocationExpression_should_use_InlineCode_for_ListT_creation()
            {
                await AssertTranslation(
                    @"
class C
{
    void Method()
    {
        List<int> list = new List<int>();
        list.AddRange(new [] { 1, 2, 3 });
    }
}",
                    @"
class C {
  private method(): void {
    let list: number[] = [];
    ss.arrayAddRange(list, [1, 2, 3]);
  }
}
",
                    SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes);
            }

            [TestMethod]
            public async Task List_of_Func_type_should_translate_to_Array_of_func()
            {
                await AssertTranslation(
                    @"
class C
{
    private static List<Func<string, bool>> Filters
    {
        get
        {
            return (List<Func<string, bool>>)new List<Func<string, bool>>();
        }
    }
}",
                    @"
class C {
  private static get filters(): Array<(string: string) => boolean> {
    return <Array<(string: string) => boolean>>[];
  }
}
",
                    SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
            }
        }
    }
}
