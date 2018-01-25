// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsImportClause.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an import clause of the form 'identifier', '* as identifier', '{ importSpecifier, ... }',
    /// 'identifier, * as identifier', or 'identifier, { importSpecifier, ... }'.
    /// </summary>
    internal class TsImportClause : AstNode<TsVisitor>, ITsImportClause
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsImportClause(
            ITsIdentifier defaultBinding,
            ITsIdentifier namespaceBinding,
            ImmutableArray<ITsImportSpecifier>? namedImports)
        {
            DefaultBinding = defaultBinding;
            NamespaceBinding = namespaceBinding;
            NamedImports = namedImports;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier DefaultBinding { get; }
        public ITsIdentifier NamespaceBinding { get; }
        public ImmutableArray<ITsImportSpecifier>? NamedImports { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, * as identifier'.
        /// </summary>
        public static TsImportClause CreateDefaultBinding(
            ITsIdentifier defaultBinding,
            ITsIdentifier namespaceBinding = null)
        {
            return new TsImportClause(
                defaultBinding ?? throw new ArgumentNullException(nameof(defaultBinding)),
                namespaceBinding,
                namedImports: null);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, { importSpecifier, importSpecifier }'.
        /// </summary>
        public static TsImportClause CreateDefaultBinding(
            ITsIdentifier defaultBinding,
            IEnumerable<ITsImportSpecifier> namedImports = null)
        {
            ImmutableArray<ITsImportSpecifier>? array = namedImports?.ToImmutableArray();

            if (array.GetValueOrDefault().IsDefaultOrEmpty)
            {
                throw new ArgumentException("Empty import specifier list", nameof(namedImports));
            }

            return new TsImportClause(
                defaultBinding ?? throw new ArgumentNullException(nameof(defaultBinding)),
                namespaceBinding: null,
                namedImports: array);
        }

        /// <summary>
        /// Create an import clause of the form '* as identifier'.
        /// </summary>
        public static TsImportClause CreateNamespaceBinding(ITsIdentifier namespaceBinding) =>
            new TsImportClause(
                defaultBinding: null,
                namespaceBinding: namespaceBinding ?? throw new ArgumentNullException(nameof(namespaceBinding)),
                namedImports: null);

        /// <summary>
        /// Create an import clause of the form '{ importSpecifier, importSpecifier }'.
        /// </summary>
        public static TsImportClause CreateNamedImports(IEnumerable<ITsImportSpecifier> namedImports)
        {
            if (namedImports == null)
            {
                throw new ArgumentNullException(nameof(namedImports));
            }

            ImmutableArray<ITsImportSpecifier> array = namedImports.ToImmutableArray();
            if (array.IsEmpty)
            {
                throw new ArgumentException("Empty import specifier list", nameof(namedImports));
            }

            return new TsImportClause(
                defaultBinding: null,
                namespaceBinding: null,
                namedImports: array);
        }

        public override void Accept(TsVisitor visitor) => visitor.VisitImportClause(this);

        public override string CodeDisplay
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                if (DefaultBinding != null)
                {
                    builder.Append(DefaultBinding.CodeDisplay);
                    if (NamespaceBinding != null || NamedImports != null)
                    {
                        builder.Append(", ");
                    }
                }

                if (NamespaceBinding != null)
                {
                    builder.Append("* as ").Append(NamespaceBinding.CodeDisplay);
                }

                if (NamedImports != null)
                {
                    builder.Append("{ ").Append(NamedImports?.ToElidedList()).Append(" }");
                }

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            if (DefaultBinding != null)
            {
                DefaultBinding.Emit(emitter);
                if (NamespaceBinding != null || NamedImports != null)
                {
                    emitter.Write(", ");
                }
            }

            if (NamespaceBinding != null)
            {
                emitter.Write("* as ");
                NamespaceBinding.Emit(emitter);
            }

            if (NamedImports != null)
            {
                emitter.WriteList(NamedImports, indent: false, prefix: "{ ", suffix: " }", itemDelimiter: ", ");
            }
        }
    }
}
