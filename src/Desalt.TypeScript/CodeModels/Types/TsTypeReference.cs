// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeReference.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript type reference.
    /// </summary>
    internal class TsTypeReference : CodeModel, ITsTypeReference
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitTypeReference(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitTypeReference(this);

        public override string ToCodeDisplay()
        {
            if (TypeArguments.Length == 0)
            {
                return TypeName.ToCodeDisplay();
            }

            return $"{TypeName}<{TypeArguments.ToElidedList()}>";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            TypeName.WriteFullCodeDisplay(writer);

            if (TypeArguments.Length > 0)
            {
                WriteItems(writer, TypeArguments, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }
        }
    }
}
