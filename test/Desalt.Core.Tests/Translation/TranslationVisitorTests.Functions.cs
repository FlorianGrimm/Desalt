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

        //// ===========================================================================================================
        //// [ScriptSkip] Tests
        //// ===========================================================================================================

        // Taken from the [ScriptSkip] documentation: This attribute causes a method to not be invoked. The method must
        // either be a static method with one argument (in case Foo.M(x) will become x), or an instance method with no
        // arguments (in which x.M() will become x). Can also be applied to a constructor, in which case the constructor
        // will not be called if used as an initializer (": base()" or ": this()").

        [Test]
        public async Task Translate_should_skip_static_methods_marked_with_ScriptSkip()
        {
            await AssertTranslation(
                @"
class A
{
    [ScriptSkip]
    public static int StaticSkip(int x) { return x; }

    public void Method()
    {
        var x = A.StaticSkip(200) + 2;
    }
}",
                @"
class A {
  public method(): void {
    let x: number = 200 + 2;
  }
}
");
        }

        [Test]
        public async Task Translate_should_skip_static_extension_methods_marked_with_ScriptSkip()
        {
            await AssertTranslation(
                @"
static class Extensions
{
    [ScriptSkip]
    public static T As<T>(this object lhs) { return default(T); }
}

class A
{
    public void Method()
    {
        var s = 10.As<string>() + ""abc"";
    }
}",
                @"
class Extensions {
}

class A {
  public method(): void {
    let s: string = 10 + 'abc';
  }
}
");
        }

        [Test]
        public async Task Translate_should_skip_instance_methods_marked_as_ScriptSkip()
        {
            await AssertTranslation(
                @"
class A
{
    [ScriptSkip]
    public int InstanceSkip() { return 0; }

    public void Method()
    {
        var instance = new A();
        var x = instance.InstanceSkip() + 4;
        new A().InstanceSkip();
    }
}",
                @"
class A {
  public method(): void {
    let instance: A = new A();
    let x: number = instance + 4;
    new A();
  }
}
");
        }

        [Test]
        public async Task Translate_should_skip_ctor_methods_marked_as_ScriptSkip()
        {
            await AssertTranslation(
                @"
class A
{
    private int _x;

    public A() : this(100)
    {
    }

    [ScriptSkip]
    public A(int x)
    {
        _x = x;
    }

    public void Method()
    {
        var instance = new A();
    }
}",
                @"
class A {
  private _x: number;

  public constructor() { }

  public constructor(x: number) {
    this._x = x;
  }

  public method(): void {
    let instance: A = new A();
  }
}
");
        }

        [Test]
        public async Task Translate_should_be_able_to_combine_ScriptSkip_and_InlineCode_correctly()
        {
            // Found this as a translation error - putting it into a unit test.
            await AssertTranslation(
                @"
[Imported, NamedValues]
enum NavigationMetricsName
{
    [ScriptName(""navigationStart"")]
    navigationStart,
}

class NavigationMetricsCollector
{
    private static JsDictionary<NavigationMetricsName, int> navMetrics = null;

    public static void CollectMetrics()
    {
        navMetrics = Script.Reinterpret<JsDictionary<NavigationMetricsName, int>>(Window.Performance.Timing);
        if (Script.In(navMetrics, Script.Reinterpret<string>(NavigationMetricsName.navigationStart)))
        {
        }
    }
}
",
                @"
const enum NavigationMetricsName {
  navigationStart = 'navigationStart',
}

class NavigationMetricsCollector {
  private static navMetrics: { [key: string]: number } = null;

  public static collectMetrics(): void {
    NavigationMetricsCollector.navMetrics = window.performance.timing;
    if (NavigationMetricsName.navigationStart in NavigationMetricsCollector.navMetrics) { }
  }
}
",
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }
    }
}
