// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTokenCode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    /// <summary>
    /// Enumerates the different types of TypeScript tokens.
    /// </summary>
    internal enum TsTokenCode
    {
        /// <summary>
        /// Represents a placeholder token that represents an error.
        /// </summary>
        ErrorPlaceholder = -2,

        /// <summary>
        /// Represents a placeholder token signifying the end of the token stream.
        /// </summary>
        EndOfTokens = -1,

        Identifier,

        /* The following keywords are reserved and cannot be used as an Identifier: */
        Break,
        Case,
        Catch,
        Class,
        Const,
        Continue,
        Debugger,
        Default,
        Delete,
        Do,
        Else,
        Enum,
        Export,
        Extends,
        False,
        Finally,
        For,
        Function,
        If,
        Import,
        In,
        Instanceof,
        New,
        Null,
        Return,
        Super,
        Switch,
        This,
        Throw,
        True,
        Try,
        Typeof,
        Var,
        Void,
        While,
        With,

        /* The following keywords cannot be used as identifiers in strict mode code, but are otherwise not restricted:*/
        Implements,
        Interface,
        Let,
        Package,
        Private,
        Protected,
        Public,
        Static,
        Yield,

        /* The following keywords cannot be used as user defined type names, but are otherwise not restricted: */
        Any,
        Boolean,
        Number,
        String,
        Symbol,

        /* The following keywords have special meaning in certain contexts, but are valid identifiers: */
        Abstract,
        As,
        Async,
        Await,
        Constructor,
        Declare,
        From,
        Get,
        Is,
        Module,
        Namespace,
        Of,
        Require,
        Set,
        Type,

        /* Punctuators */
        LeftBrace,
        RightBrace,
        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        Dot,
        DotDotDot,
        Semicolon,
        Comma,
        LessThan,
        GreaterThan,
        LessThanEquals,
        GreaterThanEquals,
        EqualsEquals,
        ExclamationEquals,
        EqualsEqualsEquals,
        ExclamationEqualsEquals,
        Plus,
        Minus,
        Asterisk,
        Slash,
        Percent,
        PlusPlus,
        MinusMinus,
        LessThanLessThan,
        GreaterThanGreaterThan,
        GreaterThanGreaterThanGreaterThan,
        Ampersand,
        Pipe,
        Caret,
        Exclamation,
        Tilde,
        AmpersandAmpersand,
        PipePipe,
        Question,
        Colon,
        Equals,
        PlusEquals,
        MinusEquals,
        AsteriskEquals,
        SlashEquals,
        PercentEquals,
        LessThanLessThanEquals,
        GreaterThanGreaterThanEquals,
        GreaterThanGreaterThanGreaterThanEquals,
        AmpersandEquals,
        PipeEquals,
        CaretEquals,
        EqualsGreaterThan,

        /* Literals */
        DecimalLiteral,
        BinaryIntegerLiteral,
        OctalIntegerLiteral,
        HexIntegerLiteral,
        StringLiteral,
    }
}
