// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBooleanLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an expression containing a numeric literal value.
    /// </summary>
    internal class TsBooleanLiteral : AstNode<TsVisitor>, ITsBooleanLiteral
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsBooleanLiteral True = new TsBooleanLiteral(true);

        public static readonly TsBooleanLiteral False = new TsBooleanLiteral(false);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsBooleanLiteral(bool value)
        {
            Value = value;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool Value { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitBooleanLiteral(this);

        public override string CodeDisplay => Value ? "true" : "false";

        public override void Emit(Emitter emitter) => emitter.Write(CodeDisplay);
    }
}
