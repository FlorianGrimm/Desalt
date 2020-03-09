// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNamer.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Determines the compiled name for a C# symbol.
    /// </summary>
    internal class ScriptNamer : IScriptNamer
    {
        private readonly IAssemblySymbol _mscorlibAssemblySymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptNamer"/> class.
        /// </summary>
        /// <param name="mscorlibAssemblySymbol">The mscorlib assembly.</param>
        /// <param name="renameRules">Options controlling the way certain symbols are renamed.</param>
        /// <returns>The name the specified symbol should have in the generated script.</returns>
        public ScriptNamer(IAssemblySymbol mscorlibAssemblySymbol, RenameRules renameRules = null)
        {
            _mscorlibAssemblySymbol =
                mscorlibAssemblySymbol ?? throw new ArgumentNullException(nameof(mscorlibAssemblySymbol));

            RenameRules = renameRules ?? RenameRules.Default;
        }

        public RenameRules RenameRules { get; }

        /// <summary>
        /// Determines the name a symbol should have in the generated script.
        /// </summary>
        /// <param name="symbol">The symbol for which to discover the script name.</param>
        public string DetermineScriptNameForSymbol(ISymbol symbol)
        {
            string scriptName;

            switch (symbol)
            {
                case ITypeSymbol typeSymbol:
                    scriptName = DetermineTypeScriptName(typeSymbol);
                    break;

                case IFieldSymbol fieldSymbol when fieldSymbol.ContainingType.TypeKind == TypeKind.Enum:
                    scriptName = DetermineEnumFieldScriptName(fieldSymbol);
                    break;

                case IFieldSymbol fieldSymbol:
                    scriptName = DetermineFieldScriptName(fieldSymbol);
                    break;

                case IMethodSymbol methodSymbol:
                    scriptName = DetermineMethodScriptName(methodSymbol);
                    break;

                default:
                    scriptName = DetermineScriptNameFromAttributes(symbol) ?? ToCamelCase(symbol.Name);
                    break;
            }

            return scriptName;
        }

        public static string ToCamelCase(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        /// <summary>
        /// Attempts to find the script name for the specified symbol based on attributes defined on
        /// the symbol, containing type, or containing namespace.
        /// </summary>
        /// <param name="symbol">The symbol to determine naming for.</param>
        /// <returns>
        /// The name to use in the generated script for the symbol, or null if there are no
        /// attributes that control the naming.
        /// </returns>
        private static string DetermineScriptNameFromAttributes(ISymbol symbol)
        {
            // Use the following precedence if there are multiple attributes:
            // [ScriptAlias]
            // [ScriptName]
            // [PreserveCase]
            // [PreserveMemberCase]

            // use [ScriptAlias] if available
            string scriptAlias = symbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptAlias);
            if (scriptAlias != null)
            {
                return scriptAlias;
            }

            // use [ScriptName] if available (even if there's also a [PreserveCase])
            string scriptName = symbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptName);
            if (scriptName != null)
            {
                return scriptName;
            }

            // for [PreserveCase], don't touch the original name as given in the C# code
            if (symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveCase))
            {
                return symbol.Name;
            }

            // see if there's a [PreserveMemberCase] on the containing type
            if (symbol.ContainingType?.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase) == true)
            {
                return symbol.Name;
            }

            // see if there's a [PreserveMemberCase] on the containing assembly
            if (symbol.ContainingAssembly?.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase) == true)
            {
                return symbol.Name;
            }

            return null;
        }

        /// <summary>
        /// Determines the name that a type should have in the generated code.
        /// </summary>
        /// <param name="typeSymbol">The type for which to determine the script name.</param>
        /// <returns>The name the type should have in the generated code.</returns>
        private string DetermineTypeScriptName(ITypeSymbol typeSymbol)
        {
            // if this is a native type (bool, string, etc.) use the special name
            if (TypeTranslator.TranslatesToNativeTypeScriptType(typeSymbol))
            {
                return TypeTranslator.GetNativeTypeScriptTypeName(typeSymbol);
            }

            string baseName = DetermineScriptNameFromAttributes(typeSymbol) ?? typeSymbol.Name;
            string scriptName = baseName;

            // if this is a type that belongs to mscorlib, prefix it with 'ss'
            if (SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, _mscorlibAssemblySymbol))
            {
                // find the script namespace by searching on the type first, then the assembly
                string scriptNamespace =
                    typeSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptNamespace) ??
                    _mscorlibAssemblySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptNamespace);
                if (scriptNamespace != null)
                {
                    scriptName = $"{scriptNamespace}.{baseName}";
                }
            }

            return scriptName;
        }

        /// <summary>
        /// Determines the name that an enum field should have in the generated code without taking
        /// an <see cref="EnumRenameRule"/> into account.
        /// </summary>
        /// <param name="enumFieldSymbol">The enum field for which to determine the script name.</param>
        /// <returns>The name the enum field should have in the generated code.</returns>
        public static string DetermineEnumFieldDefaultScriptName(IFieldSymbol enumFieldSymbol) =>
            DetermineScriptNameFromAttributes(enumFieldSymbol) ?? ToCamelCase(enumFieldSymbol.Name);

        /// <summary>
        /// Determines the name that an enum field should have in the generated code.
        /// </summary>
        /// <param name="enumFieldSymbol">The enum field for which to determine the script name.</param>
        /// <returns>The name the enum field should have in the generated code.</returns>
        private string DetermineEnumFieldScriptName(IFieldSymbol enumFieldSymbol)
        {
            string fieldName = enumFieldSymbol.Name;

            // Determine how the field should be named using this algorithm:
            // 1) If there is a [NamedValues] attribute on the parent enum declaration, then use the
            //    naming rules from the compiler options
            // 2) Otherwise use [ScriptName], [PreserveName] or other attribute that controls naming
            // 3) Otherwise use the rename rules from the options

            string fieldScriptNameFromOptions = RenameRules.EnumRule == EnumRenameRule.MatchCSharpName
                ? fieldName
                : ToCamelCase(fieldName);

            bool isNamedValues = enumFieldSymbol.ContainingType.GetFlagAttribute(SaltarelleAttributeName.NamedValues);
            string fieldScriptName = isNamedValues
                ? fieldScriptNameFromOptions
                : DetermineScriptNameFromAttributes(enumFieldSymbol) ?? fieldScriptNameFromOptions;

            return fieldScriptName;
        }

        /// <summary>
        /// Determines the name that a field should have in the generated code.
        /// </summary>
        /// <param name="fieldSymbol">The field for which to determine the script name.</param>
        /// <returns>The name the field should have in the generated code.</returns>
        private string DetermineFieldScriptName(IFieldSymbol fieldSymbol)
        {
            string scriptName;

            switch (RenameRules.FieldRule)
            {
                // ReSharper disable once RedundantCaseLabel
                case FieldRenameRule.LowerCaseFirstChar:
                default:
                    scriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? ToCamelCase(fieldSymbol.Name);
                    break;

                // add a $ prefix if the field is private OR when there's a duplicate name
                case FieldRenameRule.PrivateDollarPrefix
                    when fieldSymbol.DeclaredAccessibility == Accessibility.Private:
                case FieldRenameRule.DollarPrefixOnlyForDuplicateName when HasDuplicateFieldName(fieldSymbol):
                    scriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? $"${ToCamelCase(fieldSymbol.Name)}";
                    break;
            }

            return scriptName;
        }

        /// <summary>
        /// Returns a value indicating whether the specified field symbol has a duplicate script
        /// name. For example, a public field named "Field" will be named "field" by default in the
        /// generated script. Another field named "Other" might be marked with a
        /// [ScriptName("field")], in which case there's a conflict.
        /// </summary>
        /// <param name="fieldSymbol">The field to search for.</param>
        /// <returns>True if there is a duplicate script name; otherwise, false.</returns>
        private static bool HasDuplicateFieldName(ISymbol fieldSymbol)
        {
            string fieldScriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? ToCamelCase(fieldSymbol.Name);

            var query = from member in fieldSymbol.ContainingType.GetMembers()

                            // don't include the field we're searching against
                        where !SymbolEqualityComparer.Default.Equals(member, fieldSymbol)

                        // find the potential script name of the member
                        let scriptName = DetermineScriptNameFromAttributes(member) ?? ToCamelCase(member.Name)

                        // and only take the ones that are equal to the field's script name
                        where string.Equals(scriptName, fieldScriptName, StringComparison.Ordinal)
                        select member;

            return query.Any();
        }

        /// <summary>
        /// Determines the name that a method should have in the generated code.
        /// </summary>
        /// <param name="methodSymbol">The method for which to determine the script name.</param>
        /// <returns>The name the method should have in the generated code.</returns>
        private static string DetermineMethodScriptName(IMethodSymbol methodSymbol)
        {
            string methodName = methodSymbol.Name;
            string defaultName = ToCamelCase(methodName);

            // if the symbol has any attributes that control naming, use those
            string attributedName = DetermineScriptNameFromAttributes(methodSymbol);
            if (attributedName != null)
            {
                return attributedName;
            }

            // check for [Imported] since we don't rename overloads on imported members
            bool imported = methodSymbol.ContainingType.GetFlagAttribute(SaltarelleAttributeName.Imported);
            if (imported)
            {
                return defaultName;
            }

            // check for [AlternateSignature] and then find the name of the implementing method
            if (methodSymbol.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature))
            {
                IMethodSymbol implementingMethod = methodSymbol.ContainingType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Single(
                        m => !SymbolEqualityComparer.Default.Equals(m, methodSymbol) &&

                            // the implementing method needs to be the same (static or instance)
                            m.IsStatic == methodSymbol.IsStatic &&

                            // the implementing method needs to be named the same
                            m.Name == methodSymbol.Name &&

                            // the implementing method cannot be marked with [InlineCode]
                            !m.HasAttribute(SaltarelleAttributeName.InlineCode) &&

                            // and the implementing method should not be marked with [AlternateSignature]
                            !m.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature));

                string scriptName = DetermineMethodScriptName(implementingMethod);
                return scriptName;
            }

            // check for overloads, since those need to be renamed with a $1, $2, etc. suffix
            if (!CanBeOverloaded(methodSymbol))
            {
                return defaultName;
            }

            // overloads are only renamed if the script name will be the same, so first run through
            // all of the methods and determine their script names
            var allMethodsWithThisScriptName =
                from m in methodSymbol.ContainingType.GetMembers().OfType<IMethodSymbol>()
                where CanBeOverloaded(m)

                // only look at static/instance methods (the same as what this method is), since it's
                // fine to have a static method and an instance method that have the same script name
                where m.IsStatic == methodSymbol.IsStatic

                // take out [AlternateSignature] methods since they use the name of the implementation method
                where !m.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature)
                let sname = DetermineScriptNameFromAttributes(m) ?? ToCamelCase(m.Name)
                where sname.Equals(defaultName, StringComparison.Ordinal)
                select m;

            // find the index of this method (according to the order it was declared)
            int index = allMethodsWithThisScriptName.ToImmutableArray().IndexOf(methodSymbol);

            // if we're the first method in the declared overloads, then we keep our name (using the
            // default camelCase rename) and the other overloaded methods need to add suffixes
            if (index == 0)
            {
                return defaultName;
            }

            // if it's overloaded, just assign it a suffix of $x where x is the index of when it was declared
            return ToCamelCase($"{methodName}${index}");
        }

        private static bool CanBeOverloaded(IMethodSymbol methodSymbol)
        {
            // only normal methods and constructors can be overloaded
            return methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor;
        }
    }
}
