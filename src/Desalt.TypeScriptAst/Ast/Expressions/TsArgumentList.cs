// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArgumentList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an argument list of the form '&lt;T&gt;(x: type, y: type).
    /// </summary>
    internal class TsArgumentList : TsAstNode, ITsArgumentList
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArgumentList(IEnumerable<ITsType>? typeArguments = null, IEnumerable<ITsArgument>? arguments = null)
        {
            TypeArguments = typeArguments?.ToImmutableArray() ?? ImmutableArray<ITsType>.Empty;
            Arguments = arguments?.ToImmutableArray() ?? ImmutableArray<ITsArgument>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsType> TypeArguments { get; }
        public ImmutableArray<ITsArgument> Arguments { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitArgumentList(this);
        }

        public override string CodeDisplay =>
            (TypeArguments.IsEmpty ? "" : $"<{TypeArguments.ToElidedList()}>") + $"({Arguments.ToElidedList()})";

        protected override void EmitContent(Emitter emitter)
        {
            if (!TypeArguments.IsEmpty)
            {
                emitter.WriteList(TypeArguments, indent: false, prefix: "<", suffix: ">", itemDelimiter: ", ");
            }

            emitter.WriteParameterList(Arguments);
        }
    }
}
