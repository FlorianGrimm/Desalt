// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a class declaration.
    /// </summary>
    internal class TsClassDeclaration : TsAstNode, ITsClassDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassDeclaration(
            ITsIdentifier? className = null,
            ITsTypeParameters? typeParameters = null,
            ITsClassHeritage? heritage = null,
            bool isAbstract = false,
            IEnumerable<ITsClassElement>? classBody = null)
        {
            ClassName = className;
            TypeParameters = typeParameters ?? TsAstFactory.TypeParameters();
            Heritage = heritage;
            IsAbstract = isAbstract;
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier? ClassName { get; }
        public ITsTypeParameters TypeParameters { get; }
        public ITsClassHeritage? Heritage { get; }
        public bool IsAbstract { get; }
        public ImmutableArray<ITsClassElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitClassDeclaration(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            if (IsAbstract)
            {
                emitter.Write("abstract ");
            }

            emitter.Write("class ");
            ClassName?.Emit(emitter);
            TypeParameters.Emit(emitter);
            Heritage?.Emit(emitter);

            if ((ClassName != null && Heritage == null) || Heritage != null)
            {
                emitter.Write(" ");
            }

            emitter.WriteLine("{");
            emitter.IndentLevel++;

            for (int i = 0; i < ClassBody.Length; i++)
            {
                ITsClassElement element = ClassBody[i];
                element.Emit(emitter);

                if (i < ClassBody.Length - 1)
                {
                    emitter.WriteLineWithoutIndentation();
                }
            }

            emitter.IndentLevel--;
            emitter.WriteLine("}");
        }
    }
}
