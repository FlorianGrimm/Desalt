// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Emit
{
    using System;
    using System.IO;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Emit;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5ModelFactory;

    [TestClass]
    public partial class Es5EmitterTests
    {
        private static readonly Es5Identifier s_x = Factory.Identifier("x");
        private static readonly Es5Identifier s_y = Factory.Identifier("y");
        private static readonly Es5Identifier s_z = Factory.Identifier("z");
        private static readonly EmitOptions s_compact = EmitOptions.Compact;

        private static void VerifyOutput(IEs5AstNode model, string expected, EmitOptions options = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new Es5Emitter(stream, options: options ?? EmitOptions.Default))
            {
                emitter.Visit(model);
                stream.ReadAllText(emitter.Encoding).Should().Be(expected);
            }
        }

        [TestMethod]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new Es5Emitter(outputStream: null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("outputStream");
        }
    }
}
