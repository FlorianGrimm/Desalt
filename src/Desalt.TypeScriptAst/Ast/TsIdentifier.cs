// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIdentifier.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast
{
    using System;
    using System.Collections.Concurrent;
    using CompilerUtilities;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a TypeScript identifier.
    /// </summary>
    internal class TsIdentifier : TsAstNode, ITsIdentifier
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly StringComparer s_keyComparer = StringComparer.Ordinal;

        private static readonly ConcurrentDictionary<string, TsIdentifier> s_cache =
            new ConcurrentDictionary<string, TsIdentifier>(s_keyComparer);

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

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitIdentifier(this);
        }

        public override string CodeDisplay => Text;

        protected override void EmitInternal(Emitter emitter) => emitter.Write(Text);
    }
}
