// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es2015GrammarInterfaces.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Ast.Expressions;

    /***********************************************************************************************
     * Ecma-262 6.0 (ES 2015) Grammar
     * -------------------------------------------------------------------------
     * See http://www.ecma-international.org/ecma-262/6.0/
     **********************************************************************************************/

    /* 11.4 Comments
     * -------------
     * Comment:
     *   MultiLineComment
     *   SingleLineComment
     *
     * MultiLineComment:
     *   slash* MultiLineCommentCharsOpt *slash
     *
     * MultiLineCommentChars:
     *   MultiLineNotAsteriskChar MultiLineCommentCharsOpt
     *   * PostAsteriskCommentCharsOpt
     *
     * PostAsteriskCommentChars:
     *   MultiLineNotForwardSlashOrAsteriskChar MultiLineCommentCharsOpt
     *   *PostAsteriskCommentCharsOpt
     *
     * MultiLineNotAsteriskChar:
     *   SourceCharacter but not *
     *
     * MultiLineNotForwardSlashOrAsteriskChar:
     *   SourceCharacter but not one of / or *
     *
     * SingleLineComment:
     *   // SingleLineCommentCharsOpt
     *
     * SingleLineCommentChars:
     *   SingleLineCommentChar SingleLineCommentCharsOpt
     *
     * SingleLineCommentChar:
     *   SourceCharacter but not LineTerminator
     */

    /// <summary>
    /// Represents whitespace that can appear before or after another <see cref="ITsAstNode"/>.
    /// </summary>
    public interface ITsWhitespaceTrivia : ITsAstTriviaNode
    {
        string Text { get; }
        bool IsNewline { get; }
    }

    /// <summary>
    /// Represents a TypeScript multi-line comment of the form '/* lines */'.
    /// </summary>
    public interface ITsMultiLineComment : ITsAstTriviaNode
    {
        /// <summary>
        /// Indicates whether the comment should start with /** (JsDoc) or /*.
        /// </summary>
        bool IsJsDoc { get; }

        ImmutableArray<string> Lines { get; }
    }

    /// <summary>
    /// Represents a TypeScript single-line comment of the form '// comment'.
    /// </summary>
    public interface ITsSingleLineComment : ITsAstTriviaNode
    {
        string Text { get; }
    }

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

    public interface ITsIdentifier :
        ITsTypeName,
        ITsPropertyDefinition,
        ITsLiteralPropertyName,
        ITsBindingIdentifierOrPattern
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

    public interface ITsThis : ITsExpression
    {
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

    /// <summary>
    /// Represents a parenthesized expression, of the form '(expression)'.
    /// </summary>
    public interface ITsParenthesizedExpression : ITsExpression
    {
        ITsExpression Expression { get; }
    }

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

    public interface ITsNullLiteral : ITsExpression
    {
    }

    public interface ITsBooleanLiteral : ITsExpression
    {
        bool Value { get; }
    }

    public enum TsNumericLiteralKind
    {
        Decimal,
        BinaryInteger,
        OctalInteger,
        HexInteger,
    }

    public interface ITsNumericLiteral : ITsExpression, ITsLiteralPropertyName
    {
        TsNumericLiteralKind Kind { get; }
        double Value { get; }
    }

    public enum StringLiteralQuoteKind
    {
        DoubleQuote,
        SingleQuote
    }

    public interface ITsStringLiteral : ITsExpression, ITsLiteralPropertyName
    {
        StringLiteralQuoteKind QuoteKind { get; }
        string Value { get; }
    }

    /* 11.8.5 - Regular Expression Literal
     * -----------------------------------
     * RegularExpressionLiteral:
     *   / RegularExpressionBody / RegularExpressionFlags
     */

    public interface ITsRegularExpressionLiteral : ITsExpression
    {
        string Body { get; }
        string Flags { get; }
    }

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

    public interface ITsArrayLiteral : ITsExpression
    {
        ImmutableArray<ITsArrayElement> Elements { get; }
    }

    public interface ITsArrayElement : ITsAstNode
    {
        ITsExpression Element { get; }

        /// <summary>
        /// Indicates whether the <see cref="Element"/> is preceded by a spread operator '...'.
        /// </summary>
        bool IsSpreadElement { get; }
    }

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
     *   xx MethodDefinition xx (not in TypeScript)
     *   (see TypeScript extensions for more)
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

    public interface ITsObjectLiteral : ITsExpression
    {
        ImmutableArray<ITsPropertyDefinition> PropertyDefinitions { get; }
    }

    public interface ITsPropertyDefinition : ITsExpression { }

    public interface ITsCoverInitializedName : ITsPropertyDefinition
    {
        ITsIdentifier Identifier { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsPropertyAssignment : ITsPropertyDefinition
    {
        ITsPropertyName PropertyName { get; }
        ITsExpression Initializer { get; }
    }

    public interface ITsPropertyName : ITsAstNode { }

    public interface ITsLiteralPropertyName : ITsPropertyName { }

    public interface ITsComputedPropertyName : ITsPropertyName
    {
        ITsExpression Expression { get; }
    }

    /* 12.2.9 Template Literals (and 11.8.6 Template Literal Lexical Components)
     * -------------------------------------------------------------------------
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
     *
     * NoSubstitutionTemplate:
     *   ` TemplateCharactersOpt `
     *
     * TemplateHead:
     *   ` TemplateCharactersOpt ${
     *
     * TemplateMiddle:
     *   } TemplateCharactersOpt ${
     *
     * TemplateTail:
     *   } TemplateCharactersOpt `
     *
     * TemplateCharacters:
     *   TemplateCharacter TemplateCharactersOpt
     *
     * TemplateCharacter:
     *   $ [lookahead != {]
     *   \ EscapeSequence
     *   LineContinuation
     *   LineTerminatorSequence
     *   SourceCharacter but not one of ` or \ or $ or LineTerminator
     */

    public interface ITsTemplatePart : ITsAstNode
    {
        string Template { get; }
        ITsExpression Expression { get; }
    }

    public interface ITsTemplateLiteral : ITsExpression
    {
        ImmutableArray<ITsTemplatePart> Parts { get; }
    }

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
     */

    public interface ITsMemberBracketExpression : ITsExpression
    {
        ITsExpression LeftSide { get; }
        ITsExpression BracketContents { get; }
    }

    public interface ITsMemberDotExpression : ITsExpression
    {
        ITsExpression LeftSide { get; }
        string DotName { get; }
    }

    /* SuperProperty:
     *   super [ Expression ]
     *   super . IdentifierName
     */

    public interface ITsSuperBracketExpression : ITsExpression
    {
        ITsExpression BracketContents { get; }
    }

    public interface ITsSuperDotExpression : ITsExpression
    {
        string DotName { get; }
    }

    /* MetaProperty:
     *   NewTarget
     *
     * NewTarget:
     *   new . target
     */

    public interface ITsNewTargetExpression : ITsExpression
    {
    }

    /* NewExpression:
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
     * Arguments: (see TypeScript grammar copied below)
     *   ( )
     *   ( ArgumentList )
     *
     * Arguments: ( Modified )
     *   TypeArgumentsOpt ( ArgumentListOpt )
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

    public interface ITsCallExpression : ITsExpression
    {
        ITsExpression LeftSide { get; }
        ITsArgumentList Arguments { get; }
    }

    public interface ITsNewCallExpression : ITsCallExpression { }

    public interface ITsSuperCallExpression : ITsExpression
    {
        ITsArgumentList Arguments { get; }
    }

    public interface ITsArgumentList : ITsAstNode
    {
        ImmutableArray<ITsType> TypeArguments { get; }
        ImmutableArray<ITsArgument> Arguments { get; }
    }

    public interface ITsArgument : ITsAstNode
    {
        ITsExpression Argument { get; }

        /// <summary>
        /// Indicates whether the <see cref="Argument"/> is preceded by a spread operator '...'.
        /// </summary>
        bool IsSpreadArgument { get; }
    }

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
     *   < Type > UnaryExpression (from TypeScript grammar)
     */

    public interface ITsUnaryExpression : ITsExpression
    {
        ITsExpression Operand { get; }
        TsUnaryOperator Operator { get; }
    }

    /// <summary>
    /// Represents a unary cast expression of the form, '&lt;Type&gt;.
    /// </summary>
    public interface ITsCastExpression : ITsUnaryExpression
    {
        ITsType CastType { get; }
    }

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

    public interface ITsBinaryExpression : ITsExpression
    {
        ITsExpression LeftSide { get; }
        TsBinaryOperator Operator { get; }
        ITsExpression RightSide { get; }
    }

    /* 12.13 Conditional Operator ( ? : )
     * ----------------------------------
     * ConditionalExpression:
     *   LogicalORExpression
     *   LogicalORExpression ? AssignmentExpression : AssignmentExpression
     */

    public interface ITsConditionalExpression : ITsExpression
    {
        ITsExpression Condition { get; }
        ITsExpression WhenTrue { get; }
        ITsExpression WhenFalse { get; }
    }

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

    public interface ITsAssignmentExpression : ITsExpression
    {
        ITsExpression LeftSide { get; }
        ITsExpression RightSide { get; }
        TsAssignmentOperator Operator { get; }
    }

    /* 12.15 Comma Operator ( , )
     * --------------------------
     * Expression:
     *   AssignmentExpression
     *   Expression , AssignmentExpression
     */

    public interface ITsExpression : ITsAstNode
    {
    }

    public interface ITsCommaExpression : ITsExpression
    {
        ImmutableArray<ITsExpression> Expressions { get; }
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
     */

    public interface ITsStatement : ITsStatementListItem, ITsNamespaceElement, ITsImplementationElement { }

    /* Declaration:
     *   HoistableDeclaration
     *   ClassDeclaration
     *   LexicalDeclaration
     *   (see more in TypeScript grammar)
     *
     * HoistableDeclaration:
     *   FunctionDeclaration
     *   GeneratorDeclaration
     *
     * BreakableStatement:
     *   IterationStatement
     *   SwitchStatement
     */

    public interface ITsDeclaration : ITsStatementListItem, ITsNamespaceElement, ITsImplementationElement { }

    /* 13.2 Block
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

    public interface ITsBlockStatement : ITsStatement
    {
        ImmutableArray<ITsStatementListItem> Statements { get; }
    }

    public interface ITsStatementListItem : ITsAstNode { }

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
     * LexicalBinding: (see TypeScript grammar)
     *   BindingIdentifier InitializerOpt
     *   BindingPattern Initializer
     */

    public interface ITsLexicalDeclaration : ITsDeclaration
    {
        bool IsConst { get; }
        ImmutableArray<ITsLexicalBinding> Declarations { get; }
    }

    /* 13.3.2 Variable Statement
     * -------------------------
     * VariableStatement:
     *   var VariableDeclarationList ;
     *
     * VariableDeclarationList:
     *   VariableDeclaration
     *   VariableDeclarationList , VariableDeclaration
     *
     * VariableDeclaration: (see TypeScript definition)
     *   BindingIdentifier InitializerOpt
     *   BindingPattern Initializer
     */

    public interface ITsVariableStatement : ITsStatement
    {
        ImmutableArray<ITsVariableDeclaration> Declarations { get; }
    }

    /* 13.3.3 Destructuring Binding Patterns
     * -------------------------------------
     * BindingPattern:
     *   ObjectBindingPattern
     *   ArrayBindingPattern
     */

    public interface ITsBindingPattern : ITsBindingIdentifierOrPattern { }

    /* ObjectBindingPattern:
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
     *   BindingIdentifier InitializerOpt
     *
     * BindingRestElement:
     *   ... BindingIdentifier
     */

    public interface ITsObjectBindingPattern : ITsBindingPattern
    {
        ImmutableArray<ITsBindingProperty> Properties { get; }
    }

    public interface ITsArrayBindingPattern : ITsBindingPattern
    {
        ImmutableArray<ITsBindingElement> Elements { get; }
        ITsIdentifier RestElement { get; }
    }

    public interface ITsBindingProperty : ITsAstNode { }

    public interface ITsSingleNameBinding : ITsBindingProperty, ITsBindingElement
    {
        ITsIdentifier Name { get; }
        ITsExpression DefaultValue { get; }
    }

    public interface ITsPropertyNameBinding : ITsBindingProperty
    {
        ITsPropertyName PropertyName { get; }
        ITsBindingElement BindingElement { get; }
    }

    public interface ITsBindingElement : ITsAstNode { }

    public interface ITsPatternBinding : ITsBindingElement
    {
        ITsBindingPattern BindingPattern { get; }
        ITsExpression Initializer { get; }
    }

    /* 13.4 Empty Statement
     * --------------------
     * EmptyStatement:
     *   ;
     */

    public interface ITsEmptyStatement : ITsStatement { }

    /* 13.5 Expression Statement
     * -------------------------
     * ExpressionStatement:
     *   [lookahead not { {, function, class, let [ }] Expression ;
     */

    public interface ITsExpressionStatement : ITsStatement
    {
        ITsExpression Expression { get; }
    }

    /* 13.6 The if Statement
     * ---------------------
     * IfStatement:
     *   if ( Expression ) Statement else Statement
     *   if ( Expression ) Statement
     */

    public interface ITsIfStatement : ITsStatement
    {
        ITsExpression IfCondition { get; }
        ITsStatement IfStatement { get; }
        ITsStatement ElseStatement { get; }
    }

    /* 13.7 Iteration Statements
     * -------------------------
     * IterationStatement:
     *   do Statement while ( Expression ) ;
     */

    public interface ITsDoWhileStatement : ITsStatement
    {
        ITsStatement DoStatement { get; }
        ITsExpression WhileCondition { get; }
    }

    /*   while ( Expression ) Statement
     */

    public interface ITsWhileStatement : ITsStatement
    {
        ITsExpression WhileCondition { get; }
        ITsStatement WhileStatement { get; }
    }

    /*   for ( [lookahead not 'let ['] ExpressionOpt ; ExpressionOpt ; ExpressionOpt ) Statement
     *   for ( var VariableDeclarationList ; ExpressionOpt ; ExpressionOpt ) Statement
     *   for ( LexicalDeclaration ExpressionOpt ; ExpressionOpt ) Statement
     */

    public interface ITsForStatement : ITsStatement
    {
        ITsExpression Initializer { get; }
        ImmutableArray<ITsVariableDeclaration> InitializerWithVariableDeclarations { get; }
        ITsLexicalDeclaration InitializerWithLexicalDeclaration { get; }

        ITsExpression Condition { get; }
        ITsExpression Incrementor { get; }
        ITsStatement Statement { get; }
    }

    /*   for ( [lookahead not 'let ['] LeftHandSideExpression in Expression ) Statement
     *   for ( var ForBinding in Expression ) Statement
     *   for ( ForDeclaration in Expression ) Statement
     */

    public interface ITsForInStatement : ITsStatement
    {
        ITsExpression Initializer { get; }
        VariableDeclarationKind? DeclarationKind { get; }
        ITsBindingIdentifierOrPattern Declaration { get; }

        ITsExpression RightSide { get; }
        ITsStatement Statement { get; }
    }

    /*   for ( [lookahead not 'let'] LeftHandSideExpression of AssignmentExpression ) Statement
     *   for ( var ForBinding of AssignmentExpression ) Statement
     *   for ( ForDeclaration of AssignmentExpression ) Statement
     */

    public interface ITsForOfStatement : ITsStatement
    {
        ITsExpression Initializer { get; }
        VariableDeclarationKind? DeclarationKind { get; }
        ITsBindingIdentifierOrPattern Declaration { get; }

        ITsExpression RightSide { get; }
        ITsStatement Statement { get; }
    }

    /* ForDeclaration:
     *   LetOrConst ForBinding
     *
     * ForBinding:
     *   BindingIdentifier
     *   BindingPattern
     */

    public enum VariableDeclarationKind { Var, Let, Const }

    /* 13.8 The continue Statement
     * ---------------------------
     * ContinueStatement:
     *   continue ;
     *   continue [no LineTerminator here] LabelIdentifier ;
     */

    public interface ITsContinueStatement : ITsStatement
    {
        ITsIdentifier Label { get; }
    }

    /* 13.9 The break Statement
     * ------------------------
     * BreakStatement:
     *   break ;
     *   break [no LineTerminator here] LabelIdentifier ;
     */

    public interface ITsBreakStatement : ITsStatement
    {
        ITsIdentifier Label { get; }
    }

    /* 13.10 The return Statement
     * --------------------------
     * ReturnStatement:
     *   return ;
     *   return [no LineTerminator here] Expression ;
     */

    public interface ITsReturnStatement : ITsStatement
    {
        ITsExpression Expression { get; }
    }

    /* 13.11 The with Statement
     * ------------------------
     * WithStatement:
     *   with ( Expression ) Statement
     */

    public interface ITsWithStatement : ITsStatement
    {
        ITsExpression Expression { get; }
        ITsStatement Statement { get; }
    }

    /* 13.12 The switch Statement
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
     */

    public interface ITsSwitchStatement : ITsStatement
    {
        ITsExpression Condition { get; }
        ImmutableArray<ITsCaseOrDefaultClause> Clauses { get; }
    }

    public interface ITsCaseOrDefaultClause : ITsAstNode
    {
        ImmutableArray<ITsStatementListItem> Statements { get; }
    }

    public interface ITsCaseClause : ITsCaseOrDefaultClause
    {
        ITsExpression Expression { get; }
    }

    public interface ITsDefaultClause : ITsCaseOrDefaultClause
    {
    }

    /* 13.13 Labelled Statements
     * -------------------------
     * LabelledStatement:
     *   LabelIdentifier : LabelledItem
     *
     * LabelledItem:
     *   Statement
     *   FunctionDeclaration
     */

    public interface ITsLabelledStatement : ITsStatement
    {
        ITsIdentifier Label { get; }
        ITsStatement Statement { get; }
        ITsFunctionDeclaration FunctionDeclaration { get; }
    }

    /* 13.14 The throw Statement
     * -------------------------
     * ThrowStatement:
     *   throw [no LineTerminator here] Expression ;
     */

    public interface ITsThrowStatement : ITsStatement
    {
        ITsExpression Expression { get; }
    }

    /* 12.14 The try Statement
     * -----------------------
     * TryStatement:
     *   try Block Catch
     *   try Block Finally
     *   try Block Catch Finally
     *
     * Catch:
     *   catch Block
     *   catch ( CatchParameter ) Block
     *
     * Finally:
     *   finally Block
     *
     * CatchParameter:
     *   BindingIdentifier
     *   BindingPattern
     */

    public interface ITsTryStatement : ITsStatement
    {
        ITsBlockStatement TryBlock { get; }
        ITsBindingIdentifierOrPattern CatchParameter { get; }
        ITsBlockStatement CatchBlock { get; }
        ITsBlockStatement FinallyBlock { get; }
    }

    /* 13.16 The debugger Statement
     * ----------------------------
     * DebuggerStatement:
     *   debugger ;
     */

    public interface ITsDebuggerStatement : ITsStatement
    {
    }

    /* A.4 Functions and Classes
     *
     * 14.1 Function Definitions
     * ------------------------
     * FunctionDeclaration: (see TypeScript Grammar)
     *   function BindingIdentifier ( FormalParameters ) { FunctionBody }
     *   function ( FormalParameters ) { FunctionBody }
     *
     * FunctionExpression: (see TypeScript Grammar)
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
     * When the production `ArrowParameters: CoverParenthesizedExpressionAndArrowParameterList`
     * is recognized the following grammar is used to refine the interpretation of
     * CoverParenthesizedExpressionAndArrowParameterList:
     *
     * ArrowFormalParameters: (see TypeScript grammar copied below)
     *   ( StrictFormalParameters )
     *
     * ArrowFormalParameters: ( Modified )
     *   CallSignature
     */

    public interface ITsArrowFunction : ITsExpression
    {
        ITsIdentifier SingleParameterName { get; }
        ITsCallSignature CallSignature { get; }

        ITsExpression BodyExpression { get; }
        ImmutableArray<ITsStatementListItem> Body { get; }
    }

    /* 14.3 Method Definitions
     * -----------------------
     * MethodDefinition: (not present in TypeScript grammar)
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
     * GeneratorMethod: (not in TypeScript grammar)
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
     * ClassDeclaration: (see TypeScript override)
     *   class BindingIdentifier ClassTail
     *   class ClassTail
     *
     * ClassExpression:
     *   class BindingIdentifierOpt ClassTail
     *
     * ClassTail:
     *   ClassHeritageOpt { ClassBodyOpt }
     *
     * ClassHeritage: (see TypeScript override)
     *   extends LeftHandSideExpression
     *
     * ClassBody:
     *   ClassElementList
     *
     * ClassElementList:
     *   ClassElement
     *   ClassElementList ClassElement
     *
     * ClassElement: (see TypeScript override)
     *   MethodDefinition
     *   static MethodDefinition
     *   ;
     */

    public interface ITsClassExpression : ITsExpression
    {
        ITsIdentifier ClassName { get; }
        ITsClassHeritage Heritage { get; }
        ImmutableArray<ITsClassElement> ClassBody { get; }
    }

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
     */

    /* 15.2.2 Imports
     * --------------
     * ImportDeclaration:
     *   import ImportClause FromClause ;
     *   import ModuleSpecifier ;
     */

    public interface ITsImportDeclaration : ITsImplementationModuleElement
    {
        ITsImportClause ImportClause { get; }
        ITsFromClause FromClause { get; }
        ITsStringLiteral Module { get; }
    }

    /* ImportClause:
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
     */

    public interface ITsImportClause : ITsAstNode
    {
        ITsIdentifier DefaultBinding { get; }
        ITsIdentifier NamespaceBinding { get; }
        ImmutableArray<ITsImportSpecifier>? NamedImports { get; }
    }

    /* FromClause:
     *   from ModuleSpecifier
     */

    public interface ITsFromClause : ITsAstNode
    {
        ITsStringLiteral Module { get; }
    }

    /* ImportsList:
     *   ImportSpecifier
     *   ImportsList , ImportSpecifier
     *
     * ImportSpecifier:
     *   ImportedBinding
     *   IdentifierName as ImportedBinding
     */

    public interface ITsImportSpecifier : ITsAstNode
    {
        ITsIdentifier Name { get; }
        ITsIdentifier AsName { get; }
    }

    /* ModuleSpecifier:
     *   StringLiteral
     *
     * ImportedBinding:
     *   BindingIdentifier
     */

    /* 15.2.3 Exports
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
     */

    /* ExportClause:
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
