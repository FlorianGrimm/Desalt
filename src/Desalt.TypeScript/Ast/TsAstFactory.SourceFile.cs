// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.SourceFile.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using Desalt.TypeScript.Ast.Declarations;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Create a TypeScript implementation source file (extension '.ts'), containing statements and declarations.
        /// </summary>
        public static ITsImplementationScript ImplementationScript(params ITsImplementationScriptElement[] elements) =>
            new TsImplementationScript(elements);
    }
}
