// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using Desalt.Core.Translation;

    /// <summary>
    /// Represents the service contract for validating a C# syntax tree.
    /// </summary>
    internal interface IValidator
    {
        IExtendedResult<bool> Validate(DocumentTranslationContext context);
    }
}
