// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.Tests.Statements.cs" company="Justin Rockwood">
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
        [TestMethod]
        public async Task Translate_anonymous_methods_should_correctly_infer_the_return_type()
        {
            await AssertTranslation(
                @"
using System;

class C {
    void Method() {
        Func<string, bool> func = delegate(string x) {
            return x == ""y"";
        };
    }
}",
                @"class C {
  private method(): void {
    let func: (string: string) => boolean = (x: string) => {
      return x === 'y';
    };
  }
}
");
        }

        [TestMethod]
        public async Task Translate_for_statements()
        {
            await AssertTranslation(
                @"
class C
{
    void Method()
    {
        for (int i = 0, j = 10; i < 10; i++, j--)
        {
        }
    }
}",
                @"
class C {
  private method(): void {
    for (let i = 0, j = 10; i < 10; i++, j--) { }
  }
}
");
        }
    }
}
