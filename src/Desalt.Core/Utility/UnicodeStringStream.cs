// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="UnicodeStringStream.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a <see cref="Stream"/> that uses a string as its backing store.
    /// </summary>
    /// <remarks>
    /// Note that this is an immutable stream, supporting only read operations. Also note that this
    /// uses the <see cref="Encoding.Unicode"/> encoding so that extra memory is not allocated for a
    /// character buffer. Use <see cref="MemoryStream"/> combined with
    /// <see cref="Encoding.GetBytes(char[])"/> when encoding is desired at the cost of extra
    /// memory. The code is modified from an MSDN article at http://msdn.microsoft.com/en-us/magazine/cc163768.aspx.
    /// </remarks>
    internal class UnicodeStringStream : Stream
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly long _byteLength;
        private readonly byte[] _byteOrderMark;
        private int _position;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="UnicodeStringStream"/> class.
        /// </summary>
        /// <param name="contents">The string to use as the source of the stream.</param>
        public UnicodeStringStream(string contents)
            : this(contents, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnicodeStringStream"/> class.
        /// </summary>
        /// <param name="contents">The string to use as the source of the stream.</param>
        /// <param name="suppressByteOrderMark">Indicates whether the BOM should be written to the stream.</param>
        public UnicodeStringStream(string contents, bool suppressByteOrderMark)
        {
            Source = contents ?? throw new ArgumentNullException(nameof(contents));
            SuppressByteOrderMark = suppressByteOrderMark;
            _byteOrderMark = suppressByteOrderMark ? new byte[0] : Encoding.Unicode.GetPreamble();
            _byteLength = _byteOrderMark.Length + (Source.Length * 2); // Unicode is 2 bytes per character
            _position = 0;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public override bool CanRead => !IsClosed;

        public override bool CanSeek => !IsClosed;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                ThrowIfClosed();
                return _byteLength;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfClosed();
                return _position;
            }

            set
            {
                ThrowIfClosed();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(Position));
                }
                _position = (int)value;
            }
        }

        public string Source { get; private set; }

        public bool SuppressByteOrderMark { get; }

        private bool IsClosed => Source == null;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfClosed();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.End:
                    Position = _byteLength + offset;
                    break;

                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfClosed();

            int bytesRead = 0;
            while (bytesRead < count)
            {
                if (_position >= _byteLength) { return bytesRead; }

                if (!SuppressByteOrderMark && _position >= 0 && _position < _byteOrderMark.Length)
                {
                    buffer[offset + bytesRead] = _byteOrderMark[_position];
                }
                else
                {
                    int sourcePosition = _position - _byteOrderMark.Length;
                    char c = Source[sourcePosition / 2];
                    buffer[offset + bytesRead] = (byte)((sourcePosition % 2 == 0) ? c & 0xff : (c >> 8) & 0xff);
                }

                Position++;
                bytesRead++;
            }

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Source = null;
        }

        private void ThrowIfClosed()
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
