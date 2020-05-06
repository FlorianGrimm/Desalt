// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Linq;
    using Desalt.Core.SymbolTables;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates C# declarations (classes, interfaces, enums, etc.) into the equivalent TypeScript.
    /// </summary>
    internal static partial class EnumTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsEnumDeclaration TranslateEnumDeclaration(TranslationContext context, EnumDeclarationSyntax node)
        {
            ITsIdentifier enumName = context.TranslateDeclarationIdentifier(node);

            // Make the enum const if [NamedValues] is present.
            bool isConst = context.GetExpectedDeclaredScriptSymbol<IScriptEnumSymbol>(node).NamedValues;

            // Translate the enum body.
            var enumMembers = node.Members.Select(x => TranslateEnumMemberDeclaration(context, x));
            ITsEnumDeclaration enumDeclaration = Factory.EnumDeclaration(enumName, enumMembers, isConst);

            // Add documentation comments.
            enumDeclaration = context.AddDocumentationComment(enumDeclaration, node);
            return enumDeclaration;
        }

        /// <summary>
        /// Called when the visitor visits a EnumMemberDeclarationSyntax node.
        /// </summary>
        private static ITsEnumMember TranslateEnumMemberDeclaration(
            TranslationContext context,
            EnumMemberDeclarationSyntax node)
        {
            ITsIdentifier scriptName = context.TranslateDeclarationIdentifier(node);

            // Get the explicitly defined value if present.
            ITsExpression? value = null;
            if (node.EqualsValue != null)
            {
                var valueTranslation = ExpressionTranslator.Translate(context, node.EqualsValue.Value);
                if (valueTranslation.AdditionalStatementsRequiredBeforeExpression.Any())
                {
                    context.ReportInternalError(
                        "An enum value must be a constant, so cannot have additional statements",
                        node.EqualsValue.Value);
                }

                value = valueTranslation.Expression;
            }

            // Ignore the value if the enum is [NamedValues] and generate our own value.
            bool isNamedValues = context.GetExpectedDeclaredScriptSymbol<IScriptEnumSymbol>(node.Parent).NamedValues;
            if (isNamedValues)
            {
                var fieldSymbol = context.GetExpectedDeclaredSymbol<IFieldSymbol>(node);
                string defaultFieldName = ScriptNamer.DetermineEnumFieldDefaultScriptName(fieldSymbol);
                value = Factory.String(defaultFieldName);
            }

            ITsEnumMember translatedMember = Factory.EnumMember(scriptName, value);
            return translatedMember;
        }
    }
}
