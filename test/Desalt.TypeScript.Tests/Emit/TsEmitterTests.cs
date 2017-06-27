// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Emit
{
    using System;
    using System.IO;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast;
    using Desalt.TypeScript.Emit;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    [TestClass]
    public partial class TsEmitterTests
    {
        private static readonly ITsIdentifier s_x = Factory.Identifier("x");
        private static readonly ITsIdentifier s_y = Factory.Identifier("y");
        private static readonly ITsIdentifier s_z = Factory.Identifier("z");
        private static readonly EmitOptions s_compact = EmitOptions.Compact;

        private static void VerifyOutput(ITsAstNode node, string expected, EmitOptions options = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new TsEmitter(stream, options: options ?? EmitOptions.Default))
            {
                emitter.Visit(node);
                stream.ReadAllText(emitter.Encoding).Should().Be(expected);
            }
        }

        [TestMethod]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new TsEmitter(outputStream: null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("outputStream");
        }

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        [TestMethod]
        public void Emit_null_literal()
        {
            VerifyOutput(Factory.NullLiteral, "null");
        }

        [TestMethod]
        public void Emit_boolean_literals()
        {
            VerifyOutput(Factory.TrueLiteral, "true");
            VerifyOutput(Factory.FalseLiteral, "false");
        }

        [TestMethod]
        public void Emit_string_literals()
        {
            VerifyOutput(Factory.StringLiteral("single", StringLiteralQuoteKind.SingleQuote), "'single'");
            VerifyOutput(Factory.StringLiteral("double", StringLiteralQuoteKind.DoubleQuote), "\"double\"");
        }

        [TestMethod]
        public void Number_literals_should_be_positive()
        {
            Action action = () => Factory.DecimalLiteral(-123);
            action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.BinaryIntegerLiteral(-123);
            action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.OctalIntegerLiteral(-123);
            action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.HexIntegerLiteral(-123);
            action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");
        }

        [TestMethod]
        public void Emit_decimal_literals()
        {
            VerifyOutput(Factory.DecimalLiteral(123), "123");
            VerifyOutput(Factory.DecimalLiteral(1.23e4), "12300");
            VerifyOutput(Factory.DecimalLiteral(83e45), "8.3E+46");
            VerifyOutput(Factory.DecimalLiteral(53e-53), "5.3E-52");
        }

        [TestMethod]
        public void Emit_binary_integer_literals()
        {
            VerifyOutput(Factory.BinaryIntegerLiteral(17), "0b10001");
        }

        [TestMethod]
        public void Emit_octal_integer_literals()
        {
            VerifyOutput(Factory.OctalIntegerLiteral(20), "0o24");
        }

        [TestMethod]
        public void Emit_hex_integer_literal()
        {
            VerifyOutput(Factory.HexIntegerLiteral(415), "0x19f");
            VerifyOutput(Factory.HexIntegerLiteral(48879), "0xBEEF", EmitOptions.Default.WithLowerCaseHexLetters(false));
        }

        [TestMethod]
        public void Emit_regular_expression_literals()
        {
            VerifyOutput(Factory.RegularExpressionLiteral("a-z", "g"), "/a-z/g");
            VerifyOutput(Factory.RegularExpressionLiteral("hello", null), "/hello/");
        }

        [TestMethod]
        public void Emit_array_literals()
        {
            VerifyOutput(
                Factory.ArrayLiteral(
                    Factory.ArrayElement(s_x),
                    Factory.ArrayElement(Factory.DecimalLiteral(10))),
                "[x, 10]");

            VerifyOutput(
                Factory.ArrayLiteral(
                    Factory.ArrayElement(s_y),
                    Factory.ArrayElement(s_z),
                    Factory.ArrayElement(Factory.StringLiteral("str", StringLiteralQuoteKind.SingleQuote))),
                "[y,z,'str']",
                EmitOptions.Default.WithSpaceAfterComma(false));
        }

        [TestMethod]
        public void Emit_array_literals_with_spread_operator()
        {
            VerifyOutput(Factory.ArrayLiteral(Factory.ArrayElement(s_y, isSpreadElement: true)), "[y ...]");
        }

        [TestMethod]
        public void Emit_template_literals()
        {
            VerifyOutput(Factory.TemplateLiteral(new TsTemplatePart(template: "string")), "`string`");
            VerifyOutput(
                Factory.TemplateLiteral(
                    new TsTemplatePart("xy=", s_x),
                    new TsTemplatePart(expression: s_y)),
                "`xy=${x}${y}`");
        }
    }
}
