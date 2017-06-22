// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsStringRequiredParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a required function parameter in the form <c>parameterName: 'stringLiteral'</c>.
    /// </summary>
    internal class TsStringRequiredParameter : CodeModel, ITsStringRequiredParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsStringRequiredParameter(ITsIdentifier parameterName, ITsStringLiteral stringLiteral)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            StringLiteral = stringLiteral ?? throw new ArgumentNullException(nameof(stringLiteral));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public ITsStringLiteral StringLiteral { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitStringRequiredParameter(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitStringRequiredParameter(this);

        public override string ToCodeDisplay() => $"{ParameterName.ToCodeDisplay()}: {StringLiteral.ToCodeDisplay()}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            ParameterName.WriteFullCodeDisplay(writer);
            writer.Write(": ");
            StringLiteral.WriteFullCodeDisplay(writer);
        }
    }
}
