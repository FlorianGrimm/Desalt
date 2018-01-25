// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCallSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a call signature of the form '&gt;T&lt;(parameter: type): type'.
    /// </summary>
    internal class TsCallSignature : AstNode<TsVisitor>, ITsCallSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCallSignature(
            ITsTypeParameters typeParameters = null,
            ITsParameterList parameters = null,
            ITsType returnType = null)
        {
            TypeParameters = typeParameters ?? TsTypeParameters.Empty;
            Parameters = parameters;
            ReturnType = returnType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeParameters TypeParameters { get; }
        public ITsParameterList Parameters { get; }
        public ITsType ReturnType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitCallSignature(this);

        public override string CodeDisplay => $"{TypeParameters}({Parameters}){ReturnType.OptionalTypeAnnotation()}";

        public override void Emit(Emitter emitter)
        {
            TypeParameters.Emit(emitter);
            emitter.Write("(");
            Parameters?.Emit(emitter);
            emitter.Write(")");

            ReturnType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
