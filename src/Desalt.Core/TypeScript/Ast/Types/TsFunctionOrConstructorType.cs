// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionOrConstructorType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript function or constructor type.
    /// </summary>
    internal class TsFunctionOrConstructorType : AstNode, ITsFunctionType, ITsConstructorType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFunctionOrConstructorType(
            ITsTypeParameters typeParameters,
            ITsParameterList parameters,
            ITsType returnType,
            bool isConstructorType)
        {
            TypeParameters = typeParameters ?? TsTypeParameters.Empty;
            Parameters = parameters;
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            IsConstructorType = isConstructorType;
        }

        public TsFunctionOrConstructorType(ITsParameterList parameters, ITsType returnType, bool isConstructorType)
            : this(null, parameters, returnType, isConstructorType)
        {
        }

        public TsFunctionOrConstructorType(ITsType returnType, bool isConstructorType)
            : this(null, null, returnType, isConstructorType)
        {
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeParameters TypeParameters { get; }
        public ITsParameterList Parameters { get; }
        public ITsType ReturnType { get; }
        public bool IsConstructorType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            if (IsConstructorType)
            {
                visitor.VisitConstructorType(this);
            }
            else
            {
                visitor.VisitFunctionType(this);
            }
        }

        public override string CodeDisplay =>
            (IsConstructorType ? "new " : "") + $"{TypeParameters}({Parameters?.CodeDisplay}) => {ReturnType}";

        protected override void EmitInternal(Emitter emitter)
        {
            if (IsConstructorType)
            {
                emitter.Write("new ");
            }

            TypeParameters.Emit(emitter);
            emitter.Write("(");
            Parameters?.Emit(emitter);
            emitter.Write(")");

            emitter.Write(" => ");
            ReturnType.Emit(emitter);
        }
    }
}
