// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Desalt.Core.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) nodes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstNode : IAstNode
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public abstract string CodeDisplay { get; }

        /// <summary>
        /// Gets a consise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {CodeDisplay}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public abstract void Emit(Emitter emitter);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => CodeDisplay;

        /// <summary>
        /// Writes a list of items wrapped in a {} block to the specified text emitter.
        /// </summary>
        /// <param name="emitter">The <see cref="Emitter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteBlock(Emitter emitter, IReadOnlyList<IAstNode> items)
        {
            WriteItems(
                emitter,
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
        /// new line to the specified text emitter.
        /// </summary>
        /// <param name="emitter">The <see cref="Emitter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteCommaNewlineSeparatedBlock(Emitter emitter, IReadOnlyList<IAstNode> items)
        {
            WriteItems(
                emitter,
                items,
                indent: true,
                prefix: "{",
                suffix: "}",
                itemDelimiter: "," + Environment.NewLine,
                newLineAfterPrefix: true,
                newLineAfterLastItem: true);
        }

        /// <summary>
        /// Writes a comma-separated list wrapped in a () block to the specified text emitter.
        /// </summary>
        /// <param name="emitter">The <see cref="Emitter"/> to write to.</param>
        /// <param name="items">The items to write.</param>
        protected void WriteParameterList(Emitter emitter, IReadOnlyList<IAstNode> items)
        {
            WriteItems(emitter, items, indent: false, prefix: "(", suffix: ")", itemDelimiter: ", ");
        }

        /// <summary>
        /// Writes a list of items to the specified text emitter, surrounded by the specified prefix
        /// and suffix and delimited by the specified delimiter.
        /// </summary>
        /// <param name="emitter">The <see cref="Emitter"/> to write to.</param>
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
            Emitter emitter,
            IReadOnlyList<IAstNode> items,
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
                emitter.Write($"{prefix}{suffix}");
            }
            else
            {
                if (newLineAfterPrefix)
                {
                    emitter.WriteLine(prefix);
                }
                else
                {
                    emitter.Write(prefix);
                }

                if (indent)
                {
                    emitter.IndentLevel++;
                }

                for (int i = 0; i < items.Count; i++)
                {
                    IAstNode item = items[i];
                    item.Emit(emitter);

                    // write the delimiter
                    if (i < items.Count - 1 || delimiterAfterLastItem)
                    {
                        emitter.Write(itemDelimiter);
                    }

                    // write a new line after the last item if necessary
                    if (i < items.Count - 1 && newlineAfterItems ||
                        i == items.Count - 1 && newLineAfterLastItem)
                    {
                        emitter.WriteLine();
                    }
                }

                if (indent)
                {
                    emitter.IndentLevel--;
                }

                emitter.Write(suffix);
            }
        }
    }
}
