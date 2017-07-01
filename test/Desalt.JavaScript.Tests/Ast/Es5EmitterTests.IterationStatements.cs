// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.IterationStatements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Ast
{
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast.Expressions;
    using Desalt.JavaScript.Ast.Statements;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;

    public partial class Es5EmitterTests
    {
        [TestMethod]
        public void Emit_do_while_statement()
        {
            Es5DoStatement statement = Factory.DoStatement(
                Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixIncrement).ToStatement(),
                Factory.TrueLiteral);

            VerifyOutput(statement, "do\n  x++;\nwhile (true);\n");
        }

        [TestMethod]
        public void Emit_while_statement()
        {
            const string expected = "while (true)\n  x++;\n";
            Es5WhileStatement statement = Factory.WhileStatement(
                Factory.TrueLiteral,
                Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixIncrement).ToStatement());

            VerifyOutput(statement, expected);
        }

        [TestMethod]
        public void Emit_for_statements()
        {
            string expected = "for (var x = 0; x < y; x++)\n  z--;\n";
            Es5ForStatement statement = Factory.ForStatement(
                declarations: Factory.VariableDeclaration(s_x, Factory.DecimalLiteral("0")).ToSafeArray(),
                condition: Factory.BinaryExpression(s_x, Es5BinaryOperator.LessThan, s_y),
                incrementor: Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixIncrement),
                statement: Factory.UnaryExpression(s_z, Es5UnaryOperator.PostfixDecrement).ToStatement());

            VerifyOutput(statement, expected);

            expected = "for (var x = 0; ; x++)\n  ;\n";
            statement = Factory.ForStatement(
                declarations: Factory.VariableDeclaration(s_x, Factory.DecimalLiteral("0")).ToSafeArray(),
                condition: null,
                incrementor: Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixIncrement),
                statement: Factory.EmptyStatement);

            VerifyOutput(statement, expected);

            expected = "for (; ; )\n  ;\n";
            statement = Factory.ForStatement(
                declarations: null,
                condition: null,
                incrementor: null,
                statement: Factory.EmptyStatement);

            VerifyOutput(statement, expected);
        }

        [TestMethod]
        public void Emit_for_in_statements()
        {
            string expected = "for (x in y)\n  z--;\n";
            Es5ForInStatement statement = Factory.ForInStatement(
                s_x, s_y, Factory.UnaryExpression(s_z, Es5UnaryOperator.PostfixDecrement).ToStatement());

            VerifyOutput(statement, expected);

            expected = "for (var x in y)\n  z--;\n";
            statement = Factory.ForInStatement(
                Factory.VariableDeclaration(s_x),
                s_y,
                Factory.UnaryExpression(s_z, Es5UnaryOperator.PostfixDecrement).ToStatement());

            VerifyOutput(statement, expected);
        }
    }
}
