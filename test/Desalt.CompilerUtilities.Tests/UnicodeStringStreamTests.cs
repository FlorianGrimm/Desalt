// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="UnicodeStringStreamTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class UnicodeStringStreamTests
    {
        [Fact]
        public void Ctor_should_throw_on_null_args()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action action = () => new UnicodeStringStream(null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("contents");
        }

        [Fact]
        public void Writing_or_Flushing_should_throw_NotSupportedException()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                (string MethodName, Action Action)[] actions = new(string MethodName, Action Action)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("Flush", () => stream.Flush()),
                    ("SetLength", () => stream.SetLength(0)),
                    ("Write", () => stream.Write(new byte[1], 0, 1)),
                    ("WriteByte", () => stream.WriteByte(0)),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Action Action) tuple in actions)
                {
                    tuple.Action.Should().Throw<NotSupportedException>(tuple.MethodName);
                }
            }
        }

        [Fact]
        public async Task An_asynchronous_write_or_flush_operation_should_throw_NotSupportedException()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                stream.Dispose();

                (string MethodName, Func<Task> Function)[] actions = new(string MethodName, Func<Task> Function)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("FlushAsync", () => stream.FlushAsync()),
                    ("FlushAsync(CancellationToken)", () => stream.FlushAsync(CancellationToken.None)),
                    ("WriteAsync", () => stream.WriteAsync(new byte[1], 0, 1)),
                    (
                        "WriteAsync(CancellationToken)",
                        () => stream.WriteAsync(new byte[1], 0, 1, CancellationToken.None)
                    ),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Func<Task> Function) tuple in actions)
                {
                    tuple.Function.Should().Throw<NotSupportedException>(tuple.MethodName);
                }
            }

            await Task.Yield();
        }

        [Fact]
        public void A_synchronous_operation_on_a_closed_reader_should_throw_ObjectDisposedException()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                stream.Dispose();

                // ReSharper disable once NotAccessedVariable
                long dummyLong;
                (string MethodName, Action Action)[] actions = new(string MethodName, Action Action)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("CopyTo(Stream)", () => stream.CopyTo(new MemoryStream())),
                    ("CopyTo(Stream, int)", () => stream.CopyTo(new MemoryStream(), 10)),
                    ("Length", () => dummyLong = stream.Length),
                    ("Position-Get", () => dummyLong = stream.Position),
                    ("Position-Set", () => stream.Position = 100),
                    ("Read", () => stream.Read(new byte[1], 0, 1)),
                    ("ReadByte", () => stream.ReadByte()),
                    ("Seek", () => stream.Seek(0, SeekOrigin.Begin)),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Action Action) tuple in actions)
                {
                    tuple.Action.Should().Throw<ObjectDisposedException>(tuple.MethodName);
                }
            }
        }

        [Fact]
        public async Task An_asynchronous_operation_on_a_closed_reader_should_throw_ObjectDisposedException()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                stream.Dispose();

                (string MethodName, Func<Task> Function)[] actions = new(string MethodName, Func<Task> Function)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("CopyToAsync(Stream)", () => stream.CopyToAsync(new MemoryStream())),
                    ("CopyToAsync(Stream, int)", () => stream.CopyToAsync(new MemoryStream(), 10)),
                    (
                        "CopyToAsync(Stream, int, CancellationToken)",
                        () => stream.CopyToAsync(new MemoryStream(), 10, CancellationToken.None)
                    ),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Func<Task> Function) tuple in actions)
                {
                    tuple.Function.Should().Throw<ObjectDisposedException>(tuple.MethodName);
                }
            }

            await Task.Yield();
        }

        [Fact]
        public async Task An_asynchronous_read_operation_on_a_closed_reader_should_throw_NotSupportedException()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                stream.Dispose();

                (string MethodName, Func<Task> Function)[] actions = new(string MethodName, Func<Task> Function)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("ReadAsync", () => stream.ReadAsync(new byte[1], 0, 1)),
                    (
                        "ReadAsync(CancellationToken)",
                        () => stream.ReadAsync(new byte[1], 0, 1, CancellationToken.None)
                    ),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Func<Task> Function) tuple in actions)
                {
                    tuple.Function.Should().Throw<NotSupportedException>(tuple.MethodName);
                }
            }

            await Task.Yield();
        }

        [Fact]
        public void CanX_properties_when_closed_should_return_false()
        {
            using (UnicodeStringStream stream = new UnicodeStringStream("Sample"))
            {
                stream.Dispose();

                (string MethodName, Func<bool> Function)[] actions = new(string MethodName, Func<bool> Function)[]
                {
                    // ReSharper disable AccessToDisposedClosure
                    ("CanRead", () => stream.CanRead),
                    ("CanSeek", () => stream.CanSeek),
                    ("CanWrite", () => stream.CanWrite),
                    // ReSharper restore AccessToDisposedClosure
                };

                foreach ((string MethodName, Func<bool> Function) tuple in actions)
                {
                    tuple.Function().Should().BeFalse(tuple.MethodName);
                }
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public class An_empty_string_with_no_bom
    {
        private readonly UnicodeStringStream _stream = new UnicodeStringStream(string.Empty, true);

        [Fact]
        public void Should_return_a_zero_length()
        {
            _stream.Length.Should().Be(0);
        }

        [Fact]
        public void Should_support_reading()
        {
            _stream.CanRead.Should().BeTrue();
        }

        [Fact]
        public void Should_support_seeking()
        {
            _stream.CanSeek.Should().BeTrue();
        }

        [Fact]
        public void Should_not_support_writing()
        {
            _stream.CanWrite.Should().BeFalse();
            // ReSharper disable once AssignNullToNotNullAttribute
            new Action(() => _stream.Write(null, 0, 0)).Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void Should_not_support_flusing()
        {
            new Action(() => _stream.Flush()).Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void Should_not_support_setting_the_length()
        {
            new Action(() => _stream.SetLength(10)).Should().Throw<NotSupportedException>();
        }
    }

    // ReSharper disable once InconsistentNaming
    public class An_empty_string_with_bom
    {
        private readonly UnicodeStringStream _stream = new UnicodeStringStream(string.Empty);
        private readonly byte[] _preamble = Encoding.Unicode.GetPreamble();

        [Fact]
        public void Should_return_the_same_length_as_the_unicode_preamble()
        {
            _stream.Length.Should().Be(Encoding.Unicode.GetPreamble().Length);
        }

        [Fact]
        public void Should_only_return_the_unicode_preamble()
        {
            byte[] buffer = new byte[_preamble.Length];
            _stream.Read(buffer, 0, buffer.Length);
            buffer.Should().ContainInOrder(_preamble);
        }
    }

    // ReSharper disable once InconsistentNaming
    public class A_valid_string_with_no_bom
    {
        private readonly UnicodeStringStream _stream = new UnicodeStringStream("123456789", true);

        [Fact]
        public void Should_read_the_entire_string_in_order()
        {
            byte[] buffer = new byte[18];
            _stream.Read(buffer, 0, buffer.Length);
            buffer.Should().ContainInOrder(Encoding.Unicode.GetBytes(_stream.Source));
        }

        [Fact]
        public void Should_only_read_the_remaining_bytes_even_if_more_are_requested()
        {
            byte[] buffer = new byte[20];
            _stream.Read(buffer, 0, buffer.Length);
            buffer.Should().ContainInOrder(Encoding.Unicode.GetBytes(_stream.Source + "\0"));
        }

        [Fact]
        public void Should_write_to_the_correct_place_in_the_buffer()
        {
            byte[] buffer = new byte[22];
            _stream.Read(buffer, 2, buffer.Length - 4);
            buffer.Should().ContainInOrder(Encoding.Unicode.GetBytes("\0" + _stream.Source + "\0"));
        }

        [Fact]
        public void Should_be_able_to_seek_from_a_starting_position()
        {
            _stream.Seek(2, SeekOrigin.Begin);
            _stream.Position.Should().Be(2);
            _stream.ReadByte().Should().Be(Convert.ToByte('2'));
        }

        [Fact]
        public void Should_be_able_to_seek_from_an_ending_position()
        {
            _stream.Seek(-2, SeekOrigin.End);
            _stream.Position.Should().Be(16);
            _stream.ReadByte().Should().Be(Convert.ToByte('9'));
        }

        [Fact]
        public void Should_be_able_to_seek_from_the_current_position()
        {
            _stream.Seek(14, SeekOrigin.Current);
            _stream.Position.Should().Be(14);
            _stream.ReadByte().Should().Be(Convert.ToByte('8'));
        }
    }
}
