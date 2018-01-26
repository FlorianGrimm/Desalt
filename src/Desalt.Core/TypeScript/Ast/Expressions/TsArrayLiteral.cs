// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an array literal of the form '[element...]'.
    /// </summary>
    internal class TsArrayLiteral : AstNode, ITsArrayLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal TsArrayLiteral(IEnumerable<ITsArrayElement> elements)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<ITsArrayElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsArrayElement> Elements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitArrayLiteral(this);

        public override string CodeDisplay => $"[{Elements.ToElidedList()}]";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.WriteList(Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
