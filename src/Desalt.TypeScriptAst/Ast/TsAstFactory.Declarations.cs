// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Creates a simple lexical binding of the form 'x: type = y'.
        /// </summary>
        public static ITsSimpleLexicalBinding SimpleLexicalBinding(
            ITsIdentifier variableName,
            ITsType? variableType = null,
            ITsExpression? initializer = null)
        {
            return new TsSimpleLexicalBinding(variableName, variableType, initializer);
        }

        /// <summary>
        /// Creates a destructuring lexical binding of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringLexicalBinding DestructuringLexicalBinding(
            ITsBindingPattern bindingPattern,
            ITsType? variableType = null,
            ITsExpression? initializer = null)
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
            return new TsLexicalDeclaration(isConst, declarations.ToImmutableArray());
        }

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(ITsCallSignature callSignature)
        {
            return new TsFunctionDeclaration(
                functionName: null,
                callSignature,
                ImmutableArray<ITsStatementListItem>.Empty);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function name signature { }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return new TsFunctionDeclaration(functionName, callSignature, ImmutableArray<ITsStatementListItem>.Empty);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier? functionName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionDeclaration(functionName, callSignature, functionBody.ToImmutableArray());
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(ITsIdentifier aliasName, ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, typeParameters: null, type);
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsTypeParameter typeParameter,
            ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, TypeParameters(typeParameter), type);
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsTypeParameters typeParameters,
            ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, typeParameters, type);
        }

        /// <summary>
        /// Creates a constructor declaration within a class declaration.
        /// </summary>
        public static ITsConstructorDeclaration ConstructorDeclaration(
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList? parameterList = null,
            IEnumerable<ITsStatementListItem>? functionBody = null)
        {
            return new TsConstructorDeclaration(accessibilityModifier, parameterList, functionBody?.ToImmutableArray());
        }

        /// <summary>
        /// Creates a member variable declaration in a class.
        /// </summary>
        public static ITsMemberVariableDeclaration MemberVariableDeclaration(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType? typeAnnotation = null,
            ITsExpression? initializer = null)
        {
            return new TsMemberVariableDeclaration(
                accessibilityModifier,
                isStatic,
                isReadOnly,
                variableName,
                typeAnnotation,
                initializer);
        }

        /// <summary>
        /// Creates a member function declaration in a class.
        /// </summary>
        public static ITsMemberFunctionDeclaration MemberFunctionDeclaration(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false,
            IEnumerable<ITsStatementListItem>? functionBody = null)
        {
            return new TsMemberFunctionDeclaration(
                accessibilityModifier,
                isStatic,
                isAbstract,
                functionName,
                callSignature,
                functionBody?.ToImmutableArray());
        }

        /// <summary>
        /// Creates a 'get' member accessor declaration in a class.
        /// </summary>
        public static ITsMemberGetAccessorDeclaration MemberGetAccessorDeclaration(
            ITsGetAccessor getAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            return new TsMemberGetAccessorDeclaration(accessibilityModifier, isStatic, isAbstract, getAccessor);
        }

        /// <summary>
        /// Creates a 'set' member accessor declaration in a class.
        /// </summary>
        public static ITsMemberSetAccessorDeclaration MemberSetAccessorDeclaration(
            ITsSetAccessor setAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            return new TsMemberSetAccessorDeclaration(accessibilityModifier, isStatic, isAbstract, setAccessor);
        }

        /// <summary>
        /// Creates an index member declaration in a class.
        /// </summary>
        public static ITsIndexMemberDeclaration IndexMemberDeclaration(ITsIndexSignature indexSignature)
        {
            return new TsIndexMemberDeclaration(indexSignature);
        }

        /// <summary>
        /// Creates a class heritage of the form 'extends type implements type, type'.
        /// </summary>
        public static ITsClassHeritage ClassHeritage(
            ITsTypeReference? extendsClause,
            IEnumerable<ITsTypeReference>? implementsClause = null)
        {
            return new TsClassHeritage(
                extendsClause,
                implementsClause?.ToImmutableArray() ?? ImmutableArray<ITsTypeReference>.Empty);
        }

        /// <summary>
        /// Creates a class heritage of the form 'implements type, type'.
        /// </summary>
        public static ITsClassHeritage ClassHeritage(IEnumerable<ITsTypeReference> implementsTypes)
        {
            return new TsClassHeritage(extendsClause: null, implementsClause: implementsTypes.ToImmutableArray());
        }

        /// <summary>
        /// Creates a class declaration.
        /// </summary>
        public static ITsClassDeclaration ClassDeclaration(
            ITsIdentifier? className = null,
            ITsTypeParameters? typeParameters = null,
            ITsClassHeritage? heritage = null,
            bool isAbstract = false,
            IEnumerable<ITsClassElement>? classBody = null)
        {
            return new TsClassDeclaration(
                className,
                typeParameters,
                heritage,
                isAbstract,
                classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty);
        }

        /// <summary>
        /// Creates an interface declaration.
        /// </summary>
        public static ITsInterfaceDeclaration InterfaceDeclaration(
            ITsIdentifier interfaceName,
            ITsObjectType body,
            ITsTypeParameters? typeParameters = null,
            IEnumerable<ITsTypeReference>? extendsClause = null)
        {
            return new TsInterfaceDeclaration(
                interfaceName,
                typeParameters,
                extendsClause?.ToImmutableArray() ?? ImmutableArray<ITsTypeReference>.Empty,
                body);
        }

        /// <summary>
        /// Creates an enum member of the form, 'name = value'.
        /// </summary>
        public static ITsEnumMember EnumMember(ITsPropertyName name, ITsExpression? value = null)
        {
            return new TsEnumMember(name, value);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(
            ITsIdentifier enumName,
            IEnumerable<ITsEnumMember>? enumBody = null,
            bool isConst = false)
        {
            return new TsEnumDeclaration(
                isConst,
                enumName,
                enumBody?.ToImmutableArray() ?? ImmutableArray<ITsEnumMember>.Empty);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(
            bool isConst,
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(isConst, enumName, enumBody.ToImmutableArray());
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsEnumDeclaration EnumDeclaration(ITsIdentifier enumName, params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(isConst: false, enumName, enumBody.ToImmutableArray());
        }

        /// <summary>
        /// Creates a namespace declaration.
        /// </summary>
        public static ITsNamespaceDeclaration NamespaceDeclaration(
            ITsQualifiedName namespaceName,
            params ITsNamespaceElement[] body)
        {
            return new TsNamespaceDeclaration(namespaceName, body.ToImmutableArray());
        }

        /// <summary>
        /// Creates an exported variable statement.
        /// </summary>
        public static ITsExportedVariableStatement ExportedVariableStatement(ITsVariableStatement statement)
        {
            return new TsExportedVariableStatement(statement);
        }

        /// <summary>
        /// Creates an exported declaration.
        /// </summary>
        public static ITsExportedDeclaration ExportedDeclaration(ITsDeclaration declaration)
        {
            return new TsExportedDeclaration(declaration);
        }

        /// <summary>
        /// Creates an import alias declaration of the form, 'import alias = dotted.name'.
        /// </summary>
        public static ITsImportAliasDeclaration ImportAliasDeclaration(
            ITsIdentifier alias,
            ITsQualifiedName importedName)
        {
            return new TsImportAliasDeclaration(alias, importedName);
        }

        /// <summary>
        /// Creates an ambient variable binding of the form 'name: type'.
        /// </summary>
        public static ITsAmbientBinding AmbientBinding(ITsIdentifier variableName, ITsType? variableType = null)
        {
            return new TsAmbientBinding(variableName, variableType);
        }

        /// <summary>
        /// Creates an ambient variable declaration of the form, 'var|let|const x, y: type;'.
        /// </summary>
        public static ITsAmbientVariableDeclaration AmbientVariableDeclaration(
            VariableDeclarationKind declarationKind,
            params ITsAmbientBinding[] declarations)
        {
            return new TsAmbientVariableDeclaration(declarationKind, declarations.ToImmutableArray());
        }

        /// <summary>
        /// Creates a function declaration of the form 'function name signature;'.
        /// </summary>
        public static ITsAmbientFunctionDeclaration AmbientFunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return new TsAmbientFunctionDeclaration(functionName, callSignature);
        }

        /// <summary>
        /// Creates a constructor declaration within an ambient class declaration.
        /// </summary>
        public static ITsAmbientConstructorDeclaration AmbientConstructorDeclaration(
            ITsParameterList? parameterList = null)
        {
            return new TsAmbientConstructorDeclaration(parameterList);
        }

        /// <summary>
        /// Creates a member variable declaration in an ambient class declaration.
        /// </summary>
        public static ITsAmbientMemberVariableDeclaration AmbientMemberVariableDeclaration(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType? typeAnnotation = null)
        {
            return new TsAmbientMemberVariableDeclaration(
                accessibilityModifier,
                isStatic,
                isReadOnly,
                variableName,
                typeAnnotation);
        }

        /// <summary>
        /// Creates a member function declaration in an ambient class.
        /// </summary>
        public static ITsAmbientMemberFunctionDeclaration AmbientMemberFunctionDeclaration(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            return new TsAmbientMemberFunctionDeclaration(
                accessibilityModifier,
                isStatic,
                isAbstract,
                functionName,
                callSignature);
        }

        /// <summary>
        /// Creates an index member declaration in a class.
        /// </summary>
        public static ITsIndexMemberDeclaration AmbientIndexMemberDeclaration(ITsIndexSignature indexSignature)
        {
            return new TsIndexMemberDeclaration(indexSignature);
        }

        /// <summary>
        /// Creates an ambient class declaration.
        /// </summary>
        public static ITsAmbientClassDeclaration AmbientClassDeclaration(
            ITsIdentifier className,
            ITsTypeParameters? typeParameters = null,
            ITsClassHeritage? heritage = null,
            IEnumerable<ITsAmbientClassBodyElement>? classBody = null)
        {
            return new TsAmbientClassDeclaration(
                className,
                typeParameters,
                heritage,
                classBody?.ToImmutableArray() ?? ImmutableArray<ITsAmbientClassBodyElement>.Empty);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            ITsIdentifier enumName,
            IEnumerable<ITsEnumMember>? enumBody = null,
            bool isConst = false)
        {
            return new TsEnumDeclaration(
                isConst,
                enumName,
                enumBody?.ToImmutableArray() ?? ImmutableArray<ITsEnumMember>.Empty);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            bool isConst,
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(isConst, enumName, enumBody.ToImmutableArray());
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(isConst: false, enumName, enumBody.ToImmutableArray());
        }

        /// <summary>
        /// Creates an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceDeclaration AmbientNamespaceDeclaration(
            ITsQualifiedName namespaceName,
            params ITsAmbientNamespaceElement[] body)
        {
            return new TsAmbientNamespaceDeclaration(namespaceName, body.ToImmutableArray());
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsAmbientDeclarationElement declaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(
                hasExportKeyword,
                declaration,
                interfaceDeclaration: null,
                importAliasDeclaration: null);
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsInterfaceDeclaration interfaceDeclaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(
                hasExportKeyword,
                declaration: null,
                interfaceDeclaration,
                importAliasDeclaration: null);
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsImportAliasDeclaration importAliasDeclaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(
                hasExportKeyword,
                declaration: null,
                interfaceDeclaration: null,
                importAliasDeclaration);
        }

        /// <summary>
        /// Create an import specifier, which is either an identifier or 'identifier as identifier'.
        /// </summary>
        public static ITsImportSpecifier ImportSpecifier(ITsIdentifier name, ITsIdentifier? asName = null)
        {
            return new TsImportSpecifier(name, asName);
        }

        /// <summary>
        /// Create a from clause in an import or export statement, of the form 'from moduleName'.
        /// </summary>
        public static ITsFromClause FromClause(ITsStringLiteral module)
        {
            return new TsFromClause(module);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier'.
        /// </summary>
        public static ITsImportClause ImportClause(ITsIdentifier defaultBinding)
        {
            return new TsImportClause(
                defaultBinding,
                namespaceBinding: null,
                namedImports: ImmutableArray<ITsImportSpecifier>.Empty);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, * as identifier'.
        /// </summary>
        public static ITsImportClause ImportClause(ITsIdentifier defaultBinding, ITsIdentifier namespaceBinding)
        {
            return new TsImportClause(
                defaultBinding,
                namespaceBinding,
                namedImports: ImmutableArray<ITsImportSpecifier>.Empty);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, { importSpecifier, importSpecifier }'.
        /// </summary>
        public static ITsImportClause ImportClause(
            ITsIdentifier defaultBinding,
            params ITsImportSpecifier[] namedImports)
        {
            return new TsImportClause(defaultBinding, namespaceBinding: null, namedImports.ToImmutableArray());
        }

        /// <summary>
        /// Create an import clause of the form '* as identifier'.
        /// </summary>
        public static ITsImportClause ImportClauseNamespaceBinding(ITsIdentifier namespaceBinding)
        {
            return new TsImportClause(
                defaultBinding: null,
                namespaceBinding,
                namedImports: ImmutableArray<ITsImportSpecifier>.Empty);
        }

        /// <summary>
        /// Create an import clause of the form '{ importSpecifier, importSpecifier }'.
        /// </summary>
        public static ITsImportClause ImportClause(
            ITsImportSpecifier namedImport,
            params ITsImportSpecifier[] namedImports)
        {
            return new TsImportClause(
                defaultBinding: null,
                namespaceBinding: null,
                ImmutableArray.Create(namedImport).AddRange(namedImports));
        }

        /// <summary>
        /// Create an import declaration of the form 'import ImportClause FromClause;'.
        /// </summary>
        public static ITsImportDeclaration ImportDeclaration(ITsImportClause importClause, ITsFromClause fromClause)
        {
            return new TsImportDeclaration(importClause, fromClause, module: null);
        }

        /// <summary>
        /// Create an import declaration of the form 'import Module;'.
        /// </summary>
        public static ITsImportDeclaration ImportDeclaration(ITsStringLiteral module)
        {
            return new TsImportDeclaration(importClause: null, fromClause: null, module);
        }

        /// <summary>
        /// Create an import declaration using 'require', of the form 'import name = require(string);'.
        /// </summary>
        public static ITsImportRequireDeclaration ImportRequireDeclaration(ITsIdentifier name, ITsStringLiteral require)
        {
            return new TsImportRequireDeclaration(name, require);
        }

        /// <summary>
        /// Create an exported element in a module file.
        /// </summary>
        public static ITsExportImplementationElement ExportImplementationElement(
            ITsImplementationElement exportedElement)
        {
            return new TsExportImplementationElement(exportedElement);
        }
    }
}
