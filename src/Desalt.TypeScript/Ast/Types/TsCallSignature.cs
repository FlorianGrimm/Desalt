// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCallSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a call signature of the form '&gt;T&lt;(parameter: type): type'.
    /// </summary>
    internal class TsCallSignature : AstNode, ITsCallSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCallSignature(
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsParameterList parameters = null,
            ITsType returnType = null)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
            Parameters = parameters;
            ReturnType = returnType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
        public ITsParameterList Parameters { get; }
        public ITsType ReturnType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitCallSignature(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitCallSignature(this);

        public override string CodeDisplay
        {
            get
            {
                string code = string.Empty;

                if (TypeParameters.Length == 0)
                {
                    code += $"<{TypeParameters.ToElidedList()}>";
                }

                code += $"{Parameters.CodeDisplay}: {ReturnType}";

                return code;
            }
        }

        public override void Emit(IndentedTextWriter emitter)
        {
            if (TypeParameters.Length > 0)
            {
                WriteItems(emitter, TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            Parameters?.Emit(emitter);
            ReturnType.WriteTypeAnnotation(emitter);
        }
    }
}
