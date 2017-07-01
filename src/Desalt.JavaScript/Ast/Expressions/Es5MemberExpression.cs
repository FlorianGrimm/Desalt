// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5MemberExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    public sealed class Es5MemberExpression : AstNode<Es5Visitor>, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5MemberExpression(
            IEs5Expression memberExpression,
            IEs5Expression bracketExpression = null,
            Es5Identifier dotName = null)
        {
            MemberExpression = memberExpression ?? throw new ArgumentNullException(nameof(memberExpression));
            BracketExpression = bracketExpression;
            DotName = dotName;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a value indicating whether this member expression represents a bracket notation
        /// reference, 'member[Expression]'.
        /// </summary>
        public bool IsBracketNotation => BracketExpression != null;

        /// <summary>
        /// Gets a value indicating whether this member expression represents a dot notation
        /// reference, 'member.name'.
        /// </summary>
        public bool IsDotNotation => DotName != null;

        /// <summary>
        /// Gets the expression on the left hand side of the bracket, dot, or call.
        /// </summary>
        public IEs5Expression MemberExpression { get; }

        /// <summary>
        /// Gets the expression inside of the brackets if this is a bracket-notation member expression.
        /// </summary>
        public IEs5Expression BracketExpression { get; }

        /// <summary>
        /// Gets the name after the dot if this is a dot-notation member expression.
        /// </summary>
        public Es5Identifier DotName { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitMemberExpression(this);
        }

        public override string CodeDisplay =>
            MemberExpression + (IsBracketNotation ? $"[{BracketExpression}]" : $".{DotName}");

        public override void Emit(Emitter emitter)
        {
            MemberExpression.Emit(emitter);
            if (IsBracketNotation)
            {
                emitter.Write("[");
                BracketExpression.Emit(emitter);
                emitter.Write("]");
            }
            else
            {
                emitter.Write(".");
                DotName.Emit(emitter);
            }
        }

        internal static Es5MemberExpression CreateBracketNotation(
            IEs5Expression memberExpression,
            IEs5Expression bracketExpression)
        {
            return new Es5MemberExpression(memberExpression, bracketExpression: bracketExpression);
        }

        internal static Es5MemberExpression CreateDotNotation(
            IEs5Expression memberExpression,
            Es5Identifier dotName)
        {
            return new Es5MemberExpression(memberExpression, dotName: dotName);
        }
    }
}
