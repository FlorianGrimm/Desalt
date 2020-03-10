// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGenericName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a qualified name with type arguments. For example, 'ns.type.method&lt;T1, T2&gt;'.
    /// </summary>
    internal class TsGenericTypeName : TsQualifiedName, ITsGenericTypeName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsGenericTypeName(
            ITsIdentifier right,
            IEnumerable<ITsIdentifier> left = null,
            IEnumerable<ITsType> typeArguments = null)
            : base(right, left)
        {
            TypeArguments = typeArguments?.ToImmutableArray() ?? ImmutableArray<ITsType>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsType> TypeArguments { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitGenericTypeName(this);

        public override string CodeDisplay =>
            base.CodeDisplay + (TypeArguments.Length > 0 ? $"<{TypeArguments.ToElidedList()}>" : "");

        protected override void EmitInternal(Emitter emitter)
        {
            base.EmitInternal(emitter);

            if (TypeArguments.Length > 0)
            {
                emitter.Write("<");
                emitter.WriteCommaList(TypeArguments);
                emitter.Write(">");
            }
        }
    }
}
