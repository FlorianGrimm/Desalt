// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsJsDocCommentInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using System.Collections.Immutable;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a JSDoc structured multi-line comment.
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
    public interface ITsJsDocComment : IAstTriviaNode
    {
        /// <summary>
        /// Gets the main description.
        /// </summary>
        ITsJsDocBlock Description { get; }

        /// <summary>
        /// Gets the @summary tag, which is a shorter version of the full description.
        /// </summary>
        ITsJsDocBlock SummaryTag { get; }

        /// <summary>
        /// Gets the @file tag.
        /// </summary>
        ITsJsDocBlock FileTag { get; }

        /// <summary>
        /// Gets the @copyright tag.
        /// </summary>
        ITsJsDocBlock CopyrightTag { get; }

        /// <summary>
        /// Gets a value indicating whether the symbol to which this JSDoc is attached is meant to be
        /// package-private (@package), which is the best we can do when translating 'internal'
        /// accessibility from C#.
        /// </summary>
        bool IsPackagePrivate { get; }

        /// <summary>
        /// Gets an array of @param tags. Each param is emitted in the form '@param name - text'.
        /// </summary>
        ImmutableArray<(string paramName, ITsJsDocBlock text)> ParamsTags { get; }

        /// <summary>
        /// Gets an array of @typeparam tags. Each param is emitted in the form '@typeparam name -
        /// text'. Note that @typeparam is not a valid JSDoc tag, but it's useful in TypeScript.
        /// Emitting can be controlled via <see cref="EmitOptions"/>.
        /// </summary>
        ImmutableArray<(string paramName, ITsJsDocBlock text)> TypeParamsTags { get; }

        /// <summary>
        /// Gets the @returns tag text.
        /// </summary>
        ITsJsDocBlock ReturnsTag { get; }

        /// <summary>
        /// Gets an array of @throws tags. Each @throws is emitted in the form '@throws {type} text'.
        /// </summary>
        ImmutableArray<(string typeName, ITsJsDocBlock text)> ThrowsTags { get; }

        /// <summary>
        /// Gets an array of @example tags.
        /// </summary>
        ImmutableArray<ITsJsDocBlock> ExampleTags { get; }

        /// <summary>
        /// Gets an array of @see tags.
        /// </summary>
        ImmutableArray<ITsJsDocBlock> SeeTags { get; }

        ITsJsDocComment WithDescription(ITsJsDocBlock value);

        ITsJsDocComment WithSummaryTag(ITsJsDocBlock value);

        ITsJsDocComment WithFileTag(ITsJsDocBlock value);

        ITsJsDocComment WithCopyrightTag(ITsJsDocBlock value);

        ITsJsDocComment WithIsPackagePrivate(bool value);

        ITsJsDocComment WithParamTags(ImmutableArray<(string paramName, ITsJsDocBlock text)> value);

        ITsJsDocComment WithTypeParamTags(ImmutableArray<(string paramName, ITsJsDocBlock text)> value);

        ITsJsDocComment WithReturnsTag(ITsJsDocBlock value);

        ITsJsDocComment WithThrowsTags(ImmutableArray<(string typeName, ITsJsDocBlock text)> value);

        ITsJsDocComment WithExampleTags(ImmutableArray<ITsJsDocBlock> value);

        ITsJsDocComment WithSeeTags(ImmutableArray<ITsJsDocBlock> value);
    }

    /// <summary>
    /// Service contract for a builder for <see cref="ITsJsDocComment"/> objects.
    /// </summary>
    public interface ITsJsDocCommentBuilder
    {
        ITsJsDocCommentBuilder SetDescription(string descriptionText);

        ITsJsDocCommentBuilder SetDescription(ITsJsDocBlock descriptionTag);

        ITsJsDocCommentBuilder SetSummaryTag(string summaryText);

        ITsJsDocCommentBuilder SetSummaryTag(ITsJsDocBlock summaryTag);

        ITsJsDocCommentBuilder SetFileTag(string fileText);

        ITsJsDocCommentBuilder SetFileTag(ITsJsDocBlock fileTag);

        ITsJsDocCommentBuilder SetCopyrightTag(string copyrightText);

        ITsJsDocCommentBuilder SetCopyrightTag(ITsJsDocBlock copyrightTag);

        ITsJsDocCommentBuilder SetIsPackagePrivate(bool isPackagePrivate);

        ITsJsDocCommentBuilder AddParamTag(string paramName, string paramText);

        ITsJsDocCommentBuilder AddParamTag(string paramName, ITsJsDocBlock paramTag);

        ITsJsDocCommentBuilder AddTypeParamTag(string paramName, string paramText);

        ITsJsDocCommentBuilder AddTypeParamTag(string paramName, ITsJsDocBlock paramTag);

        ITsJsDocCommentBuilder SetReturnsTag(string returnsText);

        ITsJsDocCommentBuilder SetReturnsTag(ITsJsDocBlock returnsTag);

        ITsJsDocCommentBuilder AddThrowsTag(string typeName, string throwsText);

        ITsJsDocCommentBuilder AddThrowsTag(string typeName, ITsJsDocBlock throwsTag);

        ITsJsDocCommentBuilder AddExampleTag(string exampleText);

        ITsJsDocCommentBuilder AddExampleTag(ITsJsDocBlock exampleTag);

        ITsJsDocCommentBuilder AddSeeTag(string seeText);

        ITsJsDocCommentBuilder AddSeeTag(ITsJsDocBlock seeTag);

        ITsJsDocComment Build();
    }

    /// <summary>
    /// Represents a JSDoc block tag, for example @see, @example, and description.
    /// </summary>
    public interface ITsJsDocBlock : IAstTriviaNode
    {
        ImmutableArray<ITsJsDocInlineContent> Content { get; }

        ITsJsDocBlock WithAppendedContent(string text, bool separateWithBlankLine);

        ITsJsDocBlock WithAppendedContent(ITsJsDocInlineContent text, bool separateWithBlankLine);

        ITsJsDocBlock WithAppendedContent(ITsJsDocBlock block, bool separateWithBlankLine);
    }

    /// <summary>
    /// Represents either plain text or structured inline contents within a JSDoc block tag.
    /// </summary>
    public interface ITsJsDocInlineContent : IAstTriviaNode
    {
        /// <summary>
        /// Returns a value indicating whether this content node is empty.
        /// </summary>
        bool IsEmpty { get; }
    }

    /// <summary>
    /// Represents plain text as content within a JSDoc block tag.
    /// </summary>
    public interface ITsJsDocInlineText : ITsJsDocInlineContent
    {
        string Text { get; }
    }

    /// <summary>
    /// Represents a JSDoc inline @link tag of the format '{@link NamespaceOrUrl}' or '[Text]{@link NamespaceOrUrl}'.
    /// </summary>
    public interface ITsJsDocLinkTag : ITsJsDocInlineContent
    {
        string NamepathOrUrl { get; }
        string Text { get; }
    }
}
