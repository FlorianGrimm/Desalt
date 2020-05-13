// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript array type.
    /// </summary>
    internal class TsArrayType : TsAstNode, ITsArrayType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayType(ITsType type)
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

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitArrayType(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            // special case: if we have an array of function types, we need to use the Array<T>
            // syntax instead of brackets, otherwise it will be (x: string) => bool[], which is a
            // function that returns a boolean array instead of an array of functions that return boolean.
            if (Type is ITsFunctionType)
            {
                emitter.Write("Array<");
                Type.Emit(emitter);
                emitter.Write(">");
            }
            else
            {
                Type.Emit(emitter);
                emitter.Write("[]");
            }
        }
    }
}
