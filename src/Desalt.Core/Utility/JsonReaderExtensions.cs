// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonReaderExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using Newtonsoft.Json;

    internal static class JsonReaderExtensions
    {
        public static void Read(this JsonReader reader, JsonToken expectedToken)
        {
            VerifyToken(reader, expectedToken);
            reader.Read();
        }

        public static void VerifyToken(this JsonReader reader, JsonToken expectedToken)
        {
            if (reader.TokenType != expectedToken)
            {
                throw new InvalidOperationException(
                    $"Invalid JSON token. Expecting '{expectedToken}' but found '{reader.TokenType}': " +
                    $"path = {reader.Path}");
            }
        }
    }
}
