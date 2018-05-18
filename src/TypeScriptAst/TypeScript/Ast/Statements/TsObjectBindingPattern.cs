// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsObjectBindingPattern.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Statements
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an object binding pattern of the form '{propName = defaultValue, propName: otherPropName}'.
    /// </summary>
    internal class TsObjectBindingPattern : AstNode, ITsObjectBindingPattern
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsObjectBindingPattern(IEnumerable<ITsBindingProperty> properties)
        {
            Properties = properties?.ToImmutableArray() ?? ImmutableArray<ITsBindingProperty>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsBindingProperty> Properties { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitObjectBindingPattern(this);

        public override string CodeDisplay => $"{{{Properties.ToElidedList()}}}";

        protected override void EmitInternal(Emitter emitter) =>
            emitter.WriteList(
                Properties, indent: false, prefix: "{", suffix: "}", itemDelimiter: ", ", emptyContents: "{}");
    }
}
