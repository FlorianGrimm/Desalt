// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TemporaryVariableAllocatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using Desalt.Core.Translation;
    using FluentAssertions;
    using NUnit.Framework;

    public class TemporaryVariableAllocatorTests
    {
        [Test]
        public void Reserve_should_keep_incrementing_values()
        {
            var allocator = new TemporaryVariableAllocator();
            allocator.Reserve("test").Should().Be("test1");
            allocator.Reserve("test").Should().Be("test2");
            allocator.Reserve("test").Should().Be("test3");
        }

        [Test]
        public void Reserve_should_keep_track_of_different_prefixes()
        {
            var allocator = new TemporaryVariableAllocator();
            allocator.Reserve("a").Should().Be("a1");
            allocator.Reserve("b").Should().Be("b1");
            allocator.Reserve("a").Should().Be("a2");
        }

        [Test]
        public void Reserve_should_return_prefixes()
        {
            var allocator = new TemporaryVariableAllocator();
            allocator.Reserve("a").Should().Be("a1");
            allocator.Reserve("a").Should().Be("a2");
            allocator.Reserve("a").Should().Be("a3");

            allocator.Return("a2");

            allocator.Reserve("a").Should().Be("a4");

            allocator.Return("a4");
            allocator.Return("a3");

            allocator.Reserve("a").Should().Be("a2");
        }
    }
}
