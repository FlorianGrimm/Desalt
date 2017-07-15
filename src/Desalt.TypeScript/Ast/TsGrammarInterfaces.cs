// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System.Collections.Immutable;
    using Desalt.Core.Ast;

    /***********************************************************************************************
     * TypeScript Grammar, version 1.8 (Jan 2016)
     * ---------------------------------------------------------------------------------------------
     * See https://github.com/Microsoft/TypeScript/blob/master/doc/spec.md
     *
     * The TypeScript grammar is a superset of the grammar defined in the ECMAScript 2015 Language
     * Specification (specifically, the ECMA-262 Standard, 6th Edition) and this appendix lists only
     * productions that are new or modified from the ECMAScript grammar.
     *
     * See http://www.ecma-international.org/ecma-262/6.0/ for the ES2015 grammar.
     *
     * Many of these interface names and shapes are taken from the TypeScript source code at:
     * https://github.com/Microsoft/TypeScript/blob/master/src/compiler/types.ts.
     **********************************************************************************************/

    /* A.1 Types
     * ---------
     * TypeParameters:
     *   < TypeParameterList >
     *
     * TypeParameterList:
     *   TypeParameter
     *   TypeParameterList , TypeParameter
     *
     * TypeParameter:
     *   BindingIdentifier ConstraintOpt
     *
     * Constraint:
     *   extends Type
     */

    public interface ITsTypeParameters : IAstNode
    {
        ImmutableArray<ITsTypeParameter> TypeParameters { get; }
    }

    public interface ITsTypeParameter : IAstNode
    {
        ITsIdentifier TypeName { get; }
        ITsType Constraint { get; }
    }

    /* TypeArguments:
     *   < TypeArgumentList >
     *
     * TypeArgumentList:
     *   TypeArgument
     *   TypeArgumentList , TypeArgument
     *
     * TypeArgument:
     *   Type
     */

    /* Type:
     *   UnionOrIntersectionOrPrimaryType
     *   FunctionType
     *   ConstructorType
     */

    public interface ITsType : IAstNode { }

    /* UnionOrIntersectionOrPrimaryType:
     *   UnionType
     *   IntersectionOrPrimaryType
     *
     * IntersectionOrPrimaryType:
     *   IntersectionType
     *   PrimaryType
     */

    /* PrimaryType:
     *   PredefinedType
     *   TypeReference
     *   ParenthesizedType
     *   ObjectType
     *   ArrayType
     *   TupleType
     *   TypeQuery
     *   ThisType
     */

    /* ParenthesizedType:
     *   ( Type )
     */

    public interface ITsParenthesizedType : ITsType
    {
        ITsType Type { get; }
    }

    /* PredefinedType:
     *   any
     *   number
     *   boolean
     *   string
     *   symbol
     *   void
     */

    /* TypeReference:
     *   TypeName [no LineTerminator here] TypeArgumentsOpt
     *
     * TypeName:
     *   IdentifierReference
     *   NamespaceName . IdentifierReference
     *
     * NamespaceName:
     *   IdentifierReference
     *   NamespaceName . IdentifierReference
     */

    public interface ITsTypeReference : ITsType
    {
        ITsTypeName TypeName { get; }
        ImmutableArray<ITsType> TypeArguments { get; }
    }

    public interface ITsTypeName : ITsExpression { }

    public interface ITsQualifiedName : ITsTypeName
    {
        ImmutableArray<ITsIdentifier> Left { get; }
        ITsIdentifier Right { get; }
    }

    /* ObjectType:
     *   { TypeBodyOpt }
     *
     * TypeBody:
     *   TypeMemberList ;Opt
     *   TypeMemberList ,Opt
     *
     * TypeMemberList:
     *   TypeMember
     *   TypeMemberList ; TypeMember
     *   TypeMemberList , TypeMember
     *
     * TypeMember:
     *   PropertySignature
     *   CallSignature
     *   ConstructSignature
     *   IndexSignature
     *   MethodSignature
     */

    public interface ITsObjectType : ITsType
    {
        ImmutableArray<ITsTypeMember> TypeMembers { get; }
    }

    public interface ITsTypeMember : IAstNode { }

    /* ArrayType:
     *   PrimaryType [no LineTerminator here] [ ]
     */

    public interface ITsArrayType : ITsType
    {
        ITsType Type { get; }
    }

    /* TupleType:
     *   [ TupleElementTypes ]
     *
     * TupleElementTypes:
     *   TupleElementType
     *   TupleElementTypes , TupleElementType
     *
     * TupleElementType:
     *   Type
     */

    public interface ITsTupleType : ITsType
    {
        ImmutableArray<ITsType> ElementTypes { get; }
    }

    /* UnionType:
     *   UnionOrIntersectionOrPrimaryType | IntersectionOrPrimaryType
     */

    public interface ITsUnionType : ITsType
    {
        ImmutableArray<ITsType> Types { get; }
    }

    /* IntersectionType:
     *   IntersectionOrPrimaryType & PrimaryType
     */

    public interface ITsIntersectionType : ITsType
    {
        ImmutableArray<ITsType> Types { get; }
    }

    /* FunctionType:
     *   TypeParametersOpt ( ParameterListOpt ) => Type
     */

    public interface ITsFunctionType : ITsType
    {
        ITsTypeParameters TypeParameters { get; }
        ITsParameterList Parameters { get; }
        ITsType ReturnType { get; }
    }

    /* ConstructorType:
     *   new TypeParametersOpt ( ParameterListOpt ) => Type
     */

    public interface ITsConstructorType : ITsType
    {
        ITsTypeParameters TypeParameters { get; }
        ITsParameterList Parameters { get; }
        ITsType ReturnType { get; }
    }

    /* TypeQuery:
     *   typeof TypeQueryExpression
     *
     * TypeQueryExpression:
     *   IdentifierReference
     *   TypeQueryExpression . IdentifierName
     */

    public interface ITsTypeQuery : ITsType
    {
        ITsTypeName Query { get; }
    }

    /* ThisType:
     *   this
     */

    public interface ITsThisType : ITsType
    {
    }

    /* PropertySignature:
     *   PropertyName ?Opt TypeAnnotationOpt
     *
     * PropertyName:
     *   IdentifierName
     *   StringLiteral
     *   NumericLiteral
     *
     * TypeAnnotation:
     *   : Type
     */

    public interface ITsPropertySignature : ITsTypeMember
    {
        ITsLiteralPropertyName PropertyName { get; }
        bool IsOptional { get; }
        ITsType PropertyType { get; }
    }

    /* CallSignature:
     *   TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
     */

    public interface ITsCallSignature : ITsTypeMember
    {
        ITsTypeParameters TypeParameters { get; }
        ITsParameterList Parameters { get; }
        ITsType ReturnType { get; }
    }

    /* ParameterList:
     *   RequiredParameterList
     *   OptionalParameterList
     *   RestParameter
     *   RequiredParameterList , OptionalParameterList
     *   RequiredParameterList , RestParameter
     *   OptionalParameterList , RestParameter
     *   RequiredParameterList , OptionalParameterList , RestParameter
     */

    public interface ITsParameterList : IAstNode
    {
        ImmutableArray<ITsRequiredParameter> RequiredParameters { get; }
        ImmutableArray<ITsOptionalParameter> OptionalParameters { get; }
        ITsRestParameter RestParameter { get; }
    }

    /* RequiredParameterList:
     *   RequiredParameter
     *   RequiredParameterList , RequiredParameter
     *
     * RequiredParameter:
     *   AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt
     *   BindingIdentifier : StringLiteral
     *
     * AccessibilityModifier:
     *   public
     *   private
     *   protected
     */

    public interface ITsRequiredParameter : IAstNode { }

    public interface ITsBoundRequiredParameter : ITsRequiredParameter
    {
        TsAccessibilityModifier? Modifier { get; }
        ITsBindingIdentifierOrPattern ParameterName { get; }
        ITsType ParameterType { get; }
    }

    public interface ITsStringRequiredParameter : ITsRequiredParameter
    {
        ITsIdentifier ParameterName { get; }
        ITsStringLiteral StringLiteral { get; }
    }

    public enum TsAccessibilityModifier
    {
        Public,
        Private,
        Protected,
    }

    /* BindingIdentifierOrPattern:
     *   BindingIdentifier
     *   BindingPattern
     */

    public interface ITsBindingIdentifierOrPattern : IAstNode { }

    /* OptionalParameterList:
     *   OptionalParameter
     *   OptionalParameterList , OptionalParameter
     *
     * OptionalParameter:
     *   AccessibilityModifierOpt BindingIdentifierOrPattern ? TypeAnnotationOpt
     *   AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt Initializer
     *   BindingIdentifier ? : StringLiteral
     */

    public interface ITsOptionalParameter : IAstNode { }

    public interface ITsBoundOptionalParameter : ITsOptionalParameter
    {
        TsAccessibilityModifier? Modifier { get; }
        ITsBindingIdentifierOrPattern ParameterName { get; }
        ITsType ParameterType { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsStringOptionalParameter : ITsOptionalParameter
    {
        ITsIdentifier ParameterName { get; }
        ITsStringLiteral StringLiteral { get; }
    }

    /* RestParameter:
     *   ... BindingIdentifier TypeAnnotationOpt
     */

    public interface ITsRestParameter : IAstNode
    {
        ITsIdentifier ParameterName { get; }
        ITsType ParameterType { get; }
    }

    /* ConstructSignature:
     *   new TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
     */

    public interface ITsConstructSignature : ITsTypeMember
    {
        ITsTypeParameters TypeParameters { get; }
        ITsParameterList ParameterList { get; }
        ITsType ReturnType { get; }
    }

    /* IndexSignature:
     *   [ BindingIdentifier : string ] TypeAnnotation
     *   [ BindingIdentifier : number ] TypeAnnotation
     */

    public interface ITsIndexSignature : ITsTypeMember
    {
        ITsIdentifier ParameterName { get; }
        bool IsParameterNumberType { get; }
        ITsType ReturnType { get; }
    }

    /* MethodSignature:
     *   PropertyName ?Opt CallSignature
     */

    public interface ITsMethodSignature : ITsTypeMember
    {
        ITsPropertyName PropertyName { get; }
        bool IsOptional { get; }
        ITsCallSignature CallSignature { get; }
    }

    /* TypeAliasDeclaration:
     *   type BindingIdentifier TypeParametersOpt = Type ;
     */

    public interface ITsTypeAliasDeclaration : ITsDeclaration
    {
        ITsIdentifier AliasName { get; }
        ITsTypeParameters TypeParameters { get; }
        ITsType Type { get; }
    }

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
        ITsType PropertyType { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    public interface ITsSetAccessor : ITsPropertyDefinition
    {
        ITsPropertyName PropertyName { get; }
        ITsBindingIdentifierOrPattern ParameterName { get; }
        ITsType ParameterType { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    /* FunctionExpression: ( Modified )
     *   function BindingIdentifierOpt CallSignature { FunctionBody }
     */

    public interface ITsFunctionExpression : ITsExpression
    {
        ITsIdentifier FunctionName { get; }
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

    public interface ITsVariableDeclaration : IAstNode { }

    public interface ITsSimpleVariableDeclaration : ITsVariableDeclaration
    {
        ITsIdentifier VariableName { get; }
        ITsType VariableType { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsDestructuringVariableDeclaration : ITsVariableDeclaration
    {
        ITsBindingPattern BindingPattern { get; }
        ITsType VariableType { get; }
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

    public interface ITsLexicalBinding : IAstNode { }

    public interface ITsSimpleLexicalBinding : ITsLexicalBinding
    {
        ITsIdentifier VariableName { get; }
        ITsType VariableType { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsDestructuringLexicalBinding : ITsLexicalBinding
    {
        ITsBindingPattern BindingPattern { get; }
        ITsType VariableType { get; }
        ITsExpression Initializer { get; }
    }

    /* A.4 Functions
     * -------------
     * FunctionDeclaration: ( Modified )
     *   function BindingIdentifierOpt CallSignature { FunctionBody }
     *   function BindingIdentifierOpt CallSignature ;
     */

    public interface ITsFunctionDeclaration : ITsDeclaration
    {
        ITsIdentifier FunctionName { get; }
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
        ITsIdentifier ClassName { get; }
        ITsTypeParameters TypeParameters { get; }
        ITsClassHeritage Heritage { get; }
        ImmutableArray<ITsClassElement> ClassBody { get; }
    }

    public interface ITsClassHeritage : IAstNode
    {
        ITsTypeReference ExtendsClause { get; }
        ImmutableArray<ITsTypeReference> ImplementsClause { get; }
        bool IsEmpty { get; }
    }

    public interface ITsClassElement : IAstNode { }

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
        ITsPropertyName PropertyName { get; }
        ITsType TypeAnnotation { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsFunctionMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        ITsPropertyName FunctionName { get; }
        ITsCallSignature CallSignature { get; }
        ImmutableArray<ITsStatementListItem> FunctionBody { get; }
    }

    public interface ITsGetAccessorMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        ITsGetAccessor GetAccessor { get; }
    }

    public interface ITsSetAccessorMemberDeclaration : ITsClassElement
    {
        TsAccessibilityModifier? AccessibilityModifier { get; }
        bool IsStatic { get; }
        ITsSetAccessor SetAccessor { get; }
    }

    /* IndexMemberDeclaration:
     *   IndexSignature ;
     */

    public interface ITsIndexMemberDeclaration : ITsClassElement
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

    public interface ITsEnumDeclaration : ITsDeclaration
    {
        bool IsConst { get; }
        ITsIdentifier EnumName { get; }
        ImmutableArray<ITsEnumMember> EnumBody { get; }
    }

    public interface ITsEnumMember : IAstNode
    {
        ITsPropertyName Name { get; }
        ITsExpression Value { get; }
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

    public interface ITsNamespaceElement : IAstNode { }

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

    public interface ITsImportAliasDeclaration : ITsNamespaceElement
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

    public interface IImplementationSourceFile : IAstNode
    {
        bool IsModule { get; }
    }

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

    public interface IImplementationScript : IImplementationSourceFile
    {
        ImmutableArray<IImplementationScriptElement> Elements { get; }
    }

    public interface IImplementationScriptElement : IAstNode
    { }

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

    public interface IImplementationElement : IImplementationScriptElement { }

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
     *
     * DeclarationElement:
     *   InterfaceDeclaration
     *   TypeAliasDeclaration
     *   NamespaceDeclaration
     *   AmbientDeclaration
     *   ImportAliasDeclaration
     *
     * ImplementationModule:
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
     *
     * DeclarationModule:
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
     *
     * ImportRequireDeclaration:
     *   import BindingIdentifier = require ( StringLiteral ) ;
     *
     * ExportImplementationElement:
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
     *
     * ExportDeclarationElement:
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
    public interface ITsAmbientVariableDeclaration : IAstNode
    {
        VariableDeclarationKind DeclarationKind { get; }
        ImmutableArray<ITsAmbientBinding> Declarations { get; }
    }

    /// <summary>
    /// Represents an ambient variable binding of the form 'name: type'.
    /// </summary>
    public interface ITsAmbientBinding : IAstNode
    {
        ITsIdentifier VariableName { get; }
        ITsType VariableType { get; }
    }

    /* AmbientFunctionDeclaration:
     *   function BindingIdentifier CallSignature ;
     */

    public interface ITsAmbientFunctionDeclaration : IAstNode
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
     *   IndexSignature
     *
     * AmbientConstructorDeclaration:
     *   constructor ( ParameterListOpt ) ;
     *
     * AmbientPropertyMemberDeclaration:
     *   AccessibilityModifierOpt staticOpt PropertyName TypeAnnotationOpt ;
     *   AccessibilityModifierOpt staticOpt PropertyName CallSignature ;
     */

    public interface ITsAmbientClassBodyElement : IAstNode { }

    public interface ITsAmbientConstructorDeclaration : ITsAmbientClassBodyElement
    {
        ITsParameterList ParameterList { get; }
    }

    /* AmbientEnumDeclaration:
     *   EnumDeclaration
     */

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
     *   exportOpt AmbientLexicalDeclaration
     *   exportOpt AmbientFunctionDeclaration
     *   exportOpt AmbientClassDeclaration
     *   exportOpt InterfaceDeclaration
     *   exportOpt AmbientEnumDeclaration
     *   exportOpt AmbientNamespaceDeclaration
     *   exportOpt ImportAliasDeclaration
     *
     * AmbientModuleDeclaration:
     *   declare module StringLiteral { DeclarationModule }
     */
}
