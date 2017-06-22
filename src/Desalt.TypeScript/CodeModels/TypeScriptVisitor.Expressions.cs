// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptVisitor.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    public abstract partial class TypeScriptVisitor
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual void VisitThis(ITsThis model) => DefaultVisit(model);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual void VisitNullLiteral(ITsNullLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual void VisitBooleanLiteral(ITsBooleanLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual void VisitNumericLiteral(ITsNumericLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual void VisitStringLiteral(ITsStringLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual void VisitArrayLiteral(ITsArrayLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual void VisitArrayElement(ITsArrayElement model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual void VisitObjectLiteral(ITsObjectLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an element in an object initializer of the form 'identifer = expression'.
        /// </summary>
        /// <param name="model"></param>
        public virtual void VisitCoverInitializedName(ITsCoverInitializedName model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual void VisitPropertyAssignment(ITsPropertyAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a computed property name within an object literal of the form '[expression]'.
        /// </summary>
        public virtual void VisitComputedPropertyName(ITsComputedPropertyName model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal property function.
        /// </summary>
        public virtual void VisitPropertyFunction(ITsPropertyFunction model) => DefaultVisit(model);
    }

    public abstract partial class TypeScriptVisitor<TResult>
    {
        /// <summary>
        /// Visits an expression representing the literal 'this'.
        /// </summary>
        public virtual TResult VisitThis(ITsThis model) => DefaultVisit(model);

        /// <summary>
        /// Visits a null literal.
        /// </summary>
        public virtual TResult VisitNullLiteral(ITsNullLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a boolean literal.
        /// </summary>
        public virtual TResult VisitBooleanLiteral(ITsBooleanLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a numeric literal.
        /// </summary>
        public virtual TResult VisitNumericLiteral(ITsNumericLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a string literal.
        /// </summary>
        public virtual TResult VisitStringLiteral(ITsStringLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits a regular expression literal.
        /// </summary>
        public virtual TResult VisitRegularExpressionLiteral(ITsRegularExpressionLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array literal of the form '[element, element...]'.
        /// </summary>
        public virtual TResult VisitArrayLiteral(ITsArrayLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an array element.
        /// </summary>
        public virtual TResult VisitArrayElement(ITsArrayElement model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal of the form '{ PropertDefinition, ... }'.
        /// </summary>
        public virtual TResult VisitObjectLiteral(ITsObjectLiteral model) => DefaultVisit(model);

        /// <summary>
        /// Visits an element in an object initializer of the form 'identifer = expression'.
        /// </summary>
        /// <param name="model"></param>
        public virtual TResult VisitCoverInitializedName(ITsCoverInitializedName model) => DefaultVisit(model);

        /// <summary>
        /// Visits a property value assignment within an object literal of the form 'property: value'.
        /// </summary>
        public virtual TResult VisitPropertyAssignment(ITsPropertyAssignment model) => DefaultVisit(model);

        /// <summary>
        /// Visits a computed property name within an object literal of the form '[expression]'.
        /// </summary>
        public virtual TResult VisitComputedPropertyName(ITsComputedPropertyName model) => DefaultVisit(model);

        /// <summary>
        /// Visits an object literal property function.
        /// </summary>
        public virtual TResult VisitPropertyFunction(ITsPropertyFunction model) => DefaultVisit(model);
    }
}
