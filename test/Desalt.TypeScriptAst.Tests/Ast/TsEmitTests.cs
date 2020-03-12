// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    using System.IO;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Emit;
    using FluentAssertions;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        private static readonly ITsIdentifier s_p = Factory.Identifier("p");
        private static readonly ITsIdentifier s_x = Factory.Identifier("x");
        private static readonly ITsIdentifier s_y = Factory.Identifier("y");
        private static readonly ITsIdentifier s_z = Factory.Identifier("z");

        // ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
        private static readonly ITsIdentifier s_T = Factory.Identifier("T");
        private static readonly ITsTypeReference s_TRef = Factory.TypeReference(s_T);
        private static readonly ITsTypeReference s_MyTypeRef = Factory.TypeReference(Factory.Identifier("MyType"));
#pragma warning restore IDE1006 // Naming Styles
        // ReSharper restore InconsistentNaming

        private static void VerifyOutput(ITsAstNode node, string expected, EmitOptions? options = null)
        {
            using var stream = new MemoryStream();
            using var emitter = new Emitter(stream, options: options ?? EmitOptions.UnixSpaces);
            node.Emit(emitter);
            string actualOutput = stream.ReadAllText(emitter.Encoding);
            actualOutput.Should().Be(expected);
        }

        private static void VerifyOutput(ITsAstTriviaNode node, string expected, EmitOptions? options = null)
        {
            using var stream = new MemoryStream();
            using var emitter = new Emitter(stream, options: options ?? EmitOptions.UnixSpaces);
            node.Emit(emitter);
            string actualOutput = stream.ReadAllText(emitter.Encoding);
            actualOutput.Should().Be(expected);
        }

        [Test]
        public void Emit_generic_type_name()
        {
            VerifyOutput(
                Factory.GenericTypeName("a.b.c", Factory.BooleanType, Factory.StringType),
                "a.b.c<boolean, string>");
        }
    }
}
