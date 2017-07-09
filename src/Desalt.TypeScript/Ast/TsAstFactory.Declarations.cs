// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Generic;
    using Desalt.TypeScript.Ast.Declarations;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Creates a simple lexical binding of the form 'x: type = y'.
        /// </summary>
        public static ITsSimpleLexicalBinding SimpleLexicalBinding(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsSimpleLexicalBinding(variableName, variableType, initializer);
        }

        /// <summary>
        /// Creates a destructuring lexical binding of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringLexicalBinding DestructuringLexicalBinding(
            ITsBindingPattern bindingPattern,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsDestructuringLexicalBinding(bindingPattern, variableType, initializer);
        }

        /// <summary>
        /// Creates a lexical declaration of the form 'const|let x: type, y: type = z;'.
        /// </summary>
        public static ITsLexicalDeclaration LexicalDeclaration(
            bool isConst,
            params ITsLexicalBinding[] declarations)
        {
            return new TsLexicalDeclaration(isConst, declarations);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(ITsCallSignature callSignature) =>
            new TsFunctionDeclaration(callSignature);

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return new TsFunctionDeclaration(callSignature, functionName);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionDeclaration(callSignature, functionName, functionBody);
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(ITsIdentifier aliasName, ITsType type) =>
            new TsTypeAliasDeclaration(aliasName, type);

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsTypeParameter typeParameter,
            ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, type, TypeParameters(typeParameter));
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsTypeParameters typeParameters,
            ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, type, typeParameters);
        }

        public static ITsConstructorDeclaration ConstructorDeclaration(
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList parameterList = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsConstructorDeclaration(accessibilityModifier, parameterList, functionBody);
        }

        /// <summary>
        /// Creates a member variable declaration in a class.
        /// </summary>
        public static ITsVariableMemberDeclaration VariableMemberDeclaration(
            ITsPropertyName propertyName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            return new TsVariableMemberDeclaration(
                propertyName, accessibilityModifier, isStatic, typeAnnotation, initializer);
        }

        /// <summary>
        /// Creates a member function declaration in a class.
        /// </summary>
        public static ITsFunctionMemberDeclaration FunctionMemberDeclaration(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsFunctionMemberDeclaration(
                functionName, callSignature, accessibilityModifier, isStatic, functionBody);
        }

        /// <summary>
        /// Creates a 'get' member accessor declaration in a class.
        /// </summary>
        public static ITsGetAccessorMemberDeclaration GetAccessorMemberDeclaration(
            ITsGetAccessor getAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false)
        {
            return new TsGetSetAccessorMemberDeclaration(getAccessor, accessibilityModifier, isStatic);
        }

        /// <summary>
        /// Creates a 'set' member accessor declaration in a class.
        /// </summary>
        public static ITsSetAccessorMemberDeclaration SetAccessorMemberDeclaration(
            ITsSetAccessor setAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false)
        {
            return new TsGetSetAccessorMemberDeclaration(setAccessor, accessibilityModifier, isStatic);
        }

        /// <summary>
        /// Creates an index member declaration in a class.
        /// </summary>
        public static ITsIndexMemberDeclaration IndexMemberDeclaration(ITsIndexSignature indexSignature) =>
            new TsIndexMemberDeclaration(indexSignature);

        /// <summary>
        /// Creates a class heritage of the form 'extends type implements type, type'.
        /// </summary>
        public static ITsClassHeritage ClassHeritage(
            ITsTypeReference extendsClause,
            IEnumerable<ITsTypeReference> implementsClause = null)
        {
            return new TsClassHeritage(extendsClause, implementsClause);
        }

        /// <summary>
        /// Creates a class heritage of the form 'implements type, type'.
        /// </summary>
        public static ITsClassHeritage ClassHeritage(IEnumerable<ITsTypeReference> implementsTypes) =>
            new TsClassHeritage(extendsClause: null, implementsClause: implementsTypes);

        /// <summary>
        /// Creates a class declaration.
        /// </summary>
        public static ITsClassDeclaration ClassDeclaration(
            ITsIdentifier className = null,
            ITsTypeParameters typeParameters = null,
            ITsClassHeritage heritage = null,
            IEnumerable<ITsClassElement> classBody = null)
        {
            return new TsClassDeclaration(className, typeParameters, heritage, classBody);
        }

        /// <summary>
        /// Creates an interface declaration.
        /// </summary>
        public static ITsInterfaceDeclaration InterfaceDeclaration(
            ITsIdentifier interfaceName,
            ITsObjectType body,
            ITsTypeParameters typeParameters = null,
            IEnumerable<ITsTypeReference> extendsClause = null)
        {
            return new TsInterfaceDeclaration(interfaceName, body, typeParameters, extendsClause);
        }

        /// <summary>
        /// Creates an enum member of the form, 'name = value'.
        /// </summary>
        public static ITsEnumMember EnumMember(ITsPropertyName name, ITsExpression value = null) =>
            new TsEnumMember(name, value);

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(
            ITsIdentifier enumName,
            IEnumerable<ITsEnumMember> enumBody = null,
            bool isConst = false)
        {
            return new TsEnumDeclaration(enumName, enumBody, isConst);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(
            bool isConst,
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(enumName, enumBody, isConst);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(ITsIdentifier enumName, params ITsEnumMember[] enumBody) =>
            new TsEnumDeclaration(enumName, enumBody);

        /// <summary>
        /// Creates a namespace declaration.
        /// </summary>
        public static ITsNamespaceDeclaration NamespaceDeclaration(
            ITsQualifiedName namespaceName,
            params ITsNamespaceElement[] body)
        {
            return new TsNamespaceDeclaration(namespaceName, body);
        }
    }
}
