// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PeekingTextReaderTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PeekingTextReaderTests
    {
        private static void DoTest(string contents, Action<PeekingTextReader> action)
        {
            using (var reader = new PeekingTextReader(contents))
            {
                action(reader);
            }
        }

        //// ===========================================================================================================
        //// Construction/Destruction Tests
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_Ctor_should_throw_on_null_args()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new PeekingTextReader((Stream)null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("stream");

            // ReSharper disable once ObjectCreationAsStatement
            action = () => new PeekingTextReader((string)null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("contents");
        }

        [TestMethod]
        public void PeekingTextReader_Ctor_should_start_at_line_1_column_1()
        {
            var reader = new PeekingTextReader("hi");
            reader.Location.Should().Be(new TextReaderLocation(1, 1));
        }

        [TestMethod]
        public void PeekingTextReader_Ctor_should_start_at_the_initial_line_and_column()
        {
            var reader = new PeekingTextReader("hi")
            {
                Location = new TextReaderLocation(4, 5)
            };
            reader.Location.Should().Be(new TextReaderLocation(4, 5));
        }

        //// ===========================================================================================================
        //// Close Tests
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_Close_should_close_the_stream_when_disposing()
        {
            var stream = new MemoryStream();
            var reader = new PeekingTextReader(stream);
            reader.Close();

            Action action = () => stream.ReadByte();
            action.Should().ThrowExactly<ObjectDisposedException>();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void PeekingTextReader_Close_should_throw_ObjectDisposedException_when_doing_a_synchronous_operation_on_a_closed_reader()
        {
            using (var stream = new MemoryStream())
            using (var reader = new PeekingTextReader(stream))
            {
                reader.Close();

                var actions = new Dictionary<string, Action>
                {
                    { "Location", () => { var x = reader.Location; } },
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    { "Peek", () => reader.Peek() },
                    { "PeekN", () => reader.Peek(2) },
                    { "PeekLine", () => reader.PeekLine() },
                    { "PeekUntilChar", () => reader.PeekUntil('c') },
                    { "PeekUntilString", () => reader.PeekUntil("str") },
                    { "PeekUntilPredicate", () => reader.PeekUntil(c => true) },
                    { "PeekWhileChar", () => reader.PeekWhile('c') },
                    { "PeekWhileString", () => reader.PeekWhile("str") },
                    { "PeekWhilePredicate", () => reader.PeekWhile(c => true) },
                    { "Read", () => reader.Read() },
                    { "ReadN", () => reader.Read(2) },
                    { "ReadBlock", () => reader.ReadBlock(new char[1], 0, 1) },
                    { "ReadLine", () => reader.ReadLine() },
                    { "ReadToEnd", () => reader.ReadToEnd() },
                    { "ReadUntilChar", () => reader.ReadUntil('c') },
                    { "ReadUntilString", () => reader.ReadUntil("str") },
                    { "ReadUntilPredicate", () => reader.ReadUntil(c => true) },
                    { "ReadWhileChar", () => reader.ReadWhile('c') },
                    { "ReadWhileString", () => reader.ReadWhile("str") },
                    { "ReadWhilePredicate", () => reader.ReadWhile(c => true) }
                };

                foreach (KeyValuePair<string, Action> pair in actions)
                {
                    pair.Value.Should().ThrowExactly<ObjectDisposedException>(pair.Key);
                }
            }
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void
            PeekingTextReader_Close_should_throw_ObjectDisposedException_when_doing_an_asynchronous_operation_on_a_closed_reader()
        {
            using (var stream = new MemoryStream())
            using (var reader = new PeekingTextReader(stream))
            {
                reader.Close();

                Func<Task> action = async () => await reader.ReadAsync(new char[10], 0, 10);
                action.Should().ThrowExactly<ObjectDisposedException>();

                action = async () => await reader.ReadBlockAsync(new char[1], 0, 1);
                action.Should().ThrowExactly<ObjectDisposedException>();

                action = async () => await reader.ReadLineAsync();
                action.Should().ThrowExactly<ObjectDisposedException>();

                action = async () => await reader.ReadToEndAsync();
                action.Should().ThrowExactly<ObjectDisposedException>();
            }
        }

        //// ===========================================================================================================
        //// Location
        //// ===========================================================================================================

        [TestClass]
        public class PeekingTextReaderLocation
        {
            [TestMethod]
            public void PeekingTextReader_Location_should_increment_the_line_and_column_correctly()
            {
                var reader = new PeekingTextReader(new UnicodeStringStream("abc\n123"));
                var expectedLocations = new[]
                {
                    new TextReaderLocation(1, 1),
                    new TextReaderLocation(1, 2),
                    new TextReaderLocation(1, 3),
                    new TextReaderLocation(1, 4),
                    new TextReaderLocation(2, 1),
                    new TextReaderLocation(2, 2),
                    new TextReaderLocation(2, 3),
                    new TextReaderLocation(2, 4)
                };

                foreach (TextReaderLocation expectedLocation in expectedLocations)
                {
                    reader.Location.Should().Be(expectedLocation);
                    reader.Read();
                }
            }

            [TestMethod]
            public void PeekingTextReader_Location_should_increment_the_line_correctly()
            {
                var reader = new PeekingTextReader(new UnicodeStringStream("abc\n123"));
                reader.ReadLine();
                reader.Location.Should().Be(new TextReaderLocation(2, 1));
            }
        }

        //// ===========================================================================================================
        //// AdvanceLocation
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_AdvanceLocation_should_not_change_the_location_on_end_of_stream()
        {
            var current = new TextReaderLocation(10, 2);
            PeekingTextReader.AdvanceLocation(-1, -1, current).Should().Be(current);
        }

        [TestMethod]
        public void PeekingTextReader_AdvanceLocation_should_increment_the_column_on_a_non_line_ending()
        {
            PeekingTextReader.AdvanceLocation('a', 'b', new TextReaderLocation(1, 1))
                .Should()
                .Be(new TextReaderLocation(1, 2));
        }

        [TestMethod]
        public void
            PeekingTextReader_AdvanceLocation_should_increment_the_column_but_not_the_line_when_reading_the_first_char_of_a_crlf_combo()
        {
            PeekingTextReader.AdvanceLocation('\r', '\n', new TextReaderLocation(1, 1))
                .Should()
                .Be(new TextReaderLocation(1, 2));
        }

        [TestMethod]
        public void PeekingTextReader_AdvanceLocation_should_increment_the_line_when_reading_a_single_cr()
        {
            PeekingTextReader.AdvanceLocation('\r', 'a', new TextReaderLocation(1, 1))
                .Should()
                .Be(new TextReaderLocation(2, 1));
        }

        [TestMethod]
        public void PeekingTextReader_AdvanceLocation_should_increment_the_line_when_reading_a_single_lf()
        {
            PeekingTextReader.AdvanceLocation('\n', 'a', new TextReaderLocation(1, 1))
                .Should()
                .Be(new TextReaderLocation(2, 1));
        }

        //// ===========================================================================================================
        //// IsAtEnd
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_IsAtEnd_should_return_true_if_at_the_end_of_an_empty_string()
        {
            DoTest(string.Empty, reader => reader.IsAtEnd.Should().BeTrue());
        }

        [TestMethod]
        public void PeekingTextReader_IsAtEnd_should_return_false_if_not_at_the_end()
        {
            DoTest("abc", reader => reader.IsAtEnd.Should().BeFalse());
        }

        [TestMethod]
        public void PeekingTextReader_IsAtEnd_should_return_true_if_at_the_end_after_a_read()
        {
            DoTest(
                "a",
                reader =>
                {
                    reader.Read();
                    reader.IsAtEnd.Should().BeTrue();
                });
        }

        //// ===========================================================================================================
        //// Peek
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_Peek_should_return_neg1_if_at_the_end()
        {
            DoTest(string.Empty, reader => reader.Peek().Should().Be(-1));
        }

        [TestMethod]
        public void PeekingTextReader_Peek_should_be_able_to_peek_ahead_multiple_characters()
        {
            DoTest(
                "123456",
                reader =>
                {
                    reader.Peek().Should().Be('1');
                    reader.Peek(6).Should().Be("123456");
                    reader.Peek().Should().Be('1');
                });
        }

        [TestMethod]
        public void
            PeekingTextReader_Peek_should_be_able_to_return_a_partial_result_if_there_arent_enough_characters_in_the_stream()
        {
            DoTest("1234", reader => reader.Peek(10).Should().Be("1234"));
        }

        [TestMethod]
        public void PeekingTextReader_Peek_should_return_null_if_at_the_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.Peek(2).Should().BeNull());
        }

        [TestMethod]
        public void PeekingTextReader_Peek_should_be_able_to_peek_ahead_only_enough_to_fill_the_buffer()
        {
            DoTest(
                "1234",
                reader =>
                {
                    reader.Peek(1).Should().Be("1");
                    reader.Peek(5).Should().Be("1234");
                });
        }

        [TestMethod]
        public void PeekingTextReader_Peek_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.Peek(2);
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekUntil
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekUntil_should_be_able_to_peek_ahead_until_a_single_character_is_seen()
        {
            DoTest("foo()", reader => reader.PeekUntil('(').Should().Be("foo"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntil_should_be_able_to_peek_ahead_until_one_of_a_few_characters_is_seen()
        {
            DoTest("foo(1,2)", reader => reader.PeekUntil(',', ')').Should().Be("foo(1"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntil_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekUntil('*').Should().BeNull());
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntil_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.PeekUntil('(');
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekUntil(string)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_throw_if_the_string_is_null()
        {
            DoTest(
                "Hi",
                reader =>
                {
                    Action action = () => reader.PeekUntil((string)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("find");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_continue_peeking_until_the_find_string_is_hit()
        {
            DoTest(
                "/*comment*/",
                reader =>
                {
                    reader.PeekUntil("*/").Should().Be("/*comment");
                    reader.Read(1).Should().Be("/");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_continue_to_peek_even_if_part_of_the_string_is_hit()
        {
            DoTest("partial", reader => reader.PeekUntil("arty").Should().Be("partial"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_continue_peeking_until_the_end_of_stream_is_hit()
        {
            DoTest("/*comment", reader => reader.PeekUntil("*/").Should().Be("/*comment"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekUntil("Chum bucket").Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_PeekUntilString_should_return_an_empty_string_if_there_are_still_things_to_read_but_the_find_string_was_first()
        {
            DoTest("abc123", reader => reader.PeekUntil("abc").Should().Be(string.Empty));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilString_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.PeekUntil("bar");
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekUntil(predicate)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_throw_if_the_predicate_is_null()
        {
            DoTest(
                "abc",
                reader =>
                {
                    Action action = () => reader.PeekUntil((Func<char, bool>)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("predicate");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_peek_until_the_predicate_returns_true()
        {
            DoTest("abcd efg", reader => reader.PeekUntil(char.IsWhiteSpace).Should().Be("abcd"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_peek_but_not_remove_characters_from_the_read_stream()
        {
            DoTest(
                "abcd",
                reader =>
                {
                    reader.PeekUntil(c => c == 'd');
                    reader.Read().Should().Be('a');
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_peek_until_the_end_of_stream_is_hit()
        {
            DoTest("abc", reader => reader.PeekUntil(char.IsWhiteSpace).Should().Be("abc"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekUntil(c => false).Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_PeekUntilPredicate_should_return_an_empty_string_if_the_predicate_returns_true_on_the_first_character()
        {
            DoTest("abc", reader => reader.PeekUntil(c => c == 'a').Should().Be(string.Empty));
        }

        [TestMethod]
        public void PeekingTextReader_PeekUntilPredicate_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.PeekUntil(char.IsPunctuation);
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekWhile(char, char[])
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekWhile_should_be_able_to_peek_ahead_until_a_single_character_is_seen()
        {
            DoTest("foo()", reader => reader.PeekWhile('f').Should().Be("f"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhile_should_be_able_to_peek_ahead_until_one_of_a_few_characters_is_seen()
        {
            DoTest("foo(1,2)", reader => reader.PeekWhile('f', 'o').Should().Be("foo"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhile_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekWhile('*').Should().BeNull());
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhile_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.PeekWhile('f', 'o');
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekWhile(string)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_throw_if_the_string_is_null()
        {
            DoTest(
                "abc",
                reader =>
                {
                    Action action = () => reader.PeekWhile((string)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("find");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_peek_while_the_string_is_found()
        {
            DoTest("PeePeePoop", reader => reader.PeekWhile("Pee").Should().Be("PeePee"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_peek_but_not_remove_characters_from_the_read_stream()
        {
            DoTest(
                "ababc",
                reader =>
                {
                    reader.PeekWhile("ab");
                    reader.Read(1).Should().Be("a");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_peek_until_the_end_of_stream_is_hit()
        {
            DoTest("aaa", reader => reader.PeekWhile("a").Should().Be("aaa"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekWhile("Hi").Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_PeekWhileString_should_return_an_empty_string_if_the_predicate_returns_false_on_the_first_character()
        {
            DoTest("aaa", reader => reader.PeekWhile("b").Should().Be(string.Empty));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhileString_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "ooohaaah",
                reader =>
                {
                    reader.PeekWhile("ooo");
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekWhile(predicate)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_throw_if_the_predicate_is_null()
        {
            DoTest(
                "abc",
                reader =>
                {
                    Action action = () => reader.PeekWhile((Func<char, bool>)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("predicate");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_peek_until_the_predicate_returns_false()
        {
            DoTest("abcd efg", reader => reader.PeekWhile(c => !char.IsWhiteSpace(c)).Should().Be("abcd"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_peek_but_not_remove_characters_from_the_read_stream()
        {
            DoTest(
                "abcd",
                reader =>
                {
                    reader.PeekWhile(c => c == 'a' || c == 'b');
                    reader.Read().Should().Be('a');
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_peek_until_the_end_of_stream_is_hit()
        {
            DoTest("abc", reader => reader.PeekWhile(c => !char.IsWhiteSpace(c)).Should().Be("abc"));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekWhile(c => true).Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_PeekWhilePredicate_should_return_an_empty_string_if_the_predicate_returns_false_on_the_first_character()
        {
            DoTest("abc", reader => reader.PeekWhile(c => c != 'a').Should().Be(string.Empty));
        }

        [TestMethod]
        public void PeekingTextReader_PeekWhilePredicate_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.PeekWhile(c => true);
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// PeekLine
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_PeekLine_should_be_able_to_peek_ahead_one_line()
        {
            DoTest(
                "one\ntwo",
                reader =>
                {
                    reader.PeekLine().Should().Be("one");
                    reader.ReadLine().Should().Be("one");
                    reader.Read().Should().Be('t');
                    reader.PeekLine().Should().Be("wo");
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekLine_should_return_an_empty_string_if_its_a_blank_line()
        {
            DoTest(
                "one\n\ntwo",
                reader =>
                {
                    reader.ReadLine().Should().Be("one");
                    reader.PeekLine().Should().Be(string.Empty);
                    reader.ReadLine().Should().Be(string.Empty);
                    reader.IsAtEnd.Should().BeFalse();
                });
        }

        [TestMethod]
        public void PeekingTextReader_PeekLine_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.PeekLine().Should().BeNull());
        }

        [TestMethod]
        public void PeekingTextReader_PeekLine_should_not_change_the_location_when_peeking()
        {
            DoTest(
                "line1\nline2",
                reader =>
                {
                    reader.PeekLine();
                    reader.Location.Should().Be(new TextReaderLocation(1, 1));
                });
        }

        //// ===========================================================================================================
        //// Read
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_Read_should_read_from_the_buffer_first_if_there_was_a_peek()
        {
            DoTest(
                "1234",
                reader =>
                {
                    reader.Peek(2);
                    reader.Read().Should().Be('1');
                    reader.Read(4).Should().Be("234");
                });
        }

        [TestMethod]
        public void PeekingTextReader_Read_should_not_strip_line_characters()
        {
            const string contents = "a\nb\rc\r\nd\n";
            DoTest(
                contents,
                reader =>
                {
                    foreach (char c in contents)
                    {
                        reader.Read().Should().Be(c);
                    }

                    reader.Read().Should().Be(-1);
                });
        }

        [TestMethod]
        public void PeekingTextReader_Read_should_return_neg1_if_at_the_end()
        {
            DoTest(string.Empty, reader => reader.Read().Should().Be(-1));
        }

        [TestMethod]
        public void PeekingTextReader_Read_should_be_able_to_read_ahead_multiple_characters()
        {
            const string contents = "123456";
            DoTest(contents, reader => reader.Read(6).Should().Be(contents));
        }

        [TestMethod]
        public void
            PeekingTextReader_Read_should_be_able_to_return_a_partial_result_if_there_arent_enough_characters_in_the_stream()
        {
            DoTest("1234", reader => reader.Read(10).Should().Be("1234"));
        }

        [TestMethod]
        public void PeekingTextReader_Read_should_return_null_if_at_the_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.Read(2).Should().BeNull());
        }

        //// ===========================================================================================================
        //// ReadUntil(char, params char[])
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_ReadUntilChars_should_be_able_to_read_ahead_until_a_single_character_is_seen()
        {
            DoTest(
                "foo()",
                reader =>
                {
                    reader.ReadUntil('(').Should().Be("foo");
                    reader.Location.Should().Be(new TextReaderLocation(1, 4));
                });
        }

        [TestMethod]
        public void
            PeekingTextReader_ReadUntilChars_should_be_able_to_read_ahead_until_one_of_a_few_characters_is_seen()
        {
            DoTest(
                "foo(1,2)",
                reader =>
                {
                    reader.ReadUntil(',', ')').Should().Be("foo(1");
                    reader.Read();
                    reader.ReadUntil(',', ')').Should().Be("2");
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilChars_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.ReadUntil('*').Should().BeNull());
        }

        //// ===========================================================================================================
        //// ReadUntil(string)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_ReadUntilString_should_throw_if_the_string_is_null()
        {
            DoTest(
                "Hi",
                reader =>
                {
                    Action action = () => reader.ReadUntil((string)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("find");
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilString_should_continue_reading_until_the_find_string_is_hit()
        {
            DoTest(
                "/*comment*/",
                reader =>
                {
                    reader.ReadUntil("*/").Should().Be("/*comment");
                    reader.Read().Should().Be('*');
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilString_should_continue_to_read_even_if_part_of_the_string_is_hit()
        {
            DoTest("partial", reader => reader.ReadUntil("arty").Should().Be("partial"));
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilString_should_continue_reading_until_the_end_of_stream_is_hit()
        {
            DoTest("/*comment", reader => reader.ReadUntil("*/").Should().Be("/*comment"));
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilString_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.ReadUntil("Chum bucket").Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_ReadUntilString_should_return_an_empty_string_if_there_are_still_things_to_read_but_the_find_string_was_first()
        {
            DoTest("abc123", reader => reader.ReadUntil("abc").Should().Be(string.Empty));
        }

        //// ===========================================================================================================
        //// ReadUntil(predicate)
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_ReadUntilPredicate_should_throw_if_the_predicate_is_null()
        {
            DoTest(
                "abc",
                reader =>
                {
                    Action action = () => reader.ReadUntil((Func<char, bool>)null);
                    action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("predicate");
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilPredicate_should_read_until_the_predicate_returns_true()
        {
            DoTest(
                "abcd efg",
                reader =>
                {
                    reader.ReadUntil(char.IsWhiteSpace).Should().Be("abcd");
                    reader.Read(1).Should().Be(" ");
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilPredicate_should_read_until_the_end_of_stream_is_hit()
        {
            DoTest("abc", reader => reader.ReadUntil(char.IsWhiteSpace).Should().Be("abc"));
        }

        [TestMethod]
        public void PeekingTextReader_ReadUntilPredicate_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.ReadUntil(c => false).Should().BeNull());
        }

        [TestMethod]
        public void
            PeekingTextReader_ReadUntilPredicate_should_return_an_empty_string_if_the_predicate_returns_true_on_the_first_character()
        {
            DoTest("abc", reader => reader.ReadUntil(c => c == 'a').Should().Be(string.Empty));
        }

        //// ===========================================================================================================
        //// ReadWhile(char, char[])
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_ReadWhile_should_be_able_to_read_ahead_while_a_single_character_is_seen()
        {
            DoTest(
                "aaabbb",
                reader =>
                {
                    reader.ReadWhile('a').Should().Be("aaa");
                    reader.Read(1).Should().Be("b");
                });
        }

        [TestMethod]
        public void PeekingTextReader_ReadWhile_should_be_able_to_read_ahead_while_one_of_a_few_characters_is_seen()
        {
            DoTest("foo(1,2)", reader => reader.ReadWhile('f', 'o').Should().Be("foo"));
        }

        [TestMethod]
        public void PeekingTextReader_ReadWhile_should_return_null_if_at_end_of_stream()
        {
            DoTest(string.Empty, reader => reader.ReadWhile('*').Should().BeNull());
        }

        [TestMethod]
        public void PeekingTextReader_ReadWhile_should_change_the_location_when_reading()
        {
            DoTest(
                "foo(bar)",
                reader =>
                {
                    reader.ReadWhile('f', 'o');
                    reader.Location.Should().Be(new TextReaderLocation(1, 4));
                });
        }

        //// ===========================================================================================================
        //// SkipWhitespace
        //// ===========================================================================================================

        [TestMethod]
        public void PeekingTextReader_SkipWhitespace_should_skip_spaces_tabs_and_other_Unicode_whitespace_characters()
        {
            // http://www.fileformat.info/info/unicode/category/Zs/list.htm
            char[] unicodeSpaceChars =
            {
                '\x00A0', // NO-BREAK SPACE
                '\x1680', // OGHAM SPACE MARK
                '\x2000', // EN QUAD
                '\x2001', // EM QUAD
                '\x2002', // EN SPACE
                '\x2003', // EM SPACE
                '\x2004', // THREE-PER-EM SPACE
                '\x2005', // FOUR-PER-EM SPACE
                '\x2006', // SIX-PER-EM SPACE
                '\x2007', // FIGURE SPACE
                '\x2008', // PUNCTUATION SPACE
                '\x2009', // THIN SPACE
                '\x200A', // HAIR SPACE
                '\x202F', // NARROW NO-BREAK SPACE
                '\x205F', // MEDIUM MATHEMATICAL SPACE
                '\x3000' // IDEOGRAPHIC SPACE
            };
            string unicodeSpaces = new string(unicodeSpaceChars);

            DoTest(
                unicodeSpaces + " \t\v\f Nonspace",
                reader =>
                {
                    reader.SkipWhitespace();
                    reader.ReadToEnd().Should().Be("Nonspace");
                });
        }

        [TestMethod]
        public void PeekingTextReader_SkipWhitespace_should_skip_lines_if_requested()
        {
            // http://www.fileformat.info/info/unicode/category/Zl/list.htm
            DoTest(
                "\r\n\x20282nd line",
                reader =>
                {
                    reader.SkipWhitespace(includeLineFeeds: true);
                    reader.Location.Line.Should().Be(2);
                    reader.ReadToEnd().Should().Be("2nd line");
                });
        }
    }
}
