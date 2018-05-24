﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TypeScriptAst.Ast.Types;

    /// <summary>
    /// Provides factory methods for creating TypeScript AST nodes.
    /// </summary>
    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Singleton Properties
        //// ===========================================================================================================

        public static ITsThisType ThisType => TsThisType.Instance;

        public static readonly ITsType AnyType = TsPredefinedType.Any;
        public static readonly ITsType NumberType = TsPredefinedType.Number;
        public static readonly ITsType BooleanType = TsPredefinedType.Boolean;
        public static readonly ITsType StringType = TsPredefinedType.String;
        public static readonly ITsType SymbolType = TsPredefinedType.Symbol;
        public static readonly ITsType VoidType = TsPredefinedType.Void;

        //// ===========================================================================================================
        //// Identifiers
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

        /// <summary>
        /// Creates a qualified name, which has dots between identifiers. For example, 'ns.type.method'.
        /// </summary>
        /// <param name="dottedName">A full name with or without dots that will be parsed.</param>
        /// <returns>An <see cref="ITsQualifiedName"/>.</returns>
        public static ITsQualifiedName QualifiedName(string dottedName)
        {
            string[] parts = dottedName?.Split('.') ?? throw new ArgumentNullException(nameof(dottedName));
            if (parts.Length > 1)
            {
                return QualifiedName(parts.ToArray());
            }

            return new TsQualifiedName(TsIdentifier.Get(parts[0]));
        }

        /// <summary>
        /// Creates a qualified name, which has dots between identifiers. For example, 'ns.type.method'.
        /// </summary>
        /// <param name="identifiers">The parts of the full name.</param>
        /// <returns>An <see cref="ITsQualifiedName"/>.</returns>
        public static ITsQualifiedName QualifiedName(params ITsIdentifier[] identifiers)
        {
            return new TsQualifiedName(identifiers.Last(), identifiers.Take(identifiers.Length - 1));
        }

        /// <summary>
        /// Creates a qualified name, which has dots between identifiers. For example, 'ns.type.method'.
        /// </summary>
        /// <param name="names">The parts of the full name.</param>
        /// <returns>An <see cref="ITsQualifiedName"/>.</returns>
        public static ITsQualifiedName QualifiedName(params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                throw new ArgumentException("Empty names array", nameof(names));
            }

            var right = TsIdentifier.Get(names.Last());
            IEnumerable<TsIdentifier> left = names.Take(names.Length - 1).Select(TsIdentifier.Get);
            return new TsQualifiedName(right, left);
        }

        /// <summary>
        /// Creates a qualified name with type arguments. For example, 'ns.type.method&lt;T1, T2&gt;'.
        /// </summary>
        /// <param name="dottedName">A full name with or without dots that will be parsed.</param>
        /// <param name="typeArguments">An optional list of type arguments.</param>
        /// <returns>An <see cref="ITsGenericTypeName"/>.</returns>
        public static ITsGenericTypeName GenericTypeName(string dottedName, params ITsType[] typeArguments)
        {
            var qualifiedName = QualifiedName(dottedName);
            return new TsGenericTypeName(qualifiedName.Right, qualifiedName.Left, typeArguments);
        }
    }
}
