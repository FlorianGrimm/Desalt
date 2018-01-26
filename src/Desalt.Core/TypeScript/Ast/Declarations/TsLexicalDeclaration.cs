// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexicalDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a lexical declaration of the form 'const|let x: type, y: type = z;'.
    /// </summary>
    internal class TsLexicalDeclaration : AstNode<TsVisitor>, ITsLexicalDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsLexicalDeclaration(bool isConst, IEnumerable<ITsLexicalBinding> declarations)
        {
            IsConst = isConst;
            Declarations = declarations?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(declarations));
            if (Declarations.IsEmpty)
            {
                throw new ArgumentException("There must be at least one declaration", nameof(declarations));
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsConst { get; }
        public ImmutableArray<ITsLexicalBinding> Declarations { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitLexicalDeclaration(this);

        public override string CodeDisplay => (IsConst ? "const " : "let ") + $"{Declarations.ToElidedList()};";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write(IsConst ? "const " : "let ");
            emitter.WriteList(Declarations, indent: false, itemDelimiter: ", ");
            emitter.WriteLine(";");
        }
    }
}
