// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Visits a C# syntax tree, translating from a C# AST into a TypeScript AST.
    /// </summary>
    internal sealed partial class TranslationVisitor : CSharpSyntaxVisitor<IEnumerable<IAstNode>>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly DiagnosticList _diagnostics;
        private readonly DocumentTranslationContextWithSymbolTables _context;
        private readonly SemanticModel _semanticModel;
        private readonly ISet<string> _typesToImport = new HashSet<string>();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TranslationVisitor(DocumentTranslationContextWithSymbolTables context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _semanticModel = context.SemanticModel;
            _diagnostics = DiagnosticList.Create(context.Options);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics.AsEnumerable();

        public IEnumerable<string> TypesToImport => _typesToImport.AsEnumerable();

        //// ===========================================================================================================
        //// Visit Methods
        //// ===========================================================================================================

        public override IEnumerable<IAstNode> DefaultVisit(SyntaxNode node)
        {
            var diagnostic = DiagnosticFactory.TranslationNotSupported(node);
            _diagnostics.Add(diagnostic);

#if DEBUG

            // throwing an exception lets us fail fast and see the problem in the unit test failure window
            throw new Exception(diagnostic.ToString());
#else
            return Enumerable.Empty<IAstNode>();
#endif
        }

        /// <summary>
        /// Called when the visitor visits a CompilationUnitSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsImplementationModule"/>.</returns>
        public override IEnumerable<IAstNode> VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var elements = node.Members.SelectMany(Visit).Cast<ITsImplementationModuleElement>();
            ITsImplementationModule implementationScript = Factory.ImplementationModule(elements.ToArray());

            return implementationScript.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterListSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsParameterList"/>.</returns>
        public override IEnumerable<IAstNode> VisitParameterList(ParameterListSyntax node)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter restParameter = null;

            foreach (ParameterSyntax parameterNode in node.Parameters)
            {
                IAstNode parameter = Visit(parameterNode).Single();
                if (parameter is ITsRequiredParameter requiredParameter)
                {
                    requiredParameters.Add(requiredParameter);
                }
                else
                {
                    optionalParameters.Add((ITsOptionalParameter)parameter);
                }
            }

            // ReSharper disable once ExpressionIsAlwaysNull
            ITsParameterList parameterList = Factory.ParameterList(
                requiredParameters,
                optionalParameters,
                restParameter);
            return parameterList.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterSyntax node.
        /// </summary>
        /// <returns>Either a <see cref="ITsBoundRequiredParameter"/> or a <see cref="ITsBoundOptionalParameter"/>.</returns>
        public override IEnumerable<IAstNode> VisitParameter(ParameterSyntax node)
        {
            ITsIdentifier parameterName = Factory.Identifier(node.Identifier.Text);
            ITsType parameterType = TypeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_semanticModel),
                _typesToImport);
            IAstNode parameter;

            if (node.Default == null)
            {
                parameter = Factory.BoundRequiredParameter(parameterName, parameterType);
            }
            else
            {
                var initializer = (ITsExpression)Visit(node.Default).Single();
                parameter = Factory.BoundOptionalParameter(parameterName, initializer, parameterType);
            }

            return parameter.ToSingleEnumerable();
        }

        private ITsIdentifier TranslateIdentifier(SyntaxNode node)
        {
            string identifier;
            switch (node)
            {
                case BaseTypeDeclarationSyntax baseTypeDeclaration:
                    identifier = baseTypeDeclaration.Identifier.Text;
                    break;

                case MethodDeclarationSyntax methodDeclaration:
                    identifier = methodDeclaration.Identifier.Text;
                    break;

                case EnumMemberDeclarationSyntax enumMemberDeclaration:
                    identifier = enumMemberDeclaration.Identifier.Text;
                    break;

                case VariableDeclaratorSyntax variableDeclarator:
                    identifier = variableDeclarator.Identifier.Text;
                    break;

                case PropertyDeclarationSyntax propertyDeclaration:
                    identifier = propertyDeclaration.Identifier.Text;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported node type for retrieving an identifier: {node.GetType().Name}");
            }

            ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            string scriptName = _context.ScriptNameSymbolTable[symbol];
            return Factory.Identifier(scriptName);
        }
    }
}
