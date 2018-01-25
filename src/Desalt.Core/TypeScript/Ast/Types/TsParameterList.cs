// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParameterList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a parameter list of the form '(parameter: type)'.
    /// </summary>
    internal class TsParameterList : AstNode<TsVisitor>, ITsParameterList
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsParameterList(
            IEnumerable<ITsRequiredParameter> requiredParameters = null,
            IEnumerable<ITsOptionalParameter> optionalParameters = null,
            ITsRestParameter restParameter = null)
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
        public ITsRestParameter RestParameter { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitParameterList(this);

        public override string CodeDisplay =>
            RequiredParameters.ToElidedList() +
            OptionalParameters.ToElidedList() +
            RestParameter.CodeDisplay;

        public override void Emit(Emitter emitter)
        {
            emitter.WriteList(RequiredParameters, indent: false, itemDelimiter: ", ");
            if (RequiredParameters.Length > 0 && OptionalParameters.Length > 0 || RestParameter != null)
            {
                emitter.Write(", ");
            }

            emitter.WriteList(OptionalParameters, indent: false, itemDelimiter: ", ");
            if (OptionalParameters.Length > 0 && RestParameter != null)
            {
                emitter.Write(", ");
            }

            RestParameter?.Emit(emitter);
        }
    }
}
