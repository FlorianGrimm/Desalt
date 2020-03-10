// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTemplateLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a template literal of the form `string${Expression}`.
    /// </summary>
    internal class TsTemplateLiteral : TsAstNode, ITsTemplateLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTemplateLiteral(IEnumerable<ITsTemplatePart> parts)
        {
            Parts = parts?.ToImmutableArray() ?? ImmutableArray<ITsTemplatePart>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTemplatePart> Parts { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTemplateLiteral(this);

        public override string CodeDisplay => $"`{Parts.ToElidedList()}`";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("`");
            emitter.WriteList(Parts, indent: false);
            emitter.Write("`");
        }
    }
}
