// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsObjectType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript object type.
    /// </summary>
    internal class TsObjectType : AstNode<TsVisitor>, ITsObjectType
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

        public override void Accept(TsVisitor visitor) => visitor.VisitObjectType(this);

        public override string CodeDisplay => $"{{{TypeMembers.ToElidedList()}}}";

        public override void Emit(Emitter emitter) =>
            emitter.WriteList(
                TypeMembers,
                indent: true,
                prefix: "{", suffix: "}",
                itemDelimiter: ";" + emitter.Options.Newline,
                delimiterAfterLastItem: true,
                newLineAfterPrefix: true,
                newLineAfterLastItem: true,
                emptyContents: "{}");
    }
}
