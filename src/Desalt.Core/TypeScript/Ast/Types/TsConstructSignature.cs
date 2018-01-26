// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsConstructSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a constructor method signature of the form 'new &lt;T&gt;(parameter: type): type'.
    /// </summary>
    internal class TsConstructSignature : AstNode, ITsConstructSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsConstructSignature(
            ITsTypeParameters typeParameters = null,
            ITsParameterList parameterList = null,
            ITsType returnType = null)
        {
            TypeParameters = typeParameters ?? TsTypeParameters.Empty;
            ParameterList = parameterList;
            ReturnType = returnType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeParameters TypeParameters { get; }
        public ITsParameterList ParameterList { get; }
        public ITsType ReturnType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitConstructSignature(this);

        public override string CodeDisplay =>
            $"new {TypeParameters}(${ParameterList}){ReturnType.OptionalTypeAnnotation()}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("new ");
            TypeParameters.Emit(emitter);
            emitter.Write("(");
            ParameterList.Emit(emitter);
            emitter.Write(")");

            ReturnType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
