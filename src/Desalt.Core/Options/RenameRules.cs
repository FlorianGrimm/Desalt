// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RenameRules.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the types of renaming rules to apply to the translated TypeScript code.
    /// </summary>
    public class RenameRules
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<UserDefinedOperatorKind, string>
            s_defaultUserDefinedOperatorMethodNames = new Dictionary<UserDefinedOperatorKind, string>
            {
                [UserDefinedOperatorKind.UnaryPlus] = "op_UnaryPlus",
                [UserDefinedOperatorKind.UnaryNegation] = "op_UnaryNegation",
                [UserDefinedOperatorKind.LogicalNot] = "op_LogicalNot",
                [UserDefinedOperatorKind.OnesComplement] = "op_OnesComplement",
                [UserDefinedOperatorKind.Increment] = "op_Increment",
                [UserDefinedOperatorKind.Decrement] = "op_Decrement",
                [UserDefinedOperatorKind.True] = "op_True",
                [UserDefinedOperatorKind.False] = "op_False",
                [UserDefinedOperatorKind.Addition] = "op_Addition",
                [UserDefinedOperatorKind.Subtraction] = "op_Subtraction",
                [UserDefinedOperatorKind.Multiplication] = "op_Multiply",
                [UserDefinedOperatorKind.Division] = "op_Division",
                [UserDefinedOperatorKind.Modulus] = "op_Modulus",
                [UserDefinedOperatorKind.BitwiseAnd] = "op_BitwiseAnd",
                [UserDefinedOperatorKind.BitwiseOr] = "op_BitwiseOr",
                [UserDefinedOperatorKind.ExclusiveOr] = "op_ExclusiveOr",
                [UserDefinedOperatorKind.LeftShift] = "op_LeftShift",
                [UserDefinedOperatorKind.RightShift] = "op_RightShift",
                [UserDefinedOperatorKind.Equality] = "op_Equality",
                [UserDefinedOperatorKind.Inequality] = "op_Inequality",
                [UserDefinedOperatorKind.LessThan] = "op_LessThan",
                [UserDefinedOperatorKind.LessThanEquals] = "op_LessThanOrEqual",
                [UserDefinedOperatorKind.GreaterThan] = "op_GreaterThan",
                [UserDefinedOperatorKind.GreaterThanEquals] = "op_GreaterThanOrEqual",
                [UserDefinedOperatorKind.Explicit] = "op_Explicit",
                [UserDefinedOperatorKind.Implicit] = "op_Implicit",
            }.ToImmutableDictionary();

        public static readonly RenameRules Default = new RenameRules(
            instanceToCopy: null,
            userDefinedOperatorMethodNames: s_defaultUserDefinedOperatorMethodNames);

        /// <summary>
        /// Represents the rename rules that Saltarelle uses for JavaScript translation.
        /// </summary>
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
            FieldRenameRule fieldRule = DefaultFieldRule,
            ImmutableDictionary<UserDefinedOperatorKind, string>? userDefinedOperatorMethodNames = null)
            : this(instanceToCopy: null, enumRule, fieldRule, userDefinedOperatorMethodNames)
        {
        }

        private RenameRules(
            RenameRules? instanceToCopy = null,
            EnumRenameRule? enumRule = null,
            FieldRenameRule? fieldRule = null,
            ImmutableDictionary<UserDefinedOperatorKind, string>? userDefinedOperatorMethodNames = null)
        {
            EnumRule = enumRule ?? instanceToCopy?.EnumRule ?? DefaultEnumMemberRule;
            FieldRule = fieldRule ?? instanceToCopy?.FieldRule ?? DefaultFieldRule;
            UserDefinedOperatorMethodNames = userDefinedOperatorMethodNames ??
                instanceToCopy?.UserDefinedOperatorMethodNames ?? s_defaultUserDefinedOperatorMethodNames;

            // make sure to add any missing entries in the operator overloaded method names dictionary
            var missingPairs = s_defaultUserDefinedOperatorMethodNames
                .Where(pair => !UserDefinedOperatorMethodNames.ContainsKey(pair.Key))
                .ToImmutableArray();

            if (missingPairs.Length > 0)
            {
                UserDefinedOperatorMethodNames = UserDefinedOperatorMethodNames.AddRange(missingPairs);
            }
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

        /// <summary>
        /// Gets a dictionary keyed by <see cref="UserDefinedOperatorKind"/>, containing the method name
        /// to use for each overloaded operator method declaration.
        /// </summary>
        public ImmutableDictionary<UserDefinedOperatorKind, string> UserDefinedOperatorMethodNames { get; }

        public RenameRules WithUserDefinedOperatorMethodNames(ImmutableDictionary<UserDefinedOperatorKind, string> value)
        {
            return value == UserDefinedOperatorMethodNames
                ? this
                : new RenameRules(this, userDefinedOperatorMethodNames: value);
        }
    }
}
