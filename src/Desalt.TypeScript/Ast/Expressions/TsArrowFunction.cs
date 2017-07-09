// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrowFunction.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an arrow function expression of the form '() => body'.
    /// </summary>
    internal class TsArrowFunction : AstNode<TsVisitor>, ITsArrowFunction
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrowFunction(ITsIdentifier singleParameterName, ITsExpression bodyExpression)
        {
            SingleParameterName = singleParameterName ?? throw new ArgumentNullException(nameof(singleParameterName));
            BodyExpression = bodyExpression ?? throw new ArgumentNullException(nameof(bodyExpression));
        }

        public TsArrowFunction(ITsIdentifier singleParameterName, IEnumerable<ITsStatementListItem> body)
        {
            SingleParameterName = singleParameterName ?? throw new ArgumentNullException(nameof(singleParameterName));
            Body = body?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        public TsArrowFunction(ITsCallSignature callSignature, ITsExpression bodyExpression)
        {
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            BodyExpression = bodyExpression ?? throw new ArgumentNullException(nameof(bodyExpression));
        }

        public TsArrowFunction(ITsCallSignature callSignature, IEnumerable<ITsStatementListItem> body)
        {
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            Body = body?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier SingleParameterName { get; }
        public ITsCallSignature CallSignature { get; }

        public ITsExpression BodyExpression { get; }
        public ImmutableArray<ITsStatementListItem> Body { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitArrowFunction(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                if (SingleParameterName != null)
                {
                    builder.Append(SingleParameterName);
                }
                else
                {
                    builder.Append(CallSignature);
                }

                builder.Append(" => ");

                if (BodyExpression != null)
                {
                    builder.Append(BodyExpression);
                }
                else
                {
                    builder.Append("{ ").Append(Body.ToElidedList()).Append("}");
                }

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            if (SingleParameterName != null)
            {
                SingleParameterName.Emit(emitter);
            }
            else
            {
                CallSignature.Emit(emitter);
            }

            emitter.Write(" => ");

            if (BodyExpression != null)
            {
                BodyExpression.Emit(emitter);
            }
            else
            {
                emitter.WriteBlock(Body, skipNewlines: true);
            }
        }
    }
}
