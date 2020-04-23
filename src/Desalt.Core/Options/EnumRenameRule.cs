// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumRenameRule.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
{
    /// <summary>
    /// Controls how enum members are translated from C# to TypeScript. This rule only controls the
    /// name of the enum field; values always use [ScriptName] if present, or the default naming rule (camelCase).
    /// </summary>
    public enum EnumRenameRule
    {
        /// <summary>
        /// Indicates that the first letter of the enum member should be converted to lower case.
        /// <code>
        /// [NamedValues]
        /// public enum MyEnum
        /// {
        ///     [ScriptName("uno")]
        ///     One,
        ///     Two,
        /// }
        /// </code>
        /// gets translated to
        /// <code>
        /// export const enum MyEnum {
        ///   one = 'uno',
        ///   two = 'two',
        /// }
        /// </code>
        /// </summary>
        LowerCaseFirstChar,

        /// <summary>
        /// Indicates that the original C# name should be used when generating the enum member declaration.
        /// <code>
        /// [NamedValues]
        /// public enum MyEnum
        /// {
        /// [ScriptName("uno")]
        /// One,
        /// Two,
        /// }
        /// </code>
        /// gets translated to
        /// <code>
        /// export const enum MyEnum {
        /// One = 'uno',
        /// Two = 'two',
        /// }
        /// </code>
        /// </summary>
        MatchCSharpName,
    }
}
