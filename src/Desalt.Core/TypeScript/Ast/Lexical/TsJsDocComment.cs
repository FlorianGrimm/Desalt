// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocComment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Emit;
    using TagNames = TsJsDocTagNames;

    /// <summary>
    /// Represents a structured JSDoc comment before a declaration.
    /// </summary>
    /// <remarks>The comment is emitted in a set order:<code>
    /// /**
    ///  * Description
    ///  * @summary text
    ///  * @file text
    ///  * @copyright text
    ///  * @package
    ///  * @param name - text
    ///  * @returns text
    ///  * @throws {type} text
    ///  * @example text
    ///  * @see text
    ///  */
    /// </code></remarks>
    internal class TsJsDocComment : AstTriviaNode, ITsJsDocComment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsJsDocComment(
            ITsJsDocBlock fileTag = null,
            ITsJsDocBlock copyrightTag = null,
            bool isPackagePrivate = false,
            IEnumerable<(string paramName, ITsJsDocBlock text)> paramsTags = null,
            ITsJsDocBlock returnsTag = null,
            IEnumerable<(string typeName, ITsJsDocBlock text)> throwsTags = null,
            IEnumerable<ITsJsDocBlock> exampleTags = null,
            ITsJsDocBlock description = null,
            ITsJsDocBlock summaryTag = null,
            IEnumerable<ITsJsDocBlock> seeTags = null) : base(preserveSpacing: true)
        {
            FileTag = fileTag;
            CopyrightTag = copyrightTag;
            IsPackagePrivate = isPackagePrivate;
            ParamsTags = paramsTags?.ToImmutableArray() ?? ImmutableArray<(string paramName, ITsJsDocBlock text)>.Empty;
            ReturnsTag = returnsTag;
            ThrowsTags = throwsTags?.ToImmutableArray() ?? ImmutableArray<(string typeName, ITsJsDocBlock text)>.Empty;
            ExampleTags = exampleTags?.ToImmutableArray() ?? ImmutableArray<ITsJsDocBlock>.Empty;
            Description = description;
            SummaryTag = summaryTag;
            SeeTags = seeTags?.ToImmutableArray() ?? ImmutableArray<ITsJsDocBlock>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the main description.
        /// </summary>
        public ITsJsDocBlock Description { get; }

        /// <summary>
        /// Gets the @summary tag, which is a shorter version of the full description.
        /// </summary>
        public ITsJsDocBlock SummaryTag { get; }

        /// <summary>
        /// Gets the @file tag.
        /// </summary>
        public ITsJsDocBlock FileTag { get; }

        /// <summary>
        /// Gets the @copyright tag.
        /// </summary>
        public ITsJsDocBlock CopyrightTag { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol to which this JSDoc is attached is meant to be
        /// package-private (@package), which is the best we can do when translating 'internal'
        /// accessibility from C#.
        /// </summary>
        public bool IsPackagePrivate { get; }

        /// <summary>
        /// Gets an array of @param tags. Each param is emitted in the form '@param name - text'.
        /// </summary>
        public ImmutableArray<(string paramName, ITsJsDocBlock text)> ParamsTags { get; }

        /// <summary>
        /// Gets the @returns tag text.
        /// </summary>
        public ITsJsDocBlock ReturnsTag { get; }

        /// <summary>
        /// Gets an array of @throws tags. Each @throws is emitted in the form '@throws {type} text'.
        /// </summary>
        public ImmutableArray<(string typeName, ITsJsDocBlock text)> ThrowsTags { get; }

        /// <summary>
        /// Gets an array of @example tags.
        /// </summary>
        public ImmutableArray<ITsJsDocBlock> ExampleTags { get; }

        /// <summary>
        /// Gets an array of @see tags.
        /// </summary>
        public ImmutableArray<ITsJsDocBlock> SeeTags { get; }

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder(Description?.CodeDisplay ?? string.Empty);

                if (SummaryTag != null)
                {
                    builder.Append(TagNames.Summary).Append(" ").Append(SummaryTag.CodeDisplay);
                }

                if (SeeTags != null)
                {
                    foreach (ITsJsDocBlock seeTag in SeeTags)
                    {
                        builder.Append(TagNames.See).Append(" ").Append(seeTag);
                    }
                }

                return builder.ToString();
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public override void Emit(Emitter emitter)
        {
            IReadOnlyList<string> lines = CreateLines(emitter.Options);

            if (lines.Count == 0 && emitter.Options.SingleLineJsDocCommentsOnOneLine)
            {
                emitter.Write("/** */");
            }
            else if (lines.Count == 1 && emitter.Options.SingleLineJsDocCommentsOnOneLine)
            {
                emitter.Write("/** ");
                emitter.Write(lines[0]);
                emitter.Write(" */");
            }
            else
            {
                emitter.WriteLine("/**");
                foreach (string line in lines)
                {
                    emitter.WriteLine(" * " + line);
                }

                emitter.WriteLine(" */");
            }
        }

        private IReadOnlyList<string> CreateLines(EmitOptions options)
        {
            var lines = new List<string>();

            AddLine(Description, string.Empty, lines, options);
            AddLine(SummaryTag, TagNames.Summary, lines, options);
            AddLine(FileTag, TagNames.File, lines, options);
            AddLine(CopyrightTag, TagNames.Copyright, lines, options);

            // @package
            if (IsPackagePrivate)
            {
                lines.Add(TagNames.Package);
            }

            // @param
            lines.AddRange(
                ParamsTags.Select(tuple => $"{TagNames.Param} {tuple.paramName} - {tuple.text.EmitAsString(options)}"));

            // @returns
            AddLine(ReturnsTag, TagNames.Returns, lines, options);

            // @throws
            lines.AddRange(
                ThrowsTags.Select(tuple => $"{TagNames.Throws} {{{tuple.typeName}}} {tuple.text.EmitAsString(options)}"));

            // @example
            lines.AddRange(ExampleTags.Select(example => $"{TagNames.Example} {example.EmitAsString(options)}"));

            // @see
            lines.AddRange(SeeTags.Select(text => $"{TagNames.See} {text.EmitAsString(options)}"));

            return lines;
        }

        private static void AddLine(
            IAstTriviaNode tagBlock,
            string tagName,
            ICollection<string> list,
            EmitOptions options)
        {
            if (tagBlock != null)
            {
                string item = string.IsNullOrEmpty(tagName)
                    ? tagBlock.EmitAsString(options)
                    : $"{tagName} {tagBlock.EmitAsString(options)}";
                list.Add(item);
            }
        }
    }
}
