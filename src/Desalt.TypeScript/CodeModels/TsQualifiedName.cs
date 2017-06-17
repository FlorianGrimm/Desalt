// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsQualifiedName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a qualified name, which has dots between identifiers. For example, 'ns.type.method'.
    /// </summary>
    internal class TsQualifiedName : CodeModel, ITsQualifiedName
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitQualifiedName(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitQualifiedName(this);

        public override string ToCodeDisplay() =>
            $"{string.Join(".", Left.Select(x => x.ToCodeDisplay()))}{Right.ToCodeDisplay()}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            foreach (ITsIdentifier left in Left)
            {
                left.WriteFullCodeDisplay(writer);
                writer.Write(".");
            }

            Right.WriteFullCodeDisplay(writer);
        }
    }
}
