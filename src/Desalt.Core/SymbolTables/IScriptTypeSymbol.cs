// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptTypeSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a class, interface, struct, or enum symbol that can be used in the translation process.
    /// </summary>
    internal interface IScriptTypeSymbol : IScriptSymbol
    {
        /// <summary>
        /// This attribute can be applied to an assembly or a type to indicate whether members are
        /// reflectable by default.
        /// </summary>
        MemberReflectability DefaultMemberReflectability { get; }

        /// <summary>
        /// Indicates that the namespace of type within a system assembly should be ignored at script
        /// generation time. It is useful for creating namespaces for the purpose of C# code that
        /// don't exist at runtime.
        /// </summary>
        bool IgnoreNamespace { get; }

        /// <summary>
        /// This attribute specifies that a generic type or method should have script generated as if
        /// it was a non-generic one. Any uses of the type arguments inside the method (eg.
        /// <c>typeof(T)</c>, or calling another generic method with T as a type argument) will cause
        /// runtime errors.
        /// </summary>
        bool IncludeGenericArguments { get; }

        /// <summary>
        /// TODO - support MixinAttribute.Expression
        /// </summary>
        string MixinExpression { get; }

        /// <summary>
        /// Specifies that a type is defined in a module, which should be imported by a require() call.
        ///
        /// TODO - [ModuleName]
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Indicates that the type obeys the Saltarelle type system. If false, the type is ignored
        /// in inheritance lists, casts to it are no-ops, and `object` will be used if the type is
        /// used as a generic argument.
        /// TODO - Support ImportedAttribute.ObeysTypeSystem
        /// </summary>
        bool ObeysTypeSystem { get; }

        /// <summary>
        /// Indicates whether to allow suppressing the default behavior of converting member names of
        /// attached type to camel-cased equivalents in the generated JavaScript. When applied to an
        /// assembly, all types in the assembly are considered to have this attribute by default.
        /// </summary>
        bool PreserveMemberCase { get; }

        /// <summary>
        /// Specifies the namespace that should be used in generated script. The script namespace is
        /// typically a short name, that is often shared across multiple assemblies. The developer is
        /// responsible for ensuring that public types across assemblies that share a script
        /// namespace are unique. For internal types, the [ScriptQualifier] attribute can be used to
        /// provide a short prefix to generate unique names.
        /// </summary>
        string ScriptNamespace { get; }

        /// <summary>
        /// Indicates whether methods on a static class should be treated as global methods in the
        /// generated script. Note that the class must be static, and must contain only methods.
        /// </summary>
        bool TreatMethodsAsGlobal { get; }

        /// <summary>
        /// Code used to check whether an object is of this type. Can use the placeholder {this} to
        /// reference the object being checked, as well as all type parameter for the type.
        /// TODO - Support ImportedAttribute.TypeCheckCode
        /// </summary>
        string TypeCheckCode { get; }

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        ITypeSymbol TypeSymbol { get; }
    }

    /// <summary>
    /// This enum defines the possibilities for default member reflectability.
    /// </summary>
    internal enum MemberReflectability
    {
        /// <summary>
        /// Members are not reflectable (unless they are decorated either with any script-usable
        /// attributes or a [ReflectableAttribute])
        /// </summary>
        None,

        /// <summary>
        /// Public and protected members are reflectable, private/internal members are only
        /// reflectable if are decorated either with any script-usable attributes or a
        /// [ReflectableAttribute]. Members are reflectable even when their enclosing type is not
        /// publicly visible.
        /// </summary>
        PublicAndProtected,

        /// <summary>
        /// Public, protected and internal members are reflectable, private members are only
        /// reflectable if are decorated either with any script-usable attributes or a [ReflectableAttribute].
        /// </summary>
        NonPrivate,

        /// <summary>
        /// All members are reflectable by default (can be overridden with [Reflectable(false)]).
        /// </summary>
        All
    }
}
