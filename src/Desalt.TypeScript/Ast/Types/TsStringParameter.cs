// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsStringParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a required or optional function parameter in the form <c>parameterName: 'stringLiteral'</c>.
    /// </summary>
    internal class TsStringParameter : AstNode, ITsStringRequiredParameter, ITsStringOptionalParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsStringParameter(ITsIdentifier parameterName, ITsStringLiteral stringLiteral, bool isOptional)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            StringLiteral = stringLiteral ?? throw new ArgumentNullException(nameof(stringLiteral));
            IsOptional = isOptional;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public ITsStringLiteral StringLiteral { get; }
        public bool IsOptional { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitStringRequiredParameter(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitStringRequiredParameter(this);

        public override string ToCodeDisplay()
        {
            string display = ParameterName.ToCodeDisplay();
            if (IsOptional)
            {
                display += "?";
            }

            display += $": {StringLiteral}";

            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            ParameterName.WriteFullCodeDisplay(writer);
            if (IsOptional)
            {
                writer.Write("?");
            }

            writer.Write(": ");
            StringLiteral.WriteFullCodeDisplay(writer);
        }
    }
}
