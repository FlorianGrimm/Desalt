// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.Functions.cs" company="Justin Rockwood">
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
        //// ===========================================================================================================
        //// Invocation Expression Tests
        //// ===========================================================================================================

        [Test]
        public async Task InvocationExpression_should_use_InlineCode_for_ListT_creation()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    List<int> list = new List<int>();
    list.AddRange(new [] { 1, 2, 3 });
",
                @"
    let list: number[] = [];
    ss.arrayAddRange(list, [1, 2, 3]);
",
                SymbolDiscoveryKind.DocumentAndAllAssemblyTypes);
        }

        /// <remarks>
        /// This was giving the following error before fixing it:
        /// <![CDATA[
        /// error DSC1016: Error parsing inline code '{instance}[{fieldName}]' for
        ///    'System.TypeUtil.GetField<System.Func<System.Action, int>>(object instance, string fieldName)':
        ///     Cannot find parameter 'fieldName' in the translated argument list '(callback)'
        /// ]]>
        /// </remarks>
        [Test]
        public async Task InvocationExpression_should_handle_nested_invocations_with_InlineCode()
        {
            await AssertTranslationWithClassCAndMethod(@"
    const string requestFuncName = ""requestAnimationFrame"";
    Func<Action, int> requestAnimationFrameFunc = delegate(Action callback)
    {
        return TypeUtil.GetField<Func<Action, int>>(typeof(Window), requestFuncName)(callback);
    };
", @"
    const requestFuncName: string = 'requestAnimationFrame';
    let requestAnimationFrameFunc: (action: () => void) => number = (callback: () => void) => {
      return window[requestFuncName](callback);
    };
", SymbolDiscoveryKind.DocumentAndAllAssemblyTypes);
        }

        [Test]
        public async Task InvocationExpression_should_use_the_correct_overloaded_name_for_locally_defined_methods()
        {
            await AssertTranslation(
                @"
class A
{
    void Method() {}
    void Method(string x) {}

    void Test()
    {
        var a = new A();
        a.Method();
        a.Method(""str"");
    }
}",
                @"
class A {
  private method(): void { }

  private method$1(x: string): void { }

  private test(): void {
    let a: A = new A();
    a.method();
    a.method$1('str');
  }
}
");
        }

        //// ===========================================================================================================
        //// Extension Methods Tests
        //// ===========================================================================================================

        [Test]
        public async Task Translate_should_handle_extension_method_invocations()
        {
            await AssertTranslation(
                @"
public static class Extensions
{
    public static string Quack(this string s)
    {
        return ""Quack!"";
    }
}

class A
{
    public void Method()
    {
        string quack = ""Duck"".Quack();
    }
}",
                @"
export class Extensions {
  public static quack(s: string): string {
    return 'Quack!';
  }
}

class A {
  public method(): void {
    let quack: string = Extensions.quack('Duck');
  }
}
");
        }

        [Test]
        public async Task Translate_should_handle_extension_method_invocations_when_invoked_as_a_normal_method()
        {
            await AssertTranslation(
                @"
public static class Extensions
{
    public static string Quack(this string s)
    {
        return ""Quack!"";
    }
}

class A
{
    public void Method()
    {
        string quack = Extensions.Quack(""Duck"");
    }
}",
                @"
export class Extensions {
  public static quack(s: string): string {
    return 'Quack!';
  }
}

class A {
  public method(): void {
    let quack: string = Extensions.quack('Duck');
  }
}
");
        }

        [Test]
        public async Task Translate_should_handle_extension_method_invocations_with_ScriptName()
        {
            await AssertTranslation(
                @"
public static class Extensions
{
    [ScriptName(""quack_loudly"")]
    public static string Quack(this string s)
    {
        return ""Quack!"";
    }
}

class A
{
    public void Method()
    {
        string quack = ""Duck"".Quack();
    }
}",
                @"
export class Extensions {
  public static quack_loudly(s: string): string {
    return 'Quack!';
  }
}

class A {
  public method(): void {
    let quack: string = Extensions.quack_loudly('Duck');
  }
}
");
        }

        [Test]
        public async Task Translate_should_handle_extension_method_invocations_with_a_complex_left_side()
        {
            await AssertTranslation(
                @"
public static class Extensions
{
    public static string Quack(this string s)
    {
        return ""Quack!"";
    }

    public static int Add(this int x, int y)
    {
        return x + y;
    }
}

class A
{
    public void Method()
    {
        string quack = ""Duck"".Substr(1).Quack();
        int result = (1 * 4).Add(10);
    }
}",
                @"
export class Extensions {
  public static quack(s: string): string {
    return 'Quack!';
  }

  public static add(x: number, y: number): number {
    return x + y;
  }
}

class A {
  public method(): void {
    let quack: string = Extensions.quack('Duck'.substr(1));
    let result: number = Extensions.add((1 * 4), 10);
  }
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task Translate_should_handle_extension_method_invocations_with_inline_code()
        {
            await AssertTranslation(
                @"
public static class ArrayExtensions
{
    [InlineCode(""ss.arrayClone({array})"")]
    public static T[] Clone<T>(this T[] array)
    {
        return (T[])null;
    }
}

class A
{
    public void Method()
    {
        int[] cloned = new[] {1,2,3}.Clone();
    }
}",
                @"
export class ArrayExtensions {
}

class A {
  public method(): void {
    let cloned: number[] = ss.arrayClone([1, 2, 3]);
  }
}
");
        }
    }
}
