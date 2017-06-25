// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSetAccessor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property set accessor of the form 'set name(value: type) { body }'.
    /// </summary>
    internal class TsSetAccessor : AstNode, ITsSetAccessor
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitSetAccessor(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitSetAccessor(this);

        public override string ToCodeDisplay() =>
            $"set {PropertyName}({ParameterName}{ParameterType.ToTypeAnnotationCodeDisplay()}) " +
            $"{{{FunctionBody.ToElidedList()}}}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("set ");
            PropertyName.WriteFullCodeDisplay(writer);
            writer.Write("(");
            ParameterName.WriteFullCodeDisplay(writer);
            ParameterType.WriteTypeAnnotation(writer);
            writer.Write(") ");
            WriteBlock(writer, FunctionBody);
        }
    }
}
