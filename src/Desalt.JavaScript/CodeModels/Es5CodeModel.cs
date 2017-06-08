// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5CodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels
{
    using Desalt.Core.CodeModels;

    /// <summary>
    /// Abstract base class for all ES5 code models.
    /// </summary>
    public abstract class Es5CodeModel : CodeModel, IEs5CodeModel
    {
        public abstract T Accept<T>(Es5Visitor<T> visitor);
    }
}
