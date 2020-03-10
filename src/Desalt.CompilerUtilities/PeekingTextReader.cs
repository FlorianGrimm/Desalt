// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PeekingTextReader.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Desalt.CompilerUtilities.Extensions;

    /// <summary>
    /// Represents a <see cref="TextReader"/> optimized for peeking ahead more than one character.
    /// Also tracks its current location.
    /// </summary>
    public class PeekingTextReader : TextReader
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private StreamReader _internalReader;
        private Queue<char> _buffer = new Queue<char>();
        private TextReaderLocation _location;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public PeekingTextReader(string contents, string contentsName = null)
            : this(new UnicodeStringStream(contents), contentsName)
        {
            Param.VerifyNotNull(contents, "contents");
        }

        public PeekingTextReader(Stream stream, string streamName = null)
        {
            Param.VerifyNotNull(stream, "stream");
            _internalReader = new StreamReader(stream);
            _location = new TextReaderLocation(1, 1, streamName);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsAtEnd => Peek() == -1;

        public TextReaderLocation Location
        {
            get
            {
                ThrowIfDisposed();
                return _location;
            }

            set
            {
                ThrowIfDisposed();
                _location = value;
            }
        }

        private IEnumerable<char> Peeker
        {
            get
            {
                ThrowIfDisposed();

                foreach (char c in _buffer)
                {
                    yield return c;
                }

                int readChar;
                while ((readChar = _internalReader.Read()) != -1)
                {
                    char c = (char)readChar;
                    _buffer.Enqueue(c);
                    yield return c;
                }
            }
        }

        private IEnumerable<char> Reader
        {
            get
            {
                ThrowIfDisposed();

                while (_buffer.Count > 0)
                {
                    char c = _buffer.Dequeue();
                    int nextC = _buffer.Count > 0 ? _buffer.Peek() : _internalReader.Peek();
                    _location = AdvanceLocation(c, nextC, _location);
                    yield return c;
                }

                int readChar;
                while ((readChar = _internalReader.Read()) != -1)
                {
                    int nextC = _internalReader.Peek();
                    _location = AdvanceLocation(readChar, nextC, _location);
                    yield return (char)readChar;
                }
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns whether the specified character is a whitespace character, which are any
        /// characters with Unicode class <c>Zs</c>, horizontal tab characters '\t', vertical tab
        /// characters '\v', form feed characters '\f', or line feeds (any character with the
        /// Unicode class <c>Zl</c>.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <returns>true if the character is whitespace; otherwise, false.</returns>
        public static bool IsWhitespace(char c)
        {
            return IsWhitespace(c, includeLineFeeds: true);
        }

        /// <summary>
        /// Returns whether the specified character is a whitespace character, which are any
        /// characters with Unicode class <c>Zs</c>, horizontal tab characters '\t', vertical tab
        /// characters '\v', or form feed characters '\f'. Line feeds, which are any character with
        /// the Unicode class <c>Zl</c>, are not included.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <returns>true if the character is whitespace; otherwise, false.</returns>
        public static bool IsWhitespaceNoLineFeeds(char c)
        {
            return IsWhitespace(c, includeLineFeeds: false);
        }

        public override int Peek()
        {
            string peekString = Peek(1);
            if (peekString == null)
            {
                return -1;
            }

            return peekString[0];
        }

        public string Peek(int count)
        {
            char[] chars = Peeker.Take(count).ToArray();
            if (chars.Length == 0)
            {
                return null;
            }

            return new string(chars);
        }

        public string PeekLine()
        {
            return PeekUntil('\n', '\r');
        }

        public override int Read()
        {
            string readStr = Read(1);
            if (readStr == null)
            {
                return -1;
            }

            return readStr[0];
        }

        public string Read(int count)
        {
            char[] chars = Reader.Take(count).ToArray();
            if (chars.Length == 0)
            {
                return null;
            }

            return new string(chars);
        }

        /// <summary>
        /// Reads until a whitespace character, which are any characters with Unicode class
        /// <c>Zs</c>, horizontal tab characters '\t', vertical tab characters '\v', form feed
        /// characters '\f', or line feeds (any character with the Unicode class <c>Zl</c>.
        /// </summary>
        /// <returns>Everything read up to, but not including the first whitespace character.</returns>
        public string ReadUntilWhitespace()
        {
            return ReadUntil(IsWhitespace);
        }

        /// <summary>
        /// Skips over all whitespace characters, which are any characters with Unicode class
        /// <c>Zs</c>, horizontal tab characters '\t', vertical tab characters '\v', and form feed
        /// characters '\f'.
        /// </summary>
        /// <param name="includeLineFeeds">
        /// Whether to include line feeds, which are any characters with Unicode class <c>Zl</c>.
        /// </param>
        public void SkipWhitespace(bool includeLineFeeds = false)
        {
            ReadWhile(IsWhitespace);
        }

        //// ===========================================================================================================
        //// Peek/Read(char, chars)
        //// ===========================================================================================================

        /// <summary>
        /// Peeks until one of the specified characters is found.
        /// </summary>
        /// <param name="findChar">A character which will stop the peeking.</param>
        /// <param name="findChars">Other characters which will stop the peeking.</param>
        /// <returns>
        /// The peeked characters, up to but not including the character that was found. If at the
        /// end of the stream, null is returned.
        /// </returns>
        public string PeekUntil(char findChar, params char[] findChars)
        {
            return PeekChars(until: true, findChar: findChar, findChars: findChars);
        }

        /// <summary>
        /// Peeks while one of the specified characters is found. The next read after this method
        /// returns will be the character that was not found.
        /// </summary>
        /// <param name="findChar">A character which will continue the reading.</param>
        /// <param name="findChars">Other characters which will continue the reading.</param>
        /// <returns>
        /// The read characters, up to but not including the character that was not found. If at the
        /// end of the stream, null is returned.
        /// </returns>
        public string PeekWhile(char findChar, params char[] findChars)
        {
            return PeekChars(until: false, findChar: findChar, findChars: findChars);
        }

        /// <summary>
        /// Reads until one of the specified characters is found. The next read after this method
        /// returns will be the character that was found.
        /// </summary>
        /// <param name="findChar">A character which will stop the reading.</param>
        /// <param name="findChars">Other characters which will stop the reading.</param>
        /// <returns>
        /// The read characters, up to but not including the character that was found. If at the end
        /// of the stream, null is returned.
        /// </returns>
        public string ReadUntil(char findChar, params char[] findChars)
        {
            return ReadChars(until: true, findChar: findChar, findChars: findChars);
        }

        /// <summary>
        /// Reads while one of the specified characters is found. The next read after this method
        /// returns will be the character that was not found.
        /// </summary>
        /// <param name="findChar">A character which will continue the reading.</param>
        /// <param name="findChars">Other characters which will continue the reading.</param>
        /// <returns>
        /// The read characters, up to but not including the character that was not found. If at the
        /// end of the stream, null is returned.
        /// </returns>
        public string ReadWhile(char findChar, params char[] findChars)
        {
            return ReadChars(until: false, findChar: findChar, findChars: findChars);
        }

        //// ===========================================================================================================
        //// Peek/Read(string)
        //// ===========================================================================================================

        /// <summary>
        /// Peeks until the specified find string is encountered.
        /// </summary>
        /// <param name="find">A string which will stop the reading.</param>
        /// <returns>
        /// The peeked characters, up to but not including the first character of the specified find
        /// string. If at the end of the stream, null is returned.
        /// </returns>
        public string PeekUntil(string find)
        {
            Param.VerifyNotNull(find, "find");

            TextReaderLocation locationBeforeRead = _location;
            string readStr = ReadUntil(find);
            if (readStr != null)
            {
                _buffer = new Queue<char>(readStr.ToCharArray().Concat(_buffer));
                _location = locationBeforeRead;
            }

            return readStr;
        }

        /// <summary>
        /// Peeks while the specified find string is encountered.
        /// </summary>
        /// <param name="find">A string which will continue the peeking.</param>
        /// <returns>
        /// The peeked characters, up to but not including the first character of the character that
        /// stopped the peeking. If at the end of the stream, null is returned.
        /// </returns>
        public string PeekWhile(string find)
        {
            Param.VerifyNotNull(find, "find");

            TextReaderLocation locationBeforeRead = _location;
            string readStr = ReadWhile(find);
            if (readStr != null)
            {
                _buffer = new Queue<char>(readStr.ToCharArray().Concat(_buffer));
                _location = locationBeforeRead;
            }

            return readStr;
        }

        /// <summary>
        /// Reads until the specified find string is encountered. The next read after this method
        /// returns will be the first character that of the find string.
        /// </summary>
        /// <param name="find">A string which will stop the reading.</param>
        /// <returns>
        /// The read characters, up to but not including the first character of the specified find
        /// string. If at the end of the stream, null is returned.
        /// </returns>
        public string ReadUntil(string find)
        {
            Param.VerifyNotNull(find, "find");

            StringBuilder builder = new StringBuilder();
            string nextChunk;

            // Read until we see the first character of the find string.
            while ((nextChunk = ReadUntil(find[0])) != null)
            {
                builder.Append(nextChunk);

                // Look ahead to see if our string is next.
                string peekStr = Peek(find.Length);
                if (peekStr == find)
                {
                    break;
                }

                // Nope, keep reading.
                int c = Read();
                if (c != -1)
                {
                    builder.Append((char)c);
                }
            }

            if (builder.Length == 0 && IsAtEnd)
            {
                return null;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reads while the specified find string is encountered.
        /// </summary>
        /// <param name="find">A string which will continue the reading.</param>
        /// <returns>
        /// The peeked characters, up to but not including the first character of the character that
        /// stopped the peeking. If at the end of the stream, null is returned.
        /// </returns>
        public string ReadWhile(string find)
        {
            Param.VerifyNotNull(find, "find");

            StringBuilder builder = new StringBuilder();

            while (Peek(find.Length) == find)
            {
                builder.Append(Read(find.Length));
            }

            if (builder.Length == 0 && IsAtEnd)
            {
                return null;
            }

            return builder.ToString();
        }

        //// ===========================================================================================================
        //// Peek/Read(predicate)
        //// ===========================================================================================================

        /// <summary>
        /// Peeks until the specified predicate function returns true.
        /// </summary>
        /// <param name="predicate">The predicate function that is evaluated for each character.</param>
        /// <returns>
        /// The characters that were peeked, up to but not including the character that caused the
        /// predicate to return true. If at the end of the stream, null is returned.
        /// </returns>
        public string PeekUntil(Func<char, bool> predicate)
        {
            Param.VerifyNotNull(predicate, "predicate");
            return PeekPredicate(c => !predicate(c));
        }

        /// <summary>
        /// Reads while the specified predicate function returns true.
        /// </summary>
        /// <param name="predicate">
        /// The predicate function that is evaluated for each character before it is read.
        /// </param>
        /// <returns>
        /// The characters that were read, up to but not including the character that caused the
        /// predicate to return false. If at the end of the stream, null is returned.
        /// </returns>
        public string PeekWhile(Func<char, bool> predicate)
        {
            Param.VerifyNotNull(predicate, "predicate");
            return PeekPredicate(predicate);
        }

        /// <summary>
        /// Reads until the specified predicate function returns true.
        /// </summary>
        /// <param name="predicate">
        /// The predicate function that is evaluated for each character before it is read.
        /// </param>
        /// <returns>
        /// The characters that were read, up to but not including the character that caused the
        /// predicate to return true. If at the end of the stream, null is returned.
        /// </returns>
        public string ReadUntil(Func<char, bool> predicate)
        {
            Param.VerifyNotNull(predicate, "predicate");
            return ReadPredicate(c => !predicate(c));
        }

        /// <summary>
        /// Reads while the specified predicate function returns true.
        /// </summary>
        /// <param name="predicate">
        /// The predicate function that is evaluated for each character before it is read.
        /// </param>
        /// <returns>
        /// The characters that were read, up to but not including the character that caused the
        /// predicate to return false. If at the end of the stream, null is returned.
        /// </returns>
        public string ReadWhile(Func<char, bool> predicate)
        {
            Param.VerifyNotNull(predicate, "predicate");
            return ReadPredicate(predicate);
        }

        internal static TextReaderLocation AdvanceLocation(int c, int nextC, TextReaderLocation currentLocation)
        {
            TextReaderLocation location;

            if (c == -1)
            {
                location = currentLocation;
            }
            else if (c == '\r' && nextC != '\n' || c == '\n')
            {
                location = currentLocation.IncrementLine();
            }
            else
            {
                location = currentLocation.IncrementColumn();
            }

            return location;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _internalReader?.Dispose();
            _internalReader = null;
        }

        /// <summary>
        /// Returns whether the specified character is a whitespace character, which are any
        /// characters with Unicode class <c>Zs</c>, horizontal tab characters '\t', vertical tab
        /// characters '\v', form feed characters '\f', or line feeds (any character with the Unicode
        /// class <c>Zl</c>.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <param name="includeLineFeeds">Indicates whether to include linefeed (\r and \n).</param>
        /// <returns>true if the character is whitespace; otherwise, false.</returns>
        private static bool IsWhitespace(char c, bool includeLineFeeds)
        {
            bool isWhitespace = c.IsOneOf('\t', '\v', '\f') ||
                CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;

            if (includeLineFeeds)
            {
                isWhitespace |= c.IsOneOf('\r', '\n') ||
                    CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LineSeparator;
            }

            return isWhitespace;
        }

        private static Func<char, bool> GetCharPredicate(bool until, char findChar, params char[] findChars)
        {
            Func<char, bool> predicate = c => until ? c != findChar : c == findChar;
            if (findChars.Length > 0)
            {
                HashSet<char> set = new HashSet<char>(findChars)
                {
                    findChar
                };
                predicate = c => until ? !set.Contains(c) : set.Contains(c);
            }

            return predicate;
        }

        private string PeekChars(bool until, char findChar, params char[] findChars)
        {
            Func<char, bool> predicate = GetCharPredicate(until, findChar, findChars);
            char[] taken = Peeker.TakeWhile(predicate).ToArray();
            if (taken.Length == 0 && IsAtEnd)
            {
                return null;
            }

            return new string(taken);
        }

        private string ReadChars(bool until, char findChar, params char[] findChars)
        {
            Func<char, bool> predicate = GetCharPredicate(until, findChar, findChars);
            return ReadPredicate(predicate);
        }

        private string PeekPredicate(Func<char, bool> predicate)
        {
            char[] taken = Peeker.TakeWhile(predicate).ToArray();
            if (taken.Length == 0 && IsAtEnd)
            {
                return null;
            }

            return new string(taken);
        }

        private string ReadPredicate(Func<char, bool> predicate)
        {
            StringBuilder builder = new StringBuilder();
            int c;
            while ((c = Peek()) != -1 && predicate((char)c))
            {
                builder.Append((char)Read());
            }

            if (builder.Length == 0 && IsAtEnd)
            {
                return null;
            }

            return builder.ToString();
        }

        private void ThrowIfDisposed()
        {
            if (_internalReader == null)
            {
                throw new ObjectDisposedException(null, GetType().Name + " is closed.");
            }
        }
    }
}
