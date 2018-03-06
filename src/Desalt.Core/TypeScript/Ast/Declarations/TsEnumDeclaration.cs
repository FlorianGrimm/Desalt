// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEnumDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an enum declaration.
    /// </summary>
    internal class TsEnumDeclaration : AstNode, ITsEnumDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsEnumDeclaration(
            ITsIdentifier enumName,
            IEnumerable<ITsEnumMember> enumBody = null,
            bool isConst = false)
        {
            EnumName = enumName ?? throw new ArgumentNullException(nameof(enumName));
            EnumBody = enumBody?.ToImmutableArray() ?? ImmutableArray<ITsEnumMember>.Empty;
            IsConst = isConst;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsConst { get; }
        public ITsIdentifier EnumName { get; }
        public ImmutableArray<ITsEnumMember> EnumBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitEnumDeclaration(this);

        public override string CodeDisplay =>
            (IsConst ? "const " : "") + $"enum {EnumName} {{ {EnumBody.ToElidedList()} }}";

        protected override void EmitInternal(Emitter emitter)
        {
            if (IsConst)
            {
                emitter.WriteKeyword("const");
            }

            emitter.WriteKeyword("enum");
            EnumName.Emit(emitter);
            emitter.Write(" ");

            emitter.WriteList(
                EnumBody, indent: true, prefix: "{", suffix: "}",
                itemDelimiter: "," + emitter.Options.Newline,
                newLineBeforeFirstItem: true, newLineAfterLastItem: true,
                delimiterAfterLastItem: true,
                emptyContents: $"{{{emitter.Options.Newline}}}");
            emitter.WriteLine();
        }
    }
}
