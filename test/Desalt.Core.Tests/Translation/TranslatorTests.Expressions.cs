// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorTests.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using NUnit.Framework;
    using NUnit.Framework.Internal;

    public partial class TranslatorTests
    {
        [Test]
        public async Task This_expression_assigning_numeric_literals()
        {
            await AssertTranslation(
                "class C { private int Int32; public C() { this.Int32 = 1; } }",
                "class C {\n  private int32: number;\n\n  public constructor() {\n    this.int32 = 1;\n  }\n}\n");
        }

        [Test]
        public async Task This_expression_assigning_string_literals()
        {
            await AssertTranslation(
                "class C { private string Str; public C() { this.Str = \"s\"; } }",
                "class C {\n  private str: string;\n\n  public constructor() {\n    this.str = 's';\n  }\n}\n");
        }

        [Test]
        public async Task Cast_of_a_field_should_use_the_script_name()
        {
            await AssertTranslation(
                "class C { private string Str; public void Method() { var x = (string)Str; } }",
                "class C {\n  private str: string;\n\n  public method(): void {\n    " +
                "let x: string = <string>this.str;\n  }\n}\n");
        }

        [Test]
        public async Task Cast_of_a_method_invocation_should_use_the_script_name()
        {
            await AssertTranslation(
                "class C { public string Method() { var x = (string)Method(); return \"\"; } }",
                "class C {\n  public method(): string {\n    " +
                "let x: string = <string>this.method();\n    return '';\n  }\n}\n");
        }

        [Test]
        public async Task Invocation_expressions_on_instance_methods_should_prefix_this()
        {
            await AssertTranslation(
                "class C { public void Method() { Method(); } }",
                "class C {\n  public method(): void {\n    this.method();\n  }\n}\n");
        }

        [Test]
        public async Task Invocation_expression_on_static_method_should_prefix_class_name()
        {
            await AssertTranslation(
                "class X { public static void A() { A(); } }",
                "class X {\n  public static a(): void {\n    X.a();\n  }\n}\n");
        }

        [Test]
        public async Task IdentifierName_as_a_parameter()
        {
            await AssertTranslation(
                "class C { string name; C(string name) { this.name = name; } }",
                "class C {\n  private name: string;\n\n  private constructor(name: string) {\n    this.name = name;\n  }\n}\n");
        }

        [Test]
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
  All = 0,
}

class Logger {
  public static loggerLevelNames: string[] = [];

  private static init(): void {
    Logger.loggerLevelNames[<number>LoggerLevel.All] = 'all';
  }
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        //// ===========================================================================================================
        //// Unary and Binary Expressions
        //// ===========================================================================================================

        [Test]
        public async Task Prefix_unary_expressions()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
int x = 123;
x = +x;
x = -x;
x = ~x;
x = ++x;
x = --x;
bool y = !true;",
                @"
let x: number = 123;
x = +x;
x = -x;
x = ~x;
x = ++x;
x = --x;
let y: boolean = !true;
");
        }

        [Test]
        public async Task Postfix_unary_expressions()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
int x = 123;
x = x++;
x = x--;
",
                @"
let x: number = 123;
x = x++;
x = x--;
");
        }

        [Test]
        public async Task Binary_expressions_on_numbers()
        {
            await AssertTranslationWithClassCAndMethod(@"
int x = 1;
int y = 2;
x = x * y;
x = x / y;
x = x % y;
x = x + y;
x = x - y;
x = x << y;
x = x >> y;
x = x & y;
x = x ^ y;
x = x | y;
",
                @"
let x: number = 1;
let y: number = 2;
x = x * y;
x = x / y;
x = x % y;
x = x + y;
x = x - y;
x = x << y;
x = x >> y;
x = x & y;
x = x ^ y;
x = x | y;
");
        }

        [Test]
        public async Task Binary_expressions_on_comparisons()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
int x = 1;
int y = 2;
bool z = x < y;
z = x > y;
z = x <= y;
z = x >= y;
z = x == y;
z = x != y;
",
                @"
let x: number = 1;
let y: number = 2;
let z: boolean = x < y;
z = x > y;
z = x <= y;
z = x >= y;
z = x === y;
z = x !== y;
");
        }

        [Test]
        public async Task Binary_expressions_on_booleans()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
bool x = true;
bool y = false;
x = x && y;
x = x || y;
",
                @"
let x: boolean = true;
let y: boolean = false;
x = x && y;
x = x || y;
");
        }

        [Test]
        public async Task Conditional_expressions()
        {
            await AssertTranslation(@"
class C
{
    int Method()
    {
        bool x = true;
        int y = 1, z = 2;

        return x ? y : z;
    }
}", @"
class C {
  private method(): number {
    let x: boolean = true;
    let y: number = 1, z: number = 2;
    return x ? y : z;
  }
}
");
        }

        [Test]
        public async Task Assignment_expressions()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
int x = 1;
x = x;
x *= x;
x /= x;
x %= x;
x += x;
x -= x;
x <<= x;
x >>= x;
x &= x;
x ^= x;
x |= x;
",
                @"
let x: number = 1;
x = x;
x *= x;
x /= x;
x %= x;
x += x;
x -= x;
x <<= x;
x >>= x;
x &= x;
x ^= x;
x |= x;
");
        }

        //// ===========================================================================================================
        //// Array Creation Expression Tests
        //// ===========================================================================================================

        [Test]
        public async Task ArrayCreationExpression_with_an_initializer_should_work()
        {
            await AssertTranslationWithClassCAndMethod(
                "var arr = new int[] {1, 2, 3};",
                "let arr: number[] = [1, 2, 3];");
        }

        [Test]
        public async Task ArrayCreationExpression_with_no_initializer_should_work()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
var arr = new int[0];
arr = new int[20];
arr = new int[1 + 10];
",
                @"
let arr: number[] = [];
arr = new Array(20);
arr = new Array(1 + 10);
");
        }

        //// ===========================================================================================================
        //// Object Creation Expression Tests
        //// ===========================================================================================================

        [Test]
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
                SymbolDiscoveryKind.DocumentAndAllAssemblyTypes);
        }

        [Test]
        public async Task ObjectCreationExpression_should_detect_JsDictionary_special_case_initialization()
        {
            await AssertTranslation(
                @"
class C
{
    private JsDictionary notGeneric = new JsDictionary(""key1"", ""value1"", ""key2"", ""value2"");
    private JsDictionary<int, bool> generic = new JsDictionary<int, bool>(1, true, 2, false);
}",
                @"
class C {
  private notGeneric: { [key: string]: any } = {
    'key1': 'value1',
    'key2': 'value2'
  };

  private generic: { [key: number]: boolean } = {
    1: true,
    2: false
  };
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task ObjectCreationExpression_should_detect_expressions_in_JsDictionary_initialization()
        {
            await AssertTranslation(
                @"
class Strings
{
    public const string Key2 = ""key2"";
}

class C
{
    private JsDictionary notGeneric = new JsDictionary(""key1"", ""value1"", Strings.Key2, ""value2"");
    private JsDictionary<int, bool> generic = new JsDictionary<int, bool>(1, true, 2, false);
}",
                @"
class Strings {
  public static readonly key2: string = 'key2';
}

class C {
  private notGeneric: { [key: string]: any } = {
    'key1': 'value1',
    [Strings.key2]: 'value2'
  };

  private generic: { [key: number]: boolean } = {
    1: true,
    2: false
  };
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        //// ===========================================================================================================
        //// IntrinsicProperty Tests
        //// ===========================================================================================================

        [Test]
        public async Task Translate_should_treat_IntrinsicProperties_assignments_as_normal_property_assignments()
        {
            await AssertTranslation(
                @"
class C
{
    [IntrinsicProperty]
    public int Prop { get; set; }

    public void Method()
    {
        Prop = 200;
    }
}
",
                @"
class C {
  public method(): void {
    this.prop = 200;
  }
}
");
        }

        //// ===========================================================================================================
        //// Other Expression Types Tests
        //// ===========================================================================================================

        [Test]
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
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }
    }
}
