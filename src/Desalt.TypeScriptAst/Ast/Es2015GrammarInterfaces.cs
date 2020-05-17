// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es2015GrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;

    /***********************************************************************************************
     * Ecma-262 6.0 (ES 2015) Grammar
     * -------------------------------------------------------------------------
     * See http://www.ecma-international.org/ecma-262/6.0/
     **********************************************************************************************/

    /* 11.4 Comments
     * -------------
     * Comment:
     *   MultiLineComment
     *   SingleLineComment
     *
     * MultiLineComment:
     *   slash* MultiLineCommentCharsOpt *slash
     *
     * MultiLineCommentChars:
     *   MultiLineNotAsteriskChar MultiLineCommentCharsOpt
     *   * PostAsteriskCommentCharsOpt
     *
     * PostAsteriskCommentChars:
     *   MultiLineNotForwardSlashOrAsteriskChar MultiLineCommentCharsOpt
     *   *PostAsteriskCommentCharsOpt
     *
     * MultiLineNotAsteriskChar:
     *   SourceCharacter but not *
     *
     * MultiLineNotForwardSlashOrAsteriskChar:
     *   SourceCharacter but not one of / or *
     *
     * SingleLineComment:
     *   // SingleLineCommentCharsOpt
     *
     * SingleLineCommentChars:
     *   SingleLineCommentChar SingleLineCommentCharsOpt
     *
     * SingleLineCommentChar:
     *   SourceCharacter but not LineTerminator
     */

    /// <summary>
    /// Represents whitespace that can appear before or after another <see cref="ITsAstNode"/>.
    /// </summary>
    public interface ITsWhitespaceTrivia : ITsAstTriviaNode
    {
        string Text { get; }
        bool IsNewline { get; }
    }

    /// <summary>
    /// Represents a TypeScript multi-line comment of the form '/* lines */'.
    /// </summary>
    public interface ITsMultiLineComment : ITsAstTriviaNode
    {
        /// <summary>
        /// Indicates whether the comment should start with /** (JsDoc) or /*.
        /// </summary>
        bool IsJsDoc { get; }

        ImmutableArray<string> Lines { get; }
    }

    /// <summary>
    /// Represents a TypeScript single-line comment of the form '// comment'.
    /// </summary>
    public interface ITsSingleLineComment : ITsAstTriviaNode
    {
        string Text { get; }
    }
}
