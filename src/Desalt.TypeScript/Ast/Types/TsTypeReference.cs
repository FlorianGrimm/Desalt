// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeReference.cs" company="Justin Rockwood">
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
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript type reference.
    /// </summary>
    internal class TsTypeReference : AstNode, ITsTypeReference
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeReference(ITsTypeName typeName, IEnumerable<ITsType> typeArguments = null)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            TypeArguments = typeArguments?.ToImmutableArray() ?? ImmutableArray<ITsType>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeName TypeName { get; }
        public ImmutableArray<ITsType> TypeArguments { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitTypeReference(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitTypeReference(this);

        public override string CodeDisplay
        {
            get
            {
                if (TypeArguments.Length == 0)
                {
                    return TypeName.CodeDisplay;
                }

                return $"{TypeName}<{TypeArguments.ToElidedList()}>";
            }
        }

        public override void Emit(IndentedTextWriter emitter)
        {
            TypeName.Emit(emitter);

            if (TypeArguments.Length > 0)
            {
                WriteItems(emitter, TypeArguments, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }
        }
    }
}
