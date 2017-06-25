// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIdentifier.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System;
    using System.Collections.Generic;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a TypeScript identifier.
    /// </summary>
    internal class TsIdentifier : AstNode, ITsIdentifier
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly StringComparer s_keyComparer = StringComparer.Ordinal;

        private static readonly Dictionary<string, TsIdentifier> s_cache =
            new Dictionary<string, TsIdentifier>(s_keyComparer);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsIdentifier(string text)
        {
            Param.VerifyString(text, nameof(text));
            Text = text;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Text { get; }

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        public static implicit operator TsIdentifier(string text) => Get(text);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsIdentifier Get(string text)
        {
            if (!s_cache.TryGetValue(text, out TsIdentifier identifier))
            {
                s_cache[text] = identifier = new TsIdentifier(text);
            }

            return identifier;
        }

        public void Accept(TypeScriptVisitor visitor)
        {
            visitor.VisitIdentifier(this);
        }

        public T Accept<T>(TypeScriptVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }

        public override string ToCodeDisplay() => Text;

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(Text);
    }
}
