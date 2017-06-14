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

    [TestClass]
    public partial class TsEmitterTests
    {
        private static void VerifyOutput(ITsAstNode model, string expected, EmitOptions options = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new TsEmitter(stream, options: options ?? EmitOptions.Default))
            {
                emitter.Visit(model);
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
    }
}
