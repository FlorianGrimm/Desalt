// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMultiLineComment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Lexical
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript multi-line comment of the form '/* lines */'.
    /// </summary>
    internal class TsMultiLineComment : TsAstTriviaNode, ITsMultiLineComment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMultiLineComment(IEnumerable<string> lines, bool isJsDoc = false, bool preserveSpacing = false)
            : base(preserveSpacing)
        {
            Lines = lines.ToImmutableArray();
            IsJsDoc = isJsDoc;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether the comment should start with /** (JsDoc) or /*.
        /// </summary>
        public bool IsJsDoc { get; }

        public ImmutableArray<string> Lines { get; }

        private string Prefix => IsJsDoc ? "/**" : "/*";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Emit(Emitter emitter)
        {
            int count = Lines.Count();
            if (count == 0)
            {
                emitter.Write($"{Prefix}*/");
            }
            else if (count == 1 && !IsJsDoc)
            {
                emitter.Write($"{Prefix}{Space}{Lines.First()}{Space}*/");
            }
            else
            {
                if (IsJsDoc)
                {
                    emitter.WriteLine(Prefix);
                    emitter.WriteLine($" * {Lines.First()}");
                }
                else
                {
                    emitter.WriteLine($"{Prefix}{Space}{Lines.First()}");
                }

                foreach (string line in Lines.Skip(1))
                {
                    emitter.Write(" * ");
                    emitter.WriteLine(line);
                }

                emitter.WriteLine(" */");
            }
        }
    }
}
