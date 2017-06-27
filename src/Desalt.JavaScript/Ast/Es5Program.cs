﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Program.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a top-level JavaScript program, consisting of a collection of source elements.
    /// </summary>
    public class Es5Program : Es5AstNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5Program(IEnumerable<IEs5SourceElement> sourceElements)
        {
            SourceElements = sourceElements?.ToImmutableArray() ?? ImmutableArray<IEs5SourceElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<IEs5SourceElement> SourceElements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitProgram(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitProgram(this);
        }

        public override string ToCodeDisplay() => $"Es5Program, SourceElements.Count = {SourceElements.Length}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteItems(writer, SourceElements, indent: false, itemDelimiter: Environment.NewLine);
        }
    }
}