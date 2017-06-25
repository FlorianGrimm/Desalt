// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsObjectType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript object type.
    /// </summary>
    internal class TsObjectType : AstNode, ITsObjectType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsObjectType(IEnumerable<ITsTypeMember> typeMembers = null)
        {
            TypeMembers = typeMembers?.ToImmutableArray() ?? ImmutableArray<ITsTypeMember>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeMember> TypeMembers { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitObjectType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitObjectType(this);

        public override string ToCodeDisplay() => $"{{{TypeMembers.ToElidedList()}}}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(
                writer,
                TypeMembers,
                indent: true,
                prefix: "{", suffix: "}",
                itemDelimiter: ",",
                newLineAfterPrefix: true,
                delimiterAfterLastItem: false,
                newLineAfterLastItem: true);
        }
    }
}
