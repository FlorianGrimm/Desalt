// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PathUtil.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.IO;

    /// <summary>
    /// Contains utility methods for working with paths.
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>
        /// Replaces the file extension and leaves the rest of the path intact.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="extension">The new extension, with or without the leading period.</param>
        /// <returns>The same path, but with the extension replaced.</returns>
        public static string ReplaceExtension(string? filePath, string? extension)
        {
            // add the leading period if necessary
            extension = extension?.Trim() ?? string.Empty;
            if (extension.Length > 0 && extension[0] != '.')
            {
                extension = '.' + extension;
            }

            string newFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                newFilePath = extension;
            }
            else
            {
                string directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                newFilePath = Path.Combine(directoryName, fileNameWithoutExtension + extension);
            }

            return newFilePath;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">
        /// Contains the directory that defines the start of the relative path.
        /// </param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>
        /// The relative path from the start directory to the end path or <c>toPath</c> if the paths
        /// are not related.
        /// </returns>
        /// <remarks>Taken from Stack Overflow at <see href="https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path"/></remarks>
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // path can't be made relative
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
