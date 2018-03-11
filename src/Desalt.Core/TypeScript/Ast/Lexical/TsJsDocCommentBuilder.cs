// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocCommentBuilder.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
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

        public ITsJsDocCommentBuilder SetDescription(string descriptionText)
        {
            _description = string.IsNullOrEmpty(descriptionText) ? null : Factory.JsDocBlock(descriptionText);
            return this;
        }

        public ITsJsDocCommentBuilder SetDescription(ITsJsDocBlock descriptionTag) =>
            Set(ref _description, descriptionTag);

        public ITsJsDocCommentBuilder SetSummaryTag(string summaryText)
        {
            _summaryTag = string.IsNullOrEmpty(summaryText) ? null : Factory.JsDocBlock(summaryText);
            return this;
        }

        public ITsJsDocCommentBuilder SetSummaryTag(ITsJsDocBlock summaryTag) => Set(ref _summaryTag, summaryTag);

        public ITsJsDocCommentBuilder SetFileTag(string fileText)
        {
            _fileTag = string.IsNullOrEmpty(fileText) ? null : Factory.JsDocBlock(fileText);
            return this;
        }

        public ITsJsDocCommentBuilder SetFileTag(ITsJsDocBlock fileTag) => Set(ref _fileTag, fileTag);

        public ITsJsDocCommentBuilder SetCopyrightTag(string copyrightText)
        {
            _copyrightTag = string.IsNullOrEmpty(copyrightText) ? null : Factory.JsDocBlock(copyrightText);
            return this;
        }

        public ITsJsDocCommentBuilder SetCopyrightTag(ITsJsDocBlock copyrightTag) =>
            Set(ref _copyrightTag, copyrightTag);

        public ITsJsDocCommentBuilder SetIsPackagePrivate(bool isPackagePrivate)
        {
            _isPackagePrivate = isPackagePrivate;
            return this;
        }

        public ITsJsDocCommentBuilder AddParamTag(string paramName, string paramText) =>
            AddParamTag(paramName, Factory.JsDocBlock(paramText ?? string.Empty));

        public ITsJsDocCommentBuilder AddParamTag(string paramName, ITsJsDocBlock paramTag)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                return this;
            }

            if (_paramTags == null)
            {
                _paramTags = new List<(string paramName, ITsJsDocBlock paramTag)>();
            }

            _paramTags.Add((paramName, paramTag ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder AddTypeParamTag(string paramName, string paramText) =>
            AddTypeParamTag(paramName, Factory.JsDocBlock(paramText ?? string.Empty));

        public ITsJsDocCommentBuilder AddTypeParamTag(string paramName, ITsJsDocBlock paramTag)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                return this;
            }

            if (_typeparamTags == null)
            {
                _typeparamTags = new List<(string paramName, ITsJsDocBlock paramTag)>();
            }

            _typeparamTags.Add((paramName, paramTag ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder SetReturnsTag(string returnsText)
        {
            _returnsTag = string.IsNullOrEmpty(returnsText) ? null : Factory.JsDocBlock(returnsText);
            return this;
        }

        public ITsJsDocCommentBuilder SetReturnsTag(ITsJsDocBlock returnsTag) => Set(ref _returnsTag, returnsTag);

        public ITsJsDocCommentBuilder AddThrowsTag(string typeName, string throwsText) =>
            AddThrowsTag(typeName, Factory.JsDocBlock(throwsText ?? string.Empty));

        public ITsJsDocCommentBuilder AddThrowsTag(string typeName, ITsJsDocBlock throwsTag)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return this;
            }

            if (_throwsTags == null)
            {
                _throwsTags = new List<(string typeName, ITsJsDocBlock throwsTag)>();
            }

            _throwsTags.Add((typeName, throwsTag ?? Factory.JsDocBlock(string.Empty)));
            return this;
        }

        public ITsJsDocCommentBuilder AddExampleTag(string exampleText) =>
            AddExampleTag(Factory.JsDocBlock(exampleText ?? string.Empty));

        public ITsJsDocCommentBuilder AddExampleTag(ITsJsDocBlock exampleTag)
        {
            Set(ref exampleTag, exampleTag);
            if (exampleTag == null)
            {
                return this;
            }

            if (_exampleTags == null)
            {
                _exampleTags = new List<ITsJsDocBlock>();
            }

            _exampleTags.Add(exampleTag);
            return this;
        }

        public ITsJsDocCommentBuilder AddSeeTag(string seeText) =>
            AddSeeTag(Factory.JsDocBlock(seeText ?? string.Empty));

        public ITsJsDocCommentBuilder AddSeeTag(ITsJsDocBlock seeTag)
        {
            Set(ref seeTag, seeTag);
            if (seeTag == null)
            {
                return this;
            }

            if (_seeTags == null)
            {
                _seeTags = new List<ITsJsDocBlock>();
            }

            _seeTags.Add(seeTag);
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
            else if (value.Content.All(x => string.IsNullOrEmpty(x.Text)))
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
