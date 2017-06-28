// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsRegularExpressionLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a regular expression literal.
    /// </summary>
    internal class TsRegularExpressionLiteral : AstNode<TsVisitor>, ITsRegularExpressionLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsRegularExpressionLiteral(string body, string flags)
        {
            Param.VerifyString(body, nameof(body));

            Body = body;
            Flags = flags ?? string.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Body { get; }
        public string Flags { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitRegularExpressionLiteral(this);

        public override string CodeDisplay => $"/{Body}/{Flags}";

        public override void Emit(Emitter emitter) => emitter.Write(CodeDisplay);
    }
}
