// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptGrammar.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System.Collections.Immutable;

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
     *  < TypeParameterList >
     *
     * TypeParameterList:
     *  TypeParameter
     *  TypeParameterList , TypeParameter
     *
     * TypeParameter:
     *  BindingIdentifier ConstraintOpt
     *
     * Constraint:
     *  extends Type
     *
     * TypeArguments:
     *  < TypeArgumentList >
     *
     * TypeArgumentList:
     *  TypeArgument
     *  TypeArgumentList , TypeArgument
     *
     * TypeArgument:
     *  Type
     *
     * Type:
     *  UnionOrIntersectionOrPrimaryType
     *  FunctionType
     *  ConstructorType
     *
     * UnionOrIntersectionOrPrimaryType:
     *  UnionType
     *  IntersectionOrPrimaryType
     *
     * IntersectionOrPrimaryType:
     *  IntersectionType
     *  PrimaryType
     *
     * PrimaryType:
     *  ParenthesizedType
     *  PredefinedType
     *  TypeReference
     *  ObjectType
     *  ArrayType
     *  TupleType
     *  TypeQuery
     *  ThisType
     *
     * ParenthesizedType:
     *  ( Type )
     *
     * PredefinedType:
     *  any
     *  number
     *  boolean
     *  string
     *  symbol
     *  void
     *
     * TypeReference:
     *  TypeName [no LineTerminator here] TypeArgumentsOpt
     *
     * TypeName:
     *  IdentifierReference
     *  NamespaceName . IdentifierReference
     *
     * NamespaceName:
     *  IdentifierReference
     *  NamespaceName . IdentifierReference
     *
     * ObjectType:
     *  { TypeBodyOpt }
     *
     * TypeBody:
     *  TypeMemberList ;Opt
     *  TypeMemberList ,Opt
     *
     * TypeMemberList:
     *  TypeMember
     *  TypeMemberList ; TypeMember
     *  TypeMemberList , TypeMember
     *
     * TypeMember:
     *  PropertySignature
     *  CallSignature
     *  ConstructSignature
     *  IndexSignature
     *  MethodSignature
     *
     * ArrayType:
     *  PrimaryType [no LineTerminator here] [ ]
     *
     * TupleType:
     *  [ TupleElementTypes ]
     *
     * TupleElementTypes:
     *  TupleElementType
     *  TupleElementTypes , TupleElementType
     *
     * TupleElementType:
     *  Type
     *
     * UnionType:
     *  UnionOrIntersectionOrPrimaryType | IntersectionOrPrimaryType
     *
     * IntersectionType:
     *  IntersectionOrPrimaryType & PrimaryType
     *
     * FunctionType:
     *  TypeParametersOpt ( ParameterListOpt ) => Type
     *
     * ConstructorType:
     *  new TypeParametersOpt ( ParameterListOpt ) => Type
     *
     * TypeQuery:
     *  typeof TypeQueryExpression
     *
     * TypeQueryExpression:
     *  IdentifierReference
     *  TypeQueryExpression . IdentifierName
     *
     * ThisType:
     *  this
     *
     * PropertySignature:
     *  PropertyName ?Opt TypeAnnotationOpt
     *
     * PropertyName:
     *  IdentifierName
     *  StringLiteral
     *  NumericLiteral
     *
     * TypeAnnotation:
     *  : Type
     *
     * CallSignature:
     *  TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
     *
     * ParameterList:
     *  RequiredParameterList
     *  OptionalParameterList
     *  RestParameter
     *  RequiredParameterList , OptionalParameterList
     *  RequiredParameterList , RestParameter
     *  OptionalParameterList , RestParameter
     *  RequiredParameterList , OptionalParameterList , RestParameter
     *
     * RequiredParameterList:
     *  RequiredParameter
     *  RequiredParameterList , RequiredParameter
     *
     * RequiredParameter:
     *  AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt
     *  BindingIdentifier : StringLiteral
     *
     * AccessibilityModifier:
     *  public
     *  private
     *  protected
     *
     * BindingIdentifierOrPattern:
     *  BindingIdentifier
     *  BindingPattern
     *
     * OptionalParameterList:
     *  OptionalParameter
     *  OptionalParameterList , OptionalParameter
     *
     * OptionalParameter:
     *  AccessibilityModifierOpt BindingIdentifierOrPattern ? TypeAnnotationOpt
     *  AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt Initializer
     *  BindingIdentifier ? : StringLiteral
     *
     * RestParameter:
     *  ... BindingIdentifier TypeAnnotationOpt
     *
     * ConstructSignature:
     *  new TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
     *
     * IndexSignature:
     *  [ BindingIdentifier : string ] TypeAnnotation
     *  [ BindingIdentifier : number ] TypeAnnotation
     *
     * MethodSignature:
     *  PropertyName ?Opt CallSignature
     *
     * TypeAliasDeclaration:
     *  type BindingIdentifier TypeParametersOpt = Type ;
     */

    /* A.2 Expressions
     * ---------------
     * PropertyDefinition: ( Modified )
     *  IdentifierReference
     *  CoverInitializedName
     *  PropertyName : AssignmentExpression
     *  PropertyName CallSignature { FunctionBody }
     *  GetAccessor
     *  SetAccessor
     *
     * GetAccessor:
     *  get PropertyName ( ) TypeAnnotationOpt { FunctionBody }
     *
     * SetAccessor:
     *  set PropertyName ( BindingIdentifierOrPattern TypeAnnotationOpt ) { FunctionBody }
     *
     * FunctionExpression: ( Modified )
     *  function BindingIdentifierOpt CallSignature { FunctionBody }
     *
     * ArrowFormalParameters: ( Modified )
     *  CallSignature
     *
     * Arguments: ( Modified )
     *  TypeArgumentsOpt ( ArgumentListOpt )
     *
     * UnaryExpression: ( Modified )
     *  …
     *  < Type > UnaryExpression
     */

    /* A.3 Statements
     * --------------
     * Declaration: ( Modified )
     *  …
     *  InterfaceDeclaration
     *  TypeAliasDeclaration
     *  EnumDeclaration
     *
     * VariableDeclaration: ( Modified )
     *  SimpleVariableDeclaration
     *  DestructuringVariableDeclaration
     *
     * SimpleVariableDeclaration:
     *  BindingIdentifier TypeAnnotationOpt InitializerOpt
     *
     * DestructuringVariableDeclaration:
     *  BindingPattern TypeAnnotationOpt Initializer
     *
     * LexicalBinding: ( Modified )
     *  SimpleLexicalBinding
     *  DestructuringLexicalBinding
     *
     * SimpleLexicalBinding:
     *  BindingIdentifier TypeAnnotationOpt InitializerOpt
     *
     * DestructuringLexicalBinding:
     *  BindingPattern TypeAnnotationOpt InitializerOpt
     */

    /* A.4 Functions
     * -------------
     * FunctionDeclaration: ( Modified )
     *  function BindingIdentifierOpt CallSignature { FunctionBody }
     *  function BindingIdentifierOpt CallSignature ;
     */

    /* A.5 Interfaces
     * --------------
     * InterfaceDeclaration:
     *  interface BindingIdentifier TypeParametersOpt InterfaceExtendsClauseOpt ObjectType
     *
     * InterfaceExtendsClause:
     *  extends ClassOrInterfaceTypeList
     *
     * ClassOrInterfaceTypeList:
     *  ClassOrInterfaceType
     *  ClassOrInterfaceTypeList , ClassOrInterfaceType
     *
     * ClassOrInterfaceType:
     *  TypeReference
     */

    /* A.6 Classes
     * -----------
     * ClassDeclaration: ( Modified )
     *  class BindingIdentifierOpt TypeParametersOpt ClassHeritage { ClassBody }
     *
     * ClassHeritage: ( Modified )
     *  ClassExtendsClauseOpt ImplementsClauseOpt
     *
     * ClassExtendsClause:
     *  extends  ClassType
     *
     * ClassType:
     *  TypeReference
     *
     * ImplementsClause:
     *  implements ClassOrInterfaceTypeList
     *
     * ClassElement: ( Modified )
     *  ConstructorDeclaration
     *  PropertyMemberDeclaration
     *  IndexMemberDeclaration
     *
     * ConstructorDeclaration:
     *  AccessibilityModifierOpt constructor ( ParameterListOpt ) { FunctionBody }
     *  AccessibilityModifierOpt constructor ( ParameterListOpt ) ;
     *
     * PropertyMemberDeclaration:
     *  MemberVariableDeclaration
     *  MemberFunctionDeclaration
     *  MemberAccessorDeclaration
     *
     * MemberVariableDeclaration:
     *  AccessibilityModifierOpt staticOpt PropertyName TypeAnnotationOpt InitializerOpt ;
     *
     * MemberFunctionDeclaration:
     *  AccessibilityModifierOpt staticOpt PropertyName CallSignature { FunctionBody }
     *  AccessibilityModifierOpt staticOpt PropertyName CallSignature ;
     *
     * MemberAccessorDeclaration:
     *  AccessibilityModifierOpt staticOpt GetAccessor
     *  AccessibilityModifierOpt staticOpt SetAccessor
     *
     * IndexMemberDeclaration:
     *  IndexSignature ;
     */

    /* A.7 Enums
     * ---------
     * EnumDeclaration:
     *  constOpt enum BindingIdentifier { EnumBodyOpt }
     *
     * EnumBody:
     *  EnumMemberList ,Opt
     *
     * EnumMemberList:
     *  EnumMember
     *  EnumMemberList , EnumMember
     *
     * EnumMember:
     *  PropertyName
     *  PropertyName = EnumValue
     *
     * EnumValue:
     *  AssignmentExpression
     */

    /* A.8 Namespaces
     * --------------
     * NamespaceDeclaration:
     *  namespace IdentifierPath { NamespaceBody }
     *
     * IdentifierPath:
     *  BindingIdentifier
     *  IdentifierPath . BindingIdentifier
     *
     * NamespaceBody:
     *  NamespaceElementsOpt
     *
     * NamespaceElements:
     *  NamespaceElement
     *  NamespaceElements NamespaceElement
     *
     * NamespaceElement:
     *  Statement
     *  LexicalDeclaration
     *  FunctionDeclaration
     *  GeneratorDeclaration
     *  ClassDeclaration
     *  InterfaceDeclaration
     *  TypeAliasDeclaration
     *  EnumDeclaration
     *  NamespaceDeclaration
     *  AmbientDeclaration
     *  ImportAliasDeclaration
     *  ExportNamespaceElement
     *
     * ExportNamespaceElement:
     *  export VariableStatement
     *  export LexicalDeclaration
     *  export FunctionDeclaration
     *  export GeneratorDeclaration
     *  export ClassDeclaration
     *  export InterfaceDeclaration
     *  export TypeAliasDeclaration
     *  export EnumDeclaration
     *  export NamespaceDeclaration
     *  export AmbientDeclaration
     *  export ImportAliasDeclaration
     *
     * ImportAliasDeclaration:
     *  import BindingIdentifier = EntityName ;
     *
     * EntityName:
     *  NamespaceName
     *  NamespaceName . IdentifierReference
     */

    /* A.9 Scripts and Modules
     * -----------------------
     * SourceFile:
     *  ImplementationSourceFile
     *  DeclarationSourceFile
     *
     * ImplementationSourceFile:
     *  ImplementationScript
     *  ImplementationModule
     *
     * DeclarationSourceFile:
     *  DeclarationScript
     *  DeclarationModule
     */

    public interface IImplementationSourceFile : ITsCodeModel
    {
        bool IsModule { get; }
    }

    /* ImplementationScript:
     *  ImplementationScriptElementsOpt
     *
     * ImplementationScriptElements:
     *  ImplementationScriptElement
     *  ImplementationScriptElements ImplementationScriptElement
     *
     * ImplementationScriptElement:
     *  ImplementationElement
     *  AmbientModuleDeclaration
     */

    public interface IImplementationScript : IImplementationSourceFile
    {
        ImmutableArray<IImplementationScriptElement> Elements { get; }
    }

    public interface IImplementationScriptElement : ITsCodeModel { }

    /* ImplementationElement:
     *  Statement
     *  LexicalDeclaration
     *  FunctionDeclaration
     *  GeneratorDeclaration
     *  ClassDeclaration
     *  InterfaceDeclaration
     *  TypeAliasDeclaration
     *  EnumDeclaration
     *  NamespaceDeclaration
     *  AmbientDeclaration
     *  ImportAliasDeclaration
     */

    public interface IImplementationElement : IImplementationScriptElement { }

    /* DeclarationScript:
     *  DeclarationScriptElementsOpt
     *
     * DeclarationScriptElements:
     *  DeclarationScriptElement
     *  DeclarationScriptElements DeclarationScriptElement
     *
     * DeclarationScriptElement:
     *  DeclarationElement
     *  AmbientModuleDeclaration
     *
     * DeclarationElement:
     *  InterfaceDeclaration
     *  TypeAliasDeclaration
     *  NamespaceDeclaration
     *  AmbientDeclaration
     *  ImportAliasDeclaration
     *
     * ImplementationModule:
     *  ImplementationModuleElementsOpt
     *
     * ImplementationModuleElements:
     *  ImplementationModuleElement
     *  ImplementationModuleElements ImplementationModuleElement
     *
     * ImplementationModuleElement:
     *  ImplementationElement
     *  ImportDeclaration
     *  ImportAliasDeclaration
     *  ImportRequireDeclaration
     *  ExportImplementationElement
     *  ExportDefaultImplementationElement
     *  ExportListDeclaration
     *  ExportAssignment
     *
     * DeclarationModule:
     *  DeclarationModuleElementsOpt
     *
     * DeclarationModuleElements:
     *  DeclarationModuleElement
     *  DeclarationModuleElements DeclarationModuleElement
     *
     * DeclarationModuleElement:
     *  DeclarationElement
     *  ImportDeclaration
     *  ImportAliasDeclaration
     *  ExportDeclarationElement
     *  ExportDefaultDeclarationElement
     *  ExportListDeclaration
     *  ExportAssignment
     *
     * ImportRequireDeclaration:
     *  import BindingIdentifier = require ( StringLiteral ) ;
     *
     * ExportImplementationElement:
     *  export VariableStatement
     *  export LexicalDeclaration
     *  export FunctionDeclaration
     *  export GeneratorDeclaration
     *  export ClassDeclaration
     *  export InterfaceDeclaration
     *  export TypeAliasDeclaration
     *  export EnumDeclaration
     *  export NamespaceDeclaration
     *  export AmbientDeclaration
     *  export ImportAliasDeclaration
     *
     * ExportDeclarationElement:
     *  export InterfaceDeclaration
     *  export TypeAliasDeclaration
     *  export AmbientDeclaration
     *  export ImportAliasDeclaration
     *
     * ExportDefaultImplementationElement:
     *  export default FunctionDeclaration
     *  export default GeneratorDeclaration
     *  export default ClassDeclaration
     *  export default AssignmentExpression ;
     *
     * ExportDefaultDeclarationElement:
     *  export default AmbientFunctionDeclaration
     *  export default AmbientClassDeclaration
     *  export default IdentifierReference ;
     *
     * ExportListDeclaration:
     *  export * FromClause ;
     *  export ExportClause FromClause ;
     *  export ExportClause ;
     *
     * ExportAssignment:
     *  export = IdentifierReference ;
     */

    /* A.10 Ambients
     * -------------
     * AmbientDeclaration:
     *  declare AmbientVariableDeclaration
     *  declare AmbientFunctionDeclaration
     *  declare AmbientClassDeclaration
     *  declare AmbientEnumDeclaration
     *  declare AmbientNamespaceDeclaration
     *
     * AmbientVariableDeclaration:
     *  var AmbientBindingList ;
     *  let AmbientBindingList ;
     *  const AmbientBindingList ;
     *
     * AmbientBindingList:
     *  AmbientBinding
     *  AmbientBindingList , AmbientBinding
     *
     * AmbientBinding:
     *  BindingIdentifier TypeAnnotationOpt
     *
     * AmbientFunctionDeclaration:
     *  function BindingIdentifier CallSignature ;
     *
     * AmbientClassDeclaration:
     *  class BindingIdentifier TypeParametersOpt ClassHeritage { AmbientClassBody }
     *
     * AmbientClassBody:
     *  AmbientClassBodyElementsOpt
     *
     * AmbientClassBodyElements:
     *  AmbientClassBodyElement
     *  AmbientClassBodyElements AmbientClassBodyElement
     *
     * AmbientClassBodyElement:
     *  AmbientConstructorDeclaration
     *  AmbientPropertyMemberDeclaration
     *  IndexSignature
     *
     * AmbientConstructorDeclaration:
     *  constructor ( ParameterListOpt ) ;
     *
     * AmbientPropertyMemberDeclaration:
     *  AccessibilityModifierOpt staticOpt PropertyName TypeAnnotationOpt ;
     *  AccessibilityModifierOpt staticOpt PropertyName CallSignature ;
     *
     * AmbientEnumDeclaration:
     *  EnumDeclaration
     *
     * AmbientNamespaceDeclaration:
     *  namespace IdentifierPath { AmbientNamespaceBody }
     *
     * AmbientNamespaceBody:
     *  AmbientNamespaceElementsOpt
     *
     * AmbientNamespaceElements:
     *  AmbientNamespaceElement
     *  AmbientNamespaceElements AmbientNamespaceElement
     *
     * AmbientNamespaceElement:
     *  exportOpt AmbientVariableDeclaration
     *  exportOpt AmbientLexicalDeclaration
     *  exportOpt AmbientFunctionDeclaration
     *  exportOpt AmbientClassDeclaration
     *  exportOpt InterfaceDeclaration
     *  exportOpt AmbientEnumDeclaration
     *  exportOpt AmbientNamespaceDeclaration
     *  exportOpt ImportAliasDeclaration
     *
     * AmbientModuleDeclaration:
     *  declare module StringLiteral {  DeclarationModule }
     */
}
