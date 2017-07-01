// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSimpleVariableDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a simple variable declaration of the form 'x = y'.
    /// </summary>
    internal class TsSimpleVariableDeclaration : AstNode<TsVisitor>, ITsSimpleVariableDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSimpleVariableDeclaration(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            VariableType = variableType;
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier VariableName { get; }
        public ITsType VariableType { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitSimpleVariableDeclaration(this);

        public override string CodeDisplay
        {
            get
            {
                string display = VariableName.CodeDisplay;
                display += VariableType?.ToTypeAnnotationCodeDisplay();
                display += Initializer.ToAssignmentCodeDisplay();
                return display;
            }
        }

        public override void Emit(Emitter emitter)
        {
            VariableName.Emit(emitter);
            VariableType?.EmitTypeAnnotation(emitter);
            Initializer.EmitAssignment(emitter);
        }
    }
}
