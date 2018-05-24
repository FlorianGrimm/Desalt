// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Types
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript type parameter, for example &lt;MyType extends MyBase&gt;.
    /// </summary>
    internal class TsTypeParameter : AstNode, ITsTypeParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeParameter(ITsIdentifier typeName, ITsType constraint = null)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Constraint = constraint;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier TypeName { get; }
        public ITsType Constraint { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTypeParameter(this);

        public override string CodeDisplay => TypeName.CodeDisplay + (Constraint != null ? $" extends {Constraint}" : "");

        protected override void EmitInternal(Emitter emitter)
        {
            TypeName.Emit(emitter);

            if (Constraint != null)
            {
                emitter.Write(" extends ");
                Constraint.Emit(emitter);
            }
        }
    }
}
