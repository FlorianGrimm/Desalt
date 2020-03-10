// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTupleType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript tuple type.
    /// </summary>
    internal class TsTupleType : TsAstNode, ITsTupleType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTupleType(ITsType elementType, params ITsType[] elementTypes)
        {
            if (elementType == null) { throw new ArgumentNullException(nameof(elementType)); }
            ElementTypes = new[] { elementType }.Concat(elementTypes ?? new ITsType[0]).ToImmutableArray();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsType> ElementTypes { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTupleType(this);

        public override string CodeDisplay => $"[{ElementTypes.ToElidedList()}]";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.WriteList(ElementTypes, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
