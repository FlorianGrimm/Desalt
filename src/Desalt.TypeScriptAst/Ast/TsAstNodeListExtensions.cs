// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeListExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Extension methods for <see cref="ITsAstNodeList{T}"/> objects.
    /// </summary>
    internal static class TsAstNodeListExtensions
    {
        /// <summary>
        /// Enumerates a sequence exactly once and produces an immutable <see cref="ITsArrayElementNodeList"/> of its contents.
        /// </summary>
        /// <param name="items">The sequence to enumerate.</param>
        /// <returns>An immutable <see cref="ITsArrayElementNodeList"/>.</returns>
        public static ITsArrayElementNodeList ToNodeList(this IEnumerable<ITsArrayElement> items)
        {
            return new TsArrayElementNodeList(items.ToImmutableArray());
        }

        /// <summary>
        /// Enumerates a sequence exactly once and produces an immutable <see cref="ITsArgumentNodeList"/> of its contents.
        /// </summary>
        /// <param name="items">The sequence to enumerate.</param>
        /// <returns>An immutable <see cref="ITsArgumentNodeList"/>.</returns>
        public static ITsArgumentNodeList ToNodeList(this IEnumerable<ITsArgument> items)
        {
            return new TsArgumentNodeList(items.ToImmutableArray());
        }

        /// <summary>
        /// Enumerates a sequence exactly once and produces an immutable <see cref="ITsTypeNodeList"/> of its contents.
        /// </summary>
        /// <param name="items">The sequence to enumerate.</param>
        /// <returns>An immutable <see cref="ITsTypeNodeList"/>.</returns>
        public static ITsTypeNodeList ToNodeList(this IEnumerable<ITsType> items)
        {
            return new TsTypeNodeList(items.ToImmutableArray());
        }
    }
}
