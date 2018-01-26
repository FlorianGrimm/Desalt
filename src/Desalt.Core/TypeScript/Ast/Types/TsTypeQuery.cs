﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTypeQuery.cs" company="Justin Rockwood">
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
    /// Represents a 'typeof' query.
    /// </summary>
    internal class TsTypeQuery : AstNode<TsVisitor>, ITsTypeQuery
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTypeQuery(ITsTypeName query)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsTypeName Query { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitTypeQuery(this);

        public override string CodeDisplay => $"typeof {Query}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("typeof ");
            Query.Emit(emitter);
        }
    }
}
