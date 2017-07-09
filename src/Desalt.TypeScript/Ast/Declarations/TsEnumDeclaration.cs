// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEnumDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an enum declaration.
    /// </summary>
    internal class TsEnumDeclaration : AstNode<TsVisitor>, ITsEnumDeclaration
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

        public override void Emit(Emitter emitter)
        {
            if (IsConst)
            {
                emitter.Write("const ");
            }

            emitter.Write("enum ");
            EnumName.Emit(emitter);
            emitter.Write(" ");

            emitter.WriteItems(
                EnumBody, indent: true, prefix: "{", suffix: "}",
                itemDelimiter: "," + emitter.Options.Newline,
                newLineBeforeFirstItem: true, newLineAfterLastItem: true,
                emptyContents: "{ }");
            emitter.WriteLine();
        }
    }
}
