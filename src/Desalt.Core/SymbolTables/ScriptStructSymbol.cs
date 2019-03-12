// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptStructSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a struct symbol that can be used in the translation process.
    /// </summary>
    internal sealed class ScriptStructSymbol : ScriptTypeSymbol, IScriptStructSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptStructSymbol(ITypeSymbol typeSymbol)
            : base(typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                throw new ArgumentException("Symbol is not a struct", nameof(typeSymbol));
            }

            Mutable = typeSymbol.GetFlagAttribute(SaltarelleAttributeName.Mutable);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Can be applied to a user-defined value type (struct) to instruct the compiler that it can
        /// be mutated and therefore needs to be copied whenever .net would create a copy of a value type.
        /// </summary>
        public bool Mutable { get; }
    }
}
