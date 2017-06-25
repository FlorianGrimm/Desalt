﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertyFunction.cs" company="Justin Rockwood">
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
    /// Represents an object literal property function.
    /// </summary>
    internal class TsPropertyFunction : AstNode, ITsPropertyFunction
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertyFunction(
            ITsPropertyName propertyName,
            ITsCallSignature callSignature,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsCallSignature CallSignature { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitPropertyFunction(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitPropertyFunction(this);

        public override string ToCodeDisplay() =>
            $"{PropertyName.ToCodeDisplay()} {CallSignature.ToCodeDisplay()} {{ {FunctionBody.ToElidedList()} }}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            PropertyName.WriteFullCodeDisplay(writer);
            writer.Write(" ");
            CallSignature.WriteFullCodeDisplay(writer);
            writer.Write(" ");
            WriteBlock(writer, FunctionBody);
        }
    }
}
