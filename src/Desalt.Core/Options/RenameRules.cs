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

        private static readonly ImmutableDictionary<OperatorOverloadKind, string> s_defaultOperatorOverloadMethodNames =
            new Dictionary<OperatorOverloadKind, string>
            {
                [OperatorOverloadKind.UnaryPlus] = "op_UnaryPlus",
                [OperatorOverloadKind.UnaryNegation] = "op_UnaryNegation",
                [OperatorOverloadKind.LogicalNot] = "op_LogicalNot",
                [OperatorOverloadKind.OnesComplement] = "op_OnesComplement",
                [OperatorOverloadKind.Increment] = "op_Increment",
                [OperatorOverloadKind.Decrement] = "op_Decrement",
                [OperatorOverloadKind.True] = "op_True",
                [OperatorOverloadKind.False] = "op_False",
                [OperatorOverloadKind.Addition] = "op_Addition",
                [OperatorOverloadKind.Subtraction] = "op_Subtraction",
                [OperatorOverloadKind.Multiplication] = "op_Multiply",
                [OperatorOverloadKind.Division] = "op_Division",
                [OperatorOverloadKind.Modulus] = "op_Modulus",
                [OperatorOverloadKind.BitwiseAnd] = "op_BitwiseAnd",
                [OperatorOverloadKind.BitwiseOr] = "op_BitwiseOr",
                [OperatorOverloadKind.BitwiseXor] = "op_BitwiseXor",
                [OperatorOverloadKind.LeftShift] = "op_LeftShift",
                [OperatorOverloadKind.RightShift] = "op_RightShift",
                [OperatorOverloadKind.Equality] = "op_Equality",
                [OperatorOverloadKind.Inequality] = "op_Inequality",
                [OperatorOverloadKind.LessThan] = "op_LessThan",
                [OperatorOverloadKind.LessThanEquals] = "op_LessThanOrEqual",
                [OperatorOverloadKind.GreaterThan] = "op_GreaterThan",
                [OperatorOverloadKind.GreaterThanEquals] = "op_GreaterThanOrEqual",
                [OperatorOverloadKind.Explicit] = "op_Explicit",
                [OperatorOverloadKind.Implicit] = "op_Implicit",
            }.ToImmutableDictionary();

        public static readonly RenameRules Default = new RenameRules(
            instanceToCopy: null,
            operatorOverloadMethodNames: s_defaultOperatorOverloadMethodNames);

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
            ImmutableDictionary<OperatorOverloadKind, string>? operatorOverloadMethodNames = null)
            : this(instanceToCopy: null, enumRule, fieldRule, operatorOverloadMethodNames)
        {
        }

        private RenameRules(
            RenameRules? instanceToCopy = null,
            EnumRenameRule? enumRule = null,
            FieldRenameRule? fieldRule = null,
            ImmutableDictionary<OperatorOverloadKind, string>? operatorOverloadMethodNames = null)
        {
            EnumRule = enumRule ?? instanceToCopy?.EnumRule ?? DefaultEnumMemberRule;
            FieldRule = fieldRule ?? instanceToCopy?.FieldRule ?? DefaultFieldRule;
            OperatorOverloadMethodNames = operatorOverloadMethodNames ??
                instanceToCopy?.OperatorOverloadMethodNames ?? s_defaultOperatorOverloadMethodNames;

            // make sure to add any missing entries in the operator overloaded method names dictionary
            var missingPairs = s_defaultOperatorOverloadMethodNames
                .Where(pair => !OperatorOverloadMethodNames.ContainsKey(pair.Key))
                .ToImmutableArray();

            if (missingPairs.Length > 0)
            {
                OperatorOverloadMethodNames = OperatorOverloadMethodNames.AddRange(missingPairs);
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
        /// Gets a dictionary keyed by <see cref="OperatorOverloadKind"/>, containing the method name
        /// to use for each overloaded operator method declaration.
        /// </summary>
        public ImmutableDictionary<OperatorOverloadKind, string> OperatorOverloadMethodNames { get; }

        public RenameRules WithOperatorOverloadMethodNames(ImmutableDictionary<OperatorOverloadKind, string> value)
        {
            return value == OperatorOverloadMethodNames
                ? this
                : new RenameRules(this, operatorOverloadMethodNames: value);
        }
    }
}
