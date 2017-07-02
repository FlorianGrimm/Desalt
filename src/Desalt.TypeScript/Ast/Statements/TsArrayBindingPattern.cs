// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayBindingPattern.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Statements
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an array binding pattern of the form '[x = y, z, ...p]'.
    /// </summary>
    internal class TsArrayBindingPattern : AstNode<TsVisitor>, ITsArrayBindingPattern
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayBindingPattern(IEnumerable<ITsBindingElement> elements, ITsIdentifier restElement = null)
        {
            Elements = elements?.ToImmutableArray() ?? ImmutableArray<ITsBindingElement>.Empty;
            RestElement = restElement;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsBindingElement> Elements { get; }
        public ITsIdentifier RestElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitArrayBindingPattern(this);

        public override string CodeDisplay =>
            "[" + Elements.ToElidedList() + (RestElement != null ? $", ... {RestElement}" : "") + "]";

        public override void Emit(Emitter emitter)
        {
            if (RestElement != null)
            {
                emitter.Write("[");
                emitter.WriteItems(Elements, indent: false, itemDelimiter: ", ", delimiterAfterLastItem: true);
                emitter.Write("... ");
                RestElement.Emit(emitter);
                emitter.Write("]");
            }
            else
            {
                emitter.WriteItems(
                    Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ", emptyContents: "[]");
            }
        }
    }
}
