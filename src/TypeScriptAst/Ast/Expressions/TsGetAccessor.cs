﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGetAccessor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a property get accessor of the form 'get name (): type { body }'.
    /// </summary>
    internal class TsGetAccessor : TsAstNode, ITsGetAccessor
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsGetAccessor(
            ITsPropertyName propertyName,
            ITsType propertyType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType;
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsType PropertyType { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitGetAccessor(this);

        public override string CodeDisplay =>
            $"get {PropertyName}(){PropertyType.OptionalTypeAnnotation()} " +
            $"{{ {FunctionBody.ToElidedList(Environment.NewLine)} }}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("get ");
            PropertyName.Emit(emitter);
            emitter.Write("()");
            PropertyType.EmitOptionalTypeAnnotation(emitter);
            emitter.Write(" ");
            emitter.WriteBlock(FunctionBody, skipNewlines: true);
        }
    }
}