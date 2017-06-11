// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITypeScriptCodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using Desalt.Core.CodeModels;

    /// <summary>
    /// Root interface for all TypeScript code models.
    /// </summary>
    public interface ITypeScriptCodeModel : ICodeModel
    {
        void Accept(TypeScriptVisitor visitor);

        T Accept<T>(TypeScriptVisitor<T> visitor);
    }
}
