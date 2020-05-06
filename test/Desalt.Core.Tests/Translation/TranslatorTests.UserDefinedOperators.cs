// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorTests.UserDefinedOperators.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using NUnit.Framework;

    public partial class TranslatorTests
    {
        private static Task AssertUserDefinedOperatorTranslation(
            string methodSnippet,
            string expectedTypeScriptMethodSnippet,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.DocumentAndReferencedTypes)
        {
            return AssertTranslation(
                $@"
class Num : IDisposable
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

    public static explicit operator int(Num x) => 0;

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
        int startHere = 0;
{methodSnippet}
        int endHere = 0;
    }}

    public void Dispose() {{ }}
}}
",
                $@"
class Num implements ss.IDisposable {{
  // The rest of the generated code is not examined.
  public method(): void {{
    let startHere: number = 0;
{expectedTypeScriptMethodSnippet}
    let endHere: number = 0;
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
        //// Unary increment/decrement operators inside of array bracket statements
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_on_array_bracket_statements()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
var arr = new[] { x };
arr[(int)++x] = new Num();
",
                @"
let x: Num = new Num();
let arr: Num[] = [x];
arr[<number>(x = Num.op_Increment(x))] = new Num();
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_work_on_array_bracket_statements()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y;
var arr = new[] { x };
arr[(int)(y = x++)] = new Num();
",
                @"
let x: Num = new Num(), y: Num;
let arr: Num[] = [x];
const $t1 = x;
x = Num.op_Increment($t1);
arr[<number>(y = $t1)] = new Num();
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `if` statements
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_inside_if_statement_conditions()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
if (++x < y) { }
if (--x < y) { }
",
                @"
let x: Num = new Num(), y: Num = new Num();
if (Num.op_LessThan(x = Num.op_Increment(x), y)) { }
if (Num.op_LessThan(x = Num.op_Decrement(x), y)) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_be_pulled_out_of_if_statement_conditions()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
if (x++ < y) { }
if (x-- < y) { }
",
                @"
let x: Num = new Num(), y: Num = new Num();
const $t1 = x;
x = Num.op_Increment($t1);
if (Num.op_LessThan($t1, y)) { }
const $t2 = x;
x = Num.op_Decrement($t2);
if (Num.op_LessThan($t2, y)) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_cause_a_block_statement_to_be_created()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
if (true) y = x++;
",
                @"
let x: Num = new Num(), y: Num = new Num();
if (true) {
  const $t1 = x;
  x = Num.op_Increment($t1);
  y = $t1;
}
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside `throw` statements
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_in_a_throw_statement_should_work()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num();
throw new ArgumentOutOfRangeException(""param"", ++x, null);
",
                @"
let x: Num = new Num();
throw new ArgumentOutOfRangeException('param', x = Num.op_Increment(x), null);
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_in_a_throw_statement_should_add_temp_variables()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y;
throw new ArgumentOutOfRangeException(""param"", y = x++, null);
",
                @"
let x: Num = new Num(), y: Num;
const $t1 = x;
x = Num.op_Increment($t1);
throw new ArgumentOutOfRangeException('param', y = $t1, null);
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside `try/catch/finally` statements
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_in_a_try_catch_finally_statement_should_work()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num();
try { x++; }
catch { x--; }
",
                @"
let x: Num = new Num();
try {
  x = Num.op_Increment(x);
} catch {
  x = Num.op_Decrement(x);
}
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_in_a_try_catch_statement_should_work()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y;
try { y = x++; }
catch { y = x--; }
",
                @"
let x: Num = new Num(), y: Num;
try {
  const $t1 = x;
  x = Num.op_Increment($t1);
  y = $t1;
} catch {
  const $t2 = x;
  x = Num.op_Decrement($t2);
  y = $t2;
}
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside `using` statements
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_in_a_using_statement_with_DECLARATION_should_work()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
using (var y = x++) { }
",
                @"
let x: Num = new Num();
{
  const $t1 = x;
  x = Num.op_Increment($t1);
  const y: Num = $t1;
  try { } finally {
    if (y) {
      y.dispose();
    }
  }
}
");
        }

        [Test]
        public async Task Simple_increment_and_decrement_operators_in_a_using_statement_with_EXPRESSION_should_work()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
using (x++) { }
",
                @"
let x: Num = new Num();
{
  const $using1: Num = x = Num.op_Increment(x);
  try { } finally {
    if ($using1) {
      $using1.dispose();
    }
  }
}
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `foreach` loops
        //// ===========================================================================================================

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_inside_of_foreach_initializers()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
foreach (var num in new Num[] { ++x }) { }
",
                @"
let x: Num = new Num();
for (const num of [x = Num.op_Increment(x)]) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_work_inside_of_foreach_initializers()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
var x = new Num();
foreach (var num in new Num[] { x-- }) { }
",
                @"
let x: Num = new Num();
const $t1 = x;
x = Num.op_Decrement($t1);
for (const num of [$t1]) { }
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `for` loops
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
        public async Task For_loop_initializer_lists_should_pull_out_necessary_additional_statements_for_DECLARATIONS()
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
        public async Task For_loop_initializer_lists_should_pull_out_necessary_additional_statements_for_INITIALIZERS()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y, z, a, b;
for (y = Do(x++), z = x, a = y++, b = ++x; true; ) { }
",
                @"
let x: Num = new Num(), y: Num, z: Num, a: Num, b: Num;
const $t1 = x;
x = Num.op_Increment($t1);
y = this.do($t1);
z = x;
const $t2 = y;
y = Num.op_Increment($t2);
for (a = $t2, b = x = Num.op_Increment(x); true; ) { }
");
        }

        [Test]
        public async Task Simple_increment_and_decrement_operators_should_work_inside_for_loop_conditionals()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
for (var x = new Num(); x != ++x; ) { }
",
                @"
for (let x = new Num(); Num.op_Inequality(x, x = Num.op_Increment(x)); ) { }
");
        }

        [Test]
        public async Task Complex_increment_and_decrement_operators_should_work_inside_for_loop_conditionals()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
for (var x = new Num(); x != x--; ) { }
",
                @"
const $t1 = x;
x = Num.op_Decrement($t1);
for (let x = new Num(); Num.op_Inequality(x, $t1); ) { }
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

        [Test]
        public async Task
            For_loops_with_complex_expressions_should_put_all_of_the_additional_statements_in_the_right_place()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y, z;
for (y = x++, z = y; x < y--; z = x++, y++, z--)
{
    Do(x);
}
",
                @"
let x: Num = new Num(), y: Num, z: Num;
const $t1 = x;
x = Num.op_Increment($t1);
const $t2 = y;
y = Num.op_Decrement($t2);
for (y = $t1, z = y; Num.op_LessThan(x, $t2); ) {
  this.do(x);
  const $t3 = x;
  x = Num.op_Increment($t3);
  z = $t3;
  y = Num.op_Increment(y);
  z = Num.op_Decrement(z);
}
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `while` loops
        //// ===========================================================================================================

        [Test]
        public async Task While_loops_with_simple_increment_and_decrement_conditions_should_be_translated_in_place()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
while (++x > y) { }
",
                @"
let x: Num = new Num(), y: Num = new Num();
while (Num.op_GreaterThan(x = Num.op_Increment(x), y)) { }
");
        }

        [Test]
        public async Task While_loops_with_complex_increment_and_decrement_conditions_should_be_extracted_to_the_body()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
while (x++ > y) Do(y);
",
                @"
let x: Num = new Num(), y: Num = new Num();
while (true) {
  const $t1 = x;
  x = Num.op_Increment($t1);
  if (Num.op_GreaterThan($t1, y)) {
    break;
  }
  this.do(y);
}
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `do` loops
        //// ===========================================================================================================

        [Test]
        public async Task Do_loops_with_simple_increment_and_decrement_conditions_should_be_translated_in_place()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
do {} while (++x > y);
",
                @"
let x: Num = new Num(), y: Num = new Num();
do { } while (Num.op_GreaterThan(x = Num.op_Increment(x), y));
");
        }

        [Test]
        public async Task Do_loops_with_complex_increment_and_decrement_conditions_should_be_extracted_to_the_body()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
do { Do(y); } while (x++ > y);
",
                @"
let x: Num = new Num(), y: Num = new Num();
do {
  this.do(y);
  const $t1 = x;
  x = Num.op_Increment($t1);
  if (Num.op_GreaterThan($t1, y)) {
    break;
  }
} while (true);
");
        }

        //// ===========================================================================================================
        //// Unary increment/decrement operators inside of `switch` statements
        //// ===========================================================================================================

        [Test]
        public async Task
            Switch_statements_with_simple_increment_and_decrement_conditions_should_be_translated_in_place()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
switch (++x > y)
{
    case true: return;
}
",
                @"
let x: Num = new Num(), y: Num = new Num();
switch (Num.op_GreaterThan(x = Num.op_Increment(x), y)) {
  case true:
    return;
}
");
        }

        [Test]
        public async Task
            Switch_statements_with_complex_increment_and_decrement_conditions_should_be_extracted_to_the_body()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
switch (x++ > y)
{
    case true: return;
}
",
                @"
let x: Num = new Num(), y: Num = new Num();
const $t1 = x;
x = Num.op_Increment($t1);
switch (Num.op_GreaterThan($t1, y)) {
  case true:
    return;
}
");
        }

        [Test]
        public async Task Case_statements_with_simple_increment_and_decrement_conditions_should_be_translated_in_place()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
int i = 10;
switch (i)
{
    case 10:
        ++x;
        break;

    case 20:
        x--;
        break;
}
",
                @"
let x: Num = new Num(), y: Num = new Num();
let i: number = 10;
switch (i) {
  case 10:
    x = Num.op_Increment(x);
    break;

  case 20:
    x = Num.op_Decrement(x);
    break;
}
");
        }

        [Test]
        public async Task
            Case_statements_with_complex_increment_and_decrement_conditions_should_be_translated_correctly()
        {
            await AssertUserDefinedOperatorTranslation(
                @"
Num x = new Num(), y = new Num();
int i = 10;
switch (i)
{
    case 10:
        y = x++;
        break;

    case 20:
        y = x--;
        break;
}
",
                @"
let x: Num = new Num(), y: Num = new Num();
let i: number = 10;
switch (i) {
  case 10:
    const $t1 = x;
    x = Num.op_Increment($t1);
    y = $t1;
    break;

  case 20:
    const $t2 = x;
    x = Num.op_Decrement($t2);
    y = $t2;
    break;
}
");
        }

        //// ===========================================================================================================
        //// [IntrinsicOperator] Tests
        //// ===========================================================================================================

        private static Task AssertIntrinsicOperatorTranslation(
            string methodSnippet,
            string expectedTypeScriptMethodSnippet,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.DocumentAndReferencedTypes)
        {
            return AssertTranslation(
                $@"
class Num
{{
    [IntrinsicOperator]
    public static Num operator ++(Num x) => new Num();

    [IntrinsicOperator]
    public static bool operator <(Num x, Num y) => false;

    [IntrinsicOperator]
    public static bool operator >(Num x, Num y) => false;

    [IntrinsicOperator]
    public static explicit operator int(Num x) => 0;

    public void Method()
    {{
        Num x = new Num(), y = new Num();
        int startHere = 0;
{methodSnippet}
        int endHere = 0;
    }}
}}
",
                $@"
class Num {{
  // The rest of the generated code is not examined.
  public method(): void {{
    let x: Num = new Num(), y: Num;
    let startHere: number = 0;
{expectedTypeScriptMethodSnippet}
    let endHere: number = 0;
  }}
}}
",
                discoveryKind,
                extractApplicableTypeScriptSnippet: true);
        }

        [Test]
        public async Task IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_STATEMENTS()
        {
            await AssertIntrinsicOperatorTranslation(@"x++;", @"x++;");
        }

        [Test]
        public async Task IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_LOCAL_DECLARATIONS()
        {
            await AssertIntrinsicOperatorTranslation(@"Num z = x++;", @"let z: Num = x++;");
        }

        [Test]
        public async Task IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_ARRAY_BRACKET_STATEMENTS()
        {
            await AssertIntrinsicOperatorTranslation(
                @"
Num[] arr = new[] { x };
arr[(int)x++] = x;
", @"
let arr: Num[] = [x];
arr[<number>x++] = x;
");
        }

        [Test]
        public async Task IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_IF_statements()
        {
            await AssertIntrinsicOperatorTranslation(@"if (x++ > y) { }", @"if (x++ > y) { }");
        }

        [Test]
        public async Task IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_THROW_statements()
        {
            await AssertIntrinsicOperatorTranslation(
                @"throw new ArgumentOutOfRangeException(""param"", y = x++, null);",
                @"throw new ArgumentOutOfRangeException('param', y = x++, null);");
        }

        [Test]
        public async Task
            IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_TRY_CATCH_FINALLY_statements()
        {
            await AssertIntrinsicOperatorTranslation(
                @"
try { y = x++; }
catch { y = x++; }
",
                @"
try {
  y = x++;
} catch {
  y = x++;
}
");
        }

        [Test]
        public async Task
            IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_LOOPS()
        {
            await AssertIntrinsicOperatorTranslation(
                @"
for (y = x++; x > y; y = x++) { }
foreach (var num in new Num[] { x++ }) { }
while (x++ > y) { }
do {} while (x++ > y);
",
                @"
for (y = x++; x > y; y = x++) { }
for (const num of [x++]) { }
while (x++ > y) { }
do { } while (x++ > y);
");
        }

        [Test]
        public async Task
            IntrinsicOperator_should_bypass_the_user_defined_operator_code_for_SWITCH_statements()
        {
            await AssertIntrinsicOperatorTranslation(
                @"
switch (x++ > y)
{
    case true:
        y = x++;
        break;
}
",
                @"
switch (x++ > y) {
  case true:
    y = x++;
    break;
}
");
        }
    }
}
