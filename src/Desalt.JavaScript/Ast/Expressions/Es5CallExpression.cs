// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5CallExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a function call expression.
    /// </summary>
    public sealed class Es5CallExpression : AstNode<Es5Visitor>, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5CallExpression(
            IEs5Expression callExpression,
            IEnumerable<IEs5Expression> arguments,
            bool isNewCall)
        {
            CallExpression = callExpression ?? throw new ArgumentNullException(nameof(callExpression));
            Arguments = arguments?.ToImmutableArray() ?? ImmutableArray<IEs5Expression>.Empty;
            IsNewCall = isNewCall;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the function call expression.
        /// </summary>
        public IEs5Expression CallExpression { get; }

        /// <summary>
        /// Gets the list of arguments.
        /// </summary>
        public ImmutableArray<IEs5Expression> Arguments { get; }

        /// <summary>
        /// Gets a value indicating whether this is a 'new' call or a standard function call.
        /// </summary>
        public bool IsNewCall { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitCallExpression(this);
        }

        public override string CodeDisplay =>
            (IsNewCall ? "new " : "") + $"{CallExpression}({Arguments.ToElidedList()})";

        public override void Emit(Emitter emitter)
        {
            if (IsNewCall)
            {
                emitter.Write("new ");
            }

            CallExpression.Emit(emitter);
            emitter.WriteParameterList(Arguments);
        }

        public Es5CallExpression WithArguments(ImmutableArray<IEs5Expression> arguments)
        {
            return new Es5CallExpression(CallExpression, arguments, IsNewCall);
        }
    }
}
