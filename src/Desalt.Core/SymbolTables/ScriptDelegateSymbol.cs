// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptDelegateSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a delegate symbol that can be used in the translation process.
    /// </summary>
    internal sealed class ScriptDelegateSymbol : ScriptSymbol, IScriptDelegateSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptDelegateSymbol(ITypeSymbol delegateSymbol)
            : base(delegateSymbol)
        {
            if (delegateSymbol.TypeKind != TypeKind.Delegate)
            {
                throw new ArgumentException(nameof(delegateSymbol));
            }

            BindThisToFirstParameter =
                delegateSymbol.GetFlagAttribute(SaltarelleAttributeName.BindThisToFirstParameter);
            ExpandParams = delegateSymbol.GetFlagAttribute(SaltarelleAttributeName.ExpandParams);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates that the Javascript 'this' should appear as the first argument to the delegate.
        /// </summary>
        public bool BindThisToFirstParameter { get; }

        /// <summary>
        /// Indicates whether a method with a "params" parameter should make the param array be
        /// expanded in script (eg. given <c>void F(int a, params int[] b)</c>, the invocation
        /// <c>F(1, 2, 3)</c> will be translated to <c>F(1, [2, 3])</c> without this attribute, but
        /// <c>(1, 2, 3)</c> with this attribute. Methods with this attribute can only be invoked in
        /// the expanded form.
        /// </summary>
        public bool ExpandParams { get; }
    }
}
