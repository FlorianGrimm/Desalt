// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Ast.Statements;

    /// <summary>
    /// Takes an <see cref="IEs5AstNode"/> and converts it to text.
    /// </summary>
    public partial class Es5Emitter : Es5Visitor, IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly Emitter<IEs5AstNode> _emitter;
        private readonly EmitOptions _options;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public Es5Emitter(Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            _emitter = new Emitter<IEs5AstNode>(outputStream, encoding, options);
            _options = options;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Encoding Encoding => _emitter.Encoding;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Dispose()
        {
            _emitter.Dispose();
        }

        public override void VisitProgram(Es5Program node)
        {
            foreach (IEs5SourceElement sourceElement in node.SourceElements)
            {
                Visit(sourceElement);
            }
        }

        private void WriteBlock(Es5BlockStatement blockStatement)
        {
            _emitter.WriteBlock(blockStatement.Statements, elem => elem.Accept(this));
        }

        /// <summary>
        /// Writes a list of elements using a comma between elements. The comma is not written on the
        /// last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        private void WriteCommaList(IEnumerable<IEs5AstNode> elements)
        {
            _emitter.WriteList(elements, _options.SpaceAfterComma ? ", " : ",", elem => elem.Accept(this));
        }

        /// <summary>
        /// Writes a function declaration of the form 'keyword name(params) { body }'.
        /// </summary>
        /// <param name="functionKeyword">
        /// Typically one of the following: 'function', 'get', or 'set'.
        /// </param>
        /// <param name="functionName">The name of the function. Can be null.</param>
        /// <param name="parameters">The function parameters. Can be null or empty.</param>
        /// <param name="functionBody">The function body. Can be null or empty.</param>
        private void WriteFunction(
            string functionKeyword,
            string functionName,
            IEnumerable<Es5Identifier> parameters,
            IEnumerable<IEs5SourceElement> functionBody)
        {
            bool isUnnamed = string.IsNullOrEmpty(functionName);

            // write the keyword, which is typically one of the following: 'function', 'get', or 'set
            _emitter.Write(functionKeyword);

            // if there's a function name, write it
            if (isUnnamed)
            {
                _emitter.Write(_options.SpaceBeforeAnonymousFunctionDeclarationParentheses ? " (" : "(");
            }
            else
            {
                _emitter.Write(" " + functionName);
                _emitter.Write(_options.SpaceBeforeNamedFunctionDeclarationParentheses ? " (" : "(");
            }

            // write the parameters
            WriteCommaList(parameters ?? Enumerable.Empty<IEs5AstNode>());

            // write the closing parenthesis
            bool spaceAfter = isUnnamed
                ? _options.SpaceAfterAnonymousFunctionDeclarationParentheses
                : _options.SpaceAfterNamedFunctionDeclarationParentheses;

            _emitter.Write(spaceAfter ? ") " : ")");

            // write the function body
            _emitter.WriteBlock(
                functionBody ?? Enumerable.Empty<IEs5SourceElement>(),
                elem => elem.Accept(this),
                isFunctionBlock: true);
        }

        /// <summary>
        /// Writes a statement of the form 'var x, y = 0'.
        /// </summary>
        /// <param name="declarations">The declaratios to write.</param>
        private void WriteVariableDeclarations(IEnumerable<Es5VariableDeclaration> declarations)
        {
            _emitter.Write("var ");
            _emitter.WriteCommaList(declarations, (Es5VariableDeclaration d) =>
            {
                Visit(d.Identifier);

                if (d.Initializer == null)
                {
                    return;
                }

                _emitter.Write(_options.SurroundOperatorsWithSpaces ? " = " : "=");
                Visit(d.Initializer);
            });
        }
    }
}
