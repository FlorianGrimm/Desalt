// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5PropertySetAssignment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property assignment in the following form: 'get propertyName() { }'.
    /// </summary>
    public class Es5PropertySetAssignment : AstNode<Es5Visitor>, IEs5PropertyAssignment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5PropertySetAssignment(
            string propertyName,
            Es5Identifier setParameter,
            IEnumerable<IEs5SourceElement> functionBody)
        {
            Param.VerifyString(propertyName, nameof(propertyName));
            PropertyName = propertyName;
            SetParameter = setParameter ?? throw new ArgumentNullException(nameof(setParameter));
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<IEs5SourceElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string PropertyName { get; }
        public Es5Identifier SetParameter { get; }
        public ImmutableArray<IEs5SourceElement> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitPropertySetAssignment(this);
        }

        public override string CodeDisplay =>
            $"set {PropertyName}({SetParameter}) {{ {FunctionBody.ToElidedList(Environment.NewLine)} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write($"set {PropertyName}(");
            SetParameter.Emit(emitter);
            emitter.Write(") ");
            WriteBlock(emitter, FunctionBody);
        }
    }
}
