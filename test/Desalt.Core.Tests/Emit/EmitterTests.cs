// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EmitterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Emit
{
    using System;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EmitterTests
    {
        private static readonly EmitOptions s_testOptions = EmitOptions.Default
            .WithNewline("\n")
            .WithIndentationPrefix("\t");

        [TestMethod]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable ObjectCreationAsStatement
            Action action = () => new Emitter(outputStream: null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("outputStream");
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void Ctor_should_store_the_encoding_parameter_in_the_property()
        {
            var emitter = new Emitter(new MemoryStream(), Encoding.ASCII);
            emitter.Encoding.Should().BeSameAs(Encoding.ASCII);
        }

        [TestMethod]
        public void Ctor_should_use_UTF8_no_BOM_encoding_if_not_supplied()
        {
            var emitter = new Emitter(new MemoryStream());
            emitter.Encoding.EncodingName.Should().Be(Encoding.UTF8.EncodingName);
            emitter.Encoding.GetPreamble().Should().BeEmpty();
        }

        [TestMethod]
        public void Ctor_should_store_the_options_parameter_in_the_property()
        {
            var options = EmitOptions.Default.WithIndentationPrefix("\v");
            var emitter = new Emitter(new MemoryStream(), options: options);
            emitter.Options.Should().BeSameAs(options);
        }

        [TestMethod]
        public void WriteBlock_should_surround_the_body_with_braces()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream) { AutoFlush = true })
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                // ReSharper disable once AccessToDisposedClosure
                emitter.WriteBlock(() => writer.Write("body"));
                stream.ReadAllText().Should().Be("{\nbody\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_indent_the_contents_if_necessary()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteBlock(() => emitter.Write("text"));
                stream.ReadAllText().Should().Be("{\n\ttext\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_use_the_options_for_formatting_simple_blocks()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions.WithSimpleBlockOnNewLine(false));
                emitter.WriteBlock(() => emitter.Write("text"), isSimpleBlock: true);
                stream.ReadAllText().Should().Be("{ text }");
            }

            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions.WithSimpleBlockOnNewLine(true));
                emitter.WriteBlock(() => emitter.Write("text"), isSimpleBlock: true);
                stream.ReadAllText().Should().Be("{\n\ttext\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_use_the_options_for_formatting_non_simple_blocks()
        {
            var options = s_testOptions
                .WithNewlineBeforeClosingBrace(false)
                .WithNewlineAfterOpeningBrace(false)
                .WithSpaceWithinSimpleBlockBraces(false);

            using (var stream = new MemoryStream())
            using (var emitter = new Emitter(stream, options: options))
            {
                // ReSharper disable once AccessToDisposedClosure
                emitter.WriteBlock(() => emitter.Write("text"));
                stream.ReadAllText().Should().Be("{text}");
            }
        }
    }
}
