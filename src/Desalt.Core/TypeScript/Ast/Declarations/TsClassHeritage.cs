// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassHeritage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a class heritage of the form ' extends type implements type, type'.
    /// </summary>
    internal class TsClassHeritage : AstNode<TsVisitor>, ITsClassHeritage
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassHeritage(
            ITsTypeReference extendsClause = null,
            IEnumerable<ITsTypeReference> implementsClause = null)
        {
            ExtendsClause = extendsClause;
            ImplementsClause = implementsClause?.ToImmutableArray() ?? ImmutableArray<ITsTypeReference>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeReference ExtendsClause { get; }
        public ImmutableArray<ITsTypeReference> ImplementsClause { get; }

        public bool IsEmpty => ExtendsClause == null && ImplementsClause.IsEmpty;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitClassHeritage(this);

        public override string CodeDisplay =>
            (ExtendsClause != null ? $" extends {ExtendsClause}" : "") +
            (ImplementsClause.IsEmpty ? "" : $" implements {ImplementsClause.ToElidedList()}");

        public override void Emit(Emitter emitter)
        {
            if (ExtendsClause != null)
            {
                emitter.Write(" extends ");
                ExtendsClause.Emit(emitter);
            }

            if (!ImplementsClause.IsEmpty)
            {
                emitter.Write(" implements ");
                emitter.WriteList(ImplementsClause, indent: false, itemDelimiter: ", ");
            }
        }
    }
}
