// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientClassDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.TypeScript.Ast.Types;

    /// <summary>
    /// Represents an ambient class declaration.
    /// </summary>
    internal class TsAmbientClassDeclaration : AstNode<TsVisitor>, ITsAmbientClassDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientClassDeclaration(
            ITsIdentifier className,
            ITsTypeParameters typeParameters = null,
            ITsClassHeritage heritage = null,
            IEnumerable<ITsAmbientClassBodyElement> classBody = null)
        {
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            TypeParameters = typeParameters ?? new TsTypeParameters();
            Heritage = heritage ?? new TsClassHeritage();
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsAmbientClassBodyElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ClassName { get; }
        public ITsTypeParameters TypeParameters { get; }
        public ITsClassHeritage Heritage { get; }
        public ImmutableArray<ITsAmbientClassBodyElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitAmbientClassDeclaration(this);

        public override string CodeDisplay =>
            $"class {ClassName}{TypeParameters}{Heritage} {{ {ClassBody.ToElidedList()} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("class ");
            ClassName.Emit(emitter);
            TypeParameters.Emit(emitter);
            Heritage.Emit(emitter);

            emitter.WriteLine(" {");
            emitter.IndentLevel++;

            foreach (ITsAmbientClassBodyElement element in ClassBody)
            {
                element.Emit(emitter);
            }

            emitter.IndentLevel--;
            emitter.WriteLine("}");
        }
    }
}
