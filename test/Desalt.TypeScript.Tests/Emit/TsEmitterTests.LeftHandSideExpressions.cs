// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitterTests.LeftHandSideExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Emit
{
    using Desalt.TypeScript.Ast;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitterTests
    {
        /* 12.e Left-Hand-Side Expressions
         * -------------------------------
         * MemberExpression:
         *   PrimaryExpression
         *   MemberExpression [ Expression ]
         *   MemberExpression . IdentifierName
         *   MemberExpression TemplateLiteral
         *   SuperProperty
         *   MetaProperty
         *   new MemberExpression Arguments
         */

        [TestMethod]
        public void Emit_bracket_member_expression()
        {
            const string expected = @"x['throw']";
            ITsMemberBracketExpression expression =
                Factory.MemberBracketExpression(s_x, Factory.StringLiteral("throw", StringLiteralQuoteKind.SingleQuote));

            VerifyOutput(expression, expected);
        }
    }
}
