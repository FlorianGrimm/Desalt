// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsConstructSignature.cs" company="Justin Rockwood">
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
    /// Represents a constructor method signature of the form 'new &lt;T&gt;(parameter: type): type'.
    /// </summary>
    internal class TsConstructSignature : AstNode<TsVisitor>, ITsConstructSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsConstructSignature(
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsParameterList parameterList = null,
            ITsType returnType = null)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
            ParameterList = parameterList;
            ReturnType = returnType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
        public ITsParameterList ParameterList { get; }
        public ITsType ReturnType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitConstructSignature(this);

        public override string CodeDisplay
        {
            get
            {
                string display = "new ";

                if (TypeParameters.Length > 0)
                {
                    display += $"<{TypeParameters.ToElidedList()}>";
                }

                display += $"(${ParameterList}){ReturnType.OptionalTypeAnnotation()}";
                return display;
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("new ");

            if (TypeParameters.Length > 0)
            {
                emitter.WriteItems(TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            emitter.Write("(");
            ParameterList.Emit(emitter);
            emitter.Write(")");

            ReturnType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
