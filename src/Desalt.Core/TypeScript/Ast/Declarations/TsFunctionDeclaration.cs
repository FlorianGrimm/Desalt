// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a function declaration of the form 'function [name] signature { body }'.
    /// </summary>
    internal class TsFunctionDeclaration : AstNode, ITsFunctionDeclaration, ITsAmbientFunctionDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsFunctionDeclaration(
            bool isAmbient,
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            IsAmbient = isAmbient;
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

        private bool IsAmbient { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsFunctionDeclaration Create(
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsFunctionDeclaration(false, callSignature, functionName, functionBody);
        }

        public static ITsAmbientFunctionDeclaration CreateAmbient(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return new TsFunctionDeclaration(
                true,
                callSignature,
                functionName ?? throw new ArgumentNullException(nameof(functionName)));
        }

        public override void Accept(TsVisitor visitor)
        {
            if (IsAmbient)
            {
                visitor.VisitAmbientFunctionDeclaration(this);
            }
            else
            {
                visitor.VisitFunctionDeclaration(this);
            }
        }

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

        protected override void EmitInternal(Emitter emitter)
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
