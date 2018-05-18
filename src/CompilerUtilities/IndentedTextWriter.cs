// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IndentedTextWriter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a text writer that can indent new lines by an indentation prefix token.
    /// </summary>
    /// <remarks>
    /// Most of this class is modified from the equivalent System.CodeDom.Compiler.IndentedTextWriter.
    /// The source is at <see href="http://referencesource.microsoft.com/#System/compmod/system/codedom/compiler/IndentTextWriter.cs,bae755007e6f4473"/>.
    /// </remarks>
    public class IndentedTextWriter : TextWriter
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Specifies the default indentation prefix, which is two spaces.
        /// </summary>
        public static readonly string DefaultIndentationPrefix = "  ";

        private int _indentLevel;
        private bool _indentPending;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedTextWriter"/> class using the
        /// specified text writer and default indentation prefix.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to use for output.</param>
        public IndentedTextWriter(TextWriter writer)
            : this(writer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedTextWriter"/> class using the
        /// specified text writer and indentation prefix.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to use for output.</param>
        /// <param name="indentationPrefix">The indentation prefix to use for indentation.</param>
        public IndentedTextWriter(TextWriter writer, string indentationPrefix)
            : base(CultureInfo.InvariantCulture)
        {
            InnerWriter = writer ?? throw new ArgumentNullException(nameof(writer));
            IndentationPrefix = indentationPrefix ?? DefaultIndentationPrefix;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets or sets the number of spaces to indent.
        /// </summary>
        public int IndentLevel
        {
            get => _indentLevel;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                _indentLevel = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> to use.
        /// </summary>
        public TextWriter InnerWriter { get; }

        /// <summary>
        /// Gets or sets the new line character to use.
        /// </summary>
        public override string NewLine
        {
            get => InnerWriter.NewLine;
            set => InnerWriter.NewLine = value;
        }

        /// <summary>
        /// Gets the indentation prefix to use for each level of indentation.
        /// </summary>
        public string IndentationPrefix { get; }

        public override Encoding Encoding => InnerWriter.Encoding;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Flush()
        {
            InnerWriter.Flush();
        }

        public override Task FlushAsync()
        {
            return InnerWriter.FlushAsync();
        }

        /// <summary>
        /// Writes the specified string to a line without tabs.
        /// </summary>
        public void WriteLineWithoutIndentation()
        {
            InnerWriter.WriteLine();
            _indentPending = true;
        }

        //// ===========================================================================================================
        //// Overridden Write methods
        //// ===========================================================================================================

        public override void Write(char value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(bool value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(char[] buffer)
        {
            OutputTabs();
            InnerWriter.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            OutputTabs();
            InnerWriter.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(double value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(float value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(int value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(long value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(object value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            OutputTabs();
            InnerWriter.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            OutputTabs();
            InnerWriter.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            OutputTabs();
            InnerWriter.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            OutputTabs();
            InnerWriter.Write(format, arg);
        }

        public override void Write(string value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(uint value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override void Write(ulong value)
        {
            OutputTabs();
            InnerWriter.Write(value);
        }

        public override Task WriteAsync(char value)
        {
            OutputTabs();
            return InnerWriter.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            OutputTabs();
            return InnerWriter.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            OutputTabs();
            return InnerWriter.WriteAsync(value);
        }

        //// ===========================================================================================================
        //// Overridden WriteLine methods
        //// ===========================================================================================================

        public override void WriteLine()
        {
            OutputTabs();
            InnerWriter.WriteLine();
            _indentPending = true;
        }

        public override void WriteLine(bool value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(char value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(char[] buffer)
        {
            OutputTabs();
            InnerWriter.WriteLine(buffer);
            _indentPending = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            OutputTabs();
            InnerWriter.WriteLine(buffer, index, count);
            _indentPending = true;
        }

        public override void WriteLine(decimal value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(double value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(float value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(int value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(long value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(object value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            OutputTabs();
            InnerWriter.WriteLine(format, arg0);
            _indentPending = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            OutputTabs();
            InnerWriter.WriteLine(format, arg0, arg1);
            _indentPending = true;
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            OutputTabs();
            InnerWriter.WriteLine(format, arg0, arg1, arg2);
            _indentPending = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            OutputTabs();
            InnerWriter.WriteLine(format, arg);
            _indentPending = true;
        }

        public override void WriteLine(string value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(uint value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override void WriteLine(ulong value)
        {
            OutputTabs();
            InnerWriter.WriteLine(value);
            _indentPending = true;
        }

        public override async Task WriteLineAsync()
        {
            OutputTabs();
            await InnerWriter.WriteLineAsync();
            _indentPending = true;
        }

        public override async Task WriteLineAsync(char value)
        {
            OutputTabs();
            await InnerWriter.WriteLineAsync(value);
            _indentPending = true;
        }

        public override async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            OutputTabs();
            await InnerWriter.WriteLineAsync(buffer, index, count);
            _indentPending = true;
        }

        public override async Task WriteLineAsync(string value)
        {
            OutputTabs();
            await InnerWriter.WriteLineAsync(value);
            _indentPending = true;
        }

        protected virtual void OutputTabs()
        {
            if (!_indentPending)
            {
                return;
            }

            for (int i = 0; i < _indentLevel; i++)
            {
                InnerWriter.Write(IndentationPrefix);
            }

            _indentPending = false;
        }
    }
}
