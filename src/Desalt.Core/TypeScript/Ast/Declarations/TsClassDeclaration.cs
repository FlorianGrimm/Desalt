// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.TypeScript.Ast.Types;

    /// <summary>
    /// Represents a class declaration.
    /// </summary>
    internal class TsClassDeclaration : AstNode, ITsClassDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassDeclaration(
            ITsIdentifier className = null,
            ITsTypeParameters typeParameters = null,
            ITsClassHeritage heritage = null,
            IEnumerable<ITsClassElement> classBody = null)
        {
            ClassName = className;
            TypeParameters = typeParameters ?? new TsTypeParameters();
            Heritage = heritage ?? new TsClassHeritage();
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ClassName { get; }
        public ITsTypeParameters TypeParameters { get; }
        public ITsClassHeritage Heritage { get; }
        public ImmutableArray<ITsClassElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitClassDeclaration(this);

        public override string CodeDisplay =>
            $"class {ClassName?.CodeDisplay}{TypeParameters}{Heritage} {{ {ClassBody.ToElidedList()} }}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("class ");
            ClassName?.Emit(emitter);
            TypeParameters.Emit(emitter);
            Heritage.Emit(emitter);

            if (ClassName != null && Heritage.IsEmpty || !Heritage.IsEmpty)
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
