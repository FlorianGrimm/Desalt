// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPredefinedType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents one of the predefined types: any, number, boolean, string, symbol, void.
    /// </summary>
    internal class TsPredefinedType : AstNode, ITsPredefinedType
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsPredefinedType Any = new TsPredefinedType("any");
        public static readonly TsPredefinedType Number = new TsPredefinedType("number");
        public static readonly TsPredefinedType Boolean = new TsPredefinedType("boolean");
        public static readonly TsPredefinedType String = new TsPredefinedType("string");
        public static readonly TsPredefinedType Symbol = new TsPredefinedType("symbol");
        public static readonly TsPredefinedType Void = new TsPredefinedType("void");

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsPredefinedType(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Name { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitPredefinedType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitPredefinedType(this);

        public override string ToCodeDisplay() => Name;

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(Name);
    }
}
