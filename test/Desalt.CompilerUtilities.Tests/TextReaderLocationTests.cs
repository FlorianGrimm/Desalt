// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TextReaderLocationTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities.Tests
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;

    public class TextReaderLocationTests
    {
        //// ===========================================================================================================
        //// Constructor Tests
        //// ===========================================================================================================

        [Test]
        public void TextReaderLocation_Ctor_by_range()
        {
            var location = new TextReaderLocation(21, 2, "file");
            location.Line.Should().Be(21);
            location.Column.Should().Be(2);
            location.Source.Should().Be("file");
        }

        [Test]
        public void TextReaderLocation_Ctor_only_line()
        {
            var location = new TextReaderLocation(100, 1);
            location.Line.Should().Be(100);
            location.Column.Should().Be(1);
            location.Source.Should().Be(string.Empty);
        }

        //// ===========================================================================================================
        //// ToString Tests
        //// ===========================================================================================================

        [Test]
        public void TextReaderLocation_ToString_full_range()
        {
            var location = new TextReaderLocation(21, 2, "file");
            location.ToString().Should().Be("file(21,2)");
        }

        [Test]
        public void TextReaderLocation_ToString_full_range_no_source()
        {
            var location = new TextReaderLocation(21, 2);
            location.ToString().Should().Be("(21,2)");
        }

        //// ===========================================================================================================
        //// Equality Tests
        //// ===========================================================================================================

        [Test]
        public void TextReaderLocation_Equality_should_not_equal_null()
        {
            var location = new TextReaderLocation(100, 1);
            location.Equals(null).Should().BeFalse();
        }

        [Test]
        public void TextReaderLocation_Equality_should_equal_for_the_same_instance()
        {
            var location = new TextReaderLocation(100, 1);
            location.Equals(location).Should().BeTrue();
        }

        [Test]
        public void TextReaderLocation_Equality_should_equal_for_two_different_instances_with_the_same_contents()
        {
            var location1 = new TextReaderLocation(100, 1, "source");
            var location2 = new TextReaderLocation(100, 1, "source");
            location1.Equals(location2).Should().BeTrue();
        }

        [Test]
        public void TextReaderLocation_Equality_should_not_equal_for_each_field_that_is_different()
        {
            var location = new TextReaderLocation(1, 1, "source");
            location.Equals(new TextReaderLocation(2, 1, "source")).Should().BeFalse();
            location.Equals(new TextReaderLocation(1, 2, "source")).Should().BeFalse();
            location.Equals(new TextReaderLocation(1, 1, "source1")).Should().BeFalse();
            location.Equals(new TextReaderLocation(1, 1, "Source")).Should().BeFalse();
        }

        [Test]
        public void TextReaderLocation_Equality_should_equal_using_double_equal_sign()
        {
            var location = new TextReaderLocation(100, 1);
            (location == new TextReaderLocation(100, 1)).Should().BeTrue();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            (location == null).Should().BeFalse();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            (null == location).Should().BeFalse();
        }

        [Test]
        public void TextReaderLocation_Equality_should_not_be_equal_using_bang_equal_sign()
        {
            var location = new TextReaderLocation(100, 1);
            (location != new TextReaderLocation(100, 1)).Should().BeFalse();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            (location != null).Should().BeTrue();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            (null != location).Should().BeTrue();
        }

        [Test]
        public void TextReaderLocation_Equality_should_return_the_same_hash_code_for_equal_objects()
        {
            var location1 = new TextReaderLocation(4, 5);
            var location2 = new TextReaderLocation(4, 5);
            location1.GetHashCode().Should().Be(location2.GetHashCode());
        }

        //// ===========================================================================================================
        //// IncrementLine Tests
        //// ===========================================================================================================

        [Test]
        public void
            TextReaderLocation_IncrementLine_should_return_a_new_object_with_the_line_incremented_and_the_column_at_1()
        {
            var location = new TextReaderLocation(100, 50, "source");
            TextReaderLocation increment = location.IncrementLine();
            // ReSharper disable once ReferenceEqualsWithValueType
            ReferenceEquals(location, increment).Should().BeFalse();
            increment.Line.Should().Be(101);
            increment.Column.Should().Be(1);
            increment.Source.Should().Be("source");
        }

        [Test]
        public void TextReaderLocation_IncrementLine_should_not_modify_its_own_contents()
        {
            var location = new TextReaderLocation(100, 1);
            location.IncrementLine();
            location.Line.Should().Be(100);
            location.Column.Should().Be(1);
            location.Source.Should().Be(string.Empty);
        }

        //// ===========================================================================================================
        //// IncrementColumn Tests
        //// ===========================================================================================================

        [Test]
        public void
            TextReaderLocation_IncrementColumn_should_return_a_new_object_with_the_column_incremented_and_the_line_the_same()
        {
            var location = new TextReaderLocation(100, 50, "source");
            TextReaderLocation increment = location.IncrementColumn();
            // ReSharper disable once ReferenceEqualsWithValueType
            ReferenceEquals(location, increment).Should().BeFalse();
            increment.Line.Should().Be(100);
            increment.Column.Should().Be(51);
            increment.Source.Should().Be("source");
        }

        [Test]
        public void TextReaderLocation_IncrementColumn_should_not_modify_its_own_contents()
        {
            var location = new TextReaderLocation(100, 50);
            location.IncrementColumn();
            location.Line.Should().Be(100);
            location.Column.Should().Be(50);
            location.Source.Should().Be(string.Empty);
        }

        //// ===========================================================================================================
        //// DecrementColumn Tests
        //// ===========================================================================================================

        [Test]
        public void
            TextReaderLocation_DecrementColumn_should_return_a_new_object_with_the_column_decremented_and_the_line_the_same()
        {
            var location = new TextReaderLocation(100, 50, "source");
            TextReaderLocation decrement = location.DecrementColumn();
            decrement.Line.Should().Be(100);
            decrement.Column.Should().Be(49);
            decrement.Source.Should().Be("source");
        }

        [Test]
        public void TextReaderLocation_DecrementColumn_should_throw_if_at_the_beginning_of_the_line()
        {
            var location = new TextReaderLocation(100, 1, "source");
            Action action = () => location.DecrementColumn();
            action.Should()
                .ThrowExactly<InvalidOperationException>()
                .And.Message.Should()
                .Be("Cannot decrement the column before the beginning of the line.");
        }
    }
}
