// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocCommentBuilder.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Lexical
{
    using System.Collections.Generic;
    using System.Linq;
    using Factory = TsAstFactory;

    /// <summary>
    /// Service contract for a builder for <see cref="ITsJsDocComment"/> objects.
    /// </summary>
    internal class TsJsDocCommentBuilder : ITsJsDocCommentBuilder
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private ITsJsDocBlock _description;
        private ITsJsDocBlock _summaryTag;
        private ITsJsDocBlock _fileTag;
        private ITsJsDocBlock _copyrightTag;
        private ITsJsDocBlock _returnsTag;
        private bool _isPackagePrivate;

        private List<(string paramName, ITsJsDocBlock paramTag)> _paramTags;
        private List<(string paramName, ITsJsDocBlock paramTag)> _typeparamTags;
        private List<(string typeName, ITsJsDocBlock throwsTag)> _throwsTags;
        private List<ITsJsDocBlock> _exampleTags;
        private List<ITsJsDocBlock> _seeTags;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public ITsJsDocCommentBuilder PrependDescription(ITsJsDocBlock text, bool separateWithBlankLine)
        {
            if (_description == null)
            {
                _description = text;
                return this;
            }

            var newContentBuilder = text.Content.ToBuilder();
            if (separateWithBlankLine && !_description.Content.IsEmpty)
            {
                newContentBuilder.Add(Factory.JsDocInlineText("\n"));
            }

            newContentBuilder.AddRange(_description.Content);
            _description = Factory.JsDocBlock(newContentBuilder.ToArray());
            return this;
        }

        public ITsJsDocCommentBuilder AppendDescription(string text, bool separateWithBlankLine)
        {
            _description = _description?.WithAppendedContent(text, separateWithBlankLine) ??
                Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder AppendDescription(ITsJsDocBlock block, bool separateWithBlankLine)
        {
            _description = _description?.WithAppendedContent(block, separateWithBlankLine) ?? block;
            return this;
        }

        public ITsJsDocCommentBuilder SetDescription(string text)
        {
            _description = string.IsNullOrEmpty(text) ? null : Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder SetDescription(ITsJsDocBlock text) =>
            Set(ref _description, text);

        public ITsJsDocCommentBuilder SetSummaryTag(string text)
        {
            _summaryTag = string.IsNullOrEmpty(text) ? null : Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder SetSummaryTag(ITsJsDocBlock text) => Set(ref _summaryTag, text);

        public ITsJsDocCommentBuilder SetFileTag(string text)
        {
            _fileTag = string.IsNullOrEmpty(text) ? null : Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder SetFileTag(ITsJsDocBlock text) => Set(ref _fileTag, text);

        public ITsJsDocCommentBuilder SetCopyrightTag(string text)
        {
            _copyrightTag = string.IsNullOrEmpty(text) ? null : Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder SetCopyrightTag(ITsJsDocBlock text) =>
            Set(ref _copyrightTag, text);

        public ITsJsDocCommentBuilder SetIsPackagePrivate(bool value)
        {
            _isPackagePrivate = value;
            return this;
        }

        public ITsJsDocCommentBuilder AddParamTag(string name, string text) =>
            AddParamTag(name, Factory.JsDocBlock(text ?? string.Empty));

        public ITsJsDocCommentBuilder AddParamTag(string name, ITsJsDocBlock text)
        {
            if (string.IsNullOrEmpty(name))
            {
                return this;
            }

            if (_paramTags == null)
            {
                _paramTags = new List<(string paramName, ITsJsDocBlock paramTag)>();
            }

            _paramTags.Add((name, text ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder AddTypeParamTag(string name, string text) =>
            AddTypeParamTag(name, Factory.JsDocBlock(text ?? string.Empty));

        public ITsJsDocCommentBuilder AddTypeParamTag(string name, ITsJsDocBlock text)
        {
            if (string.IsNullOrEmpty(name))
            {
                return this;
            }

            if (_typeparamTags == null)
            {
                _typeparamTags = new List<(string paramName, ITsJsDocBlock paramTag)>();
            }

            _typeparamTags.Add((name, text ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder SetReturnsTag(string text)
        {
            _returnsTag = string.IsNullOrEmpty(text) ? null : Factory.JsDocBlock(text);
            return this;
        }

        public ITsJsDocCommentBuilder SetReturnsTag(ITsJsDocBlock text) => Set(ref _returnsTag, text);

        public ITsJsDocCommentBuilder AddThrowsTag(string typeName, string text) =>
            AddThrowsTag(typeName, Factory.JsDocBlock(text ?? string.Empty));

        public ITsJsDocCommentBuilder AddThrowsTag(string typeName, ITsJsDocBlock text)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return this;
            }

            if (_throwsTags == null)
            {
                _throwsTags = new List<(string typeName, ITsJsDocBlock throwsTag)>();
            }

            _throwsTags.Add((typeName, text ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder AddExampleTag(string text) =>
            AddExampleTag(Factory.JsDocBlock(text ?? string.Empty));

        public ITsJsDocCommentBuilder AddExampleTag(ITsJsDocBlock text)
        {
            Set(ref text, text);
            if (text == null)
            {
                return this;
            }

            if (_exampleTags == null)
            {
                _exampleTags = new List<ITsJsDocBlock>();
            }

            _exampleTags.Add(text);
            return this;
        }

        public ITsJsDocCommentBuilder AddSeeTag(string text) =>
            AddSeeTag(Factory.JsDocBlock(text ?? string.Empty));

        public ITsJsDocCommentBuilder AddSeeTag(ITsJsDocBlock text)
        {
            Set(ref text, text);
            if (text == null)
            {
                return this;
            }

            if (_seeTags == null)
            {
                _seeTags = new List<ITsJsDocBlock>();
            }

            _seeTags.Add(text);
            return this;
        }

        public ITsJsDocComment Build()
        {
            return new TsJsDocComment(
                fileTag: _fileTag,
                copyrightTag: _copyrightTag,
                isPackagePrivate: _isPackagePrivate,
                paramsTags: _paramTags,
                typeParamTags: _typeparamTags,
                returnsTag: _returnsTag,
                throwsTags: _throwsTags,
                exampleTags: _exampleTags,
                description: _description,
                summaryTag: _summaryTag,
                seeTags: _seeTags);
        }

        // ReSharper disable once RedundantAssignment
        private ITsJsDocCommentBuilder Set(ref ITsJsDocBlock tag, ITsJsDocBlock value)
        {
            if (value == null)
            {
                tag = null;
            }
            else if (value.Content.IsEmpty || value.Content.All(x => x.IsEmpty))
            {
                tag = null;
            }
            else
            {
                tag = value;
            }

            return this;
        }
    }
}
