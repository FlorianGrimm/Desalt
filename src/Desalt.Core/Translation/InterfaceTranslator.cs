// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="InterfaceTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    internal static class InterfaceTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsInterfaceDeclaration TranslateInterfaceDeclaration(
            TranslationContext context,
            InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = context.TranslateDeclarationIdentifier(node);

            // Translate the interface body.
            var visitor = new Visitor(context);
            var translatedMembers = node.Members.Select(visitor.Visit).WhereNotNull().ToArray();
            ITsObjectType body = Factory.ObjectType(translatedMembers);

            // Translate the generic type parameters.
            ITsTypeParameters typeParameters = Factory.TypeParameters();

            // Translate the extends clause.
            IEnumerable<ITsTypeReference> extendsClause = Enumerable.Empty<ITsTypeReference>();

            // Create the interface declaration.
            ITsInterfaceDeclaration interfaceDeclaration = Factory.InterfaceDeclaration(
                interfaceName,
                body,
                typeParameters,
                extendsClause);

            // Add documentation comments.
            ITsInterfaceDeclaration final = context.AddDocumentationComment(interfaceDeclaration, node);
            return final;
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class Visitor : BaseTranslationVisitor<MemberDeclarationSyntax, ITsTypeMember?>
        {
            public Visitor(TranslationContext context)
                : base(context)
            {
            }

            /// <summary>
            /// Called when the visitor visits a MethodDeclarationSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="ITsMethodSignature"/>.
            /// </returns>
            public override ITsTypeMember? VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                IMethodSymbol methodSymbol = SemanticModel.GetDeclaredSymbol(node);

                // If the method is decorated with [InlineCode] or [ScriptSkip] then we shouldn't output the declaration.
                if (ScriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) &&
                    (scriptMethodSymbol.InlineCode != null || scriptMethodSymbol.ScriptSkip))
                {
                    return null;
                }

                ITsIdentifier functionName = Context.TranslateDeclarationIdentifier(node);

                // Create the call signature.
                ITsCallSignature callSignature = FunctionTranslator.TranslateCallSignature(
                    Context,
                    node.ParameterList,
                    node.TypeParameterList,
                    node.ReturnType,
                    methodSymbol);

                ITsMethodSignature methodSignature = Factory.MethodSignature(
                    functionName,
                    isOptional: false,
                    callSignature: callSignature);
                methodSignature = Context.AddDocumentationComment(methodSignature, node);

                return methodSignature;
            }

            //// =======================================================================================================
            //// Property Declarations
            //// =======================================================================================================

            /// <summary>
            /// Called when the visitor visits a PropertyDeclarationSyntax node.
            /// </summary>
            /// <returns>
            /// An <see cref="ITsPropertySignature"/> for the get and set methods (for interfaces).
            /// </returns>
            public override ITsTypeMember? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                // If the property is marked with [IntrinsicProperty], don't write out the declaration.
                if (Context.GetExpectedDeclaredScriptSymbol<IScriptPropertySymbol>(node).IntrinsicProperty)
                {
                    return null;
                }

                ITsIdentifier propertyName = Context.TranslateDeclarationIdentifier(node);
                ITypeSymbol typeSymbol = node.Type.GetTypeSymbol(SemanticModel);
                ITsType propertyType = TypeTranslator.TranslateTypeSymbol(Context, typeSymbol, node.Type.GetLocation);

                if (node.AccessorList == null)
                {
                    if (node.ExpressionBody != null)
                    {
                        Diagnostics.Add(
                            DiagnosticFactory.DefaultInterfaceImplementationNotSupported(node.ExpressionBody));
                    }
                    else
                    {
                        Context.ReportInternalError(
                            "A property declaration with a null accessor list should have an expression body.",
                            node);
                    }

                    return null;
                }

                // Property declarations within interfaces don't have a body and are a different translation type
                // (ITsPropertySignature). We only need to create a single property signature for both the getter and setter.
                bool hasSetter = node.AccessorList.Accessors.Any(x => x.Kind() == SyntaxKind.SetAccessorDeclaration);
                ITsPropertySignature propertySignature = Factory.PropertySignature(
                    propertyName,
                    propertyType,
                    isReadOnly: !hasSetter);
                return propertySignature;
            }
        }
    }
}
