// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTupleType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript tuple type.
    /// </summary>
    internal class TsTupleType : AstNode, ITsTupleType
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

        public void Accept(TsVisitor visitor) => visitor.VisitTupleType(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitTupleType(this);

        public override string CodeDisplay => $"[{ElementTypes.ToElidedList()}]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(writer, ElementTypes, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }
    }
}
