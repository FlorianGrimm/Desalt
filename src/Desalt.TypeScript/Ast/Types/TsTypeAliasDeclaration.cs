// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeAliasDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a type alias of the form 'type alias&lt;T&gt; = type'.
    /// </summary>
    internal class TsTypeAliasDeclaration : AstNode<TsVisitor>, ITsTypeAliasDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsType type,
            IEnumerable<ITsTypeParameter> typeParameters = null)
        {
            AliasName = aliasName ?? throw new ArgumentNullException(nameof(aliasName));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier AliasName { get; }
        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
        public ITsType Type { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTypeAliasDeclaration(this);

        public override string CodeDisplay
        {
            get
            {
                string display = $"type {AliasName}";
                if (TypeParameters.Length > 0)
                {
                    display += $"<{TypeParameters.ToElidedList()}>";
                }

                display += $" = {Type}";
                return display;
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("type ");
            AliasName.Emit(emitter);

            if (TypeParameters.Length > 0)
            {
                WriteItems(emitter, TypeParameters, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            emitter.Write(" = ");
            Type.Emit(emitter);
        }
    }
}
