// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a class declaration.
    /// </summary>
    internal class TsClassDeclaration : AstNode<TsVisitor>, ITsClassDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassDeclaration(
            ITsIdentifier className = null,
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsClassHeritage heritage = null,
            IEnumerable<ITsClassElement> classBody = null)
        {
            ClassName = className;
            TypeParameters = typeParameters?.ToImmutableArray() ?? ImmutableArray<ITsTypeParameter>.Empty;
            Heritage = heritage ?? new TsClassHeritage();
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ClassName { get; }
        public ImmutableArray<ITsTypeParameter> TypeParameters { get; }
        public ITsClassHeritage Heritage { get; }
        public ImmutableArray<ITsClassElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitClassDeclaration(this);

        public override string CodeDisplay =>
            $"class {ClassName?.CodeDisplay}{TypeParameters.OptionalCodeDisplay()}{Heritage} " +
            $"{{ {ClassBody.ToElidedList()} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("class ");
            ClassName?.Emit(emitter);
            TypeParameters.EmitOptional(emitter);
            Heritage.Emit(emitter);

            emitter.WriteLine(" {");
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
