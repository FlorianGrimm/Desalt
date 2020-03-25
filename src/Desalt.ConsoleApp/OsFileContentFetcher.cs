// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="OsFileContentFetcher.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System.IO;

    /// <summary>
    /// Implementation of <see cref="IFileContentFetcher"/> that uses the OS file system.
    /// </summary>
    internal sealed class OsFileContentFetcher : IFileContentFetcher
    {
        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public Stream OpenRead(string filePath)
        {
            return File.OpenRead(filePath);
        }
    }
}
