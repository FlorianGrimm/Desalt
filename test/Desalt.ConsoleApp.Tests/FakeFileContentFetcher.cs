// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FakeFileContentFetcher.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Desalt.CompilerUtilities;

    /// <summary>
    /// Fake implementation of <see cref="IFileContentFetcher"/> that just returns the contents for the specified file.
    /// </summary>
    internal sealed class FakeFileContentFetcher : IFileContentFetcher
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public FakeFileContentFetcher()
        {
        }

        public FakeFileContentFetcher(string filePath, string contents)
        {
            Files.Add(filePath, contents);
        }

        public FakeFileContentFetcher(string filePath, params string[] lines)
            : this(filePath, string.Join(Environment.NewLine, lines))
        {
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IDictionary<string, string> Files { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public string ReadAllText(string filePath)
        {
            if (!Files.TryGetValue(filePath, out string? contents))
            {
                throw new FileNotFoundException();
            }

            return contents;
        }

        public Stream OpenRead(string filePath)
        {
            string contents = ReadAllText(filePath);
            return new UnicodeStringStream(contents);
        }
    }
}
