// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast
{
    public abstract partial class TsVisitor
    {
        /// <summary>
        /// Visits a type surrounded in parentheses.
        /// </summary>
        public virtual void VisitParenthesizedType(ITsParenthesizedType node) => Visit(node);

        /// <summary>
        /// Visits a predefined type, which is one of any, number, boolean, string, symbol, or void.
        /// </summary>
        public virtual void VisitPredefinedType(ITsType node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript type reference.
        /// </summary>
        public virtual void VisitTypeReference(ITsTypeReference node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript object type.
        /// </summary>
        public virtual void VisitObjectType(ITsObjectType node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript array type.
        /// </summary>
        public virtual void VisitArrayType(ITsArrayType node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript tuple type.
        /// </summary>
        public virtual void VisitTupleType(ITsTupleType node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript function type.
        /// </summary>
        public virtual void VisitFunctionType(ITsFunctionType node) => Visit(node);

        /// <summary>
        /// Visits a TypeScript constructor type.
        /// </summary>
        public virtual void VisitConstructorType(ITsFunctionType node) => Visit(node);

        /// <summary>
        /// Visits a typeof query.
        /// </summary>
        public virtual void VisitTypeQuery(ITsTypeQuery node) => Visit(node);

        /// <summary>
        /// Visits a 'this' type.
        /// </summary>
        public virtual void VisitThisType(ITsThisType node) => Visit(node);

        /// <summary>
        /// Visits a property signature.
        /// </summary>
        public virtual void VisitPropertySignature(ITsPropertySignature node) => Visit(node);

        /// <summary>
        /// Visits a a call signature of the form '&gt;T&lt;(parameter: type): type'..
        /// </summary>
        public virtual void VisitCallSignature(ITsCallSignature node) => Visit(node);

        /// <summary>
        /// Visits a parameter list of the form '(parameter: type)'..
        /// </summary>
        public virtual void VisitParameterList(ITsParameterList node) => Visit(node);

        /// <summary>
        /// Visits a list of type parameters of the form '&lt;type, type&gt;'.
        /// </summary>
        public virtual void VisitTypeParameters(ITsTypeParameters node) => Visit(node);

        /// <summary>
        /// Visits a type parameter of the form &lt;MyType extends MyBase&gt;.
        /// </summary>
        public virtual void VisitTypeParameter(ITsTypeParameter node) => Visit(node);

        /// <summary>
        /// Visits a bound required parameter in a parameter list for a function.
        /// </summary>
        public virtual void VisitBoundRequiredParameter(ITsBoundRequiredParameter node) => Visit(node);

        /// <summary>
        /// Visits a required function parameter in the form <c>parameterName: 'stringLiteral'</c>.
        /// </summary>
        public virtual void VisitStringRequiredParameter(ITsStringRequiredParameter node) => Visit(node);

        /// <summary>
        /// Visits a a bound optional parameter in a function.
        /// </summary>
        public virtual void VisitBoundOptionalParameter(ITsBoundOptionalParameter node) => Visit(node);

        /// <summary>
        /// Visits a required function parameter in the form <c>parameterName: 'stringLiteral'</c>.
        /// </summary>
        public virtual void VisitStringOptionalParameter(ITsStringOptionalParameter node) => Visit(node);

        /// <summary>
        /// Visits a a function parameter of the form '... parameterName: type'.
        /// </summary>
        public virtual void VisitRestParameter(ITsRestParameter node) => Visit(node);

        /// <summary>
        /// Visits a constructor method signature of the form 'new &lt;T&gt;(parameter: type): type'.
        /// </summary>
        public virtual void VisitConstructSignature(ITsConstructSignature node) => Visit(node);

        /// <summary>
        /// Visits an index signature of the form '[parameterName: string|number]: type'.
        /// </summary>
        public virtual void VisitIndexSignature(ITsIndexSignature node) => Visit(node);

        /// <summary>
        /// Visits a method signature, which is a shorthand for declaring a property of a function type.
        /// </summary>
        public virtual void VisitMethodSignature(ITsMethodSignature node) => Visit(node);

        /// <summary>
        /// Visits a union type of the form 'type1 | type2'.
        /// </summary>
        public virtual void VisitUnionType(ITsUnionType node) => Visit(node);

        /// <summary>
        /// Visits an interection type of the form 'type1 &amp; type2'.
        /// </summary>
        public virtual void VisitIntersectionType(ITsIntersectionType node) => Visit(node);
    }
}
