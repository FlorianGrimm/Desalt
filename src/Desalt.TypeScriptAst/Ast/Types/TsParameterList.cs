// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParameterList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a parameter list of the form '(parameter: type)'.
    /// </summary>
    internal class TsParameterList : TsAstNode, ITsParameterList
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsParameterList Empty = new TsParameterList();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsParameterList(
            IEnumerable<ITsRequiredParameter>? requiredParameters = null,
            IEnumerable<ITsOptionalParameter>? optionalParameters = null,
            ITsRestParameter? restParameter = null)
        {
            RequiredParameters = requiredParameters?.ToImmutableArray() ?? ImmutableArray<ITsRequiredParameter>.Empty;
            OptionalParameters = optionalParameters?.ToImmutableArray() ?? ImmutableArray<ITsOptionalParameter>.Empty;
            RestParameter = restParameter;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsRequiredParameter> RequiredParameters { get; }
        public ImmutableArray<ITsOptionalParameter> OptionalParameters { get; }
        public ITsRestParameter? RestParameter { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitParameterList(this);
        }

        public override string CodeDisplay =>
            RequiredParameters.ToElidedList() +
            (RequiredParameters.Length > 0 && (OptionalParameters.Length > 0 || RestParameter != null) ? ", " : "") +
            OptionalParameters.ToElidedList() +
            (OptionalParameters.Length > 0 && RestParameter != null ? ", " : "") +
            RestParameter?.CodeDisplay;

        protected override void EmitContent(Emitter emitter)
        {
            if (RequiredParameters.Length > 0)
            {
                emitter.WriteList(RequiredParameters, indent: false, itemDelimiter: ", ");
            }

            if (RequiredParameters.Length > 0 && (OptionalParameters.Length > 0 || RestParameter != null))
            {
                emitter.Write(", ");
            }

            if (OptionalParameters.Length > 0)
            {
                emitter.WriteList(OptionalParameters, indent: false, itemDelimiter: ", ");
            }

            if (OptionalParameters.Length > 0 && RestParameter != null)
            {
                emitter.Write(", ");
            }

            RestParameter?.Emit(emitter);
        }
    }
}
