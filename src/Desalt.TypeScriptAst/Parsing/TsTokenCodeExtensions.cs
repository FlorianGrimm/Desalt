// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTokenCodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;

    /// <summary>
    /// Contains extension methods for working with <see cref="TsTokenCode"/> enumeration values.
    /// </summary>
    internal static class TsTokenCodeExtensions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        // These arrays are internal for unit testing only. Hence, I'm leaving the 's_' prefix to indicate that it's
        // really private.

#pragma warning disable IDE1006 // Naming Styles

        // The following keywords are reserved and cannot be used as an Identifier.
        internal static readonly TsTokenCode[] s_reservedKeywords =
        {
            TsTokenCode.Break,
            TsTokenCode.Case,
            TsTokenCode.Catch,
            TsTokenCode.Class,
            TsTokenCode.Const,
            TsTokenCode.Continue,
            TsTokenCode.Debugger,
            TsTokenCode.Default,
            TsTokenCode.Delete,
            TsTokenCode.Do,
            TsTokenCode.Else,
            TsTokenCode.Enum,
            TsTokenCode.Export,
            TsTokenCode.Extends,
            TsTokenCode.False,
            TsTokenCode.Finally,
            TsTokenCode.For,
            TsTokenCode.Function,
            TsTokenCode.If,
            TsTokenCode.Import,
            TsTokenCode.In,
            TsTokenCode.Instanceof,
            TsTokenCode.New,
            TsTokenCode.Null,
            TsTokenCode.Return,
            TsTokenCode.Super,
            TsTokenCode.Switch,
            TsTokenCode.This,
            TsTokenCode.Throw,
            TsTokenCode.True,
            TsTokenCode.Try,
            TsTokenCode.Typeof,
            TsTokenCode.Var,
            TsTokenCode.Void,
            TsTokenCode.While,
            TsTokenCode.With,
        };

        // The following keywords cannot be used as identifiers in strict mode code, but are otherwise not restricted.
        internal static readonly TsTokenCode[] s_reservedInStrictModeKeywords =
        {
            TsTokenCode.Implements,
            TsTokenCode.Interface,
            TsTokenCode.Let,
            TsTokenCode.Package,
            TsTokenCode.Private,
            TsTokenCode.Protected,
            TsTokenCode.Public,
            TsTokenCode.Static,
            TsTokenCode.Yield,
        };

        // The following keywords cannot be used as user defined type names, but are otherwise not restricted.
        internal static readonly TsTokenCode[] s_restrictedInUserDefinedTypeNamesKeywords =
        {
            TsTokenCode.Any, TsTokenCode.Boolean, TsTokenCode.Number, TsTokenCode.String, TsTokenCode.Symbol,
        };

        // The following keywords have special meaning in certain contexts, but are valid identifiers.
        internal static readonly TsTokenCode[] s_validIdentifierKeywords =
        {
            TsTokenCode.Abstract,
            TsTokenCode.As,
            TsTokenCode.Async,
            TsTokenCode.Await,
            TsTokenCode.Constructor,
            TsTokenCode.Declare,
            TsTokenCode.From,
            TsTokenCode.Get,
            TsTokenCode.Is,
            TsTokenCode.Module,
            TsTokenCode.Namespace,
            TsTokenCode.Of,
            TsTokenCode.Readonly,
            TsTokenCode.Require,
            TsTokenCode.Set,
            TsTokenCode.Type
        };

#pragma warning restore IDE1006 // Naming Styles

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public static IEnumerable<TsTokenCode> AllKeywords =>
            s_reservedKeywords.Concat(s_reservedInStrictModeKeywords)
                .Concat(s_restrictedInUserDefinedTypeNamesKeywords)
                .Concat(s_validIdentifierKeywords);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static bool IsIdentifierOrKeyword(this TsTokenCode tokenCode)
        {
            return tokenCode == TsTokenCode.Identifier || IsKeyword(tokenCode);
        }

        public static bool IsKeyword(this TsTokenCode tokenCode)
        {
            return tokenCode.IsOneOf(s_reservedKeywords) ||
                tokenCode.IsOneOf(s_reservedInStrictModeKeywords) ||
                tokenCode.IsOneOf(s_restrictedInUserDefinedTypeNamesKeywords) ||
                tokenCode.IsOneOf(s_validIdentifierKeywords);
        }

        /// <summary>
        /// Returns a value indicating if the specified keyword is valid as an identifier.
        /// </summary>
        /// <param name="tokenCode">The token code to check.</param>
        /// <param name="isTypeDeclaration">
        /// Indicates whether the identifier will be a user-defined type name (class, interface, etc.) since some
        /// keywords are not valid as type name identifiers ('boolean', 'number', etc.).
        /// </param>
        /// <param name="isStrictMode">
        /// Indicates whether the TypeScript code is in strict mode, since some keywords are not valid in strict mode
        /// ('private', 'static', etc.).
        /// </param>
        /// <returns></returns>
        public static bool IsKeywordAllowedAsIdentifier(
            this TsTokenCode tokenCode,
            bool isTypeDeclaration,
            bool isStrictMode)
        {
            bool valid = tokenCode.IsOneOf(s_validIdentifierKeywords);
            if (!isTypeDeclaration)
            {
                valid = valid || tokenCode.IsOneOf(s_restrictedInUserDefinedTypeNamesKeywords);
            }

            if (!isStrictMode)
            {
                valid = valid || tokenCode.IsOneOf(s_reservedInStrictModeKeywords);
            }

            return valid;
        }
    }
}
