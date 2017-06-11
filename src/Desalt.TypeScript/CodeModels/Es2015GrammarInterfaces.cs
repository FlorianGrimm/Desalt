// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es2015GrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    /***********************************************************************************************
     * Ecma-262 6.0 (ES 2015) Grammar
     * -------------------------------------------------------------------------
     * See http://www.ecma-international.org/ecma-262/6.0/
     **********************************************************************************************/

    // Since the grammar is mainly the same as the ES5 predecessor, differences are marked with a +
    // symbol for additions and a - symbol for deletions. Where there are no differences, the
    // productions may not be shown in this document and you should refer to the
    // Es5GrammarInterfaces.cs file.

    /* 12.1 Identifiers
     * ----------------
     * +IdentifierReference:
     *     Identifier
     *     [~Yield] yield
     *
     * +BindingIdentifier:
     *     Identifier
     *     [~Yield] yield
     *
     * +LabelIdentifier:
     *     Identifier
     *     [~Yield] yield
     */

    public interface IEntityName { }

    public interface IIdentifier : IEntityName, IPrimaryExpression
    {
        string Text { get; }
    }

    public interface IQualifiedName : IEntityName, ITypeScriptCodeModel
    {
        IEntityName Left { get; }
        IIdentifier Right { get; }
    }

    /* 12.2 Primary Expressions
     * ------------------------
     * PrimaryExpression:
     *     this
     *     -Identifier
     *     +IdentifierReference
     *     Literal
     *     ArrayLiteral
     *     ObjectLiteral
     *     -( Expression )
     *     +FunctionExpression
     *     +ClassExpression
     *     +GeneratorExpression
     *     +RegularExpressionLiteral
     *     +TemplateLiteral
     *     +CoverParenthesizedExpressionAndArrowParameterList
     */

    public interface IPrimaryExpression : ITypeScriptCodeModel
    {
    }

    /*
     * CoverParenthesizedExpressionAndArrowParameterList:
     *     ( Expression )
     *     ( )
     *     ( ... BindingIdentifier )
     *     ( Expression , ... BindingIdentifier )
     */

    /* ArrayLiteral:
     *     [ ElisionOpt ]
     *     [ ElementList ]
     *     [ ElementList , ElisionOpt ]
     *
     * ElementList:
     *     ElisionOpt AssignmentExpression
     *     ElementList , ElisionOpt AssignmentExpression
     *
     * Elision:
     *     ,
     *     Elision ,
     */

    /* ObjectLiteral:
     *     { }
     *     { PropertyNameAndValueList }
     *     { PropertyNameAndValueList , }
     *
     * PropertyNameAndValueList:
     *     PropertyAssignment
     *     PropertyNameAndValueList , PropertyAssignment
     *
     * PropertyAssignment:
     *     PropertyName : AssignmentExpression
     *     get PropertyName ( ) { FunctionBody }
     *     set PropertyName ( PropertySetParameterList ) { FunctionBody }
     *
     * PropertyName:
     *     IdentifierName
     *     StringLiteral
     *     NumericLiteral
     *
     * PropertySetParameterList:
     *     Identifier
     */

    /* 11.2 Left-Hand-Side Expressions
     * -------------------------------
     * MemberExpression:
     *     PrimaryExpression
     *     FunctionExpression
     *     MemberExpression [ Expression ]
     *     MemberExpression . IdentifierName
     *     new MemberExpression Arguments
     *
     * NewExpression:
     *     MemberExpression
     *     new NewExpression
     *
     * CallExpression:
     *     MemberExpression Arguments
     *     CallExpression Arguments
     *     CallExpression [ Expression ]
     *     CallExpression . IdentifierName
     *
     * Arguments:
     *     ( )
     *     ( ArgumentList )
     *
     * ArgumentList:
     *     AssignmentExpression
     *     ArgumentList , AssignmentExpression
     *
     * LeftHandSideExpression:
     *     NewExpression
     *     CallExpression
     */

    /* 11.3 Postfix Expressions
     * ------------------------
     * PostfixExpression:
     *     LeftHandSideExpression
     *     LeftHandSideExpression [no LineTerminator here] ++
     *     LeftHandSideExpression [no LineTerminator here] --
     */

    /* 11.4 Unary Operators
     * --------------------
     * UnaryExpression:
     *     PostfixExpression
     *     delete UnaryExpression
     *     void UnaryExpression
     *     typeof UnaryExpression
     *     ++ UnaryExpression
     *     EqualityExpressionNoIn
     *     BitwiseANDExpressionNoIn & EqualityExpressionNoIn
     *
     * BitwiseXORExpression:
     *     BitwiseANDExpression
     *     BitwiseXORExpression ^ BitwiseANDExpression
     *
     * BitwiseXORExpressionNoIn:
     *     BitwiseANDExpressionNoIn
     *     BitwiseXORExpressionNoIn ^ BitwiseANDExpressionNoIn
     *
     * BitwiseORExpression:
     *     BitwiseXORExpression
     *     BitwiseORExpression | BitwiseXORExpression
     *
     * BitwiseORExpressionNoIn:
     *     BitwiseXORExpressionNoIn
     *     BitwiseORExpressionNoIn | BitwiseXORExpressionNoIn
     *
     * 11.11 Binary Logical Operators
     * ------------------------------
     * LogicalANDExpression:
     *     BitwiseORExpression
     *     LogicalANDExpression && BitwiseORExpression
     *
     * LogicalANDExpressionNoIn:
     *     BitwiseORExpressionNoIn
     *     LogicalANDExpressionNoIn && BitwiseORExpressionNoIn
     *
     * LogicalORExpression:
     *     LogicalANDExpression
     *     LogicalORExpression || LogicalANDExpression
     *
     * LogicalORExpressionNoIn:
     *     LogicalANDExpressionNoIn
     *     LogicalORExpressionNoIn || LogicalANDExpressionNoIn
     */

    /* 11.12 Conditional Operator ( ? : )
     * ----------------------------------
     * ConditionalExpression:
     *     LogicalORExpression
     *     LogicalORExpression ? AssignmentExpression : AssignmentExpression
     *
     * ConditionalExpressionNoIn:
     *     LogicalORExpressionNoIn
     *     LogicalORExpressionNoIn ? AssignmentExpression : AssignmentExpressionNoIn
     */

    /* 11.13 Assignment Operators
     * --------------------------
     * AssignmentExpression:
     *     ConditionalExpression
     *     LeftHandSideExpression = AssignmentExpression
     *     LeftHandSideExpression AssignmentOperator AssignmentExpression
     *
     * AssignmentExpressionNoIn:
     *     ConditionalExpressionNoIn
     *     LeftHandSideExpression = AssignmentExpressionNoIn
     *     LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn
     *
     * AssignmentOperator: one of
     *     *=   /=   %=   +=   -=   <<=   >>=   >>>=   &=   ^=   |=
     */

    /* 11.14 Comma Operator ( , )
     * --------------------------
     * Expression:
     *     AssignmentExpression
     *     Expression , AssignmentExpression
     *
     * ExpressionNoIn:
     *     AssignmentExpressionNoIn
     *     ExpressionNoIn , AssignmentExpressionNoIn
     */

    /* 12. Statements
     * --------------
     * Statement:
     *     Block
     *     VariableStatement
     *     EmptyStatement
     *     ExpressionStatement
     *     IfStatement
     *     IterationStatement
     *     ContinueStatement
     *     BreakStatement
     *     ReturnStatement
     *     WithStatement
     *     LabelledStatement
     *     SwitchStatement
     *     ThrowStatement
     *     TryStatement
     *     DebuggerStatement
     *
     * 12.1 Block
     * ----------
     * Block:
     *     { StatementListOpt }
     *
     * StatementList:
     *     Statement
     *     StatementList Statement
     */

    /* 12.2 Variable Statement
     * -----------------------
     * VariableStatement:
     *     var VariableDeclarationList ;
     *
     * VariableDeclarationList:
     *     VariableDeclaration
     *     VariableDeclarationList , VariableDeclaration
     *
     * VariableDeclarationListNoIn:
     *     VariableDeclarationNoIn
     *     VariableDeclarationListNoIn , VariableDeclarationNoIn
     *
     * VariableDeclaration:
     *     Identifier InitialiserOpt
     *
     * VariableDeclarationNoIn:
     *     Identifier InitialiserNoInOpt
     *
     * Initialiser:
     *     = AssignmentExpression
     *
     * InitialiserNoIn:
     *     = AssignmentExpressionNoIn
     */

    /* 12.3 Empty Statement
     * --------------------
     * EmptyStatement:
     *     ;
     *
     * 12.4 Expression Statement
     * -------------------------
     * ExpressionStatement:
     *     [lookahead not { or function] Expression ;
     *
     * 12.5 The if Statement
     * ---------------------
     * IfStatement:
     *     if ( Expression ) Statement else Statement
     *     if ( Expression ) Statement
     *
     * 12.6 Iteration Statements
     * -------------------------
     * IterationStatement:
     *     do Statement while ( Expression ) ;
     *     while ( Expression ) Statement
     *     for ( ExpressionNoInOpt ; ExpressionOpt ; ExpressionOpt ) Statement
     *     for ( var VariableDeclarationListNoIn ; ExpressionOpt ; ExpressionOpt ) Statement
     *     for ( LeftHandSideExpression in Expression ) Statement
     *     for ( var VariableDeclarationNoIn in Expression ) Statement
     *
     * 12.7 The continue Statement
     * ---------------------------
     * ContinueStatement:
     *     continue ;
     *     continue [no LineTerminator here] Identifier ;
     *
     * 12.8 The break Statement
     * ------------------------
     * BreakStatement:
     *     break ;
     *     break [no LineTerminator here] Identifier ;
     *
     * 12.9 The return Statement
     * -------------------------
     * ReturnStatement:
     *     return ;
     *     return [no LineTerminator here] Expression ;
     *
     * 12.10 The with Statement
     * ------------------------
     * WithStatement:
     *     with ( Expression ) Statement
     *
     * 12.11 The switch Statement
     * --------------------------
     * SwitchStatement:
     *     switch ( Expression ) CaseBlock
     *
     * CaseBlock:
     *     { CaseClausesOpt }
     *     { CaseClausesOpt DefaultClause CaseClausesOpt }
     *
     * CaseClauses:
     *     CaseClause
     *     CaseClauses CaseClause
     *
     * CaseClause:
     *     case Expression : StatementListOpt
     *
     * DefaultClause:
     *     default : StatementListOpt
     *
     * 12.12 Labelled Statements
     * -------------------------
     * LabelledStatement:
     *     Identifier : Statement
     *
     * 12.13 The throw Statement
     * -------------------------
     * ThrowStatement:
     *     throw [no LineTerminator here] Expression ;
     *
     * 12.14 The try Statement
     * -----------------------
     * TryStatement:
     *     try Block Catch
     *     try Block Finally
     *     try Block Catch Finally
     *
     * Catch:
     *     catch ( Identifier ) Block
     *
     * Finally:
     *     finally Block
     *
     * 12.15 The debugger Statement
     * ----------------------------
     * DebuggerStatement:
     *     debugger ;
     */

    /* 13. Function Definition
     * -----------------------
     * FunctionDeclaration:
     *     function Identifier ( FormalParameterListOpt ) { FunctionBody }
     *
     * FunctionExpression:
     *     function IdentifierOpt ( FormalParameterListOpt ) { FunctionBody }
     *
     * FormalParameterList:
     *     Identifier
     *     FormalParameterList , Identifier
     *
     * FunctionBody:
     *     SourceElementsOpt
     */

    /* 14. Program
     * ------------
     * Program:
     *     SourceElementsOpt
     *
     * SourceElements:
     *     SourceElement
     *     SourceElements SourceElement
     *
     * SourceElement:
     *     Statement
     *     FunctionDeclaration
     */
}
