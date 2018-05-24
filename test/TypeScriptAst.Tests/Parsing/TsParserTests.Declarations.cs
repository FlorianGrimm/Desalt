// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.Parsing
{
    using FluentAssertions;
    using TypeScriptAst.Ast;
    using TypeScriptAst.Parsing;
    using Xunit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        private static void AssertParseDeclaration(string code, ITsDeclaration expected)
        {
            ITsDeclaration actual = TsParser.ParseDeclaration(code);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TsParser_should_parse_function_declarations()
        {
            AssertParseDeclaration(
                "function () { }",
                Factory.FunctionDeclaration(null, Factory.CallSignature(), new ITsStatementListItem[0]));
        }
    }
}
