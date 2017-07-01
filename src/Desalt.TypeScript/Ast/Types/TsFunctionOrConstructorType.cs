// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionOrConstructorType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a TypeScript function or constructor type.
    /// </summary>
    internal class TsFunctionOrConstructorType : AstNode<TsVisitor>, ITsFunctionType, ITsConstructorType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFunctionOrConstructorType(
            IEnumerable<ITsTypeParameter> typeParameters,
            ITsParameterList parameters,
            ITsType returnType,
            bool isConstructorType)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
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

        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
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

        public override string CodeDisplay
        {
            get
            {
                string code = string.Empty;

                if (IsConstructorType)
                {
                    code += "new ";
                }

                if (TypeParameters.Length == 0)
                {
                    code += $"<{TypeParameters.ToElidedList()}>";
                }

                code += $"{Parameters?.CodeDisplay} => {ReturnType}";

                return code;
            }
        }

        public override void Emit(Emitter emitter)
        {
            if (IsConstructorType)
            {
                emitter.Write("new ");
            }

            if (TypeParameters.Length > 0)
            {
                emitter.WriteItems(TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            Parameters?.Emit(emitter);
            emitter.Write(" => ");
            ReturnType.Emit(emitter);
        }
    }
}
