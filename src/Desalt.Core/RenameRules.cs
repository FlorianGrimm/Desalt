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

        public static readonly RenameRules Saltarelle =
            new RenameRules(privateFieldRule: PrivateFieldRenameRule.DollarPrefix);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public RenameRules(PrivateFieldRenameRule privateFieldRule = PrivateFieldRenameRule.LowerCaseFirstChar)
            : this(instanceToCopy: null, privateFieldRule: privateFieldRule)
        {
        }

        private RenameRules(RenameRules instanceToCopy = null, PrivateFieldRenameRule? privateFieldRule = null)
        {
            PrivateFieldRule = privateFieldRule ??
                instanceToCopy?.PrivateFieldRule ?? PrivateFieldRenameRule.LowerCaseFirstChar;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a value indicating how private fields are translated from C# to TypeScript.
        /// </summary>
        public PrivateFieldRenameRule PrivateFieldRule { get; }

        public RenameRules WithPrivateFieldRule(PrivateFieldRenameRule value) =>
            value == PrivateFieldRule ? this : new RenameRules(this, privateFieldRule: value);
    }

    /// <summary>
    /// Controls how private fields are translated from C# to TypeScript.
    /// </summary>
    public enum PrivateFieldRenameRule
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
        DollarPrefix
    }
}
