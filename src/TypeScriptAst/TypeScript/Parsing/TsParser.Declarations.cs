// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Parsing
{
    using CompilerUtilities.Extensions;
    using TypeScriptAst.TypeScript.Ast;
    using Factory = Ast.TsAstFactory;

    public partial class TsParser
    {
        /// <summary>
        /// Parses a TypeScript declaration.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Declaration:
        ///   HoistableDeclaration      (starts with 'function')
        ///   ClassDeclaration          (starts with 'class')
        ///   LexicalDeclaration        (starts with 'const' or 'let')
        ///   InterfaceDeclaration      (starts with 'interface')
        ///   TypeAliasDeclaration      (starts with 'type')
        ///   EnumDeclaration           (starts with 'enum')
        ///
        /// HoistableDeclaration:
        ///   FunctionDeclaration       (starts with 'function')
        ///   GeneratorDeclaration      (starts with 'function')
        /// ]]></code></remarks>
        public ITsDeclaration ParseDeclaration()
        {
            switch (_reader.Peek().TokenCode)
            {
                case TsTokenCode.Function:
                    return ParseHoistableDeclaration();

                default:
                    throw new TsParserException(
                        $"Unknown token in declaration: {_reader.Peek()}",
                        _reader.Peek().Location);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the token code is a potential start of the Declaration production.
        /// </summary>
        public bool IsStartOfDeclaration() =>
            _reader.Peek()
                .TokenCode.IsOneOf(
                    TsTokenCode.Function,
                    TsTokenCode.Class,
                    TsTokenCode.Const,
                    TsTokenCode.Let,
                    TsTokenCode.Interface,
                    TsTokenCode.Type,
                    TsTokenCode.Enum);

        /// <summary>
        /// Parses a hoistable declaration, which is either a function or generator function declaration.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// HoistableDeclaration:
        ///   FunctionDeclaration       (starts with 'function')
        ///   GeneratorDeclaration      (starts with 'function *')
        /// ]]></code></remarks>
        public ITsDeclaration ParseHoistableDeclaration()
        {
            if (_reader.IsNext(TsTokenCode.Function, TsTokenCode.Asterisk))
            {
                throw new TsParserException("Generator functions are not yet supported", _reader.Peek().Location);
            }

            return ParseFunctionDeclaration();
        }

        /// <summary>
        /// Parses a function declaration.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// FunctionDeclaration: (Modified)
        ///   function BindingIdentifierOpt CallSignature { FunctionBody }
        ///   function BindingIdentifierOpt CallSignature ;
        /// ]]></code></remarks>
        public ITsFunctionDeclaration ParseFunctionDeclaration()
        {
            Read(TsTokenCode.Function);
            TryParseIdentifier(out ITsIdentifier functionName);
            ITsCallSignature callSignature = ParseCallSignature();

            ITsStatementListItem[] functionBody = null;
            if (_reader.IsNext(TsTokenCode.LeftBrace))
            {
                functionBody = ParseFunctionBody(withBraces: true);
            }
            else
            {
                Read(TsTokenCode.Semicolon);
            }

            return Factory.FunctionDeclaration(functionName, callSignature, functionBody);
        }
    }
}
