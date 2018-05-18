// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TypeScript.Parsing
{
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.TypeScript.Parsing;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Core.TypeScript.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        private static void AssertParseDeclaration(string code, ITsDeclaration expected)
        {
            ITsDeclaration actual = TsParser.ParseDeclaration(code);
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void TsParser_should_parse_function_declarations()
        {
            AssertParseDeclaration(
                "function () { }",
                Factory.FunctionDeclaration(null, Factory.CallSignature(), new ITsStatementListItem[0]));
        }
    }
}
