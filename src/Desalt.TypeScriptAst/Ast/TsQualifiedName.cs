// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsQualifiedName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a qualified name, which has dots between identifiers. For example, 'ns.type.method'.
    /// </summary>
    internal class TsQualifiedName : TsAstNode, ITsQualifiedName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsQualifiedName(ITsIdentifier right, IEnumerable<ITsIdentifier> left = null)
        {
            Left = left?.ToImmutableArray() ?? ImmutableArray<ITsIdentifier>.Empty;
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsIdentifier> Left { get; }
        public ITsIdentifier Right { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitQualifiedName(this);
        }

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                foreach (ITsIdentifier identifier in Left)
                {
                    builder.Append(identifier.CodeDisplay).Append('.');
                }

                builder.Append(Right.CodeDisplay);
                return builder.ToString();
            }
        }

        protected override void EmitInternal(Emitter emitter)
        {
            foreach (ITsIdentifier left in Left)
            {
                left.Emit(emitter);
                emitter.Write(".");
            }

            Right.Emit(emitter);
        }
    }
}