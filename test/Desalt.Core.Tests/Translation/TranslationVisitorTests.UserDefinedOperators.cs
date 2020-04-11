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

  public method(): void {{
{expectedTypeScriptMethodSnippet}
  }}
}}
",
                discoveryKind,
                extractApplicableTypeScriptSnippetFunc: code => ExtractApplicableLines(code, "method"));
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
    }
}
