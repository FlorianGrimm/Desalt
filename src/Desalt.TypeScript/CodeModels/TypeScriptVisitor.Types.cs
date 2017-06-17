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
        /// Visits a TypeScript object type.
        /// </summary>
        public virtual void VisitObjectType(ITsObjectType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript array type.
        /// </summary>
        public virtual void VisitArrayType(ITsArrayType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript tuple type.
        /// </summary>
        public virtual void VisitTupleType(ITsTupleType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript function type.
        /// </summary>
        public virtual void VisitFunctionType(ITsFunctionType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript constructor type.
        /// </summary>
        public virtual void VisitConstructorType(ITsFunctionType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a typeof query.
        /// </summary>
        public virtual void VisitTypeQuery(ITsTypeQuery model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'this' type.
        /// </summary>
        public virtual void VisitThisType(ITsThisType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property signature.
        /// </summary>
        public virtual void VisitPropertySignature(ITsPropertySignature model) => DefaultVisit(model);

        /// <summary>
        /// Visits a a call signature of the form '&gt;T&lt;(parameter: type): type'..
        /// </summary>
        public virtual void VisitCallSignature(ITsCallSignature model) => DefaultVisit(model);

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
        /// Visits a TypeScript object type.
        /// </summary>
        public virtual TResult VisitObjectType(ITsObjectType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript array type.
        /// </summary>
        public virtual TResult VisitArrayType(ITsArrayType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript tuple type.
        /// </summary>
        public virtual TResult VisitTupleType(ITsTupleType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript function type.
        /// </summary>
        public virtual TResult VisitFunctionType(ITsFunctionType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a TypeScript constructor type.
        /// </summary>
        public virtual TResult VisitConstructorType(ITsFunctionType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a typeof query.
        /// </summary>
        public virtual TResult VisitTypeQuery(ITsTypeQuery model) => DefaultVisit(model);

        /// <summary>
        /// Visits a 'this' type.
        /// </summary>
        public virtual TResult VisitThisType(ITsThisType model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property signature.
        /// </summary>
        public virtual TResult VisitPropertySignature(ITsPropertySignature model) => DefaultVisit(model);

        /// <summary>
        /// Visits a a call signature of the form '&gt;T&lt;(parameter: type): type'..
        /// </summary>
        public virtual TResult VisitCallSignature(ITsCallSignature model) => DefaultVisit(model);

        /// <summary>
        /// Visits a type parameter of the form &lt;MyType extends MyBase&gt;.
        /// </summary>
        public virtual TResult VisitTypeParameter(ITsTypeParameter model) => DefaultVisit(model);
    }
}
