// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsWhitespaceTrivia.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Lexical
{
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents whitespace that can appear before or after another <see cref="IAstNode"/>.
    /// </summary>
    internal class TsWhitespaceTrivia : AstTriviaNode, ITsWhitespaceTrivia
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsWhitespaceTrivia Newline = new TsWhitespaceTrivia(isNewline: true, text: "\n");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsWhitespaceTrivia(bool isNewline, string text)
            : base(preserveSpacing: true)
        {
            IsNewline = isNewline;
            Text = text;
        }

        public static TsWhitespaceTrivia Create(string text) => new TsWhitespaceTrivia(isNewline: false, text: text);

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsNewline { get; }
        public string Text { get; }

        public override string CodeDisplay => Text;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Emit(Emitter emitter)
        {
            if (IsNewline)
            {
                emitter.WriteLine();
            }
            else
            {
                emitter.Write(Text);
            }
        }
    }
}
