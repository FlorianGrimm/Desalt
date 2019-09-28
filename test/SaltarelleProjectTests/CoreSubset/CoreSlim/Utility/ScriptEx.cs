// // ---------------------------------------------------------------------------------------------------------------------
// // <copyright file="ScriptEx.cs" company="Tableau Software">
// //   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
// //   applicable U.S. and international laws and regulations.
// //
// //   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// // </copyright>
// // ---------------------------------------------------------------------------------------------------------------------
namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extension methods for <see cref="Script"/>.
    /// </summary>
    public static class ScriptEx
    {
        /// <summary>
        /// <b>This is not the method you're looking for</b>.  Please use use <see cref="Script.Coalesce{T}"/> if at all possible possible.
        /// Equivalent to Script#'s Script.Value.  Only here for backwards compatibility.
        /// </summary>
        /// <typeparam name="T">any type will do</typeparam>
        [InlineCode("{a} || {b}")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        public static T Value<T>(T a, T b)
        {
            return default(T);
        }

        [InlineCode("arguments")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        public static object Arguments()
        {
            return null;
        }    
    }
}
