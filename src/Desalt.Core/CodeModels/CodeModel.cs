// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CodeModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Desalt.Core.Utility;

    /// <summary>
    /// Abstract base class for all code model classes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class CodeModel : ICodeModel
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a consise string representing the current code model to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {ToCodeDisplay()}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns an abbreviated string representation of the code model, which is useful for debugging.
        /// </summary>
        /// <returns>A string representation of this code model.</returns>
        public abstract string ToCodeDisplay();

        /// <summary>
        /// Returns a string representation of the full code model, which is useful for debugging and
        /// printing to logs. This should not be used to actually emit generated code.
        /// </summary>
        /// <returns>A string representation of the full code model.</returns>
        public virtual string ToFullCodeDisplay()
        {
            using (var stringWriter = new StringWriter())
            using (var writer = new IndentedTextWriter(stringWriter))
            {
                WriteFullCodeDisplay(writer);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Writes a string representation of this code model to the specified <see
        /// cref="IndentedTextWriter"/>, which is useful for debugging and printing to logs. This
        /// should not be used to actually emit generated code.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public abstract void WriteFullCodeDisplay(IndentedTextWriter writer);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => ToFullCodeDisplay();

        /// <summary>
        /// Writes a list of items wrapped in a {} block to the specified text writer.
        /// </summary>
        /// <param name="writer">The <see cref="IndentedTextWriter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteBlock(IndentedTextWriter writer, IReadOnlyList<ICodeModel> items)
        {
            WriteItems(
                writer,
                items,
                indent: true,
                prefix: "{",
                suffix: "}",
                itemDelimiter: Environment.NewLine,
                newLineAfterPrefix: true,
                delimiterAfterLastItem: true,
                newLineAfterLastItem: true);
        }

        /// <summary>
        /// Writes a list of items wrapped in a {} block where each item is separated by a comma and
        /// new line to the specified text writer.
        /// </summary>
        /// <param name="writer">The <see cref="IndentedTextWriter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteCommaNewlineSeparatedBlock(IndentedTextWriter writer, IReadOnlyList<ICodeModel> items)
        {
            WriteItems(
                writer,
                items,
                indent: true,
                prefix: "{",
                suffix: "}",
                itemDelimiter: "," + Environment.NewLine,
                newLineAfterPrefix: true,
                newLineAfterLastItem: true);
        }

        /// <summary>
        /// Writes a comma-separated list wrapped in a () block to the specified text writer.
        /// </summary>
        /// <param name="writer">The <see cref="IndentedTextWriter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteParameterList(IndentedTextWriter writer, IReadOnlyList<ICodeModel> items)
        {
            WriteItems(writer, items, indent: false, prefix: "(", suffix: ")", itemDelimiter: ", ");
        }

        /// <summary>
        /// Writes a list of items to the specified text writer, surrounded by the specified prefix
        /// and suffix and delimited by the specified delimiter.
        /// </summary>
        /// <param name="writer">The <see cref="IndentedTextWriter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        /// <param name="indent">Indicates whether to indent the items.</param>
        /// <param name="prefix">The prefix to write before writing the items.</param>
        /// <param name="suffix">The suffix to write after writing the items.</param>
        /// <param name="itemDelimiter">The delimiter to write between items.</param>
        /// <param name="newLineAfterPrefix">
        /// Indicates whether a newline should be written after the prefix (useful for blocks).
        /// </param>
        /// <param name="delimiterAfterLastItem">
        /// Indicates whether the last item should also have a delimiter at the end (useful for
        /// blocks to put a newline before the last brace.
        /// </param>
        /// <param name="newLineAfterLastItem">
        /// Indicates whether the last item should include a newline.
        /// </param>
        protected void WriteItems(
            IndentedTextWriter writer,
            IReadOnlyList<ICodeModel> items,
            bool indent,
            string prefix = null,
            string suffix = null,
            string itemDelimiter = null,
            bool newLineAfterPrefix = false,
            bool delimiterAfterLastItem = false,
            bool newLineAfterLastItem = false)
        {
            prefix = prefix ?? string.Empty;
            suffix = suffix ?? string.Empty;
            itemDelimiter = itemDelimiter ?? string.Empty;

            bool newlineAfterItems = false;
            if (itemDelimiter.EndsWith("\r\n"))
            {
                newlineAfterItems = true;
                itemDelimiter = itemDelimiter.Substring(0, itemDelimiter.Length - 2);
            }
            else if (itemDelimiter.EndsWith("\r") || itemDelimiter.EndsWith("\n"))
            {
                newlineAfterItems = true;
                itemDelimiter = itemDelimiter.Substring(0, itemDelimiter.Length - 1);
            }

            if (items.Count == 0)
            {
                writer.Write($"{prefix}{suffix}");
            }
            else
            {
                if (newLineAfterPrefix)
                {
                    writer.WriteLine(prefix);
                }
                else
                {
                    writer.Write(prefix);
                }

                if (indent)
                {
                    writer.IndentLevel++;
                }

                for (int i = 0; i < items.Count; i++)
                {
                    ICodeModel item = items[i];
                    item.WriteFullCodeDisplay(writer);

                    // write the delimiter
                    if (i < items.Count - 1 || delimiterAfterLastItem)
                    {
                        writer.Write(itemDelimiter);
                    }

                    // write a new line after the last item if necessary
                    if (i < items.Count - 1 && newlineAfterItems ||
                        i == items.Count - 1 && newLineAfterLastItem)
                    {
                        writer.WriteLine();
                    }
                }

                if (indent)
                {
                    writer.IndentLevel--;
                }

                writer.Write(suffix);
            }
        }
    }
}
