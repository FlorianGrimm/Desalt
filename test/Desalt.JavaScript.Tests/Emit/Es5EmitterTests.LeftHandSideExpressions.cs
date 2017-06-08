// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.LeftHandSideExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Emit
{
    using Desalt.Core.Emit;
    using Desalt.JavaScript.CodeModels.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.CodeModels.Es5ModelFactory;

    public partial class Es5EmitterTests
    {
        /* 11.2 Left-Hand-Side Expressions
         * -------------------------------
         * MemberExpression:
         *     PrimaryExpression
         *     Function
         *     MemberExpression [ Expression ]
         *     MemberExpression . IdentifierName
         *     new MemberExpression Arguments
         *
         * NewExpression:
         *     MemberExpression
         *     new NewExpression
         *
         * CallExpression:
         *     MemberExpression Arguments
         *     CallExpression Arguments
         *     CallExpression [ Expression ]
         *     CallExpression . IdentifierName
         *
         * Arguments:
         *     ( )
         *     ( ArgumentList )
         *
         * ArgumentList:
         *     AssignmentExpression
         *     ArgumentList , AssignmentExpression
         *
         * LeftHandSideExpression:
         *     NewExpression
         *     CallExpression
         */

        [TestMethod]
        public void Emit_bracket_member_expression()
        {
            const string expected = @"x[""throw""]";
            Es5MemberExpression expression =
                Factory.MemberBracket(s_x, Factory.StringLiteral("\"throw\""));

            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_dot_notation_member_expression()
        {
            VerifyOutput(Factory.MemberDot(s_x, s_y), "x.y");
        }

        [TestMethod]
        public void Emit_function_call_expression()
        {
            Es5CallExpression expression = Factory.Call(
                Factory.MemberDot(s_x, s_y),
                Factory.ParamList("p1", "p2", "p3"));
            VerifyOutput(expression, "x.y(p1, p2, p3)");
        }

        [TestMethod]
        public void Emit_function_call_expression_compact()
        {
            Es5CallExpression expression = Factory.Call(
                Factory.MemberDot(s_x, s_y),
                Factory.ParamList("p1", "p2", "p3"));
            VerifyOutput(expression, "x.y(p1,p2,p3)", EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_new_call_expression()
        {
            Es5CallExpression expression = Factory.NewCall(
                Factory.MemberDot(s_x, s_y),
                Factory.ParamList("p1", "p2", "p3"));
            VerifyOutput(expression, "new x.y(p1, p2, p3)");
        }

        [TestMethod]
        public void Emit_new_call_expression_compact()
        {
            Es5CallExpression expression = Factory.NewCall(
                Factory.MemberDot(s_x, s_y),
                Factory.ParamList("p1", "p2", "p3"));
            VerifyOutput(expression, "new x.y(p1,p2,p3)", EmitOptions.Compact);
        }

        /*
         * Function:
         *     function IdentifierOpt ( FormalParameterListOpt ) { FunctionBody }
         */

        [TestMethod]
        public void Emit_full_function_expression()
        {
            const string expected = @"function funcName(param1, param2) {
  return x;
}";
            Es5FunctionExpression expression = Factory.Function(
                "funcName",
                Factory.ParamList("param1", "param2"),
                Factory.ReturnStatement(s_x));

            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_full_function_expression_compact()
        {
            const string expected = @"function funcName(param1,param2){return x;}";
            Es5FunctionExpression expression = Factory.Function(
                "funcName",
                Factory.ParamList("param1", "param2"),
                Factory.ReturnStatement(s_x));

            VerifyOutput(expression, expected, EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_unnamed_function_expression()
        {
            const string expected = @"function (param1, param2) {
  return x;
}";
            Es5FunctionExpression expression = Factory.Function(
                functionName: null,
                parameters: Factory.ParamList("param1", "param2"),
                functionBody: Factory.ReturnStatement(s_x));

            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_unnamed_function_expression_compact()
        {
            const string expected = @"function(param1,param2){return x;}";
            Es5FunctionExpression expression = Factory.Function(
                functionName: null,
                parameters: Factory.ParamList("param1", "param2"),
                functionBody: Factory.ReturnStatement(s_x));

            VerifyOutput(expression, expected, EmitOptions.Compact);
        }
    }
}
