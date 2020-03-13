// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptAssemblySymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents an assembly symbol that can be used in the translation process.
    /// </summary>
    internal sealed class ScriptAssemblySymbol : ScriptSymbol, IScriptAssemblySymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptAssemblySymbol(IAssemblySymbol assemblySymbol, string computedScriptName)
            : base(assemblySymbol, computedScriptName)
        {
            AssemblySymbol = assemblySymbol;
            DefaultReflectability = assemblySymbol.GetAttributeValueOrDefault(
                SaltarelleAttributeName.DefaultMemberReflectability,
                defaultValue: MemberReflectability.All);

            IncludeGenericArgumentsMethodDefault = assemblySymbol.GetAttributeValueOrDefault(
                SaltarelleAttributeName.IncludeGenericArgumentsDefault,
                propertyName: SaltarelleAttributeArgumentName.MethodDefault,
                defaultValue: GenericArgumentsBehaviorType.Ignore);
            IncludeGenericArgumentsTypeDefault = assemblySymbol.GetAttributeValueOrDefault(
                SaltarelleAttributeName.IncludeGenericArgumentsDefault,
                propertyName: SaltarelleAttributeArgumentName.TypeDefault,
                defaultValue: GenericArgumentsBehaviorType.Ignore);

            MinimizePublicNames = assemblySymbol.GetFlagAttribute(SaltarelleAttributeName.MinimizePublicNames);
            ModuleName = assemblySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ModuleName);

            OmitDowncasts = assemblySymbol.GetAttributeValueOrDefault(
                SaltarelleAttributeName.ScriptSharpCompatibility,
                propertyName: SaltarelleAttributeArgumentName.OmitDowncasts,
                defaultValue: false);
            OmitNullableChecks = assemblySymbol.GetAttributeValueOrDefault(
                SaltarelleAttributeName.ScriptSharpCompatibility,
                propertyName: SaltarelleAttributeArgumentName.OmitNullableChecks,
                defaultValue: false);

            ScriptAssemblyName = assemblySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptAssembly);
            ScriptNamespace = assemblySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptNamespace);
            ScriptQualifier = assemblySymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptQualifier);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        public IAssemblySymbol AssemblySymbol { get; }

        /// <summary>
        /// This attribute can be applied to an assembly or a type to indicate whether members are
        /// reflectable by default.
        /// </summary>
        public MemberReflectability DefaultReflectability { get; }

        /// <summary>
        /// Indicates whether generic arguments for methods are included, but can always be
        /// overridden by specifying an [IncludeGenericArguments] attribute on methods.
        /// </summary>
        public GenericArgumentsBehaviorType IncludeGenericArgumentsMethodDefault { get; }

        /// <summary>
        /// Indicates whether generic arguments for types are included, but can always be overridden
        /// by specifying an [IncludeGenericArguments] attribute on types.
        /// </summary>
        public GenericArgumentsBehaviorType IncludeGenericArgumentsTypeDefault { get; }

        /// <summary>
        /// Indicates whether to allow public symbols inside an assembly to be minimized, in addition
        /// to non-public ones, when generating release scripts.
        /// </summary>
        public bool MinimizePublicNames { get; }

        /// <summary>
        /// Specifies that a type is defined in a module, which should be imported by a require() call.
        /// </summary>
        public string? ModuleName { get; }

        /// <summary>
        /// If true, code will not be generated for casts of type '(MyClass)someValue'. Code will
        /// still be generated for 'someValue is MyClass' and 'someValue as MyClass'.
        /// </summary>
        public bool OmitDowncasts { get; }

        /// <summary>
        /// If true, code will not be generated to verify that a nullable value is not null before
        /// converting it to its underlying type.
        /// </summary>
        public bool OmitNullableChecks { get; }

        /// <summary>
        /// Marks an assembly as a script assembly that can be used with Script#. Additionally, each
        /// script must have a unique name that can be used as a dependency name. This name is also
        /// used to generate unique names for internal types defined within the assembly. The
        /// ScriptQualifier attribute can be used to provide a shorter name if needed.
        /// </summary>
        public string? ScriptAssemblyName { get; }

        /// <summary>
        /// Specifies the namespace that should be used in generated script. The script namespace is
        /// typically a short name, that is often shared across multiple assemblies. The developer is
        /// responsible for ensuring that public types across assemblies that share a script
        /// namespace are unique. For internal types, the [ScriptQualifier] attribute can be used to
        /// provide a short prefix to generate unique names.
        /// </summary>
        public string? ScriptNamespace { get; }

        /// <summary>
        /// Provides a prefix to use when generating types internal to this assembly so that they can
        /// be unique within a given a script namespace. The specified prefix overrides the script
        /// name provided in <see cref="ScriptAssemblyName"/>.
        /// </summary>
        public string? ScriptQualifier { get; }
    }
}
