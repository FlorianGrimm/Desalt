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
        private static async Task AssertExpressionTranslation(string expression, string expectedTypeScriptCode)
        {
            string code = $@"
public class C
{{
    private int Int32;
    private string Str;

    public void Method()
    {{
        {expression};
    }}
}}";

            string expected = $@"export class C {{
  private int32: number;

  private str: string;

  public method(): void {{
    {expectedTypeScriptCode};
  }}
}}
".Replace("\r\n", "\n");

            await AssertTranslation(code, expected);
        }

        [TestClass]
        public class Expressions
        {
            [TestMethod]
            public async Task This_expression_assigning_literals()
            {
                await AssertExpressionTranslation("this.Int32 = 1", "this.int32 = 1");
                await AssertExpressionTranslation("this.Str = \"string\"", "this.str = 'string'");
            }

            [TestMethod]
            public async Task Cast_expressions()
            {
                await AssertExpressionTranslation("var x = (string)Str", "let x: string = <string>this.str");
            }
        }
    }
}
