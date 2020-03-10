// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast.Declarations;

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
        public static ITsFunctionDeclaration FunctionDeclaration(ITsCallSignature callSignature)
        {
            return TsFunctionDeclaration.Create(callSignature);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function name signature { }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return TsFunctionDeclaration.Create(callSignature, functionName);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function [name] signature { body }'.
        /// </summary>
        public static ITsFunctionDeclaration FunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return TsFunctionDeclaration.Create(callSignature, functionName, functionBody);
        }

        /// <summary>
        /// Creates a type alias of the form 'type alias&lt;T&gt; = type'.
        /// </summary>
        public static ITsTypeAliasDeclaration TypeAliasDeclaration(ITsIdentifier aliasName, ITsType type)
        {
            return new TsTypeAliasDeclaration(aliasName, type);
        }

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

        /// <summary>
        /// Creates a constructor declaration within a class declaration.
        /// </summary>
        public static ITsConstructorDeclaration ConstructorDeclaration(
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList parameterList = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return TsConstructorDeclaration.Create(accessibilityModifier, parameterList, functionBody);
        }

        /// <summary>
        /// Creates a member variable declaration in a class.
        /// </summary>
        public static ITsVariableMemberDeclaration VariableMemberDeclaration(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            return TsVariableMemberDeclaration.Create(
                variableName, accessibilityModifier, isStatic, isReadOnly, typeAnnotation, initializer);
        }

        /// <summary>
        /// Creates a member function declaration in a class.
        /// </summary>
        public static ITsFunctionMemberDeclaration FunctionMemberDeclaration(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return TsFunctionMemberDeclaration.Create(
                functionName, callSignature, accessibilityModifier, isStatic, isAbstract, functionBody);
        }

        /// <summary>
        /// Creates a 'get' member accessor declaration in a class.
        /// </summary>
        public static ITsGetAccessorMemberDeclaration GetAccessorMemberDeclaration(
            ITsGetAccessor getAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            return new TsGetSetAccessorMemberDeclaration(getAccessor, accessibilityModifier, isStatic, isAbstract);
        }

        /// <summary>
        /// Creates a 'set' member accessor declaration in a class.
        /// </summary>
        public static ITsSetAccessorMemberDeclaration SetAccessorMemberDeclaration(
            ITsSetAccessor setAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            return new TsGetSetAccessorMemberDeclaration(setAccessor, accessibilityModifier, isStatic, isAbstract);
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
            ITsTypeReference extendsClause,
            IEnumerable<ITsTypeReference> implementsClause = null)
        {
            return new TsClassHeritage(extendsClause, implementsClause);
        }

        /// <summary>
        /// Creates a class heritage of the form 'implements type, type'.
        /// </summary>
        public static ITsClassHeritage ClassHeritage(IEnumerable<ITsTypeReference> implementsTypes)
        {
            return new TsClassHeritage(extendsClause: null, implementsClause: implementsTypes);
        }

        /// <summary>
        /// Creates a class declaration.
        /// </summary>
        public static ITsClassDeclaration ClassDeclaration(
            ITsIdentifier className = null,
            ITsTypeParameters typeParameters = null,
            ITsClassHeritage heritage = null,
            bool isAbstract = false,
            IEnumerable<ITsClassElement> classBody = null)
        {
            return new TsClassDeclaration(className, typeParameters, heritage, isAbstract, classBody);
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
        public static ITsEnumMember EnumMember(ITsPropertyName name, ITsExpression value = null)
        {
            return new TsEnumMember(name, value);
        }

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
        public static ITsEnumDeclaration EnumDeclaration(ITsIdentifier enumName, params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(enumName, enumBody);
        }

        /// <summary>
        /// Creates a namespace declaration.
        /// </summary>
        public static ITsNamespaceDeclaration NamespaceDeclaration(
            ITsQualifiedName namespaceName,
            params ITsNamespaceElement[] body)
        {
            return new TsNamespaceDeclaration(namespaceName, body);
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
        public static ITsAmbientBinding AmbientBinding(ITsIdentifier variableName, ITsType variableType = null)
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
            return new TsAmbientVariableDeclaration(declarationKind, declarations);
        }

        /// <summary>
        /// Creates a function declaration of the form 'function name signature;'.
        /// </summary>
        public static ITsAmbientFunctionDeclaration AmbientFunctionDeclaration(
            ITsIdentifier functionName,
            ITsCallSignature callSignature)
        {
            return TsFunctionDeclaration.CreateAmbient(functionName, callSignature);
        }

        /// <summary>
        /// Creates a constructor declaration within an ambient class declaration.
        /// </summary>
        public static ITsAmbientConstructorDeclaration AmbientConstructorDeclaration(
            ITsParameterList parameterList = null)
        {
            return TsConstructorDeclaration.CreateAmbient(parameterList);
        }

        /// <summary>
        /// Creates a member variable declaration in an ambient class declaration.
        /// </summary>
        public static ITsAmbientVariableMemberDeclaration AmbientVariableMemberDeclaration(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType typeAnnotation = null)
        {
            return TsVariableMemberDeclaration.CreateAmbient(
                variableName, accessibilityModifier, isStatic, isReadOnly, typeAnnotation);
        }

        /// <summary>
        /// Creates a member function declaration in an ambient class.
        /// </summary>
        public static ITsAmbientFunctionMemberDeclaration AmbientFunctionMemberDeclaration(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false)
        {
            return TsFunctionMemberDeclaration.CreateAmbient(
                functionName, callSignature, accessibilityModifier, isStatic);
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
            ITsTypeParameters typeParameters = null,
            ITsClassHeritage heritage = null,
            IEnumerable<ITsAmbientClassBodyElement> classBody = null)
        {
            return new TsAmbientClassDeclaration(className, typeParameters, heritage, classBody);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            ITsIdentifier enumName,
            IEnumerable<ITsEnumMember> enumBody = null,
            bool isConst = false)
        {
            return new TsEnumDeclaration(enumName, enumBody, isConst);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            bool isConst,
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(enumName, enumBody, isConst);
        }

        /// <summary>
        /// Creates an enum declaration.
        /// </summary>
        public static ITsAmbientEnumDeclaration AmbientEnumDeclaration(
            ITsIdentifier enumName,
            params ITsEnumMember[] enumBody)
        {
            return new TsEnumDeclaration(enumName, enumBody);
        }

        /// <summary>
        /// Creates an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceDeclaration AmbientNamespaceDeclaration(
            ITsQualifiedName namespaceName,
            params ITsAmbientNamespaceElement[] body)
        {
            return new TsAmbientNamespaceDeclaration(namespaceName, body);
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsAmbientDeclarationElement declaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(declaration, hasExportKeyword);
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsInterfaceDeclaration interfaceDeclaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(interfaceDeclaration, hasExportKeyword);
        }

        /// <summary>
        /// Create an element in an ambient namespace declaration.
        /// </summary>
        public static ITsAmbientNamespaceElement AmbientNamespaceElement(
            ITsImportAliasDeclaration importAliasDeclaration,
            bool hasExportKeyword = false)
        {
            return new TsAmbientNamespaceElement(importAliasDeclaration, hasExportKeyword);
        }

        /// <summary>
        /// Create an import specifier, which is either an identifier or 'identifier as identifier'.
        /// </summary>
        public static ITsImportSpecifier ImportSpecifier(ITsIdentifier name, ITsIdentifier asName = null)
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
            return TsImportClause.CreateDefaultBinding(defaultBinding, namespaceBinding: null);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, * as identifier'.
        /// </summary>
        public static ITsImportClause ImportClause(ITsIdentifier defaultBinding, ITsIdentifier namespaceBinding)
        {
            return TsImportClause.CreateDefaultBinding(defaultBinding, namespaceBinding);
        }

        /// <summary>
        /// Create an import clause of the form 'identifier' or 'identifier, { importSpecifier, importSpecifier }'.
        /// </summary>
        public static ITsImportClause ImportClause(
            ITsIdentifier defaultBinding,
            params ITsImportSpecifier[] namedImports)
        {
            return TsImportClause.CreateDefaultBinding(defaultBinding, namedImports);
        }

        /// <summary>
        /// Create an import clause of the form '* as identifier'.
        /// </summary>
        public static ITsImportClause ImportClauseNamespaceBinding(ITsIdentifier namespaceBinding)
        {
            return TsImportClause.CreateNamespaceBinding(namespaceBinding);
        }

        /// <summary>
        /// Create an import clause of the form '{ importSpecifier, importSpecifier }'.
        /// </summary>
        public static ITsImportClause ImportClause(
            ITsImportSpecifier namedImport,
            params ITsImportSpecifier[] namedImports)
        {
            return TsImportClause.CreateNamedImports(namedImport.ToSafeArray().Concat(namedImports));
        }

        /// <summary>
        /// Create an import declaration of the form 'import ImportClause FromClause;'.
        /// </summary>
        public static ITsImportDeclaration ImportDeclaration(ITsImportClause importClause, ITsFromClause fromClause)
        {
            return new TsImportDeclaration(importClause, fromClause);
        }

        /// <summary>
        /// Create an import declaration of the form 'import Module;'.
        /// </summary>
        public static ITsImportDeclaration ImportDeclaration(ITsStringLiteral module)
        {
            return new TsImportDeclaration(module);
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
