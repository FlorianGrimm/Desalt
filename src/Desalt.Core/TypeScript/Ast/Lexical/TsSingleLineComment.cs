// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSingleLineComment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Lexical
{
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript single-line comment of the form '// comment'.
    /// </summary>
    internal class TsSingleLineComment : AstTriviaNode, ITsSingleLineComment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSingleLineComment(string comment, bool preserveSpacing = false, bool omitNewLineAtEnd = false)
            : base(preserveSpacing)
        {
            Comment = comment;
            OmitNewLineAtEnd = omitNewLineAtEnd;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Comment { get; }

        public bool OmitNewLineAtEnd { get; }

        public override string CodeDisplay => $"// {Comment}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Emit(Emitter emitter)
        {
            if (OmitNewLineAtEnd)
            {
                emitter.Write($"//{Space}{Comment}");
            }
            else
            {
                emitter.WriteLine($"//{Space}{Comment}");
            }
        }
    }
}
