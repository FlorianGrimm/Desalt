// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParameterList.cs" company="Justin Rockwood">
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
    /// Represents a parameter list of the form '(parameter: type)'.
    /// </summary>
    internal class TsParameterList : AstNode, ITsParameterList
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

        public void Accept(TsVisitor visitor) => visitor.VisitParameterList(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitParameterList(this);

        public override string CodeDisplay
        {
            get
            {
                return
                    RequiredParameters.ToElidedList() +
                    OptionalParameters.ToElidedList() +
                    RestParameter.CodeDisplay;
            }
        }

        public override void Emit(IndentedTextWriter emitter)
        {
            WriteItems(emitter, RequiredParameters, indent: false, itemDelimiter: ", ");
            WriteItems(emitter, OptionalParameters, indent: false, itemDelimiter: ", ");
            RestParameter?.Emit(emitter);
        }
    }
}
