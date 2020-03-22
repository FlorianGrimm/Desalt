// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliAppTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pastel;

    public class CliAppTests
    {
        private static readonly string[] s_logo =
        {
            "Desalt C# to TypeScript Compiler version 1.0.0-alpha",
            "Copyright (C) Justin Rockwood. All rights reserved."
        };

        private static async Task VerifyOutput(
            IEnumerable<string> args,
            string expectedOutput,
            int expectedReturnValue = 0,
            bool skipLogo = false)
        {
            await VerifyOutput(args, expectedOutput.Replace("\r\n", "\n").Split('\n'), expectedReturnValue, skipLogo);
        }

        private static async Task VerifyOutput(
            IEnumerable<string> args,
            IEnumerable<string> expectedOutputLines,
            int expectedReturnValue = 0,
            bool skipLogo = false)
        {
            await using var writer = new StringWriter { NewLine = "\n" };
            var app = new CliApp(writer);
            int returnValue = await app.RunAsync(args);
            returnValue.Should().Be(expectedReturnValue);

            string actualOutput = writer.ToString();
            string expectedOutput = string.Join(
                '\n',
                skipLogo ? expectedOutputLines : s_logo.Concat(expectedOutputLines));
            expectedOutput += '\n';

            actualOutput.Should().Be(expectedOutput);
        }

        [Test]
        public async Task RunAsync_should_print_the_version_if_requested()
        {
            await VerifyOutput(new[] { "--version" }, "1.0.0-alpha", skipLogo: true);
        }

        [Test]
        public async Task RunAsync_should_suppress_the_logo_if_requested()
        {
            await VerifyOutput(new[] { "--help", "--nologo" }, CliOptions.HelpText, skipLogo: true);
        }

        [Test]
        public async Task RunAsync_should_print_help_when_requested()
        {
            await VerifyOutput(new[] { "--help" }, CliOptions.HelpText);
        }

        [Test]
        public async Task RunAsync_should_output_any_errors_relating_to_CLI_arguments()
        {
            await VerifyOutput(
                new[] { "--project" },
                "error DSC1020: Missing file specification for '--project' option.".Pastel(Color.Red),
                1);

            await VerifyOutput(
                Array.Empty<string>(),
                "error DSC1023: Missing required option '--project'.".Pastel(Color.Red),
                1);
        }
    }
}
