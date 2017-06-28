// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5PropertyGetAssignment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property assignment in the following form: 'get propertyName() { }'.
    /// </summary>
    public class Es5PropertyGetAssignment : Es5AstNode, IEs5PropertyAssignment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5PropertyGetAssignment(string propertyName, IEnumerable<IEs5SourceElement> functionBody)
        {
            Param.VerifyString(propertyName, nameof(propertyName));
            PropertyName = propertyName;
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<IEs5SourceElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string PropertyName { get; }
        public ImmutableArray<IEs5SourceElement> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitPropertyGetAssignment(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitPropertyGetAssignment(this);
        }

        public override string CodeDisplay => $"get {PropertyName}() {{...}}";

        public override void Emit(IndentedTextWriter emitter)
        {
            emitter.Write($"get {PropertyName}() ");
            WriteBlock(emitter, FunctionBody);
        }
    }
}
