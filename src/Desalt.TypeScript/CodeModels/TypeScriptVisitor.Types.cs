// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptVisitor.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    public abstract partial class TypeScriptVisitor
    {
        /// <summary>
        /// Visits a type surrounded in parentheses.
        /// </summary>
        public virtual void VisitParenthesizedType(ITsParenthesizedType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a predefined type, which is one of any, number, boolean, string, symbol, or void.
        /// </summary>
        public virtual void VisitPredefinedType(ITsPredefinedType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript type reference.
        /// </summary>
        public virtual void VisitTypeReference(ITsTypeReference model) => DefaultVisit(model);

        /// <summary>
        /// Visits a type parameter of the form &lt;MyType extends MyBase&gt;.
        /// </summary>
        public virtual void VisitTypeParameter(ITsTypeParameter model) => DefaultVisit(model);
    }

    public abstract partial class TypeScriptVisitor<TResult>
    {
        /// <summary>
        /// Visits a type surrounded in parentheses.
        /// </summary>
        public virtual TResult VisitParenthesizedType(ITsParenthesizedType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a predefined type, which is one of any, number, boolean, string, symbol, or void.
        /// </summary>
        public virtual TResult VisitPredefinedType(ITsPredefinedType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript type reference.
        /// </summary>
        public virtual TResult VisitTypeReference(ITsTypeReference model) => DefaultVisit(model);

        /// <summary>
        /// Visits a type parameter of the form &lt;MyType extends MyBase&gt;.
        /// </summary>
        public virtual TResult VisitTypeParameter(ITsTypeParameter model) => DefaultVisit(model);
    }
}
