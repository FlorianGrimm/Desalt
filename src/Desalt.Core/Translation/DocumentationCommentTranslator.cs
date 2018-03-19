// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using Desalt.Core.Pipeline;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Converts a CSharp XML documentation comment into a TypeScript JsDoc comment.
    /// </summary>
    internal partial class DocumentationCommentTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Converts a CSharp XML documentation comment into a TypeScript JsDoc comment.
        /// </summary>
        /// <param name="commentNode">The node to translate.</param>
        public static IExtendedResult<ITsJsDocComment> Translate(DocumentationCommentTriviaSyntax commentNode)
        {
            if (commentNode == null)
            {
                throw new ArgumentNullException(nameof(commentNode));
            }

            var walker = new Walker();
            walker.VisitDocumentationCommentTrivia(commentNode);
            return new ExtendedResult<ITsJsDocComment>(walker.TranslatedComment, walker.Diagnostics);
        }
    }
}
