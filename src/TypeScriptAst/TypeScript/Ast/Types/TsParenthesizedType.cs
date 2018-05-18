// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParenthesizedType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Types
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a parenthesized type, of the form '(Type)'.
    /// </summary>
    internal class TsParenthesizedType : AstNode, ITsParenthesizedType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsParenthesizedType(ITsType type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsType Type { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitParenthesizedType(this);

        public override string CodeDisplay => $"({Type.CodeDisplay})";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("(");
            Type.Emit(emitter);
            emitter.Write(")");
        }
    }
}
