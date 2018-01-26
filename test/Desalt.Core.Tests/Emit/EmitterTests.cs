// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EmitterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Emit
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EmitterTests
    {
        private static readonly EmitOptions s_testOptions = EmitOptions.UnixTabs;

        private static readonly IAstNode[] s_statements =
        {
            new Identifier("One"),
            new Identifier("Two"),
            new Identifier("Three")
        };

        private sealed class Identifier : IAstNode
        {
            public Identifier(string name) => Name = name;

            private string Name { get; }

            public string CodeDisplay => Name;

            public void Accept<TVisitor>(TVisitor visitor) where TVisitor : IAstVisitor =>
                throw new NotImplementedException();

            public void Emit(Emitter emitter) => emitter.Write(Name);
        }

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
        public void WriteBlock_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                Action action = () => emitter.WriteBlock(items: null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("items");
            }
        }

        [TestMethod]
        public void WriteBlock_should_surround_the_body_with_braces_and_indent()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteBlock(new[] { new Identifier("body") });
                stream.ReadAllText().Should().Be("{\n\tbody\n}");
            }
        }

        [TestMethod]
        public void WriteBlock_should_add_newlines_between_empty_block_braces_if_the_options_specify_it()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteBlock(ImmutableArray<IAstNode>.Empty);
                stream.ReadAllText().Should().Be("{ }");
            }
        }

        [TestMethod]
        public void WriteBlock_should_add_a_space_between_empty_block_braces_if_the_options_specifiy_it()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteBlock(ImmutableArray<IAstNode>.Empty);
                stream.ReadAllText().Should().Be("{ }");
            }
        }

        [TestMethod]
        public void WriteBlock_should_write_all_of_the_statements()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteBlock(s_statements);
                stream.ReadAllText().Should().Be("{\n\tOne\n\tTwo\n\tThree\n}");
            }
        }

        [TestMethod]
        public void WriteCommaNewlineSeparatedBlock_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                Action action = () => emitter.WriteCommaNewlineSeparatedBlock(null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("items");
            }
        }

        [TestMethod]
        public void WriteCommaNewlineSeparatedBlock_should_add_commas_and_newlines_between_elements()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteCommaNewlineSeparatedBlock(s_statements);
                stream.ReadAllText().Should().Be("{\n\tOne,\n\tTwo,\n\tThree\n}");
            }
        }

        [TestMethod]
        public void WriteParameterList_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                Action action = () => emitter.WriteParameterList(null);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("items");
            }
        }

        [TestMethod]
        public void WriteParameterList_should_write_all_of_the_parameters()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream, options: s_testOptions);
                emitter.WriteParameterList(s_statements);
                stream.ReadAllText().Should().Be("(One, Two, Three)");
            }
        }

        [TestMethod]
        public void WriteItems_should_throw_on_null_args()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                Action action = () => emitter.WriteList(null, true);
                action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("items");
            }
        }

        [TestMethod]
        public void WriteItems_should_add_delimiters_between_elements()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                emitter.WriteList(s_statements, indent: false, itemDelimiter: "-");
                stream.ReadAllText().Should().Be("One-Two-Three");
            }
        }

        [TestMethod]
        public void WriteList_should_not_add_a_delimiter_when_there_is_only_one_element()
        {
            using (var stream = new MemoryStream())
            {
                var emitter = new Emitter(stream);
                emitter.WriteList(s_statements.Take(1).ToImmutableArray(), indent: false, itemDelimiter: "-");
                stream.ReadAllText().Should().Be("One");
            }
        }
    }
}
