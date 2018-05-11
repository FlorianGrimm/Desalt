// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.TypeScript.Ast.Types;
    using Microsoft.CodeAnalysis;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Translates the parameters of a method or constructor declaration to adjust the types
    /// depending on the signatures of the methods marked with [AlternateSignature].
    /// </summary>
    internal class AlternateSignatureTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly TypeTranslator _typeTranslator;
        private readonly AlternateSignatureSymbolTable _alternateSignatureSymbolTable;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new instance of a <see cref="InlineCodeTranslator"/> from the specified
        /// semantic model and symbol tables.
        /// </summary>
        /// <param name="alternateSignatureSymbolTable">
        /// A symbol table containing methods that have been decorated with the [AlternateSignature] attribute.
        /// </param>
        /// <param name="typeTranslator">The <see cref="TypeTranslator"/> to use.</param>
        public AlternateSignatureTranslator(
            AlternateSignatureSymbolTable alternateSignatureSymbolTable,
            TypeTranslator typeTranslator)
        {
            _alternateSignatureSymbolTable = alternateSignatureSymbolTable ??
                throw new ArgumentNullException(nameof(alternateSignatureSymbolTable));

            _typeTranslator = typeTranslator ?? throw new ArgumentNullException(nameof(typeTranslator));
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adjusts the parameter list if necessary by taking into account all of the methods in an
        /// [AlternateSignature] method group and changing the parameter types to match the union of
        /// types amongst all of the methods in the group.
        /// </summary>
        /// <param name="methodSymbol">The method declaration that should be examined.</param>
        /// <param name="translatedParameterList">
        /// The already-translated parameter list that may be adjusted.
        /// </param>
        /// <param name="adjustedParameterList">The newly-translated parameter list.</param>
        /// <param name="diagnostics">Any diagnostics produced during adjustment.</param>
        /// <returns>true if the parameter list was adjusted; otherwise, false.</returns>
        public bool TryAdjustParameterListTypes(
            IMethodSymbol methodSymbol,
            ITsParameterList translatedParameterList,
            out ITsParameterList adjustedParameterList,
            out IEnumerable<Diagnostic> diagnostics)
        {
            adjustedParameterList = translatedParameterList;
            var diagnosticsList = new List<Diagnostic>();
            diagnostics = diagnosticsList;

            // we don't need to adjust anything if the method doesn't belong to an [AlternateSignature] group
            if (!_alternateSignatureSymbolTable.TryGetValue(
                methodSymbol,
                out AlternateSignatureMethodGroup methodGroup))
            {
                return false;
            }

            // make sure the calculated implementing method is actually the same as the methodSymbol
            // that is passed in since we only want to translate the implementing method
            IMethodSymbol implementingMethod = methodGroup.ImplementingMethod;
            if (!Equals(methodSymbol, implementingMethod))
            {
                return false;
            }

            // cast the translated arrays to the appropriate type since we only support bound
            // parameters (C# doesn't support string parameter types of the form `p: 'str'`)
            var translatedRequiredParameters = translatedParameterList.RequiredParameters
                .Cast<ITsBoundRequiredParameter>()
                .ToImmutableArray();

            var translatedOptionalParameters = translatedParameterList.OptionalParameters
                .Cast<ITsBoundOptionalParameter>()
                .ToImmutableArray();

            // adjust all of the required and optional parameters
            var requiredParameters = AdjustRequiredParameters(
                methodGroup,
                translatedRequiredParameters,
                diagnosticsList);

            var optionalParameters = AdjustOptionalParameters(
                methodGroup,
                translatedRequiredParameters,
                translatedOptionalParameters,
                diagnosticsList);

            var restParameter = translatedParameterList.RestParameter;

            // create the new parameter list and compare against the old one
            adjustedParameterList = Factory.ParameterList(requiredParameters, optionalParameters, restParameter);
            bool changed = !translatedParameterList.Equals(adjustedParameterList);
            return changed;
        }

        /// <summary>
        /// Adjusts the translated parameter list by treating only the first N parameters as
        /// required, where N is the minimum number of parameters in the method group.
        /// </summary>
        private ImmutableArray<ITsBoundRequiredParameter> AdjustRequiredParameters(
            AlternateSignatureMethodGroup methodGroup,
            ImmutableArray<ITsBoundRequiredParameter> translatedRequiredParameters,
            ICollection<Diagnostic> diagnostics)
        {
            int requiredParamCount = Math.Min(methodGroup.MinParameterCount, translatedRequiredParameters.Length);

            // adjust the types for the required parameters (but only the ones that are shared with
            // everything else - the other ones will be converted to optional parameters below)
            var requiredParameters = translatedRequiredParameters.Take(requiredParamCount)
                .Select(
                    (param, index) => param.WithParameterType(DetermineParameterType(methodGroup, index, diagnostics)))
                .ToImmutableArray();

            return requiredParameters;
        }

        /// <summary>
        /// Adjusts the translated parameter list by converting required parameters to optional,
        /// adding optional parameters if necessary, and adding union types for the set of all
        /// supported parameter types.
        /// </summary>
        private ImmutableArray<ITsBoundOptionalParameter> AdjustOptionalParameters(
            AlternateSignatureMethodGroup methodGroup,
            ImmutableArray<ITsBoundRequiredParameter> translatedRequiredParameters,
            ImmutableArray<ITsBoundOptionalParameter> translatedOptionalParameters,
            ICollection<Diagnostic> diagnostics)
        {
            int requiredParamCount = Math.Min(methodGroup.MinParameterCount, translatedRequiredParameters.Length);

            // Convert required parameters to optional parameters - this occurs when this method has
            // more parameters than other methods with [AlternateSignature]. For example:
            //
            // [AlternateSignature] extern void Method(int x);
            // Method(int x, string y) {}
            //
            // y needs to be optional
            var convertedOptionalParameters = translatedRequiredParameters.Skip(requiredParamCount)
                .Select(
                    (translatedParameter, index) => Factory.BoundOptionalParameter(
                        translatedParameter.ParameterName,
                        DetermineParameterType(methodGroup, index + requiredParamCount, diagnostics)));

            // translate the optional parameter types
            var originalOptionalParameters = translatedOptionalParameters.Select(
                (param, index) => param.WithParameterType(
                    DetermineParameterType(methodGroup, index + translatedRequiredParameters.Length, diagnostics)));

            // add additional optional parameters if the implementing method doesn't have enough
            int maxParamsCount = methodGroup.MaxParameterCount;
            int paramsCount = translatedRequiredParameters.Length + translatedOptionalParameters.Length;

            var addedOptionalParameters = Enumerable.Empty<ITsBoundOptionalParameter>();
            if (paramsCount < maxParamsCount)
            {
                addedOptionalParameters = Enumerable.Range(paramsCount, maxParamsCount - paramsCount)
                    .Select(
                        index => Factory.BoundOptionalParameter(
                            Factory.Identifier(methodGroup.MethodWithMaxParams.Parameters[index].Name),
                            DetermineParameterType(methodGroup, index, diagnostics)));
            }

            var optionalParameters = convertedOptionalParameters.Concat(originalOptionalParameters)
                .Concat(addedOptionalParameters)
                .ToImmutableArray();

            return optionalParameters;
        }

        /// <summary>
        /// Determines the translated type of the parameter by examining the types of all of the
        /// methods in the group and creating union types if necessary.
        /// </summary>
        /// <param name="group">The method group.</param>
        /// <param name="parameterIndex">The index of the parameter to examine.</param>
        /// <param name="diagnostics">The diagnostics to add errors to.</param>
        /// <returns>
        /// Either the parmeter type or a union type of all of the possible types for the parameter.
        /// </returns>
        private ITsType DetermineParameterType(
            AlternateSignatureMethodGroup group,
            int parameterIndex,
            ICollection<Diagnostic> diagnostics)
        {
            Location GetLocation() =>
                group.ImplementingMethod.Parameters[parameterIndex]
                    .DeclaringSyntaxReferences[0]
                    .GetSyntax()
                    .GetLocation();

            var translatedTypes = group.TypesForParameter(parameterIndex)
                .Select(
                    typeSymbol => _typeTranslator.TranslateSymbol(
                        typeSymbol,
                        typesToImport: null,
                        diagnostics: diagnostics,
                        getLocationFunc: GetLocation))
                .Distinct()
                .ToArray();

            return translatedTypes.Length == 1 ? translatedTypes[0] : Factory.UnionType(translatedTypes);
        }
    }
}
