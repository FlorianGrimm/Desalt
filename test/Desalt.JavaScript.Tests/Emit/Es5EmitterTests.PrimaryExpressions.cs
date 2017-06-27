// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.PrimaryExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Emit
{
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;
    using Op = Desalt.JavaScript.Ast.Expressions.Es5AssignmentOperator;

    public partial class Es5EmitterTests
    {
        /* 11.1 Primary Expressions
         * ------------------------
         * PrimaryExpression:
         *     this
         *     Identifier
         *     Literal
         *     ArrayLiteral
         *     ObjectLiteral
         *     ( Expression )
         */

        [TestMethod]
        public void Emit_this_expression()
        {
            VerifyOutput(Factory.ThisExpression, "this");
        }

        [TestMethod]
        public void Emit_identifier_expression()
        {
            VerifyOutput(Factory.Identifier("name"), "name");
        }

        [TestMethod]
        public void Emit_parenthesized_expression()
        {
            VerifyOutput(Factory.Identifier("id").WithParentheses(), "(id)");
        }

        [TestMethod]
        public void Emit_literal_expressions()
        {
            VerifyOutput(Factory.NullLiteral, "null");
            VerifyOutput(Factory.TrueLiteral, "true");
            VerifyOutput(Factory.FalseLiteral, "false");
            VerifyOutput(Factory.StringLiteral("'single'"), "'single'");
            VerifyOutput(Factory.StringLiteral("\"double\""), "\"double\"");
            VerifyOutput(Factory.DecimalLiteral("-123.45e67"), "-123.45e67");
            VerifyOutput(Factory.HexIntegerLiteral("0x123"), "0x123");
            VerifyOutput(Factory.RegExpLiteral("/a?b*/i"), "/a?b*/i");
        }

        /* ArrayLiteral:
         *     [ ElisionOpt ]
         *     [ ElementList ]
         *     [ ElementList , ElisionOpt ]
         *
         * ElementList:
         *     ElisionOpt AssignmentExpression
         *     ElementList , ElisionOpt AssignmentExpression
         *
         * Elision:
         *     ,
         *     Elision ,
         */

        [TestMethod]
        public void Emit_empty_array_literal()
        {
            VerifyOutput(Factory.ArrayLiteral(null), "[]");
        }

        [TestMethod]
        public void Emit_array_literal_expressions()
        {
            VerifyOutput(Factory.ArrayLiteral(s_x), "[x]");
            VerifyOutput(Factory.ArrayLiteral(s_x, s_y), "[x, y]");
            VerifyOutput(Factory.ArrayLiteral(null, s_x, null, s_y, null), "[, x, , y, ]");
        }

        /* ObjectLiteral:
         *     { }
         *     { PropertyNameAndValueList }
         *     { PropertyNameAndValueList , }
         *
         * PropertyNameAndValueList:
         *     PropertyAssignment
         *     PropertyNameAndValueList , PropertyAssignment
         *
         * PropertyAssignment:
         *     PropertyName : AssignmentExpression
         *     get PropertyName ( ) { FunctionBody }
         *     set PropertyName ( PropertySetParameterList ) { FunctionBody }
         *
         * PropertyName:
         *     IdentifierName
         *     StringLiteral
         *     NumericLiteral
         *
         * PropertySetParameterList:
         *     Identifier
         */

        [TestMethod]
        public void Emit_empty_object_literal()
        {
            VerifyOutput(Factory.ObjectLiteral(null), "{}");
        }

        [TestMethod]
        public void Emit_simple_property_assignment()
        {
            const string expected = @"{
  boolProp: true,
  intProp: 4
}";
            Es5ObjectLiteralExpression expression = Factory.ObjectLiteral(
                Factory.PropertyValueAssignment("boolProp", Factory.TrueLiteral),
                Factory.PropertyValueAssignment("intProp", Factory.DecimalLiteral("4")));

            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_object_literal_with_property_get()
        {
            const string expected = @"{ get getter() { return x; } }";
            Es5ObjectLiteralExpression expression = Factory.ObjectLiteral(
                Factory.PropertyGet("getter", Factory.ReturnStatement(s_x)));

            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_object_literal_with_property_set()
        {
            const string expected = @"{ set setter(value) { x = value; } }";
            Es5Identifier valueId = Factory.Identifier("value");
            Es5ObjectLiteralExpression expression = Factory.ObjectLiteral(
                Factory.PropertySet(
                    "setter",
                    valueId,
                    Factory.AssignmentExpression(s_x, Op.SimpleAssign, valueId).ToStatement()));

            VerifyOutput(expression, expected);
        }
    }
}
