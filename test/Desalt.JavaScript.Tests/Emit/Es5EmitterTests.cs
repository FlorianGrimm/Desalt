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
    using Desalt.JavaScript.CodeModels;
    using Desalt.JavaScript.Emit;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Es5EmitterTests
    {
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
