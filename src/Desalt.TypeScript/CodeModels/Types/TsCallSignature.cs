// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCallSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a call signature of the form '&gt;T&lt;(parameter: type): type'.
    /// </summary>
    internal class TsCallSignature : CodeModel, ITsCallSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCallSignature(
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsParameterList parameters = null,
            ITsType typeAnnotation = null)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
            Parameters = parameters;
            TypeAnnotation = typeAnnotation;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
        public ITsParameterList Parameters { get; }
        public ITsType TypeAnnotation { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitCallSignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitCallSignature(this);

        public override string ToCodeDisplay()
        {
            string code = string.Empty;

            if (TypeParameters.Length == 0)
            {
                code += $"<{TypeParameters.ToElidedList()}>";
            }

            code += $"{Parameters?.ToCodeDisplay()}: {TypeAnnotation}";

            return code;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (TypeParameters.Length > 0)
            {
                WriteItems(writer, TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            Parameters?.WriteFullCodeDisplay(writer);
            TypeAnnotation.WriteTypeAnnotation(writer);
        }
    }
}
