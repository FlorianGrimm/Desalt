// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldRenameRule.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
{
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
}
