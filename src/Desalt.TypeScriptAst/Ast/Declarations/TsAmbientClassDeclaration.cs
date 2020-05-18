// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientClassDeclaration.cs" company="Justin Rockwood">
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
    /// Represents an ambient class declaration.
    /// </summary>
    internal class TsAmbientClassDeclaration : TsAstNode, ITsAmbientClassDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientClassDeclaration(
            ITsIdentifier className,
            ITsTypeParameters? typeParameters = null,
            ITsClassHeritage? heritage = null,
            IEnumerable<ITsAmbientClassBodyElement>? classBody = null)
        {
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            TypeParameters = typeParameters ?? TsAstFactory.TypeParameters();
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

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitAmbientClassDeclaration(this);
        }

        protected override void EmitContent(Emitter emitter)
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
