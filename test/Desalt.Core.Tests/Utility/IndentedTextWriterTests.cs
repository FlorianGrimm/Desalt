// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IndentedTextWriterTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Utility
{
    using System;
    using System.IO;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IndentedTextWriterTests
    {
        private static void RunTest(Action<IndentedTextWriter> test, string expected)
        {
            string actual;

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var indentedWriter = new IndentedTextWriter(writer, "  ") { NewLine = "\n" })
            {
                // Run the test.
                test(indentedWriter);

                // Flush the streams to make sure the content is all there.
                indentedWriter.Flush();

                // Reset the memory stream so that we can read in the contents.
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    actual = reader.ReadToEnd();
                }
            }

            // Do the comparison.
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable ObjectCreationAsStatement
            Action action = () => new IndentedTextWriter(null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("writer");

            action = () => new IndentedTextWriter(null, " ");
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("writer");
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void Ctor_should_use_the_default_IndentationPrefix_if_not_specified()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                using (var indentedWriter = new IndentedTextWriter(writer))
                {
                    indentedWriter.IndentationPrefix.Should().Be(IndentedTextWriter.DefaultIndentationPrefix);
                }

                using (var indentedWriter = new IndentedTextWriter(writer, indentationPrefix: null))
                {
                    indentedWriter.IndentationPrefix.Should().Be(IndentedTextWriter.DefaultIndentationPrefix);
                }
            }
        }

        [TestMethod]
        public void Write_should_not_indent_by_default()
        {
            Action<IndentedTextWriter> test = writer =>
            {
                writer.Write("123");
                writer.WriteLine("45");
            };

            RunTest(test, "12345\n");
        }

        [TestMethod]
        public void Write_should_correctly_indent_one_level()
        {
            Action<IndentedTextWriter> test = writer =>
            {
                writer.WriteLine("{");
                writer.IndentLevel++;
                writer.WriteLine("test");
                writer.IndentLevel--;
                writer.WriteLine("}");
            };
            RunTest(test, "{\n  test\n}\n");
        }

        [TestMethod]
        public void Write_should_handle_indents_less_than_zero()
        {
            Action<IndentedTextWriter> test = writer =>
            {
                writer.IndentLevel = -2;
                writer.WriteLine("abc");
                writer.WriteLine("def");
            };
            RunTest(test, "abc\ndef\n");
        }

        [TestMethod]
        public void Write_should_not_indent_the_first_line_even_if_Indent_is_greater_than_zero()
        {
            Action<IndentedTextWriter> test = writer =>
            {
                writer.IndentLevel = 2;
                writer.WriteLine("abc");
                writer.WriteLine("def");
            };
            RunTest(test, "abc\n    def\n");
        }

        [TestMethod]
        public void Write_should_add_the_indentation_when_writing_an_empty_string()
        {
            Action<IndentedTextWriter> test = writer =>
            {
                writer.IndentLevel++;
                writer.WriteLine();
                writer.Write(string.Empty);
            };
            RunTest(test, "\n  ");
        }
    }
}
