// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyClassTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using FluentAssertions;
    using Xunit;

    public class DummyClassTests
    {
        private readonly DummyClass _dummyClass = new DummyClass();

        [Fact]
        public void Add_should_add_two_numbers()
        {
            _dummyClass.Add(1, 4).Should().Be(5);
        }
    }
}
