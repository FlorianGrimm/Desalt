// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptCodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using Desalt.Core.CodeModels;

    /// <summary>
    /// Abstract base class for all TypeScript code models.
    /// </summary>
    public abstract class TypeScriptCodeModel : CodeModel, ITypeScriptCodeModel
    {
        public abstract T Accept<T>(TypeScriptVisitor<T> visitor);
    }
}
