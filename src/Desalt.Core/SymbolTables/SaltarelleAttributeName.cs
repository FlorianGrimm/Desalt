// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SaltarelleAttributeName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    /// <summary>
    /// Enumerates the supported Saltarelle attributes that control the way C# code is translated
    /// into script.
    /// </summary>
    internal enum SaltarelleAttributeName
    {
        AlternateSignature,
        BackingFieldName,
        BindThisToFirstParameter,
        CustomInitialization,
        DefaultMemberReflectability,
        DontGenerate,
        EnumerateAsArray,
        ExpandParams,
        GlobalMethods,
        IgnoreNamespace,
        Imported,
        IncludeGenericArguments,
        IncludeGenericArgumentsDefault,
        InlineCode,
        InlineConstant,
        InstanceMethodOnFirstArgument,
        IntrinsicOperator,
        IntrinsicProperty,
        MinimizePublicNames,
        Mixin,
        ModuleName,
        Mutable,
        NamedValues,
        NumericValues,
        NoInline,
        NonScriptable,
        ObjectLiteral,
        PreserveCase,
        PreserveMemberCase,
        PreserveName,
        Reflectable,
        ScriptAlias,
        ScriptAssembly,
        ScriptName,
        ScriptNamespace,
        ScriptQualifier,
        ScriptSharpCompatibility,
        ScriptSkip,
    }

    /// <summary>
    /// Enumerates the supported Saltarelle attribute named arguments.
    /// </summary>
    internal enum SaltarelleAttributeArgumentName
    {
        GeneratedMethodName,
        MethodDefault,
        NonExpandedFormCode,
        NonVirtualCode,
        ObeysTypeSystem,
        OmitDowncasts,
        OmitNullableChecks,
        TypeCheckCode,
        TypeDefault,
    }
}
