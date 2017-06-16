// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsRegularExpressionLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a regular expression literal.
    /// </summary>
    internal class TsRegularExpressionLiteral : CodeModel, ITsRegularExpressionLiteral
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitRegularExpressionLiteral(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitRegularExpressionLiteral(this);

        public override string ToCodeDisplay() => $"/{Body}/{Flags}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
