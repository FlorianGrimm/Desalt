// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript function type.
    /// </summary>
    internal class TsFunctionType : CodeModel, ITsFunctionType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFunctionType(
            IEnumerable<ITsTypeParameter> typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
            Parameters = parameters;
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        }

        public TsFunctionType(ITsParameterList parameters, ITsType returnType)
            : this(null, parameters, returnType)
        {
        }

        public TsFunctionType(ITsType returnType)
            : this(null, null, returnType)
        {
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitFunctionType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitFunctionType(this);

        public override string ToCodeDisplay()
        {
            throw new System.NotImplementedException();
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (TypeParameters.Length > 0)
            {
                WriteItems(writer, TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            Parameters.WriteFullCodeDisplay(writer);
            writer.Write(" => ");
            ReturnType.WriteFullCodeDisplay(writer);
        }
    }
}
