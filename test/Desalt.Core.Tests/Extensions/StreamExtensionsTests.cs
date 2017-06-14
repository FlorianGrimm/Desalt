// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamExtensionsTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using Desalt.Core.Extensions;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamExtensionsTests
    {
        [TestMethod]
        public void ReadAllText_should_throw_on_null_args()
        {
            Action action = () => StreamExtensions.ReadAllText(null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("stream");
        }

        [TestMethod]
        public void ReadAllText_should_get_all_of_the_text()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadAllText().Should().Be("Hello");
            }
        }

        [TestMethod]
        public void ReadAllText_should_not_preserve_the_position_by_default()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadAllText();
                stream.Position.Should().Be("Hello".Length);
            }
        }

        [TestMethod]
        public void ReadAllText_should_preserve_the_position_if_requested()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);

                stream.Position = 1;
                stream.ReadAllText(preservePosition: true);
                stream.Position.Should().Be(1);
            }
        }

        [TestMethod]
        public void ReadAllText_should_use_UTF8_encoding_by_default()
        {
            const string chinese = "中文";
            var utfBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var utfNoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            // use UTF8 encoding with a BOM
            using (var stream = new MemoryStream())
            {
                byte[] bytes = utfNoBom.GetBytes(chinese);
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadAllText().Should().Be(chinese);
            }

            // use UTF8 encoding without a BOM
            using (var stream = new MemoryStream())
            {
                byte[] bytes = utfBom.GetBytes(chinese);
                stream.Write(utfBom.GetPreamble(), 0, utfBom.GetPreamble().Length);
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadAllText().Should().Be(chinese);
            }

            // UTF32 should fail
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.UTF32.GetBytes(chinese);
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadAllText().Should().NotBe(chinese);
            }
        }

        [TestMethod]
        public void ReadToEnd_should_throw_on_null_args()
        {
            Action action = () => StreamExtensions.ReadToEnd(null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("stream");
        }

        [TestMethod]
        public void ReadToEnd_should_get_all_of_the_text_when_the_stream_is_at_the_beginning()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                stream.ReadToEnd().Should().Be("Hello");
            }
        }

        [TestMethod]
        public void ReadToEnd_should_get_the_remaining_text_when_the_stream_is_not_at_the_beginning()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("First Second");
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = "First ".Length;

                stream.ReadToEnd().Should().Be("Second");
            }
        }

        [TestMethod]
        public void ReadToEnd_should_return_an_empty_string_if_already_at_the_end()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEnd().Should().BeEmpty();
            }
        }

        [TestMethod]
        public void ReadToEnd_should_not_preserve_the_position_by_default()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadToEnd();
                stream.Position.Should().Be("Hello".Length);
            }
        }

        [TestMethod]
        public void ReadToEnd_should_preserve_the_position_if_requested()
        {
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes("Hello");
                stream.Write(bytes, 0, bytes.Length);

                stream.Position = 1;
                stream.ReadAllText(preservePosition: true);
                stream.Position.Should().Be(1);
            }
        }

        [TestMethod]
        public void ReadToEnd_should_use_UTF8_encoding_by_default()
        {
            const string chinese = "中文";
            string lastChar = chinese[1].ToString();

            var utfBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var utfNoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            // use UTF8 encoding with a BOM
            using (var stream = new MemoryStream())
            {
                byte[] bytes = utfNoBom.GetBytes(chinese);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = utfNoBom.GetBytes(chinese.ToCharArray(), 0, 1).Length;

                stream.ReadToEnd().Should().Be(lastChar);
            }

            // use UTF8 encoding without a BOM
            using (var stream = new MemoryStream())
            {
                byte[] bytes = utfBom.GetBytes(chinese);
                stream.Write(utfBom.GetPreamble(), 0, utfBom.GetPreamble().Length);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = utfBom.GetBytes(chinese.ToCharArray(), 0, 1).Length + utfBom.GetPreamble().Length;

                stream.ReadToEnd().Should().Be(lastChar);
            }

            // UTF32 should fail
            using (var stream = new MemoryStream())
            {
                byte[] bytes = Encoding.UTF32.GetBytes(chinese);
                stream.Write(bytes, 0, bytes.Length);

                stream.ReadToEnd().Should().NotBe(chinese);
            }
        }
    }
}
