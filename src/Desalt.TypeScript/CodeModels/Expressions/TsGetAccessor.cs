// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGetAccessor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property get accessor of the form 'get name (): type { body }'.
    /// </summary>
    internal class TsGetAccessor : CodeModel, ITsGetAccessor
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsGetAccessor(
            ITsPropertyName propertyName,
            ITsType typeAnnotation = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            TypeAnnotation = typeAnnotation;
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsType TypeAnnotation { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitGetAccessor(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitGetAccessor(this);

        public override string ToCodeDisplay() =>
            $"get {PropertyName}(){TypeAnnotation.TypeAnnotationCodeDisplay()} {{{FunctionBody.ToElidedList()}}}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("get ");
            PropertyName.WriteFullCodeDisplay(writer);
            writer.Write("()");
            TypeAnnotation.WriteTypeAnnotation(writer);
            WriteBlock(writer, FunctionBody);
        }
    }
}
