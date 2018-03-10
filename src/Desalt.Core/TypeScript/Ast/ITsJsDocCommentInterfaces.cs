// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsJsDocCommentInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    /// <summary>
    /// Represents either plain text or structured inline contents within a JSDoc block tag.
    /// </summary>
    public interface ITsJsDocInlineContent : IAstTriviaNode
    {
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
