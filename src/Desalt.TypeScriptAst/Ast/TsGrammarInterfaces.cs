// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;

    /* A.2 Expressions
     * ---------------
     * PropertyDefinition: ( Modified )
     *   IdentifierReference
     *   CoverInitializedName
     *   PropertyName : AssignmentExpression
     *   PropertyName CallSignature { FunctionBody }
     *   GetAccessor
     *   SetAccessor
     *
     * GetAccessor:
     *   get PropertyName ( ) TypeAnnotationOpt { FunctionBody }
     *
     * SetAccessor:
     *   set PropertyName ( BindingIdentifierOrPattern TypeAnnotationOpt ) { FunctionBody }
     */

    public interface ITsPropertyFunction : ITsPropertyDefinition
    {
        ITsPropertyName PropertyName { get; }
        ITsCallSignature CallSignature { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    public interface ITsGetAccessor : ITsPropertyDefinition
    {
        ITsPropertyName PropertyName { get; }
        ITsType? PropertyType { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    public interface ITsSetAccessor : ITsPropertyDefinition
    {
        ITsPropertyName PropertyName { get; }
        ITsBindingIdentifierOrPattern ParameterName { get; }
        ITsType? ParameterType { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    /* FunctionExpression: ( Modified )
     *   function BindingIdentifierOpt CallSignature { FunctionBody }
     */

    public interface ITsFunctionExpression : ITsExpression
    {
        ITsIdentifier? FunctionName { get; }
        ITsCallSignature CallSignature { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    /* ArrowFormalParameters: ( Modified )
     *   CallSignature
     *
     * Arguments: ( Modified )
     *   TypeArgumentsOpt ( ArgumentListOpt )
     *
     * UnaryExpression: ( Modified )
     *   ...
     *   < Type > UnaryExpression
     */

    /* A.3 Statements
     * --------------
     * Declaration: ( Modified )
     *   ...
     *   InterfaceDeclaration
     *   TypeAliasDeclaration
     *   EnumDeclaration
     */

    /* VariableDeclaration: ( Modified )
     *   SimpleVariableDeclaration
     *   DestructuringVariableDeclaration
     *
     * SimpleVariableDeclaration:
     *   BindingIdentifier TypeAnnotationOpt InitializerOpt
     *
     * DestructuringVariableDeclaration:
     *   BindingPattern TypeAnnotationOpt Initializer
     */

    public interface ITsVariableDeclaration : ITsAstNode { }

    public interface ITsSimpleVariableDeclaration : ITsVariableDeclaration
    {
        ITsIdentifier VariableName { get; }
        ITsType? VariableType { get; }
        ITsExpression? Initializer { get; }
    }

    public interface ITsDestructuringVariableDeclaration : ITsVariableDeclaration
    {
        ITsBindingPattern BindingPattern { get; }
        ITsType? VariableType { get; }
        ITsExpression Initializer { get; }
    }

    /* LexicalBinding: ( Modified )
     *   SimpleLexicalBinding
     *   DestructuringLexicalBinding
     *
     * SimpleLexicalBinding:
     *   BindingIdentifier TypeAnnotationOpt InitializerOpt
     *
     * DestructuringLexicalBinding:
     *   BindingPattern TypeAnnotationOpt InitializerOpt
     */

    public interface ITsLexicalBinding : ITsAstNode { }

    public interface ITsSimpleLexicalBinding : ITsLexicalBinding
    {
        ITsIdentifier VariableName { get; }
        ITsType? VariableType { get; }
        ITsExpression? Initializer { get; }
    }

    public interface ITsDestructuringLexicalBinding : ITsLexicalBinding
    {
        ITsBindingPattern BindingPattern { get; }
        ITsType? VariableType { get; }
        ITsExpression? Initializer { get; }
    }

    /* A.4 Functions
     * -------------
     * FunctionDeclaration: ( Modified )
     *   function BindingIdentifierOpt CallSignature { FunctionBody }
     *   function BindingIdentifierOpt CallSignature ;
     */

    public interface ITsFunctionDeclaration : ITsDeclaration
    {
        ITsIdentifier? FunctionName { get; }
        ITsCallSignature CallSignature { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    /* A.5 Interfaces
     * --------------
     * InterfaceDeclaration:
     *   interface BindingIdentifier TypeParametersOpt InterfaceExtendsClauseOpt ObjectType
     *
     * InterfaceExtendsClause:
     *   extends ClassOrInterfaceTypeList
     *
     * ClassOrInterfaceTypeList:
     *   ClassOrInterfaceType
     *   ClassOrInterfaceTypeList , ClassOrInterfaceType
     *
     * ClassOrInterfaceType:
     *   TypeReference
     */

    public interface ITsInterfaceDeclaration : ITsDeclaration
    {
        ITsIdentifier InterfaceName { get; }
        ITsTypeParameters TypeParameters { get; }
        ImmutableArray<ITsTypeReference> ExtendsClause { get; }
        ITsObjectType Body { get; }
    }

    /* A.6 Classes
     * -----------
     * ClassDeclaration: ( Modified )
     *   class BindingIdentifierOpt TypeParametersOpt ClassHeritage { ClassBody }
     *
     * ClassHeritage: ( Modified )
     *   ClassExtendsClauseOpt ImplementsClauseOpt
     *
     * ClassExtendsClause:
     *   extends ClassType
     *
     * ClassType:
     *   TypeReference
     *
     * ImplementsClause:
     *   implements ClassOrInterfaceTypeList
     *
     * ClassBody:
     *   ClassElementList
     *
     * ClassElementList:
     *   ClassElement
     *   ClassElementList ClassElement
     *
     * ClassElement: ( Modified )
     *   ConstructorDeclaration
     *   PropertyMemberDeclaration
     *   IndexMemberDeclaration
     */

    public interface ITsClassDeclaration : ITsDeclaration
    {
        ITsIdentifier? ClassName { get; }
        ITsTypeParameters TypeParameters { get; }
        ITsClassHeritage? Heritage { get; }
        bool IsAbstract { get; }
        ImmutableArray<ITsClassElement> ClassBody { get; }
    }

    public interface ITsClassHeritage : ITsAstNode
    {
        ITsTypeReference? ExtendsClause { get; }
        ImmutableArray<ITsTypeReference> ImplementsClause { get; }
    }

    public interface ITsClassElement : ITsAstNode { }

    /* ConstructorDeclaration:
     *   AccessibilityModifierOpt constructor ( ParameterListOpt ) { FunctionBody }
     *   AccessibilityModifierOpt constructor ( ParameterListOpt ) ;
     */

    public interface ITsConstructorDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        ITsParameterList ParameterList { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    /* PropertyMemberDeclaration:
     *   MemberVariableDeclaration
     *   MemberFunctionDeclaration
     *   MemberAccessorDeclaration
     *
     * MemberVariableDeclaration:
     *   AccessibilityModifierOpt staticOpt PropertyName TypeAnnotationOpt InitializerOpt ;
     *
     * MemberFunctionDeclaration:
     *   AccessibilityModifierOpt staticOpt PropertyName CallSignature { FunctionBody }
     *   AccessibilityModifierOpt staticOpt PropertyName CallSignature ;
     *
     * MemberAccessorDeclaration:
     *   AccessibilityModifierOpt staticOpt GetAccessor
     *   AccessibilityModifierOpt staticOpt SetAccessor
     */

    public interface ITsVariableMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        bool IsReadOnly { get; }
        ITsPropertyName VariableName { get; }
        ITsType? TypeAnnotation { get; }
        ITsExpression? Initializer { get; }
    }

    public interface ITsFunctionMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        bool IsAbstract { get; }
        ITsPropertyName FunctionName { get; }
        ITsCallSignature CallSignature { get; }
        ImmutableArray<ITsStatementListItem>? FunctionBody { get; }
    }

    public interface ITsGetAccessorMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        bool IsAbstract { get; }
        ITsGetAccessor GetAccessor { get; }
    }

    public interface ITsSetAccessorMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        bool IsAbstract { get; }
        ITsSetAccessor SetAccessor { get; }
    }

    /* IndexMemberDeclaration:
     *   IndexSignature ;
     */

    public interface ITsIndexMemberDeclaration : ITsClassElement, ITsAmbientClassBodyElement
    {
        ITsIndexSignature IndexSignature { get; }
    }

    /* A.7 Enums
     * ---------
     * EnumDeclaration:
     *   constOpt enum BindingIdentifier { EnumBodyOpt }
     *
     * EnumBody:
     *   EnumMemberList ,Opt
     *
     * EnumMemberList:
     *   EnumMember
     *   EnumMemberList , EnumMember
     *
     * EnumMember:
     *   PropertyName
     *   PropertyName = EnumValue
     *
     * EnumValue:
     *   AssignmentExpression
     */

    // ReSharper disable once RedundantExtendsListEntry
    public interface ITsEnumDeclaration : ITsDeclaration, ITsAmbientEnumDeclaration
    {
        bool IsConst { get; }
        ITsIdentifier EnumName { get; }
        ImmutableArray<ITsEnumMember> EnumBody { get; }
    }

    public interface ITsEnumMember : ITsAstNode
    {
        ITsPropertyName Name { get; }
        ITsExpression? Value { get; }
    }

    /* A.8 Namespaces
     * --------------
     * NamespaceDeclaration:
     *   namespace IdentifierPath { NamespaceBody }
     *
     * IdentifierPath:
     *   BindingIdentifier
     *   IdentifierPath . BindingIdentifier
     *
     * NamespaceBody:
     *   NamespaceElementsOpt
     *
     * NamespaceElements:
     *   NamespaceElement
     *   NamespaceElements NamespaceElement
     */

    public interface ITsNamespaceDeclaration : ITsDeclaration
    {
        ITsQualifiedName NamespaceName { get; }
        ImmutableArray<ITsNamespaceElement> Body { get; }
    }

    /* NamespaceElement:
     *   Statement
     *   LexicalDeclaration
     *   FunctionDeclaration
     *   GeneratorDeclaration
     *   ClassDeclaration
     *   InterfaceDeclaration
     *   TypeAliasDeclaration
     *   EnumDeclaration
     *   NamespaceDeclaration
     *   AmbientDeclaration
     *   ImportAliasDeclaration
     *   ExportNamespaceElement
     */

    public interface ITsNamespaceElement : ITsAstNode { }

    /* ExportNamespaceElement:
     *   export VariableStatement
     *   export LexicalDeclaration
     *   export FunctionDeclaration
     *   export GeneratorDeclaration
     *   export ClassDeclaration
     *   export InterfaceDeclaration
     *   export TypeAliasDeclaration
     *   export EnumDeclaration
     *   export NamespaceDeclaration
     *   export AmbientDeclaration
     *   export ImportAliasDeclaration
     */

    public interface ITsExportedVariableStatement : ITsNamespaceElement
    {
        ITsVariableStatement ExportedStatement { get; }
    }

    public interface ITsExportedDeclaration : ITsNamespaceElement
    {
        ITsDeclaration ExportedDeclaration { get; }
    }

    /* ImportAliasDeclaration:
     *   import BindingIdentifier = EntityName ;
     *
     * EntityName:
     *   NamespaceName
     *   NamespaceName . IdentifierReference
     */

    public interface ITsImportAliasDeclaration : ITsNamespaceElement, ITsImplementationElement
    {
        ITsIdentifier Alias { get; }
        ITsQualifiedName ImportedName { get; }
    }

    /* A.9 Scripts and Modules
     * -----------------------
     * SourceFile:
     *   ImplementationSourceFile
     *   DeclarationSourceFile
     *
     * ImplementationSourceFile:
     *   ImplementationScript
     *   ImplementationModule
     *
     * DeclarationSourceFile:
     *   DeclarationScript
     *   DeclarationModule
     */

    public interface ITsImplementationSourceFile : ITsAstNode { }

    /* ImplementationScript:
     *   ImplementationScriptElementsOpt
     *
     * ImplementationScriptElements:
     *   ImplementationScriptElement
     *   ImplementationScriptElements ImplementationScriptElement
     *
     * ImplementationScriptElement:
     *   ImplementationElement
     *   AmbientModuleDeclaration
     */

    public interface ITsImplementationScript : ITsImplementationSourceFile
    {
        ImmutableArray<ITsImplementationScriptElement> Elements { get; }
    }

    public interface ITsImplementationScriptElement : ITsAstNode { }

    /* ImplementationElement:
     *   Statement
     *   LexicalDeclaration
     *   FunctionDeclaration
     *   GeneratorDeclaration
     *   ClassDeclaration
     *   InterfaceDeclaration
     *   TypeAliasDeclaration
     *   EnumDeclaration
     *   NamespaceDeclaration
     *   AmbientDeclaration
     *   ImportAliasDeclaration
     */

    public interface ITsImplementationElement : ITsImplementationScriptElement, ITsImplementationModuleElement { }

    /* DeclarationScript:
     *   DeclarationScriptElementsOpt
     *
     * DeclarationScriptElements:
     *   DeclarationScriptElement
     *   DeclarationScriptElements DeclarationScriptElement
     *
     * DeclarationScriptElement:
     *   DeclarationElement
     *   AmbientModuleDeclaration
     */

    /* DeclarationElement:
     *   InterfaceDeclaration
     *   TypeAliasDeclaration
     *   NamespaceDeclaration
     *   AmbientDeclaration
     *   ImportAliasDeclaration
     */

    /* ImplementationModule:
     *   ImplementationModuleElementsOpt
     *
     * ImplementationModuleElements:
     *   ImplementationModuleElement
     *   ImplementationModuleElements ImplementationModuleElement
     *
     * ImplementationModuleElement:
     *   ImplementationElement
     *   ImportDeclaration
     *   ImportAliasDeclaration
     *   ImportRequireDeclaration
     *   ExportImplementationElement
     *   ExportDefaultImplementationElement
     *   ExportListDeclaration
     *   ExportAssignment
     */

    public interface ITsImplementationModule : ITsImplementationSourceFile
    {
        ImmutableArray<ITsImplementationModuleElement> Elements { get; }
    }

    public interface ITsImplementationModuleElement : ITsAstNode { }

    /* DeclarationModule:
     *   DeclarationModuleElementsOpt
     *
     * DeclarationModuleElements:
     *   DeclarationModuleElement
     *   DeclarationModuleElements DeclarationModuleElement
     *
     * DeclarationModuleElement:
     *   DeclarationElement
     *   ImportDeclaration
     *   ImportAliasDeclaration
     *   ExportDeclarationElement
     *   ExportDefaultDeclarationElement
     *   ExportListDeclaration
     *   ExportAssignment
     */

    /* ImportRequireDeclaration:
     *   import BindingIdentifier = require ( StringLiteral ) ;
     */

    public interface ITsImportRequireDeclaration : ITsImplementationModuleElement
    {
        ITsIdentifier Name { get; }
        ITsStringLiteral Require { get; }
    }

    /* ExportImplementationElement:
     *   export VariableStatement
     *   export LexicalDeclaration
     *   export FunctionDeclaration
     *   export GeneratorDeclaration
     *   export ClassDeclaration
     *   export InterfaceDeclaration
     *   export TypeAliasDeclaration
     *   export EnumDeclaration
     *   export NamespaceDeclaration
     *   export AmbientDeclaration
     *   export ImportAliasDeclaration
     */

    public interface ITsExportImplementationElement : ITsImplementationModuleElement
    {
        ITsImplementationElement ExportedElement { get; }
    }

    /* ExportDeclarationElement:
     *   export InterfaceDeclaration
     *   export TypeAliasDeclaration
     *   export AmbientDeclaration
     *   export ImportAliasDeclaration
     *
     * ExportDefaultImplementationElement:
     *   export default FunctionDeclaration
     *   export default GeneratorDeclaration
     *   export default ClassDeclaration
     *   export default AssignmentExpression ;
     *
     * ExportDefaultDeclarationElement:
     *   export default AmbientFunctionDeclaration
     *   export default AmbientClassDeclaration
     *   export default IdentifierReference ;
     *
     * ExportListDeclaration:
     *   export * FromClause ;
     *   export ExportClause FromClause ;
     *   export ExportClause ;
     *
     * ExportAssignment:
     *   export = IdentifierReference ;
     */

    /* A.10 Ambients
     * -------------
     * AmbientDeclaration:
     *   declare AmbientVariableDeclaration
     *   declare AmbientFunctionDeclaration
     *   declare AmbientClassDeclaration
     *   declare AmbientEnumDeclaration
     *   declare AmbientNamespaceDeclaration
     */

    public interface ITsAmbientDeclaration : ITsImplementationElement
    {
        ITsAmbientDeclarationElement Declaration { get; }
    }

    public interface ITsAmbientDeclarationElement : ITsAstNode { }

    /* AmbientVariableDeclaration:
     *   var AmbientBindingList ;
     *   let AmbientBindingList ;
     *   const AmbientBindingList ;
     *
     * AmbientBindingList:
     *   AmbientBinding
     *   AmbientBindingList , AmbientBinding
     *
     * AmbientBinding:
     *   BindingIdentifier TypeAnnotationOpt
     */

    /// <summary>
    /// Represents an ambient variable declaration of the form, 'var|let|const x, y: type;'.
    /// </summary>
    public interface ITsAmbientVariableDeclaration : ITsAmbientDeclarationElement
    {
        VariableDeclarationKind DeclarationKind { get; }
        ImmutableArray<ITsAmbientBinding> Declarations { get; }
    }

    /// <summary>
    /// Represents an ambient variable binding of the form 'name: type'.
    /// </summary>
    public interface ITsAmbientBinding : ITsAstNode
    {
        ITsIdentifier VariableName { get; }
        ITsType? VariableType { get; }
    }

    /* AmbientFunctionDeclaration:
     *   function BindingIdentifier CallSignature ;
     */

    public interface ITsAmbientFunctionDeclaration : ITsAmbientDeclarationElement
    {
        ITsIdentifier FunctionName { get; }
        ITsCallSignature CallSignature { get; }
    }

    /* AmbientClassDeclaration:
     *   class BindingIdentifier TypeParametersOpt ClassHeritage { AmbientClassBody }
     *
     * AmbientClassBody:
     *   AmbientClassBodyElementsOpt
     *
     * AmbientClassBodyElements:
     *   AmbientClassBodyElement
     *   AmbientClassBodyElements AmbientClassBodyElement
     *
     * AmbientClassBodyElement:
     *   AmbientConstructorDeclaration
     *   AmbientPropertyMemberDeclaration
     *   IndexSignature (I think this is supposed to be IndexDeclaration)
     *
     * AmbientConstructorDeclaration:
     *   constructor ( ParameterListOpt ) ;
     *
     * AmbientPropertyMemberDeclaration:
     *   AccessibilityModifierOpt staticOpt PropertyName TypeAnnotationOpt ;
     *   AccessibilityModifierOpt staticOpt PropertyName CallSignature ;
     */

    public interface ITsAmbientClassDeclaration : ITsAmbientDeclarationElement
    {
        ITsIdentifier ClassName { get; }
        ITsTypeParameters TypeParameters { get; }
        ITsClassHeritage? Heritage { get; }
        ImmutableArray<ITsAmbientClassBodyElement> ClassBody { get; }
    }

    public interface ITsAmbientClassBodyElement : ITsAstNode { }

    public interface ITsAmbientConstructorDeclaration : ITsAmbientClassBodyElement
    {
        ITsParameterList ParameterList { get; }
    }

    public interface ITsAmbientVariableMemberDeclaration : ITsAmbientClassBodyElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        bool IsReadOnly { get; }
        ITsPropertyName VariableName { get; }
        ITsType? TypeAnnotation { get; }
    }

    public interface ITsAmbientFunctionMemberDeclaration : ITsAmbientClassBodyElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        ITsPropertyName FunctionName { get; }
        ITsCallSignature CallSignature { get; }
    }

    /* AmbientEnumDeclaration:
     *   EnumDeclaration
     */

    public interface ITsAmbientEnumDeclaration : ITsAmbientDeclarationElement { }

    /* AmbientNamespaceDeclaration:
     *   namespace IdentifierPath { AmbientNamespaceBody }
     *
     * AmbientNamespaceBody:
     *   AmbientNamespaceElementsOpt
     *
     * AmbientNamespaceElements:
     *   AmbientNamespaceElement
     *   AmbientNamespaceElements AmbientNamespaceElement
     *
     * AmbientNamespaceElement:
     *   exportOpt AmbientVariableDeclaration
     *   exportOpt AmbientLexicalDeclaration (I think this is a mistake - it's just an AmbientVariableDeclaration)
     *   exportOpt AmbientFunctionDeclaration
     *   exportOpt AmbientClassDeclaration
     *   exportOpt InterfaceDeclaration
     *   exportOpt AmbientEnumDeclaration
     *   exportOpt AmbientNamespaceDeclaration
     *   exportOpt ImportAliasDeclaration
     */

    public interface ITsAmbientNamespaceDeclaration : ITsAmbientDeclarationElement
    {
        ITsQualifiedName NamespaceName { get; }
        ImmutableArray<ITsAmbientNamespaceElement> Body { get; }
    }

    public interface ITsAmbientNamespaceElement : ITsAstNode
    {
        bool HasExportKeyword { get; }
        ITsAmbientDeclarationElement? Declaration { get; }
        ITsInterfaceDeclaration? InterfaceDeclaration { get; }
        ITsImportAliasDeclaration? ImportAliasDeclaration { get; }
    }

    /* AmbientModuleDeclaration:
     *   declare module StringLiteral { DeclarationModule }
     */
}
