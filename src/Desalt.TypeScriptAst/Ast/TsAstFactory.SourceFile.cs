// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.SourceFile.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Create a TypeScript implementation source file (extension '.ts'), containing statements
        /// and declarations.
        /// </summary>
        public static ITsImplementationScript ImplementationScript(params ITsImplementationScriptElement[] elements)
        {
            return new TsImplementationScript(elements.ToImmutableArray());
        }

        /// <summary>
        /// Create a TypeScript implementation source file (extension '.ts'), containing exported
        /// statements and declarations.
        /// </summary>
        public static ITsImplementationModule ImplementationModule(IEnumerable<ITsImplementationModuleElement> elements)
        {
            return new TsImplementationModule(elements.ToImmutableArray());
        }

        /// <summary>
        /// Create a TypeScript implementation source file (extension '.ts'), containing exported
        /// statements and declarations.
        /// </summary>
        public static ITsImplementationModule ImplementationModule(params ITsImplementationModuleElement[] elements)
        {
            return new TsImplementationModule(elements.ToImmutableArray());
        }
    }
}
