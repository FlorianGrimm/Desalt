// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineCodeTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Parses and translates [InlineCode] attribute contents.
    /// </summary>
    internal class InlineCodeTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly IMethodSymbol _symbol;
        private readonly string _inlineCode;
        private readonly ITsExpression _translatedLeftSide;
        private readonly ITsArgumentList _translatedArgumentList;

        private PeekingTextReader _reader;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private InlineCodeTranslator(
            IMethodSymbol symbol,
            string inlineCode,
            ITsExpression translatedLeftSide,
            ITsArgumentList translatedArgumentList)
        {
            _symbol = symbol;
            _inlineCode = inlineCode;
            _translatedLeftSide = translatedLeftSide;
            _translatedArgumentList = translatedArgumentList;
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Attempts to translate the method call by using the specified [InlineCode]. A method call
        /// can be either a constructor, regular method, or a property get/set method. If the inline
        /// code cannot be parsed, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="symbol">The method symbol to translate.</param>
        /// <param name="inlineCode">The inline code to parse and translate.</param>
        /// <param name="translatedLeftSide">
        /// The translated left side of the method call. Used for {this} parameter substitution.
        /// </param>
        /// <param name="translatedArgumentList">
        /// The translated argument list associated with this method.
        /// </param>
        /// <returns>
        /// The translated TypeScript code or null if no translation is possible (an error condition).
        /// </returns>
        public static IAstNode Translate(
            IMethodSymbol symbol,
            string inlineCode,
            ITsExpression translatedLeftSide,
            ITsArgumentList translatedArgumentList)
        {
            var translator = new InlineCodeTranslator(
                symbol,
                inlineCode,
                translatedLeftSide,
                translatedArgumentList);
            return translator.Translate();
        }

        private IAstNode Translate()
        {
            // do a simple string replace for the special parameters
            string replacedInlineCode = _inlineCode.Replace("{$System.Script}", "ss");

            using (_reader = new PeekingTextReader(replacedInlineCode))
            {
                IAstNode translated = TranslateIf('[', TranslateArray);
                translated = translated ?? TranslateCall();
                return translated;
            }
        }

        private IAstNode TranslateIf(char expected, Func<IAstNode> translateFunc)
        {
            _reader.SkipWhitespace();
            return _reader.Peek() == expected ? translateFunc() : null;
        }

        private ITsArrayLiteral TranslateArray()
        {
            Read('[');
            var elements = TranslateParameterList(']').Select(x => Factory.ArrayElement(x));
            return Factory.Array(elements.ToArray());
        }

        private ITsCallExpression TranslateCall()
        {
            string dottedName = _reader.ReadUntil('(');
            Read('(');
            ITsQualifiedName leftSide = Factory.QualifiedName(dottedName);

            var arguments = TranslateParameterList(')').Select(p => Factory.Argument(p));

            return Factory.Call(leftSide, Factory.ArgumentList(arguments.ToArray()));
        }

        private IEnumerable<ITsExpression> TranslateParameterList(char endingChar)
        {
            while (!ReadIf(endingChar))
            {
                if (_reader.Peek() == '{')
                {
                    IEnumerable<ITsExpression> elementList = TranslateParameter();
                    foreach (ITsExpression tsExpression in elementList)
                    {
                        yield return tsExpression;
                    }
                }
                else
                {
                    string paramName = _reader.ReadUntil(',', endingChar);
                    yield return Factory.Identifier(paramName);
                }

                ReadIf(',');
            }
        }

        /// <summary>
        /// Substitutes parameters of the form '{name}' with the translated parameter.
        /// </summary>
        private IEnumerable<ITsExpression> TranslateParameter()
        {
            Read('{');
            string parameterName = _reader.ReadUntil('}');
            Read('}');

            // a parameter of the form '*rest' means to expand the parameter array
            bool expandParams = parameterName[0] == '*';
            parameterName = expandParams ? parameterName.Substring(1) : parameterName;

            // translate {this} parameter to the symbol invoking the method
            if (parameterName == "this")
            {
                if (_translatedLeftSide is ITsMemberDotExpression memberDotExpression)
                {
                    yield return memberDotExpression.LeftSide;
                }
                else
                {
                    yield return _translatedLeftSide;
                }

                yield break;
            }

            // find the index of the parameter in the method
            int parameterIndex = Array.IndexOf(_symbol.Parameters.Select(p => p.Name).ToArray(), parameterName);
            if (parameterIndex < 0)
            {
                throw CreateParseException($"Can't find parameter '{parameterName}'.");
            }

            // a parameter of the form '*rest' means to expand the parameter array
            if (expandParams)
            {
                if (!_symbol.Parameters[parameterIndex].IsParams)
                {
                    throw CreateParseException($"Parameter '{parameterName}' is not a 'params' parameter.");
                }

                foreach (ITsArgument translatedValue in _translatedArgumentList.Arguments.Skip(parameterIndex))
                {
                    yield return translatedValue.Argument;
                }
            }
            else
            {
                // get the translated value of the parameter
                ITsArgument translatedValue = _translatedArgumentList.Arguments[parameterIndex];
                yield return translatedValue.Argument;
            }
        }

        private void Read(char expected)
        {
            int read = _reader.Read();
            if (read != expected)
            {
                throw CreateParseException($"Expected to read '{expected}'.");
            }

            _reader.SkipWhitespace();
        }

        private bool ReadIf(char expected)
        {
            if (_reader.IsAtEnd)
            {
                return true;
            }

            if (_reader.Peek() == expected)
            {
                _reader.Read();
                _reader.SkipWhitespace();
                return true;
            }

            return false;
        }

        private Exception CreateParseException(string message)
        {
            return new InvalidOperationException(
                $"Error parsing inline code '{_inlineCode}' for '{SymbolTableUtils.KeyFromSymbol(_symbol)}': {message}");
        }
    }
}
