// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientFunctionDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using System.Text;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an ambient function declaration of the form 'function name signature;'.
    /// </summary>
    internal class TsAmbientFunctionDeclaration : TsAstNode, ITsAmbientFunctionDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientFunctionDeclaration(ITsIdentifier functionName, ITsCallSignature callSignature)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier FunctionName { get; }
        public ITsCallSignature CallSignature { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitAmbientFunctionDeclaration(this);
        }

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("function ").Append(FunctionName).Append(" ");
                builder.Append(CallSignature.CodeDisplay);
                builder.Append(";");
                return builder.ToString();
            }
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("function ");
            FunctionName.Emit(emitter);
            CallSignature.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
