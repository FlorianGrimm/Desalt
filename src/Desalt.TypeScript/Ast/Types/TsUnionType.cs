// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsUnionType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a union type of the form 'type1 | type2'.
    /// </summary>
    internal class TsUnionType : AstNode<TsVisitor>, ITsUnionType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsUnionType(ITsType type1, ITsType type2, params ITsType[] otherTypes)
        {
            var list = new List<ITsType>
            {
                type1 ?? throw new ArgumentNullException(nameof(type1)),
                type2 ?? throw new ArgumentNullException(nameof(type2))
            };
            list.AddRange(otherTypes);

            Types = list.ToImmutableArray();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsType> Types { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitUnionType(this);

        public override string CodeDisplay => Types.ToElidedList(" | ");

        public override void Emit(Emitter emitter) => emitter.WriteItems(Types, indent: false, itemDelimiter: " | ");
    }
}
