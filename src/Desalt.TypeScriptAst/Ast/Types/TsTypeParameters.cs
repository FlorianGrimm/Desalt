// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeParameters.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a list of type parameters of the form '&lt;type, type&gt;'.
    /// </summary>
    internal class TsTypeParameters : TsAstNode, ITsTypeParameters
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsTypeParameters Empty = new TsTypeParameters();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeParameters(params ITsTypeParameter[] typeParameters)
        {
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitTypeParameters(this);
        }

        public override string CodeDisplay => TypeParameters.IsEmpty ? "" : $"<{TypeParameters.ToElidedList()}>";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.WriteList(
                TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ", emptyContents: "");
        }
    }
}
