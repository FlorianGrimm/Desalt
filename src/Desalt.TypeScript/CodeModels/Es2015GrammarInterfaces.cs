// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es2015GrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System.Collections.Immutable;
    using Desalt.TypeScript.CodeModels.Expressions;

    /***********************************************************************************************
     * Ecma-262 6.0 (ES 2015) Grammar
     * -------------------------------------------------------------------------
     * See http://www.ecma-international.org/ecma-262/6.0/
     **********************************************************************************************/

    /* 12.1 Identifiers
     * ----------------
     * IdentifierReference:
     *   Identifier
     *   [~Yield] yield
     *
     * BindingIdentifier:
     *   Identifier
     *   [~Yield] yield
     *
     * LabelIdentifier:
     *   Identifier
     *   [~Yield] yield
     *
     * Identifier:
     *   IdentifierName but not ReservedWord
     */

    public interface ITsIdentifier : ITsExpression
    {
        string Text { get; }
    }

    /* 12.2 Primary Expression
     * -----------------------
     * PrimaryExpression:
     *   this
     *   IdentifierReference
     *   Literal
     *   ArrayLiteral
     *   ObjectLiteral
     *   FunctionExpression
     *   ClassExpression
     *   GeneratorExpression
     *   RegularExpressionLiteral
     *   TemplateLiteral
     *   CoverParenthesizedExpressionAndArrowParameterList
     */

    public interface ITsThisExpression : ITsExpression { }

    public interface ITsLiteralExpression : ITsExpression
    {
        TsLiteralKind Kind { get; }
        string Literal { get; }
    }

    public interface ITsArrayLiteralExpression : ITsExpression
    {
        ImmutableArray<ITsExpression> Elements { get; }
    }

    /*
     * CoverParenthesizedExpressionAndArrowParameterList:
     *   ( Expression )
     *   ( )
     *   ( ... BindingIdentifier )
     *   ( Expression , ... BindingIdentifier )
     *
     * When processing the production
     * PrimaryExpression: CoverParenthesizedExpressionAndArrowParameterList the interpretation of
     * CoverParenthesizedExpressionAndArrowParameterList is refined using the following grammar:
     *
     * ParenthesizedExpression:
     *   ( Expression )
     */

    /* 12.2.4 Literals
     * ---------------
     * Literal:
     *   NullLiteral
     *   BooleanLiteral
     *   NumericLiteral
     *   StringLiteral
     *
     * NullLiteral: null
     * BooleanLiteral: true | false
     * NumericLiteral:
     *   DecimalLiteral
     *   BinaryIntegerLiteral
     *   OctalIntegerLiteral
     *   HexIntegerLiteral
     *
     * StringLiteral:
     *   " DoubleStringCharacters "
     *   ' SingleStringCharacters '
     */

    /* 12.2.5 Array Initializer
     * ------------------------
     * ArrayLiteral:
     *   [ ElisionOpt ]
     *   [ ElementList ]
     *   [ ElementList , ElisionOpt ]
     *
     * ElementList:
     *   ElisionOpt AssignmentExpression
     *   ElisionOpt SpreadElement
     *   ElementList , ElisionOpt AssignmentExpression
     *   ElementList , ElisionOpt SpreadElement
     *
     * Elision:
     *   ,
     *   Elision ,
     *
     * SpreadElement:
     *   ... AssignmentExpression
     */

    /* 12.2.6 Object Initializer
     * -------------------------
     * ObjectLiteral:
     *   { }
     *   { PropertyDefinition }
     *   { PropertyDefinitionList , PropertyDefinition }
     *
     * PropertyDefinitionList:
     *   PropertyDefinition
     *   PropertyDefinitionList , PropertyDefinition
     *
     * PropertyDefinition:
     *   IdentifierReference
     *   CoverInitializedName
     *   PropertyName : AssignmentExpression
     *   MethodDefinition
     *
     * PropertyName:
     *   LiteralPropertyName
     *   ComputedPropertyName
     *
     * LiteralPropertyName:
     *   IdentifierName
     *   StringLiteral
     *   NumericLiteral
     *
     * ComputedPropertyName:
     *   [ AssignmentExpression ]
     *
     * CoverInitializedName:
     *   IdentifierReference Initializer
     *
     * Initializer:
     *   = AssignmentExpression
     */

    /* 12.2.9 Template Literals
     * ------------------------
     * TemplateLiteral:
     *   NoSubstitutionTemplate
     *   TemplateHead Expression TemplateSpans
     *
     * TemplateSpans:
     *   TemplateTail
     *   TemplateMiddleList TemplateTail
     *
     * TemplateMiddleList:
     *   TemplateMiddle Expression
     *   TemplateMiddleList TemplateMiddle Expression
     */

    /* 12.e Left-Hand-Side Expressions
     * -------------------------------
     * MemberExpression:
     *   PrimaryExpression
     *   MemberExpression [ Expression ]
     *   MemberExpression . IdentifierName
     *   MemberExpression TemplateLiteral
     *   SuperProperty
     *   MetaProperty
     *   new MemberExpression Arguments
     *
     * SuperProperty:
     *   super [ Expression ]
     *   super . IdentifierName
     *
     * MetaProperty:
     *   NewTarget
     *
     * NewTarget:
     *   new . target
     *
     * NewExpression:
     *   MemberExpression
     *   new NewExpression
     *
     * CallExpression:
     *   MemberExpression Arguments
     *   SuperCall
     *   CallExpression Arguments
     *   CallExpression [ Expression ]
     *   CallExpression . IdentifierName
     *   CallExpression TemplateLiteral
     *
     * SuperCall:
     *   super Arguments
     *
     * Arguments:
     *   ( )
     *   ( ArgumentList )
     *
     * ArgumentList:
     *   AssignmentExpression
     *   ... AssignmentExpression
     *   ArgumentList , AssignmentExpression
     *   ArgumentList , ... AssignmentExpression
     *
     * LeftHandSideExpression:
     *   NewExpression
     *   CallExpression
     */

    /* 12.4 Postfix Expressions
     * ------------------------
     * PostfixExpression:
     *   LeftHandSideExpression
     *   LeftHandSideExpression [no LineTerminator here] ++
     *   LeftHandSideExpression [no LineTerminator here] --
     */

    /* 12.5 Unary Operators
     * --------------------
     * UnaryExpression:
     *   PostfixExpression
     *   delete UnaryExpression
     *   void UnaryExpression
     *   typeof UnaryExpression
     *   ++ UnaryExpression
     *   -- UnaryExpression
     *   + UnaryExpression
     *   - UnaryExpression
     *   ~ UnaryExpression
     *   ! UnaryExpression
     */

    /* 12.6 Multiplicative Operators
     * -----------------------------
     * MultiplicativeExpression:
     *   UnaryExpression
     *   MultiplicativeExpression MultiplicativeOperator UnaryExpression
     *
     * MultiplicativeOperator: one of
     *   * / %
     */

    /* 12.7 Additive Operators
     * -----------------------
     * AdditiveExpression:
     *   MultiplicativeExpression
     *   AdditiveExpression + MultiplicativeExpression
     *   AdditiveExpression - MultiplicativeExpression
     */

    /* 12.8 Bitwise Shift Operators
     * ----------------------------
     * ShiftExpression:
     *   AdditiveExpression
     *   ShiftExpression << AdditiveExpression
     *   ShiftExpression >> AdditiveExpression
     *   ShiftExpression >>> AdditiveExpression
     */

    /* 12.9 Relational Operators
     * -------------------------
     * RelationalExpression:
     *   ShiftExpression
     *   RelationalExpression < ShiftExpression
     *   RelationalExpression > ShiftExpression
     *   RelationalExpression <= ShiftExpression
     *   RelationalExpression >= ShiftExpression
     *   RelationalExpression instanceof ShiftExpression
     *   RelationalExpression in ShiftExpression
     */

    /* 12.10 Equality Operators
     * ------------------------
     * EqualityExpression:
     *   RelationalExpression
     *   EqualityExpression == RelationalExpression
     *   EqualityExpression != RelationalExpression
     *   EqualityExpression === RelationalExpression
     *   EqualityExpression !== RelationalExpression
     */

    /* 12.11 Binary Bitwise Operators
     * ------------------------------
     * BitwiseANDExpression:
     *   EqualityExpression
     *   BitwiseANDExpression & EqualityExpression
     *
     * BitwiseXORExpression:
     *   BitwiseANDExpression
     *   BitwiseXORExpression ^ BitwiseANDExpression
     *
     * BitwiseORExpression:
     *   BitwiseXORExpression
     *   BitwiseORExpression | BitwiseXORExpression
     */

    /* 12.12 Binary Logical Operators
     * ------------------------------
     * LogicalANDExpression:
     *   BitwiseORExpression
     *   LogicalANDExpression && BitwiseORExpression
     *
     * LogicalORExpression:
     *   LogicalANDExpression
     *   LogicalORExpression || LogicalANDExpression
     */

    /* 12.13 Conditional Operator ( ? : )
     * ----------------------------------
     * ConditionalExpression:
     *   LogicalORExpression
     *   LogicalORExpression ? AssignmentExpression : AssignmentExpression
     */

    /* 12.14 Assignment Operators
     * --------------------------
     * AssignmentExpression:
     *   ConditionalExpression
     *   YieldExpression
     *   ArrowFunction
     *   LeftHandSideExpression = AssignmentExpression
     *   LeftHandSideExpression AssignmentOperator AssignmentExpression
     *
     * AssignmentOperator: one of
     *   *=   /=   %=   +=   -=   <<=   >>=   >>>=   &=   ^=   |=
     */

    /* 12.15 Comma Operator ( , )
     * --------------------------
     * Expression:
     *   AssignmentExpression
     *   Expression , AssignmentExpression
     */

    public interface ITsExpression : ITsCodeModel
    {
    }

    /* A.3 Statements
     * --------------
     * Statement:
     *   BlockStatement
     *   VariableStatement
     *   EmptyStatement
     *   ExpressionStatement
     *   IfStatement
     *   BreakableStatement
     *   ContinueStatement
     *   BreakStatement
     *   ReturnStatement
     *   WithStatement
     *   LabelledStatement
     *   ThrowStatement
     *   TryStatement
     *   DebuggerStatement
     *
     * Declaration:
     *   HoistableDeclaration
     *   ClassDeclaration
     *   LexicalDeclaration
     *
     * HoistableDeclaration:
     *   FunctionDeclaration
     *   GeneratorDeclaration
     *
     * BreakableStatement:
     *   IterationStatement
     *   SwitchStatement
     *
     * 13.2 Block
     * ----------
     * BlockStatement:
     *   Block
     *
     * Block:
     *   { StatementListOpt }
     *
     * StatementList:
     *   StatementListItem
     *   StatementList StatementListItem
     *
     * StatementListItem:
     *   Statement
     *   Declaration
     */

    /* 13.3.1 Let and Const Declarations
     * ---------------------------------
     * LexicalDeclaration:
     *   LetOrConst BindingList ;
     *
     * LetOrConst:
     *   let
     *   const
     *
     * BindingList:
     *   LexicalBinding
     *   BindingList , LexicalBinding
     *
     * LexicalBinding:
     *   BindingIdentifier InitializerOpt
     *   BindingPattern Initializer
     */

    /* 13.3.2 Variable Statement
     * -------------------------
     * VariableStatement:
     *   var VariableDeclarationList ;
     *
     * VariableDeclarationList:
     *   VariableDeclaration
     *   VariableDeclarationList , VariableDeclaration
     *
     * VariableDeclaration:
     *   BindingIdentifier InitializerOpt
     *   BindingPattern Initializer
     */

    /* 13.3.3 Destructuring Binding Patterns
     * -------------------------------------
     * BindingPattern:
     *   ObjectBindingPattern
     *   ArrayBindingPattern
     *
     * ObjectBindingPattern:
     *   { }
     *   { BindingPropertyList }
     *   { BindingPropertyList , }
     *
     * ArrayBindingPattern:
     *   [ ElisionOpt BindingRestElementOpt ]
     *   [ BindingElementList ]
     *   [ BindingElementList , ElisionOpt BindingRestElementOpt ]
     *
     * BindingPropertyList:
     *   BindingProperty
     *   BindingPropertyList , BindingProperty
     *
     * BindingElementList:
     *   BindingElisionElement
     *   BindingElementList , BindingElisionElement
     *
     * BindingElisionElement:
     *   ElisionOpt BindingElement
     *
     * BindingProperty:
     *   SingleNameBinding
     *   PropertyName : BindingElement
     *
     * BindingElement:
     *   SingleNameBinding
     *   BindingPattern InitializerOpt
     *
     * SingleNameBinding:
     *   BindingIdentifier Initializer
     *
     * BindingRestElement:
     *   ... BindingIdentifier
     */

    /* 13.4 Empty Statement
     * --------------------
     * EmptyStatement:
     *   ;
     *
     * 13.5 Expression Statement
     * -------------------------
     * ExpressionStatement:
     *   [lookahead not { {, function, class, let [ }] Expression ;
     *
     * 13.6 The if Statement
     * ---------------------
     * IfStatement:
     *   if ( Expression ) Statement else Statement
     *   if ( Expression ) Statement
     *
     * 13.7 Iteration Statements
     * -------------------------
     * IterationStatement:
     *   do Statement while ( Expression ) ;
     *   while ( Expression ) Statement
     *   for ( [lookahead not 'let ['] ExpressionOpt ; ExpressionOpt ; ExpressionOpt ) Statement
     *   for ( var VariableDeclarationList ; ExpressionOpt ; ExpressionOpt ) Statement
     *   for ( LexicalDeclaration ExpressionOpt ; ExpressionOpt ) Statement
     *   for ( [lookahead not 'let ['] LeftHandSideExpression in Expression ) Statement
     *   for ( var ForBinding in Expression ) Statement
     *   for ( ForDeclaration in Expression ) Statement
     *   for ( [lookahead not 'let'] LeftHandSideExpression of AssignmentExpression ) Statement
     *   for ( var ForBinding of AssignmentExpression ) Statement
     *   for ( ForDeclaration of AssignmentExpression ) Statement
     *
     * ForDeclaration:
     *   LetOrConst ForBinding
     *
     * ForBinding:
     *   BindingIdentifier
     *   BindingPattern
     *
     * 13.8 The continue Statement
     * ---------------------------
     * ContinueStatement:
     *   continue ;
     *   continue [no LineTerminator here] LabelIdentifier ;
     *
     * 13.9 The break Statement
     * ------------------------
     * BreakStatement:
     *   break ;
     *   break [no LineTerminator here] LabelIdentifier ;
     *
     * 13.10 The return Statement
     * --------------------------
     * ReturnStatement:
     *   return ;
     *   return [no LineTerminator here] Expression ;
     *
     * 13.11 The with Statement
     * ------------------------
     * WithStatement:
     *   with ( Expression ) Statement
     *
     * 13.12 The switch Statement
     * --------------------------
     * SwitchStatement:
     *   switch ( Expression ) CaseBlock
     *
     * CaseBlock:
     *   { CaseClausesOpt }
     *   { CaseClausesOpt DefaultClause CaseClausesOpt }
     *
     * CaseClauses:
     *   CaseClause
     *   CaseClauses CaseClause
     *
     * CaseClause:
     *   case Expression : StatementListOpt
     *
     * DefaultClause:
     *   default : StatementListOpt
     *
     * 13.13 Labelled Statements
     * -------------------------
     * LabelledStatement:
     *   LabelIdentifier : LabelledItem
     *
     * LabelledItem:
     *   Statement
     *   FunctionDeclaration
     *
     * 13.14 The throw Statement
     * -------------------------
     * ThrowStatement:
     *   throw [no LineTerminator here] Expression ;
     *
     * 12.14 The try Statement
     * -----------------------
     * TryStatement:
     *   try Block Catch
     *   try Block Finally
     *   try Block Catch Finally
     *
     * Catch:
     *   catch ( CatchParameter ) Block
     *
     * Finally:
     *   finally Block
     *
     * CatchParameter:
     *   BindingIdentifier
     *   BindingPattern
     *
     * 13.16 The debugger Statement
     * ----------------------------
     * DebuggerStatement:
     *   debugger ;
     */

    /* A.4 Functions and Classes
     *
     * 14.1 Function Definitions
     * ------------------------
     * FunctionDeclaration:
     *   function BindingIdentifier ( FormalParameters ) { FunctionBody }
     *   function ( FormalParameters ) { FunctionBody }
     *
     * FunctionExpression:
     *   function BindingIdentifierOpt ( FormalParameters ) { FunctionBody }
     *
     * StrictFormalParameters:
     *   FormalParameters
     *
     * FormalParameters:
     *   [empty]
     *   FormalParameterList
     *
     * FormalParameterList:
     *   FunctionRestParameter
     *   FormalsList
     *   FormalsList , FunctionRestParameter
     *
     * FormalsList:
     *   FormalParameter
     *   FormalsList , FormalParameter
     *
     * FunctionRestParameter:
     *   BindingRestElement
     *
     * FormalParameter:
     *   BindingElement
     *
     * FunctionBody:
     *   FunctionStatementList
     *
     * FunctionStatementList:
     *   StatementListOpt
     */

    /* 14.2 Arrow Function Definitions
     * -------------------------------
     * ArrowFunction:
     *   ArrowParameters [no LineTerminator here] => ConciseBody
     *
     * ArrowParameters:
     *   BindingIdentifier
     *   CoverParenthesizedExpressionAndArrowParameterList
     *
     * ConciseBody:
     *   [lookahead != { ] AssignmentExpression
     *   { FunctionBody }
     *
     * When the production
     * ArrowParameters: CoverParenthesizedExpressionAndArrowParameterList is recognized the following
     * grammar is used to refine the interpretation of CoverParenthesizedExpressionAndArrowParameterList:
     *
     * ArrowFormalParameters:
     *   ( StrictFormalParameters )
     */

    /* 14.3 Method Definitions
     * -----------------------
     * MethodDefinition:
     *   PropertyName ( StrictFormalParameters ) { FunctionBody }
     *   GeneratorMethod
     *   get PropertyName ( ) { FunctionBody }
     *   set PropertyName ( PropertySetParameterList ) { FunctionBody }
     *
     * PropertySetParameterList:
     *   FormalParameter
     */

    /* 14.4 Generator Function Definitions
     * -----------------------------------
     * GeneratorMethod:
     *   * PropertyName ( StrictFormalParameters ) { GeneratorBody }
     *
     * GeneratorDeclaration:
     *   function * BindingIdentifier ( FormalParameters ) { GeneratorBody }
     *   function * ( FormalParameters ) { GeneratorBody }
     *
     * GeneratorExpression:
     *   function * BindingIdentifierOpt ( FormalParameters ) { GeneratorBody }
     *
     * GeneratorBody:
     *   FunctionBody
     *
     * YieldExpression:
     *   yield
     *   yield [no LineTerminator here] AssignmentExpression
     *   yield [no LineTerminator here] * AssignmentExpression
     */

    /* 14.5 Class Definitions
     * ----------------------
     * ClassDeclaration:
     *   class BindingIdentifier ClassTail
     *   class ClassTail
     *
     * ClassExpression:
     *   class BindingIdentifierOpt ClassTail
     *
     * ClassTail:
     *   ClassHeritageOpt { ClassBodyOpt }
     *
     * ClassHeritage:
     *   extends LeftHandSideExpression
     *
     * ClassBody:
     *   ClassElementList
     *
     * ClassElementList:
     *   ClassElement
     *   ClassElementList ClassElement
     *
     * ClassElement:
     *   MethodDefinition
     *   static MethodDefinition
     *   ;
     */

    /* A.5 Scripts and Modules
     *
     * 15.1 Scripts
     * ------------
     * Script:
     *   ScriptBodyOpt
     *
     * ScriptBody:
     *   StatementList
     *
     * 15.2 Modules
     * ------------
     * Module:
     *   ModuleBodyOpt
     *
     * ModuleBody:
     *   ModuleItemList
     *
     * ModuleItemList:
     *   ModuleItem
     *   ModuleItemList ModuleItem
     *
     * ModuleItem:
     *   ImportDeclaration
     *   ExportDeclaration
     *   StatementListItem
     *
     * 15.2.2 Imports
     * --------------
     * ImportDeclaration:
     *   import ImportClause FromClause ;
     *   import ModuleSpecifier ;
     *
     * ImportClause:
     *   ImportedDefaultBinding
     *   NameSpaceImport
     *   NamedImports
     *   ImportedDefaultBinding , NameSpaceImport
     *   ImportedDefaultBinding , NamedImports
     *
     * ImportedDefaultBinding:
     *   ImportedBinding
     *
     * NameSpaceImport:
     *   * as ImportedBinding
     *
     * NamedImports:
     *   { }
     *   { ImportsList }
     *   { ImportsList , }
     *
     * FromClause:
     *   from ModuleSpecifier
     *
     * ImportsList:
     *   ImportSpecifier
     *   ImportsList , ImportSpecifier
     *
     * ImportSpecifier:
     *   ImportedBinding
     *   IdentifierName as ImportedBinding
     *
     * ModuleSpecifier:
     *   StringLiteral
     *
     * ImportedBinding:
     *   BindingIdentifier
     *
     * 15.2.3 Exports
     * --------------
     * ExportDeclaration:
     *   export * FromClause ;
     *   export ExportClause FromClause ;
     *   export ExportClause ;
     *   export VariableStatement
     *   export Declaration
     *   export default HoistableDeclaration
     *   export default ClassDeclaration
     *   export default [lookahead not in 'function', 'class'] AssignmentExpression ;
     *
     * ExportClause:
     *   { }
     *   { ExportsList }
     *   { ExportsList , }
     *
     * ExportsList:
     *   ExportSpecifier
     *   ExportsList , ExportSpecifier
     *
     * ExportSpecifier:
     *   IdentifierName
     *   IdentifierName as IdentifierName
     */
}
