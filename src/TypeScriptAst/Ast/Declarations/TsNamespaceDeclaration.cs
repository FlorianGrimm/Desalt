// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNamespaceDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a namespace declaration.
    /// </summary>
    internal class TsNamespaceDeclaration : AstNode, ITsNamespaceDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsNamespaceDeclaration(ITsQualifiedName namespaceName, IEnumerable<ITsNamespaceElement> body = null)
        {
            NamespaceName = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
            Body = body?.ToImmutableArray() ?? ImmutableArray<ITsNamespaceElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsQualifiedName NamespaceName { get; }
        public ImmutableArray<ITsNamespaceElement> Body { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitNamespaceDeclaration(this);

        public override string CodeDisplay => $"namespace {NamespaceName} {{ {Body.ToElidedList()} }}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("namespace ");
            NamespaceName.Emit(emitter);
            emitter.Write(" ");

            emitter.WriteBlock(Body, skipNewlines: true);
            emitter.WriteLine();
        }
    }
}
