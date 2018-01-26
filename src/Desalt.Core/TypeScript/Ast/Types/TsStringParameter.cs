// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsStringParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a required or optional function parameter in the form <c>parameterName: 'stringLiteral'</c>.
    /// </summary>
    internal class TsStringParameter : AstNode<TsVisitor>, ITsStringRequiredParameter, ITsStringOptionalParameter
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

        public override void Accept(TsVisitor visitor) => visitor.VisitStringRequiredParameter(this);

        public override string CodeDisplay
        {
            get
            {
                string display = ParameterName.CodeDisplay;
                if (IsOptional)
                {
                    display += "?";
                }

                display += $": {StringLiteral}";

                return display;
            }
        }

        protected override void EmitInternal(Emitter emitter)
        {
            ParameterName.Emit(emitter);
            if (IsOptional)
            {
                emitter.Write("?");
            }

            emitter.Write(": ");
            StringLiteral.Emit(emitter);
        }
    }
}
