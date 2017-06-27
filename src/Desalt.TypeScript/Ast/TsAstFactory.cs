// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.TypeScript.Ast.Types;

    /// <summary>
    /// Provides factory methods for creating TypeScript AST nodes.
    /// </summary>
    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Singleton Properties
        //// ===========================================================================================================

        public static ITsThisType ThisType => TsThisType.Instance;

        public static readonly ITsPredefinedType Any = TsPredefinedType.Any;
        public static readonly ITsPredefinedType Number = TsPredefinedType.Number;
        public static readonly ITsPredefinedType Boolean = TsPredefinedType.Boolean;
        public static readonly ITsPredefinedType String = TsPredefinedType.String;
        public static readonly ITsPredefinedType Symbol = TsPredefinedType.Symbol;
        public static readonly ITsPredefinedType Void = TsPredefinedType.Void;

        //// ===========================================================================================================
        //// Identifiers
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

        public static ITsQualifiedName QualifiedName(string dottedName)
        {
            string[] parts = dottedName.Split('.');
            if (parts.Length > 1)
            {
                return QualifiedName(parts[0], parts.Skip(1).ToArray());
            }

            return new TsQualifiedName(TsIdentifier.Get(parts[0]));
        }

        public static ITsQualifiedName QualifiedName(string name, params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return new TsQualifiedName(TsIdentifier.Get(name));
            }

            var right = TsIdentifier.Get(names.Last());
            IEnumerable<TsIdentifier> left = new[] { name }.Concat(names.Take(names.Length - 1)).Select(TsIdentifier.Get);
            return new TsQualifiedName(right, left);
        }

        //// ===========================================================================================================
        //// Source Files
        //// ===========================================================================================================

        public static ImplementationSourceFile ImplementationSourceFile(
            IEnumerable<IImplementationScriptElement> scriptElements)
        {
            return new ImplementationSourceFile(scriptElements);
        }

        public static ImplementationSourceFile ImplementationSourceFile(
            params IImplementationScriptElement[] scriptElements)
        {
            return new ImplementationSourceFile(scriptElements);
        }
    }
}
