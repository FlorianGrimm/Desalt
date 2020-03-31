// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RenameRules.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
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
            RenameRules? instanceToCopy = null,
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

        public RenameRules WithEnumRule(EnumRenameRule value)
        {
            return value == EnumRule ? this : new RenameRules(this, enumRule: value);
        }

        /// <summary>
        /// Gets a value indicating how fields are translated from C# to TypeScript.
        /// </summary>
        public FieldRenameRule FieldRule { get; }

        public RenameRules WithFieldRule(FieldRenameRule value)
        {
            return value == FieldRule ? this : new RenameRules(this, fieldRule: value);
        }
    }
}
