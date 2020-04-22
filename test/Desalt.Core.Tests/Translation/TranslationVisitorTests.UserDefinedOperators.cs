// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.UserDefinedOperators.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using NUnit.Framework;

    public partial class TranslationVisitorTests
    {
        private static Task AssertUserDefinedOperatorTranslation(
            string methodSnippet,
            string expectedTypeScriptMethodSnippet,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes)
        {
            return AssertTranslation(
                $@"
class Num
{{
    // Unary operators
    public static Num operator +(Num x) => new Num();
    public static Num operator -(Num x) => new Num();
    public static Num operator !(Num x) => new Num();
    public static Num operator ~(Num x) => new Num();
    public static Num operator ++(Num x) => new Num();
    public static Num operator --(Num x) => new Num();

    // Binary operators
    public static Num operator +(Num x, Num y) => new Num();
    public static Num operator -(Num x, Num y) => new Num();
    public static Num operator *(Num x, Num y) => new Num();
    public static Num operator /(Num x, Num y) => new Num();
    public static Num operator %(Num x, Num y) => new Num();
    public static Num operator <<(Num x, int y) => new Num();
    public static Num operator >>(Num x, int y) => new Num();
    public static bool operator <(Num x, Num y) => false;
    public static bool operator <=(Num x, Num y) => false;
    public static bool operator >(Num x, Num y) => false;
    public static bool operator >=(Num x, Num y) => false;
    public static bool operator ==(Num x, Num y) => false;
    public static bool operator !=(Num x, Num y) => false;
    public static Num operator &(Num x, Num y) => new Num();
    public static Num operator ^(Num x, Num y) => new Num();
    public static Num operator |(Num x, Num y) => new Num();

    public Num MyNum
    {{
        get {{ return new Num(); }}
        set {{ }}
    }}

    public Num Do(Num num)
    {{
        return num;
    }}

    public void Method()
    {{
{methodSnippet}
    }}
}}
",
                $@"
class Num {{
  public static op_UnaryPlus(x: Num): Num {{
    return new Num();
  }}

  public static op_UnaryNegation(x: Num): Num {{
    return new Num();
  }}

  public static op_LogicalNot(x: Num): Num {{
    return new Num();
  }}

  public static op_OnesComplement(x: Num): Num {{
    return new Num();
  }}

  public static op_Increment(x: Num): Num {{
    return new Num();
  }}

  public static op_Decrement(x: Num): Num {{
    return new Num();
  }}

  public static op_Addition(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_Subtraction(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_Multiply(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_Division(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_Modulus(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_LeftShift(x: Num, y: number): Num {{
    return new Num();
  }}

  public static op_RightShift(x: Num, y: number): Num {{
    return new Num();
  }}

  public static op_LessThan(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_LessThanOrEqual(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_GreaterThan(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_GreaterThanOrEqual(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_Equality(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_Inequality(x: Num, y: Num): boolean {{
    return false;
  }}

  public static op_BitwiseAnd(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_BitwiseXor(x: Num, y: Num): Num {{
    return new Num();
  }}

  public static op_BitwiseOr(x: Num, y: Num): Num {{
    return new Num();
  }}

  public get myNum(): Num {{
    return new Num();
  }}

  public set myNum(value: Num) {{ }}

  public do(num: Num): Num {{
    return num;
  }}

  public method(): void {{
{expectedTypeScriptMethodSnippet}
  }}
}}
",
                discoveryKind,
                extractApplicableTypeScriptSnippet: true);
        }

        [Test]
        public async Task Invoking_overloaded_prefix_unary_operators_should_translate_correctly()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
x = +x;
x = -x;
x = !x;
x = ~x;
",
                @"
let x: Num = new Num();
x = Num.op_UnaryPlus(x);
x = Num.op_UnaryNegation(x);
x = Num.op_LogicalNot(x);
x = Num.op_OnesComplement(x);
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task Invoking_overloaded_binary_operators_should_translate_correctly()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num(), z;
z = x + y;
z = x - y;
z = x * y;
z = x / y;
z = x % y;
z = x << 2;
z = x >> 4;
bool b = x < y;
b = x <= y;
b = x > y;
b = x >= y;
b = x == y;
b = x != y;
z = x & y;
z = x ^ y;
z = x | y;
",
                @"
let x: Num = new Num(), y: Num = new Num(), z: Num;
z = Num.op_Addition(x, y);
z = Num.op_Subtraction(x, y);
z = Num.op_Multiply(x, y);
z = Num.op_Division(x, y);
z = Num.op_Modulus(x, y);
z = Num.op_LeftShift(x, 2);
z = Num.op_RightShift(x, 4);
let b: boolean = Num.op_LessThan(x, y);
b = Num.op_LessThanOrEqual(x, y);
b = Num.op_GreaterThan(x, y);
b = Num.op_GreaterThanOrEqual(x, y);
b = Num.op_Equality(x, y);
b = Num.op_Inequality(x, y);
z = Num.op_BitwiseAnd(x, y);
z = Num.op_ExclusiveOr(x, y);
z = Num.op_BitwiseOr(x, y);
");
        }

        //// ===========================================================================================================
        //// Unary Increment/Decrement Operator Tests
        //// ===========================================================================================================

        [Test]
        public async Task
            Invoking_overloaded_increment_and_decrement_unary_operators_should_translate_correctly_for_VARIABLES_with_an_IMPLICIT_assignment()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
++x;
--x;
x++;
x--;
",
                @"
let x: Num = new Num();
x = Num.op_Increment(x);
x = Num.op_Decrement(x);
x = Num.op_Increment(x);
x = Num.op_Decrement(x);
");
        }

        [Test]
        public async Task
            Invoking_overloaded_increment_and_decrement_unary_operators_should_translate_correctly_for_PROPERTIES_with_an_IMPLICIT_assignment()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
MyNum++;
MyNum--;
++MyNum;
--MyNum;
",
                @"
this.myNum = Num.op_Increment(this.myNum);
this.myNum = Num.op_Decrement(this.myNum);
this.myNum = Num.op_Increment(this.myNum);
this.myNum = Num.op_Decrement(this.myNum);
");
        }

        [Test]
        public async Task
            Invoking_overloaded_increment_and_decrement_unary_operators_should_translate_correctly_for_VARIABLES_with_an_EXPLICIT_assignment()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
var xPreInc = ++x;
var xPostInc = x++;
var xPreDec = --x;
var xPostDec = x--;
",
                @"
let x: Num = new Num();
let xPreInc: Num = x = Num.op_Increment(x);
const $t1 = x;
x = Num.op_Increment($t1);
let xPostInc: Num = $t1;
let xPreDec: Num = x = Num.op_Decrement(x);
const $t2 = x;
x = Num.op_Decrement($t2);
let xPostDec: Num = $t2;
");
        }

        [Test]
        public async Task
            Invoking_overloaded_increment_and_decrement_unary_operators_should_translate_correctly_for_PROPERTIES_with_an_EXPLICIT_assignment()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
var xPreInc = ++MyNum;
var xPreDec = --MyNum;
var xPostInc = MyNum++;
var xPostDec = MyNum--;
",
                @"
let x: Num = new Num();
let xPreInc: Num = this.myNum = Num.op_Increment(this.myNum);
let xPreDec: Num = this.myNum = Num.op_Decrement(this.myNum);
const $t1 = this.myNum;
this.myNum = Num.op_Increment($t1);
let xPostInc: Num = $t1;
const $t2 = this.myNum;
this.myNum = Num.op_Decrement($t2);
let xPostDec: Num = $t2;
");
        }

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_for_local_variable_declarations()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
var y = ++x;
var z = --x;",
                @"
let x: Num = new Num();
let y: Num = x = Num.op_Increment(x);
let z: Num = x = Num.op_Decrement(x);");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_work_for_local_variable_declarations()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
Num y = Do(x++), z = x, a = y++, b = ++x;",
                @"
let x: Num = new Num();
const $t1 = x;
x = Num.op_Increment($t1);
let y: Num = this.do($t1), z: Num = x;
const $t2 = y;
y = Num.op_Increment($t2);
let a: Num = $t2, b: Num = x = Num.op_Increment(x);");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of for loops
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_inside_for_loop_initializers()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
for (++x; true; ) { }
for (x++; true; ) { }
for (--x; true; ) { }
for (x--; true; ) { }
",
                @"
let x: Num = new Num();
for (x = Num.op_Increment(x); true; ) { }
for (x = Num.op_Increment(x); true; ) { }
for (x = Num.op_Decrement(x); true; ) { }
for (x = Num.op_Decrement(x); true; ) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_be_pulled_outside_the_for_loop_initializers()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
for (var y = x++; true; ) { }
for (var y = x--; true; ) { }
",
                @"
let x: Num = new Num();
const $t1 = x;
x = Num.op_Increment($t1);
for (let y = $t1; true; ) { }
const $t2 = x;
x = Num.op_Decrement($t2);
for (let y = $t2; true; ) { }
");
        }

        [Test]
        public async Task For_loop_initializer_lists_should_pull_out_necessary_additional_statements()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
for (Num y = Do(x++), z = x, a = y++, b = ++x; true; ) { }
",
                @"
let x: Num = new Num();
const $t1 = x;
x = Num.op_Increment($t1);
let y = this.do($t1), z = x;
const $t2 = y;
y = Num.op_Increment($t2);
for (let a = $t2, b = x = Num.op_Increment(x); true; ) { }
");
        }

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_inside_for_loop_incrementors()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
for (var x = new Num(); true; ++x) { }
for (var x = new Num(); true; --x) { }
for (var x = new Num(); true; x++) { }
for (var x = new Num(); true; x--) { }
",
                @"
for (let x = new Num(); true; x = Num.op_Increment(x)) { }
for (let x = new Num(); true; x = Num.op_Decrement(x)) { }
for (let x = new Num(); true; x = Num.op_Increment(x)) { }
for (let x = new Num(); true; x = Num.op_Decrement(x)) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_work_inside_for_loop_incrementors()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
for (Num x = new Num(), y = new Num(); true; y = x++)
{
    Do(x);
}
",
                @"
for (let x = new Num(), y = new Num(); true; ) {
  this.do(x);
  const $t1 = x;
  x = Num.op_Increment($t1);
  y = $t1;
}");
        }

        [Test]
        public async Task Once_a_for_loop_incrementor_needs_temporary_variables_all_incrementors_should_be_pulled_out()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
int i = 0;
for (Num x = new Num(), y = new Num(); true; i++, y = x++, --i)
{
    Do(x);
}
",
                @"
let i: number = 0;
for (let x = new Num(), y = new Num(); true; ) {
  this.do(x);
  i++;
  const $t1 = x;
  x = Num.op_Increment($t1);
  y = $t1;
  --i;
}");
        }
    }
}
