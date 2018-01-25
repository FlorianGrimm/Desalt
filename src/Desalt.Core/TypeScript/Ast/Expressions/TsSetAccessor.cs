// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSetAccessor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a property set accessor of the form 'set name(value: type) { body }'.
    /// </summary>
    internal class TsSetAccessor : AstNode<TsVisitor>, ITsSetAccessor
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            ParameterType = parameterType;
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsBindingIdentifierOrPattern ParameterName { get; }
        public ITsType ParameterType { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitSetAccessor(this);

        public override string CodeDisplay =>
            $"set {PropertyName}({ParameterName}{ParameterType.OptionalTypeAnnotation()}) " +
            $"{{ {FunctionBody.ToElidedList()} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("set ");
            PropertyName.Emit(emitter);
            emitter.Write("(");
            ParameterName.Emit(emitter);
            ParameterType.EmitOptionalTypeAnnotation(emitter);
            emitter.Write(") ");
            emitter.WriteBlock(FunctionBody, skipNewlines: true);
        }
    }
}
