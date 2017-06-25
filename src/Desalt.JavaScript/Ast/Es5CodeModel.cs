// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5CodeModel.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using Desalt.Core.Ast;

    /// <summary>
    /// Abstract base class for all ES5 code models.
    /// </summary>
    public abstract class Es5CodeModel : AstNode, IEs5CodeModel
    {
        public abstract void Accept(Es5Visitor visitor);

        public abstract T Accept<T>(Es5Visitor<T> visitor);
    }
}
