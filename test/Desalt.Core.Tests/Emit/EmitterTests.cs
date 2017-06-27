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
    using System.Linq;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EmitterTests
    {
        private static readonly EmitOptions s_testOptions = EmitOptions.Default
            .WithNewline("\n")
            .WithIndentationPrefix("\t");

        private static readonly IAstNode[] s_mockStatements = new[]
        {
            CreateMockStatement("One"),
            CreateMockStatement("Two"),
            CreateMockStatement("Three")
        };

        private static IAstNode CreateMockStatement(string text)
        {
            var mock = new Mock<IAstNode>();
            mock.Setup(m => m.ToCodeDisplay()).Returns(text);
            return mock.Object;
        }

        [TestMethod]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable ObjectCreationAsStatement
            Action action = () => new Emitter<IAstNode>(outputStream: null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("outputStream");
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void Ctor_should_store_the_encoding_parameter_in_the_property()
        {
            var emitter = new Emitter<IAstNode>(new MemoryStream(), Encoding.ASCII);
            emitter.Encoding.Should().BeSameAs(Encoding.ASCII);
        }

        [TestMethod]
        public void Ctor_should_use_UTF8_no_BOM_encoding_if_not_supplied()
        {
            var emitter = new Emitter<IAstNode>(new MemoryStream());
            emitter.Encoding.EncodingName.Should().Be(Encoding.UTF8.EncodingName);
            emitter.Encoding.GetPreamble().Should().BeEmpty();
        }

        [TestMethod]
        public void Ctor_should_store_the_options_parameter_in_the_property()
        {
            var options = EmitOptions.Default.WithIndentationPrefix("\v");
            var emitter = new Emitter<IAstNode>(new MemoryStream(), options: options);
            emitter.Options.Should().BeSameAs(options);
        }

        [TestMethod]
        public void WriteBlock_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream);
                Action action = () => emitter.WriteBlock(writeBodyAction: null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("writeBodyAction");

                action = () => emitter.WriteBlock<IAstNode>(blockElements: null, elementAction: _ => { });
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("blockElements");

                action = () => emitter.WriteBlock(blockElements: new IAstNode[0], elementAction: null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("elementAction");
            }
        }

        [TestMethod]
        public void WriteBlock_should_surround_the_body_with_braces()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream) { AutoFlush = true })
            {
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions);
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
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions);
                emitter.WriteBlock(() => emitter.Write("text"));
                stream.ReadAllText().Should().Be("{\n\ttext\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_add_a_space_between_empty_block_braces_if_the_options_specifiy_it()
        {
            using (var stream = new MemoryStream())
            {
                EmitOptions options = s_testOptions.WithSimpleBlockOnNewLine(false);
                var emitter = new Emitter<IAstNode>(stream, options: options);
                emitter.WriteBlock(Enumerable.Empty<IAstNode>(), elem => emitter.Write("Element"));
                stream.ReadAllText().Should().Be("{ }");
            }
        }

        [TestMethod]
        public void WriteBlock_should_add_a_space_between_empty_function_block_braces_if_the_options_specifiy_it()
        {
            using (var stream = new MemoryStream())
            {
                EmitOptions options = s_testOptions.WithSimpleBlockOnNewLine(false);
                var emitter = new Emitter<IAstNode>(stream, options: options);
                emitter.WriteBlock(Enumerable.Empty<IAstNode>(), elem => emitter.Write("Element"));
                stream.ReadAllText().Should().Be("{ }");
            }
        }

        [TestMethod]
        public void WriteBlock_should_use_the_options_for_formatting_simple_blocks()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions.WithSimpleBlockOnNewLine(false));
                emitter.WriteBlock(() => emitter.Write("text"), isSimpleBlock: true);
                stream.ReadAllText().Should().Be("{ text }");
            }

            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions.WithSimpleBlockOnNewLine(true));
                emitter.WriteBlock(() => emitter.Write("text"), isSimpleBlock: true);
                stream.ReadAllText().Should().Be("{\n\ttext\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_write_all_of_the_statements_using_an_action()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions);
                emitter.WriteBlock(s_mockStatements, elem => emitter.Write(elem.ToCodeDisplay()));
                stream.ReadAllText().Should().Be("{\n\tOne\n\tTwo\n\tThree\n}");
            }
        }

        [TestMethod]
        public void WriteList_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream);
                Action action = () => emitter.WriteList<IAstNode>(null, "-", elem => emitter.Write(elem.ToCodeDisplay()));
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("elements");

                action = () => emitter.WriteList(s_mockStatements, null, elem => emitter.Write(elem.ToCodeDisplay()));
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("delimiter");

                action = () => emitter.WriteList(s_mockStatements, "", elem => emitter.Write(elem.ToCodeDisplay()));
                action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("delimiter");

                action = () => emitter.WriteList(s_mockStatements, "-", elementAction: null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("elementAction");
            }
        }

        [TestMethod]
        public void WriteList_should_add_delimiters_between_elements()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream);
                emitter.WriteList(s_mockStatements, "-", elem => emitter.Write(elem.ToCodeDisplay()));
                stream.ReadAllText().Should().Be("One-Two-Three");
            }
        }

        [TestMethod]
        public void WriteList_should_not_add_a_delimiter_when_there_is_only_one_element()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream);
                emitter.WriteList(s_mockStatements.Take(1), "-", elem => emitter.Write(elem.ToCodeDisplay()));
                stream.ReadAllText().Should().Be("One");
            }
        }

        [TestMethod]
        public void WriteCommaList_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream);
                Action action = () => emitter.WriteCommaList<IAstNode>(null, elem => emitter.Write(elem.ToCodeDisplay()));
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("elements");

                action = () => emitter.WriteCommaList(s_mockStatements, elementAction: null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("elementAction");
            }
        }

        [TestMethod]
        public void WriteCommaList_should_add_commas_with_spaces_between_elements()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter<IAstNode>(stream, options: s_testOptions);
                emitter.WriteCommaList(s_mockStatements, elem => emitter.Write(elem.ToCodeDisplay()));
                stream.ReadAllText().Should().Be("One, Two, Three");
            }
        }
    }
}
