// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptMethodSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a method symbol that can be used in the translation process.
    /// </summary>
    internal class ScriptMethodSymbol : ScriptSymbol, IScriptMethodSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptMethodSymbol(IMethodSymbol methodSymbol, string computedScriptName)
            : this(methodSymbol, computedScriptName, instanceToCopy: null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptMethodSymbol"/> class. Values are set using the
        /// following precedence:
        /// * The named parameter, if specified
        /// * Otherwise, the value of <paramref name="instanceToCopy"/>, if specified
        /// * Otherwise, the default value of the parameter's type
        /// </summary>
        // ReSharper disable once FunctionComplexityOverflow
        private ScriptMethodSymbol(
            IMethodSymbol methodSymbol,
            string computedScriptName,
            IScriptMethodSymbol? instanceToCopy = null,
            bool? alternateSignature = null,
            // ReSharper disable once IdentifierTypo
            bool? dontGenerate = null,
            bool? enumerateAsArray = null,
            bool? expandParams = null,
            string? inlineCode = null,
            string? inlineCodeGeneratedMethodName = null,
            string? inlineCodeNonExpendedFormCode = null,
            string? inlineCodeNonVirtualCode = null,
            bool? instanceMethodOnFirstArgument = null,
            bool? intrinsicOperator = null,
            bool? objectLiteral = null,
            string? scriptAlias = null,
            bool? scriptSkip = null)
            : base(methodSymbol, computedScriptName)
        {
            AlternateSignature = alternateSignature ?? instanceToCopy?.AlternateSignature ??
                methodSymbol.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature);

            DontGenerate = dontGenerate ??
                instanceToCopy?.DontGenerate ?? methodSymbol.GetFlagAttribute(SaltarelleAttributeName.DontGenerate);

            EnumerateAsArray = enumerateAsArray ??
                instanceToCopy?.EnumerateAsArray ??
                methodSymbol.GetFlagAttribute(SaltarelleAttributeName.EnumerateAsArray);

            ExpandParams = expandParams ??
                instanceToCopy?.ExpandParams ?? methodSymbol.GetFlagAttribute(SaltarelleAttributeName.ExpandParams);

            InlineCode = inlineCode ??
                instanceToCopy?.InlineCode ??
                methodSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.InlineCode);

            InlineCodeGeneratedMethodName = inlineCodeGeneratedMethodName ??
                instanceToCopy?.InlineCodeGeneratedMethodName ??
                methodSymbol.GetAttributeValueOrDefault(
                    SaltarelleAttributeName.InlineCode,
                    propertyName: SaltarelleAttributeArgumentName.GeneratedMethodName);

            InlineCodeNonExpandedFormCode = inlineCodeNonExpendedFormCode ??
                instanceToCopy?.InlineCodeNonExpandedFormCode ??
                methodSymbol.GetAttributeValueOrDefault(
                    SaltarelleAttributeName.InlineCode,
                    propertyName: SaltarelleAttributeArgumentName.NonExpandedFormCode);

            InlineCodeNonVirtualCode = inlineCodeNonVirtualCode ??
                instanceToCopy?.InlineCodeNonVirtualCode ??
                methodSymbol.GetAttributeValueOrDefault(
                    SaltarelleAttributeName.InlineCode,
                    propertyName: SaltarelleAttributeArgumentName.NonVirtualCode);

            InstanceMethodOnFirstArgument = instanceMethodOnFirstArgument ??
                instanceToCopy?.InstanceMethodOnFirstArgument ??
                methodSymbol.GetFlagAttribute(SaltarelleAttributeName.InstanceMethodOnFirstArgument);

            IntrinsicOperator = intrinsicOperator ??
                instanceToCopy?.IntrinsicOperator ??
                methodSymbol.GetFlagAttribute(SaltarelleAttributeName.IntrinsicOperator);

            MethodSymbol = methodSymbol;

            ObjectLiteral = objectLiteral ??
                instanceToCopy?.ObjectLiteral ?? methodSymbol.GetFlagAttribute(SaltarelleAttributeName.ObjectLiteral);

            ScriptAlias = scriptAlias ??
                instanceToCopy?.ScriptAlias ??
                methodSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptAlias);

            ScriptSkip = scriptSkip ??
                instanceToCopy?.ScriptSkip ?? methodSymbol.GetFlagAttribute(SaltarelleAttributeName.ScriptSkip);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether this method is an alternate method signature that is not generated into
        /// script, but can be used for defining overloads to enable optional parameter semantics for
        /// a method. It must be applied on a method defined as extern, since an alternate signature
        /// method does not contain an actual method body.
        /// </summary>
        public bool AlternateSignature { get; }

        /// <summary>
        /// Can be specified on a method or a constructor to indicate that no code should be
        /// generated for the member, but it has no effect on any usage of the member.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        public bool DontGenerate { get; }

        /// <summary>
        /// Can be applied to a GetEnumerator() method to indicate that that array-style enumeration
        /// should be used.
        /// </summary>
        public bool EnumerateAsArray { get; }

        /// <summary>
        /// Indicates whether a method with a "params" parameter should make the param array be
        /// expanded in script (eg. given <c>void F(int a, params int[] b)</c>, the invocation
        /// <c>F(1, 2, 3)</c> will be translated to <c>F(1, [2, 3])</c> without this attribute, but
        /// <c>(1, 2, 3)</c> with this attribute. Methods with this attribute can only be invoked in
        /// the expanded form.
        /// </summary>
        public bool ExpandParams { get; }

        /// <summary>
        /// The method is implemented as inline code, eg Debugger.Break() =&gt; debugger. Can use the
        /// parameters {this} (for instance methods), as well as all type names and argument names in
        /// braces (eg. {arg0}, {TArg0}). If a parameter name is preceded by an @ sign, {@arg0},
        /// that argument must be a literal string during invocation, and the supplied string will be
        /// inserted as an identifier into the script (eg '{this}.set_{@arg0}({arg1})' can transform
        /// the call 'c.F("MyProp", v)' to 'c.set_MyProp(v)'. If a parameter name is preceded by an
        /// asterisk {*arg} that parameter must be a param array, and all invocations of the method
        /// must use the expanded invocation form. The entire array supplied for the parameter will
        /// be inserted into the call. Pretend that the parameter is a normal parameter, and commas
        /// will be inserted or omitted at the correct locations. The format string can also use
        /// identifiers starting with a dollar {$Namespace.Name} to construct type references. The
        /// name must be the fully qualified type name in this case.
        /// </summary>
        public string? InlineCode { get; }

        public ScriptMethodSymbol WithInlineCode(string? value)
        {
            return new ScriptMethodSymbol(MethodSymbol, ComputedScriptName, this, inlineCode: value);
        }

        /// <summary>
        /// If set, a method with this name will be generated from the method source.
        /// </summary>
        public string? InlineCodeGeneratedMethodName { get; }

        /// <summary>
        /// This code is used when the method, which should be a method with a param array parameter,
        /// is invoked in non-expanded form. Optional, but can be used to support non-expanded
        /// invocation of a method that has a {*param} placeholder in its code.
        /// </summary>
        public string? InlineCodeNonExpandedFormCode { get; }

        /// <summary>
        /// This code is used when the method is invoked non-virtually (eg. in a base.Method() call).
        /// </summary>
        public string? InlineCodeNonVirtualCode { get; }

        /// <summary>
        /// This attribute specifies that a static method should be treated as an instance method on
        /// its first argument. This means that <c>MyClass.Method(x, a, b)</c> will be transformed to
        /// <c>x.Method(a, b)</c>. If no other name-preserving attribute is used on the member, it
        /// will be treated as if it were decorated with a [PreserveNameAttribute]. Useful for
        /// extension methods.
        /// </summary>
        public bool InstanceMethodOnFirstArgument { get; }

        /// <summary>
        /// Indicates that a user-defined operator should be compiled as if it were built-in (eg.
        /// op_Addition(a, b) =&gt; a + b). It can only be used on non-conversion operator methods.
        /// </summary>
        public bool IntrinsicOperator { get; }

        /// <summary>
        /// The associated C# symbol.
        /// </summary>
        public IMethodSymbol MethodSymbol { get; }

        /// <summary>
        /// If this attribute is applied to a constructor for a serializable type, it means that the
        /// constructor will not be called, but rather an object initializer will be created. Eg.
        /// <c>new MyRecord(1, "X")</c> can become <c>{ a: 1, b: 'X' }</c>. All parameters must have
        /// a field or property with the same (case-insensitive) name, of the same type. This
        /// attribute is implicit on constructors of imported serializable types.
        /// </summary>
        public bool ObjectLiteral { get; }

        /// <summary>
        /// Specifies a script name for an imported method. The method is interpreted as a global
        /// method. As a result it this attribute only applies to static methods.
        /// </summary>
        public string? ScriptAlias { get; }

        /// <summary>
        /// Indicates whether a method should not be invoked. The method must either be a static
        /// method with one argument (in case Foo.M(x) will become x), or an instance method with no
        /// arguments (in which x.M() will become x). Can also be applied to a constructor, in which
        /// case the constructor will not be called if used as an initializer (": base()" or ": this()").
        /// </summary>
        public bool ScriptSkip { get; }
    }
}
