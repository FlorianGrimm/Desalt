// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertyAssignment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a property assignment in the following form: 'propertyName: value'.
    /// </summary>
    internal class TsPropertyAssignment : AstNode, ITsPropertyAssignment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertyAssignment(ITsPropertyName propertyName, ITsExpression initializer)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitPropertyAssignment(this);

        public override string CodeDisplay => $"{PropertyName.CodeDisplay}: {Initializer.CodeDisplay}";

        protected override void EmitInternal(Emitter emitter)
        {
            PropertyName.Emit(emitter);
            emitter.Write(": ");
            Initializer.Emit(emitter);
        }
    }
}
