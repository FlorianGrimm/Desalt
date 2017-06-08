// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ICodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.CodeModels
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Root interface for all code model classes.
    /// </summary>
    public interface ICodeModel
    {
        /// <summary>
        /// Returns an abbreviated string representation of the code model, which is useful for debugging.
        /// </summary>
        /// <returns>A string representation of this code model.</returns>
        string ToCodeDisplay();

        /// <summary>
        /// Returns a string representation of the full code model, which is useful for debugging and
        /// printing to logs. This should not be used to actually emit generated code.
        /// </summary>
        /// <returns>A string representation of the full code model.</returns>
        string ToFullCodeDisplay();

        /// <summary>
        /// Writes a string representation of this code model to the specified <see
        /// cref="IndentedTextWriter"/>, which is useful for debugging and printing to logs. This
        /// should not be used to actually emit generated code.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        void WriteFullCodeDisplay(IndentedTextWriter writer);
    }
}
