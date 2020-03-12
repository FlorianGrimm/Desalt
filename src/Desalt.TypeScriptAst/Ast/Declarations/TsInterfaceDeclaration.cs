// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsInterfaceDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an interface declaration.
    /// </summary>
    internal class TsInterfaceDeclaration : TsAstNode, ITsInterfaceDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsInterfaceDeclaration(
            ITsIdentifier interfaceName,
            ITsObjectType body,
            ITsTypeParameters? typeParameters = null,
            IEnumerable<ITsTypeReference>? extendsClause = null)
        {
            InterfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            TypeParameters = typeParameters;
            ExtendsClause = extendsClause?.ToImmutableArray();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier InterfaceName { get; }
        public ITsTypeParameters? TypeParameters { get; }
        public ImmutableArray<ITsTypeReference>? ExtendsClause { get; }
        public ITsObjectType Body { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitInterfaceDeclaration(this);
        }

        public override string CodeDisplay =>
            $"interface {InterfaceName}{TypeParameters}" +
            (ExtendsClause?.IsEmpty == true ? "" : $" extends {ExtendsClause?.ToElidedList()}") + Body;

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("interface ");
            InterfaceName.Emit(emitter);
            TypeParameters?.Emit(emitter);

            if (ExtendsClause?.IsEmpty == false)
            {
                emitter.Write(" extends ");
                emitter.WriteList(ExtendsClause, indent: false, itemDelimiter: ", ");
            }

            emitter.Write(" ");

            if (Body.TypeMembers.IsEmpty)
            {
                emitter.WriteLine("{");
                emitter.WriteLine("}");
            }
            else
            {
                Body.Emit(emitter);
                emitter.WriteLine();
            }
        }
    }
}
