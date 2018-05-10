// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PathUtilTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Utility
{
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PathUtilTests
    {
        [TestMethod]
        public void ReplaceExtension_should_replace_the_extension_on_an_absolute_path()
        {
            PathUtil.ReplaceExtension(@"C:\Dir\File.cs", ".ts").Should().Be(@"C:\Dir\File.ts");
        }

        [TestMethod]
        public void ReplaceExtension_should_replace_the_extension_with_or_without_a_leading_period()
        {
            PathUtil.ReplaceExtension(@"C:\Dir\File.cs", ".ts").Should().Be(@"C:\Dir\File.ts");
            PathUtil.ReplaceExtension(@"C:\Dir\File.cs", "ts").Should().Be(@"C:\Dir\File.ts");
        }

        [TestMethod]
        public void ReplaceExtension_should_replace_the_extension_on_a_relative_path()
        {
            PathUtil.ReplaceExtension(@"..\File.cs", "ts").Should().Be(@"..\File.ts");
        }

        [TestMethod]
        public void ReplaceExtension_should_add_the_extension_if_there_isnt_one_already()
        {
            PathUtil.ReplaceExtension("File", ".cs").Should().Be("File.cs");
        }

        [TestMethod]
        public void ReplaceExtension_should_remove_the_extension_if_requested()
        {
            PathUtil.ReplaceExtension(@"Dir\File.cs", "").Should().Be(@"Dir\File");
        }

        [TestMethod]
        public void ReplaceExtension_should_simply_add_the_extension_if_the_filePath_is_null_or_empty()
        {
            PathUtil.ReplaceExtension(null, "ts").Should().Be(".ts");
            PathUtil.ReplaceExtension(string.Empty, "ts").Should().Be(".ts");
            PathUtil.ReplaceExtension(string.Empty, string.Empty).Should().BeEmpty();
        }
    }
}
