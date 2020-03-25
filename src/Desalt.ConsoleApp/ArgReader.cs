// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgReader.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Desalt.CompilerUtilities;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a reader that knows how to process individual command-line arguments. Also supports reading in
    /// response files.
    /// </summary>
    internal sealed class ArgReader
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly List<string> _args;
        private readonly IFileContentFetcher _fileContentFetcher;
        private readonly IList<Diagnostic> _diagnostics;

        private int _currentIndex;
        private string? _currentValue;
        private bool _parsedCurrentValue;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ArgReader(
            IEnumerable<string> args,
            IFileContentFetcher? fileContentFetcher = null,
            IList<Diagnostic>? diagnostics = null)
        {
            _args = new List<string>(args);
            _fileContentFetcher = fileContentFetcher ?? new OsFileContentFetcher();
            _diagnostics = diagnostics ?? new List<Diagnostic>();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsAtEnd => Peek() == null;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Peeks ahead one argument without reading.
        /// </summary>
        public string? Peek()
        {
            if (!_parsedCurrentValue)
            {
                string? currentRawArg = _currentIndex < _args.Count ? _args[_currentIndex] : null;
                _currentValue = currentRawArg == null ? null : ProcessArg(currentRawArg);
                _parsedCurrentValue = true;
            }

            return _currentValue;
        }

        /// <summary>
        /// Reads the next argument.
        /// </summary>
        public string? Read()
        {
            string? current = Peek();
            if (current != null)
            {
                _currentIndex++;
                _parsedCurrentValue = false;
            }

            return current;
        }

        private string? ProcessArg(string rawArg)
        {
            string? processedArg;

            if (rawArg.StartsWith('"'))
            {
                using var reader = new PeekingTextReader(rawArg);
                processedArg = RemoveQuotesAndSlashes(reader);
            }
            else if (rawArg.StartsWith('@'))
            {
                string fileName = rawArg.Substring(1);
                IEnumerable<string> responseFileArgs = ParseResponseFile(fileName);

                // Skip the response file argument and insert the contents of the response file.
                _currentIndex++;
                _args.InsertRange(_currentIndex, responseFileArgs);

                // Now process the next argument.
                if (_currentIndex < _args.Count)
                {
                    processedArg = ProcessArg(_args[_currentIndex]);
                }
                else
                {
                    processedArg = null;
                }
            }
            else
            {
                processedArg = rawArg;
            }

            return processedArg;
        }

        private IEnumerable<string> ParseResponseFile(string fileName)
        {
            string contents;

            try
            {
                contents = _fileContentFetcher.ReadAllText(fileName);
            }
            catch (Exception)
            {
                _diagnostics.Add(DiagnosticFactory.ErrorOpeningResponseFile(fileName));
                return Enumerable.Empty<string>();
            }

            var flattenedArgs = new List<string>();
            using var reader = new PeekingTextReader(contents);
            reader.SkipWhitespace();

            while (!reader.IsAtEnd)
            {
                string? arg = RemoveQuotesAndSlashes(reader);
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    flattenedArgs.Add(arg);
                }

                reader.SkipWhitespace();
            }

            return flattenedArgs;
        }

        /// <summary>
        /// Remove quote characters surrounding the argument and unescape any backslashes.
        /// </summary>
        private static string? RemoveQuotesAndSlashes(PeekingTextReader reader)
        {
            reader.SkipWhitespace();
            bool withinQuote = reader.Peek() == '"';

            if (!withinQuote)
            {
                return reader.ReadUntilWhitespace();
            }

            var builder = new StringBuilder();
            reader.Read(); // quote

            do
            {
                string? nextChunk = reader.ReadUntil(c => c.IsOneOf('"', '\\'));
                builder.Append(nextChunk);

                switch (reader.Peek())
                {
                    case '"':
                        withinQuote = false;
                        reader.Read();
                        break;

                    case '\\':
                        // Check for escaped quotes or backslashes
                        if (reader.Peek(2) == "\\\"")
                        {
                            reader.Read(2);
                            builder.Append('"');
                        }
                        else if (reader.Peek(2) == "\\\\")
                        {
                            reader.Read(2);
                            builder.Append('\\');
                        }
                        else
                        {
                            reader.Read();
                            builder.Append('\\');
                        }
                        break;

                    default:
                        withinQuote = false;
                        break;
                }
            }
            while (withinQuote);

            return builder.ToString();
        }
    }
}
