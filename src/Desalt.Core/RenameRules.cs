// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RenameRules.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    /// <summary>
    /// Represents the types of renaming rules to apply to the translated TypeScript code.
    /// </summary>
    public class RenameRules
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly RenameRules Default = new RenameRules(instanceToCopy: null);

        public static readonly RenameRules Saltarelle = new RenameRules(
            enumRule: EnumRenameRule.LowerCaseFirstChar,
            fieldRule: FieldRenameRule.PrivateDollarPrefix);

        private const EnumRenameRule DefaultEnumMemberRule = EnumRenameRule.MatchCSharpName;
        private const FieldRenameRule DefaultFieldRule = FieldRenameRule.LowerCaseFirstChar;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public RenameRules(
            EnumRenameRule enumRule = DefaultEnumMemberRule,
            FieldRenameRule fieldRule = DefaultFieldRule)
            : this(instanceToCopy: null, enumRule: enumRule, fieldRule: fieldRule)
        {
        }

        private RenameRules(
            RenameRules instanceToCopy = null,
            EnumRenameRule? enumRule = null,
            FieldRenameRule? fieldRule = null)
        {
            EnumRule = enumRule ?? instanceToCopy?.EnumRule ?? DefaultEnumMemberRule;
            FieldRule = fieldRule ?? instanceToCopy?.FieldRule ?? DefaultFieldRule;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a value indicating how enum declarations are translated from C# to TypeScript.
        /// </summary>
        public EnumRenameRule EnumRule { get; }

        public RenameRules WithEnumRule(EnumRenameRule value) =>
            value == EnumRule ? this : new RenameRules(this, enumRule: value);

        /// <summary>
        /// Gets a value indicating how fields are translated from C# to TypeScript.
        /// </summary>
        public FieldRenameRule FieldRule { get; }

        public RenameRules WithFieldRule(FieldRenameRule value) =>
            value == FieldRule ? this : new RenameRules(this, fieldRule: value);
    }

    /// <summary>
    /// Controls how private fields are translated from C# to TypeScript.
    /// </summary>
    public enum FieldRenameRule
    {
        /// <summary>
        /// Indicates that the first letter of the field name should be converted to lower case. For
        /// example, if the C# is <c>MyField</c>, the TypeScript would be <c>myField</c>.
        /// </summary>
        LowerCaseFirstChar,

        /// <summary>
        /// Indicates that private fields should be prefixed with a '$' sign, which matches the
        /// Saltarelle naming.
        /// </summary>
        PrivateDollarPrefix,

        /// <summary>
        /// Indicates that fields should be prefixed with a '$' sign, but only when there is a
        /// duplicate name in the compiled code. For example, if there is a field named <c>name</c>
        /// and a property <c>Name</c>, when they are compiled it will be the same name, which is not allowed.
        /// </summary>
        DollarPrefixOnlyForDuplicateName,
    }

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
