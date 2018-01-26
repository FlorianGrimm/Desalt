// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsUnionOrIntersectionType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a union type of the form 'type1 | type2' or an intersection type of the form
    /// 'type1 &amp; type2'.
    /// </summary>
    internal class TsUnionOrIntersectionType : AstNode, ITsUnionType, ITsIntersectionType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsUnionOrIntersectionType(ITsType type1, ITsType type2, IEnumerable<ITsType> otherTypes, bool isUnion)
        {
            var list = new List<ITsType>
            {
                type1 ?? throw new ArgumentNullException(nameof(type1)),
                type2 ?? throw new ArgumentNullException(nameof(type2))
            };
            list.AddRange(otherTypes);

            Types = list.ToImmutableArray();
            IsUnion = isUnion;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsType> Types { get; }
        public bool IsUnion { get; }

        private string Delimiter => IsUnion ? " | " : " & ";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            if (IsUnion)
            {
                visitor.VisitUnionType(this);
            }
            else
            {
                visitor.VisitIntersectionType(this);
            }
        }

        public override string CodeDisplay => Types.ToElidedList(Delimiter);

        protected override void EmitInternal(Emitter emitter) => emitter.WriteList(Types, indent: false, itemDelimiter: Delimiter);
    }
}
