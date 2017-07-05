// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a function declaration of the form 'function [name] signature { body }'.
    /// </summary>
    internal class TsFunctionDeclaration : AstNode<TsVisitor>, ITsFunctionDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFunctionDeclaration(
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            FunctionName = functionName;
            if (functionBody != null)
            {
                FunctionBody = functionBody.ToImmutableArray();
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier FunctionName { get; }
        public ITsCallSignature CallSignature { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitFunctionDeclaration(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("function ");

                if (FunctionName != null)
                {
                    builder.Append(FunctionName).Append(" ");
                }

                builder.Append(CallSignature.CodeDisplay);

                if (FunctionBody.IsDefault)
                {
                    builder.Append(";");
                }
                else
                {
                    builder.Append("{").Append(FunctionBody.ToElidedList()).Append("}");
                }

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("function");

            if (FunctionName != null)
            {
                emitter.Write(" ");
                FunctionName.Emit(emitter);
            }

            CallSignature.Emit(emitter);

            if (FunctionBody.IsDefault)
            {
                emitter.WriteLine(";");
            }
            else
            {
                emitter.Write(" ");
                emitter.WriteBlock(FunctionBody, skipNewlines: true);
                emitter.WriteLine();
            }
        }
    }
}
