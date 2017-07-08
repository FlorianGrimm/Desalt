// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassHeritage.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
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
            ITsTypeReference extendsClass = null,
            IEnumerable<ITsTypeReference> implementsTypes = null)
        {
            ExtendsClass = extendsClass;
            ImplementsTypes = implementsTypes?.ToImmutableArray() ?? ImmutableArray<ITsTypeReference>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeReference ExtendsClass { get; }
        public ImmutableArray<ITsTypeReference> ImplementsTypes { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitClassHeritage(this);

        public override string CodeDisplay =>
            (ExtendsClass != null ? $" extends {ExtendsClass}" : "") +
            (ImplementsTypes.IsEmpty ? "" : $" implements {ImplementsTypes.ToElidedList()}");

        public override void Emit(Emitter emitter)
        {
            if (ExtendsClass != null)
            {
                emitter.Write(" extends ");
                ExtendsClass.Emit(emitter);
            }

            if (!ImplementsTypes.IsEmpty)
            {
                emitter.Write(" implements ");
                emitter.WriteItems(ImplementsTypes, indent: false, itemDelimiter: ", ");
            }
        }
    }
}
