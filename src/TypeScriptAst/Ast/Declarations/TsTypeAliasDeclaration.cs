// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeAliasDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Ast.Types;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a type alias of the form 'type alias&lt;T&gt; = type'.
    /// </summary>
    internal class TsTypeAliasDeclaration : AstNode, ITsTypeAliasDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsType type,
            ITsTypeParameters typeParameters = null)
        {
            AliasName = aliasName ?? throw new ArgumentNullException(nameof(aliasName));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            TypeParameters = typeParameters ?? TsTypeParameters.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier AliasName { get; }
        public ITsTypeParameters TypeParameters { get; }
        public ITsType Type { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTypeAliasDeclaration(this);

        public override string CodeDisplay => $"type {AliasName}{TypeParameters} = {Type};";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("type ");
            AliasName.Emit(emitter);
            TypeParameters.Emit(emitter);
            emitter.Write(" = ");
            Type.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
