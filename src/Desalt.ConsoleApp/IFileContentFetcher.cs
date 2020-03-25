// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileContentFetcher.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System.IO;

    /// <summary>
    /// Contract for fetching contents from a file. Unit tests will mock this to return the contents from a string.
    /// </summary>
    internal interface IFileContentFetcher
    {
        string ReadAllText(string filePath);

        Stream OpenRead(string filePath);
    }
}
