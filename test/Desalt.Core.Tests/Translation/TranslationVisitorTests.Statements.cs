// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Tests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Translation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class TranslationVisitorTests
    {
        [TestMethod]
        public async Task Translate_anonymous_methods_should_correctly_infer_the_return_type()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    Func<string, bool> func = delegate(string x) {
        return x == ""y"";
    };
",
                @"
    let func: (string: string) => boolean = (x: string) => {
      return x === 'y';
    };
");
        }

        //// ===========================================================================================================
        //// Loops Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Translate_while_statements()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    int i = 0;
    while (i < 10)
    {
        i++;
    }
",
                @"
    let i: number = 0;
    while (i < 10) {
      i++;
    }");
        }

        [TestMethod]
        public async Task Translate_do_while_statements()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    int i = 0;
    do
    {
        i++;
    } while (i < 10);
",
                @"
    let i: number = 0;
    do {
      i++;
    } while (i < 10);");
        }

        [TestMethod]
        public async Task Translate_for_statements()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    for (int i = 0, j = 10; i < 10; i++, j--)
    {
    }
",
                @"
    for (let i = 0, j = 10; i < 10; i++, j--) { }
");
        }

        //// ===========================================================================================================
        //// Switch Statment Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Translate_switch_statements()
        {
            await AssertTranslationWithClassCAndMethod(
                @"
    int num = 0;
    switch (num)
    {
        case 0:
        case 1:
            num++;
            break;

        case 2:
            break;

        default:
            num--;
            break;
    }
",
                @"
    let num: number = 0;
    switch (num) {
      case 0:
      case 1:
        num++;
        break;

      case 2:
        break;

      default:
        num--;
        break;
    }
");
        }

        //// ===========================================================================================================
        //// Using Statement Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Translate_a_single_using_block_with_a_declaration()
        {
            await AssertTranslation(
                @"
class C : IDisposable
{
    void Method()
    {
        int i = 0;

        using (var c1 = new C())
        {
            i++;
        }
    }

    public void Dispose() { }
}
",
                @"
class C implements IDisposable {
  private method(): void {
    let i: number = 0;
    {
      const c1: C = new C();
      try {
        i++;
      } finally {
        if (c1) {
          c1.dispose();
        }
      }
    }
  }

  public dispose(): void { }
}
", SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }

        [TestMethod]
        public async Task Translate_a_single_using_block_with_an_expression()
        {
            await AssertTranslation(
                @"
class C : IDisposable
{
    void Method()
    {
        int i = 0;
        var c1 = new C();

        using (c1)
        {
            i++;
        }
    }

    public void Dispose() { }
}
",
                @"
class C implements IDisposable {
  private method(): void {
    let i: number = 0;
    let c1: C = new C();
    {
      const $using1: C = c1;
      try {
        i++;
      } finally {
        if ($using1) {
          $using1.dispose();
        }
      }
    }
  }

  public dispose(): void { }
}
", SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }

        [TestMethod]
        public async Task Translate_nested_using_blocks()
        {
            await AssertTranslation(
                @"
class C : IDisposable
{
    void Method()
    {
        int i = 0;
        var c1 = new C();

        using (c1)
        using (var c2 = new C())
        using (c1.ReturnSelf())
        {
            i++;
        }
    }

    public void Dispose() { }
    public C ReturnSelf() { return this; }
}
",
                @"
class C implements IDisposable {
  private method(): void {
    let i: number = 0;
    let c1: C = new C();
    {
      const $using1: C = c1;
      try {
        const c2: C = new C();
        try {
          const $using2: C = c1.returnSelf();
          try {
            i++;
          } finally {
            if ($using2) {
              $using2.dispose();
            }
          }
        }
         finally {
          if (c2) {
            c2.dispose();
          }
        }
      }
       finally {
        if ($using1) {
          $using1.dispose();
        }
      }
    }
  }

  public dispose(): void { }

  public returnSelf(): C {
    return this;
  }
}
", SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }
    }
}
