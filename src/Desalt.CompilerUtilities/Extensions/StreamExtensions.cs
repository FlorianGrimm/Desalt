// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities.Extensions
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Contains extension methods on streams.
    /// </summary>
    public static class StreamExtensions
    {
        public static string ReadAllText(
            this MemoryStream stream,
            Encoding? encoding = null,
            bool preservePosition = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            encoding ??= Encoding.UTF8;

            long cachedPosition = stream.Position;
            stream.Position = 0;

            string text;

            using (var reader = new StreamReader(
                stream,
                encoding,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                text = reader.ReadToEnd();
            }

            if (preservePosition)
            {
                stream.Position = cachedPosition;
            }

            return text;
        }

        public static string ReadToEnd(
            this MemoryStream stream,
            Encoding? encoding = null,
            bool preservePosition = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            encoding ??= Encoding.UTF8;

            long cachedPosition = stream.Position;
            string text;

            using (var reader = new StreamReader(
                stream,
                encoding,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                text = reader.ReadToEnd();
            }

            if (preservePosition)
            {
                stream.Position = cachedPosition;
            }

            return text;
        }
    }
}
