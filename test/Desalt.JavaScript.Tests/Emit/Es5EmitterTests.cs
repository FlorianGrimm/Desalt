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
    using Desalt.JavaScript.CodeModels;
    using Desalt.JavaScript.Emit;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.CodeModels.Es5ModelFactory;

    [TestClass]
    public partial class Es5EmitterTests
    {
        private static readonly Es5Identifier s_x = Factory.Identifier("x");
        private static readonly Es5Identifier s_y = Factory.Identifier("y");
        private static readonly Es5Identifier s_z = Factory.Identifier("z");
        private static readonly EmitOptions s_compact = EmitOptions.Compact;

        private static void VerifyOutput(IEs5CodeModel model, string expected, EmitOptions options = null)
        {
            var emitter = new Es5Emitter();
            using (var stream = new MemoryStream())
            {
                emitter.Emit(model, stream, options: options ?? EmitOptions.Default);

                byte[] bytes = stream.ToArray();
                string actual = Es5Emitter.DefaultEncoding.GetString(bytes);

                actual.Should().Be(expected);
            }
        }

        [TestMethod]
        public void Emit_should_throw_on_null_args()
        {
            var emitter = new Es5Emitter();
            Action action = () => emitter.Emit(model: null, outputStream: new MemoryStream());
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("model");

            action = () => emitter.Emit(model: Es5ModelFactory.Identifier("id"), outputStream: null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("outputStream");
        }
    }
}
