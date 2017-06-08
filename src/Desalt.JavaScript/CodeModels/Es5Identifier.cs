// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Identifier.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels
{
    using System;
    using System.Collections.Generic;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a JavaScript identifier.
    /// </summary>
    public class Es5Identifier : Es5CodeModel, IEs5Expression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly StringComparer s_keyComparer = StringComparer.Ordinal;

        private static readonly Dictionary<string, Es5Identifier> s_cache =
            new Dictionary<string, Es5Identifier>(s_keyComparer);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5Identifier(string text)
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

        public static implicit operator Es5Identifier(string text) => Get(text);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static Es5Identifier Get(string text)
        {
            Es5Identifier identifier;
            if (!s_cache.TryGetValue(text, out identifier))
            {
                s_cache[text] = identifier = new Es5Identifier(text);
            }

            return identifier;
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }

        public override string ToCodeDisplay() => Text;

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write(Text);
        }
    }
}
