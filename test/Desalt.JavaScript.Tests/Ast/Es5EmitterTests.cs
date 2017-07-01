// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Ast
{
    using System.IO;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;

    [TestClass]
    public partial class Es5EmitterTests
    {
        private static readonly Es5Identifier s_x = Factory.Identifier("x");
        private static readonly Es5Identifier s_y = Factory.Identifier("y");
        private static readonly Es5Identifier s_z = Factory.Identifier("z");

        private static void VerifyOutput(IAstNode node, string expected, EmitOptions options = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new Emitter(stream, options: options ?? EmitOptions.UnixSpaces))
            {
                node.Emit(emitter);
                stream.ReadAllText(emitter.Encoding).Should().Be(expected);
            }
        }
    }
}
