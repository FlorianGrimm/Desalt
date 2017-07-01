// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitTests.Program.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Ast
{
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using AssignOp = Desalt.JavaScript.Ast.Expressions.Es5AssignmentOperator;
    using BinaryOp = Desalt.JavaScript.Ast.Expressions.Es5BinaryOperator;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;

    public partial class Es5EmitTests
    {
        [TestMethod]
        public void Emit_full_program()
        {
            string expected = @"(function() {
  'use strict';
  var $asm = {};
  global.TestNs = global.TestNs || {};
  ss.initAssembly($asm, 'Test');
  var $TestNs_Expressions = global.TestNs.Expressions = ss.mkType($asm, 'TestNs.Expressions', function() { }, {
    add: function(x, y) {
      return x + y;
    }
  });
  ss.initClass($TestNs_Expressions);
})();
".Replace("\r\n", "\n");

            // ReSharper disable InconsistentNaming
            Es5Identifier asm = Factory.Identifier("$asm");
            Es5Identifier ss = Factory.Identifier("ss");
            Es5Identifier global = Factory.Identifier("global");
            Es5Identifier testNs = Factory.Identifier("TestNs");
            Es5Identifier _TestNs_Expressions = Factory.Identifier("$TestNs_Expressions");
            Es5MemberExpression global_TestNs = Factory.MemberDot(global, testNs);
            Es5MemberExpression global_TestNs_Expressions = Factory.MemberDot(
                global_TestNs, Factory.Identifier("Expressions"));
            Es5MemberExpression ss_initAssembly = Factory.MemberDot(ss, Factory.Identifier("initAssembly"));
            Es5MemberExpression ss_initClass = Factory.MemberDot(ss, Factory.Identifier("initClass"));
            Es5MemberExpression ss_mkType = Factory.MemberDot(ss, Factory.Identifier("mkType"));
            // ReSharper restore InconsistentNaming

            // function(x, y) { return x + y; }
            Es5FunctionExpression addFunc = Factory.Function(
                    functionName: null,
                    parameters: Factory.ParamList("x", "y"),
                    functionBody: Factory.ReturnStatement(Factory.BinaryExpression(s_x, BinaryOp.Add, s_y)));

            // { add: function(x, y) { return x + y; } }
            Es5ObjectLiteralExpression classObj = Factory.ObjectLiteral(
                Factory.PropertyValueAssignment("add", addFunc));

            Es5CallExpression mkTypeCall = Factory.Call(
                ss_mkType,
                asm,
                Factory.StringLiteral("'TestNs.Expressions'"),
                Factory.Function(),
                classObj);

            Es5FunctionExpression topFunc = Factory.Function(
                // 'use strict';
                Factory.StringLiteral("'use strict'").ToStatement(),

                // var $asm = {};
                Factory.VariableStatement(Factory.VariableDeclaration(asm, Factory.EmptyObjectLiteral)),

                // global.TestNs = global.TestNs || {};
                Factory.AssignmentExpression(
                    global_TestNs,
                    AssignOp.SimpleAssign,
                    Factory.BinaryExpression(global_TestNs, BinaryOp.LogicalOr, Factory.EmptyObjectLiteral))
                    .ToStatement(),

                // ss.initAssembly($asm, 'Test');
                Factory.Call(ss_initAssembly, asm, Factory.StringLiteral("'Test'")).ToStatement(),

                // var $TestNs_Expressions = global.TestNs.Expressions = ss.mkType(
                //     $asm, 'TestNs.Expressions', function() {}, { add: function(x, y) { return x + y; } });
                Factory.VariableStatement(
                    Factory.VariableDeclaration(
                        _TestNs_Expressions,
                        Factory.AssignmentExpression(global_TestNs_Expressions, AssignOp.SimpleAssign, mkTypeCall))),

                // ss.initClass($TestNs_Expressions);
                Factory.Call(ss_initClass, _TestNs_Expressions).ToStatement());

            Es5Program program = Factory.Program(Factory.Call(topFunc.WithParentheses()).ToStatement());

            VerifyOutput(program, expected);
        }
    }
}
