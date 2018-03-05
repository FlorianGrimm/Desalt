// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlFragmentParser.Reader.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal partial class XmlFragmentParser
    {
        /// <summary>
        /// A text reader over a synthesized XML stream consisting of a single root element followed
        /// by a potentially infinite stream of fragments. Each time "SetText" is called the stream
        /// is rewound to the element immediately following the synthetic root node.
        /// </summary>
        private sealed class Reader : TextReader
        {
            //// =======================================================================================================
            //// Member Variables
            //// =======================================================================================================

            // Base the root element name on a GUID to avoid accidental (or intentional) collisions.
            // An underscore is prefixed because element names must not start with a number.
            private static readonly string s_rootElementName = "_" + Guid.NewGuid().ToString("N");

            // We insert an extra synthetic element name to allow for raw text at the root
            internal static readonly string CurrentElementName = "_" + Guid.NewGuid().ToString("N");

            private static readonly string s_rootStart = "<" + s_rootElementName + ">";
            private static readonly string s_currentStart = "<" + CurrentElementName + ">";
            private static readonly string s_currentEnd = "</" + CurrentElementName + ">";

            private string _text;
            private int _position;

            //// =======================================================================================================
            //// Methods
            //// =======================================================================================================

            public void Reset()
            {
                _text = null;
                _position = 0;
            }

            public void SetText(string text)
            {
                _text = text;

                // The first read shall read the <root>, the subsequents reads shall start with
                // <current> element
                if (_position > 0)
                {
                    _position = s_rootStart.Length;
                }
            }

            /// <summary>
            /// Reads a specified maximum number of characters from the current reader and writes the
            /// data to a buffer, beginning at the specified index.
            /// </summary>
            /// <param name="buffer">
            /// When this method returns, contains the specified character array with the values
            /// between <paramref name="index"/> and ( <paramref name="index"/> + <paramref
            /// name="count"/> - 1) replaced by the characters read from the current source.
            /// </param>
            /// <param name="index">
            /// The position in <paramref name="buffer"/> at which to begin writing.
            /// </param>
            /// <param name="count">
            /// The maximum number of characters to read. If the end of the reader is reached before
            /// the specified number of characters is read into the buffer, the method returns.
            /// </param>
            /// <returns>
            /// The number of characters that have been read. The number will be less than or equal
            /// to <paramref name="count"/>, depending on whether the data is available within the
            /// reader. This method returns 0 (zero) if it is called when no more characters are left
            /// to read.
            /// </returns>
            public override int Read(char[] buffer, int index, int count)
            {
                if (count == 0)
                {
                    return 0;
                }

                // The stream synthesizes an XML document with:
                // 1. A root element start tag
                // 2. Current element start tag
                // 3. The user text (xml fragments)
                // 4. Current element end tag

                int initialCount = count;

                // <root>
                _position += EncodeAndAdvance(s_rootStart, _position, buffer, ref index, ref count);

                // <current>
                _position += EncodeAndAdvance(
                    s_currentStart,
                    _position - s_rootStart.Length,
                    buffer,
                    ref index,
                    ref count);

                // text
                _position += EncodeAndAdvance(
                    _text,
                    _position - s_rootStart.Length - s_currentStart.Length,
                    buffer,
                    ref index,
                    ref count);

                // </current>
                _position += EncodeAndAdvance(
                    s_currentEnd,
                    _position - s_rootStart.Length - s_currentStart.Length - _text.Length,
                    buffer,
                    ref index,
                    ref count);

                // pretend that the stream is infinite, i.e. never return 0 characters read
                if (initialCount == count)
                {
                    buffer[index] = ' ';
                    count--;
                }

                return initialCount - count;
            }

            /// <summary>
            /// Fills the buffer with the contents of the source string.
            /// </summary>
            /// <param name="src">The source string.</param>
            /// <param name="srcIndex">The index within the source string from which to begin copying.</param>
            /// <param name="dest">The destination buffer.</param>
            /// <param name="destIndex">The index within the destination buffer to write.</param>
            /// <param name="destCount">The count of characters to copy.</param>
            /// <returns>The number of characters written.</returns>
            private static int EncodeAndAdvance(
                string src,
                int srcIndex,
                char[] dest,
                ref int destIndex,
                ref int destCount)
            {
                if (destCount == 0 || srcIndex < 0 || srcIndex >= src.Length)
                {
                    return 0;
                }

                // figure out how many characters to copy
                int charCount = Math.Min(src.Length - srcIndex, destCount);
                Debug.Assert(charCount > 0);
                src.CopyTo(srcIndex, dest, destIndex, charCount);

                destIndex += charCount;
                destCount -= charCount;
                Debug.Assert(destCount >= 0);
                return charCount;
            }

            public override int Read()
            {
                // XmlReader does not call this API
                throw ExceptionFactory.Unreachable;
            }

            public override int Peek()
            {
                // XmlReader does not call this API
                throw ExceptionFactory.Unreachable;
            }
        }
    }
}
