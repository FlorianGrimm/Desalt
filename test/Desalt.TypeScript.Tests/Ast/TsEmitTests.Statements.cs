// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Ast
{
    using Desalt.TypeScript.Ast;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_block_statements()
        {
            VerifyOutput(Factory.Block(Factory.Debugger, Factory.Debugger), "{\n  debugger;\n  debugger;\n}");
        }

        [TestMethod]
        public void Emit_empty_statement()
        {
            VerifyOutput(Factory.EmptyStatement, ";\n");
        }

        [TestMethod]
        public void Emit_simple_variable_declarations()
        {
            VerifyOutput(Factory.SimpleVariableDeclaration(s_x), "x");
            VerifyOutput(Factory.SimpleVariableDeclaration(s_x, Factory.ArrayType(Factory.Boolean)), "x: boolean[]");
            VerifyOutput(
                Factory.SimpleVariableDeclaration(
                    s_x,
                    Factory.String,
                    Factory.StringLiteral("hello", StringLiteralQuoteKind.SingleQuote)),
                "x: string = 'hello'");
        }

        [TestMethod]
        public void Emit_single_name_binding()
        {
            VerifyOutput(Factory.SingleNameBinding(s_x, s_y), "x = y");
        }

        [TestMethod]
        public void Emit_debugger_statement()
        {
            VerifyOutput(Factory.Debugger, "debugger;\n");
        }
    }
}
