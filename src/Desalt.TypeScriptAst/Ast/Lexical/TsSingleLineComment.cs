// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSingleLineComment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Lexical
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript single-line comment of the form '// comment'.
    /// </summary>
    internal class TsSingleLineComment : TsAstTriviaNode, ITsSingleLineComment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSingleLineComment(string comment, bool preserveSpacing = false, bool omitNewLineAtEnd = false)
            : base(preserveSpacing)
        {
            Text = comment;
            OmitNewLineAtEnd = omitNewLineAtEnd;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Text { get; }

        public bool OmitNewLineAtEnd { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Emit(Emitter emitter)
        {
            if (OmitNewLineAtEnd)
            {
                emitter.Write($"//{Space}{Text}");
            }
            else
            {
                emitter.WriteLine($"//{Space}{Text}");
            }
        }
    }
}
