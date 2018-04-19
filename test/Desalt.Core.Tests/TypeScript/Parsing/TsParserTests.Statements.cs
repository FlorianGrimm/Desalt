// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Statements.cs" company="Justin Rockwood">
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
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        private static void AssertParseStatement(string code, ITsStatement expected)
        {
            ITsStatement actual = TsParser.ParseStatement(code);
            actual.EmitAsString().Should().BeEquivalentTo(expected.EmitAsString());
        }

        [TestMethod]
        public void TsParser_ParseStatement_should_recognize_a_debugger_statement()
        {
            AssertParseStatement("debugger;", Factory.Debugger);
        }

        [TestMethod]
        public void TsParser_ParseStatement_should_recognize_an_empty_statement()
        {
            AssertParseStatement(";", Factory.EmptyStatement);
        }

        [TestMethod]
        public void TsParser_ParseStatement_should_recognize_an_empty_block()
        {
            AssertParseStatement("{}", Factory.Block());
        }

        [TestMethod]
        public void TsParser_ParseStatment_should_recognize_variable_statements()
        {
            AssertParseStatement(
                "var x: string = 'str';",
                Factory.VariableStatement(
                    Factory.SimpleVariableDeclaration(s_x, Factory.StringType, Factory.String("str"))));
        }
    }
}
