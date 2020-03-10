// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsObjectType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript object type.
    /// </summary>
    internal class TsObjectType : TsAstNode, ITsObjectType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsObjectType(IEnumerable<ITsTypeMember> typeMembers = null, bool forceSingleLine = false)
        {
            TypeMembers = typeMembers?.ToImmutableArray() ?? ImmutableArray<ITsTypeMember>.Empty;
            ForceSingleLine = forceSingleLine;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeMember> TypeMembers { get; }

        public bool ForceSingleLine { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitObjectType(this);
        }

        public override string CodeDisplay => $"{{{TypeMembers.ToElidedList()}}}";

        protected override void EmitInternal(Emitter emitter)
        {
            bool multiLine = !ForceSingleLine;
            emitter.WriteList(
                TypeMembers,
                indent: true,
                prefix: ForceSingleLine ? "{ " : "{",
                suffix: ForceSingleLine ? " }" : "}",
                itemDelimiter: ";" + emitter.Options.Newline,
                delimiterAfterLastItem: multiLine,
                newLineAfterPrefix: multiLine,
                newLineAfterLastItem: multiLine,
                emptyContents: "{}");
        }
    }
}
