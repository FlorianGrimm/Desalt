// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgReaderTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public class ArgReaderTests
    {
        [Test]
        public void Peek_should_return_null_for_empty_args()
        {
            var reader = new ArgReader(Enumerable.Empty<string>());
            reader.Peek().Should().BeNull();
        }

        [Test]
        public void Peek_should_return_the_next_arg()
        {
            var reader = new ArgReader(new[] { "--arg" });
            reader.Peek().Should().Be("--arg");
            reader.IsAtEnd.Should().BeFalse();
        }

        [Test]
        public void Peek_and_Read_should_unescape_the_arguments()
        {
            var reader = new ArgReader(new[] { @"""quote \"" and \\ backslash""" });
            reader.Peek().Should().Be("quote \" and \\ backslash");
            reader.Peek().Should().Be(reader.Read());
            reader.IsAtEnd.Should().BeTrue();
        }

        //// ===========================================================================================================
        //// Response Files Tests
        //// ===========================================================================================================

        [Test]
        public void Peek_and_Read_should_support_response_files()
        {
            string[] responseFileContents = { "--arg1", "value1", "--arg2 value2-1 value2-2" };
            var fakeFileFetcher = new FakeFileContentFetcher("response.rsp", responseFileContents);

            var reader = new ArgReader(new[] { "@response.rsp", "--arg3", "value3" }, fakeFileFetcher);
            reader.Read().Should().Be("--arg1");
            reader.Read().Should().Be("value1");
            reader.Read().Should().Be("--arg2");
            reader.Read().Should().Be("value2-1");
            reader.Read().Should().Be("value2-2");
            reader.Read().Should().Be("--arg3");
            reader.Read().Should().Be("value3");
            reader.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Peek_and_Read_should_allow_recursive_response_file_processing()
        {
            var fakeFileFetcher = new FakeFileContentFetcher("resp1", "@resp2");
            fakeFileFetcher.Files.Add("resp2", "--project");

            var reader = new ArgReader(new[] { "@resp1" }, fakeFileFetcher);
            reader.Read().Should().Be("--project");
            reader.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Peek_and_Read_should_accept_an_empty_response_file()
        {
            var fakeFileFetcher = new FakeFileContentFetcher("resp", string.Empty);
            var reader = new ArgReader(new[] { "@resp" }, fakeFileFetcher);
            reader.Read().Should().BeNull();
            reader.IsAtEnd.Should().BeTrue();
        }

        [Test]
        public void Peek_and_Read_should_log_an_error_when_trying_to_open_a_response_file()
        {
            var fakeFileFetcher = new FakeFileContentFetcher();
            var diagnostics = new List<Diagnostic>();
            var reader = new ArgReader(new[] { "@resp" }, fakeFileFetcher, diagnostics);
            reader.Read().Should().BeNull();
            reader.IsAtEnd.Should().BeTrue();
            diagnostics.Should().HaveCount(1).And.BeEquivalentTo(DiagnosticFactory.ErrorOpeningResponseFile("resp"));
        }
    }
}
