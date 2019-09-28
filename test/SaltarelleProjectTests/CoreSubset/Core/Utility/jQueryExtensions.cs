// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="jQueryExtensions.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
    using jQueryApi;

    /// <summary>
    /// Contains plugins to the native jQuery object. Generally, you should prefer to use helper methods rather than
    /// writing a jQuery plugin, but sometimes it's useful to act on a group of jQueryObjects all at once and have the
    /// chaining support built-in.
    /// </summary>
    /// <remarks>
    /// You use this by type-casting a normal <see cref="jQueryObject"/> to a <see cref="jQueryExtensionsObject"/>.
    /// </remarks>
    [Mixin("$.fn")]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "j",
        Justification = "jQuery is a brand name and thus we preserve it in our naming.")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "jQuery is a brand name and thus we preserve it in our naming.")]
    public static class jQueryExtensions
    {
        // /// <summary>
        // /// Extends the jQuery built-in <code>focus</code> function by adding the ability to call focus on a delayed
        // /// timer. This is extremely useful when inside of a browser event handler so the browser can continue to
        // /// process the event without waiting for the focus to happen.
        // /// </summary>
        // /// <param name="delayMilliseconds">The amount of time to wait in milliseconds before calling <paramref name="action"/>.</param>
        // /// <param name="action">The code to run after the delay and after the element has received focus.</param>
        // /// <remarks>Tweaked from jquery.ui.core.js</remarks>
        //         public static void FocusDelayed(int delayMilliseconds, Function action)
        //         {
        // #pragma warning disable 618 // 'jQueryApi.jQuery.Current' is obsolete: 'jQuery.Current is fragile. Migrate your code to use the methods that supply the context as a parameter.'
        //             jQuery.Current.Each((index, elem) =>
        //             {
        //                 Window.SetTimeout(() =>
        //                 {
        //                     jQuery.FromObject(elem).Focus();
        //                     if (Script.IsValue(action))
        //                     {
        //                         action.Call(elem);
        //                     }
        //                 }, delayMilliseconds);
        //             });
        //         }

        // #pragma warning restore 618

        /// <summary>
        /// TFSID 544295: this function should be packaged into the gitlab saltarelle project
        /// Returns true if complete string can be parsed into a double.
        /// "1a" returns false. "1.0" returns true.
        /// </summary>
        [InlineCode("$.isNumeric({val})")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        public static bool IsNumeric(object val)
        {
            return false;
        }

        public static TouchEvent GetTouchEvent(this jQueryEvent jqueryEvent)
        {
            return TypeUtil.GetField<TouchEvent>(jqueryEvent, "originalEvent");
        }
    }

    /// <summary>
    /// This doesn't actually contain any code and is only used to provide the ability to call into the jQuery plugins
    /// that we have defined in <see cref="jQueryExtensions"/>.
    /// </summary>
    [Imported]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "j",
        Justification = "jQuery is a brand name and thus we preserve it in our naming.")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "jQuery is a brand name and thus we preserve it in our naming.")]
    public sealed class jQueryExtensionsObject : jQueryObject
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "milliseconds")]
        public jQueryExtensionsObject Delay(int milliseconds)
        {
            return null;
        }

        /// <summary>
        /// Extends the jQuery built-in <code>focus</code> function by addint the ability to call focus on a delayed
        /// timer. This is extremely useful when inside of a browser event handler so the browser can continue to
        /// process the event without waiting for the focus to happen.
        /// </summary>
        /// <param name="delayMilliseconds">The amount of time to wait in milliseconds before focusing.</param>
        [AlternateSignature]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "delayMilliseconds",
            Justification = "The static analyzer doesn't recognize extern methods")]
        public extern void FocusDelayed(int delayMilliseconds);

        /// <summary>
        /// Extends the jQuery built-in <code>focus</code> function by addint the ability to call focus on a delayed
        /// timer. This is extremely useful when inside of a browser event handler so the browser can continue to
        /// process the event without waiting for the focus to happen.
        /// </summary>
        /// <param name="delayMilliseconds">The amount of time to wait in milliseconds before focusing.</param>
        /// <param name="action">The code to run after the delay and after the element has received focus.</param>
        /// <remarks>Taken from jquery.ui.core.js</remarks>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            Justification = "The static analyzer doesn't recognize [Imported] methods")]
        public void FocusDelayed(int delayMilliseconds, Action action)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters"), AlternateSignature]
        public extern jQueryExtensionsObject On<T>(string eventName, Action<jQueryEvent, T> handler);

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters"), AlternateSignature]
        public extern jQueryExtensionsObject On<T>(string eventName, string selector, Action<jQueryEvent, T> handler);

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public extern jQueryExtensionsObject On(string eventName, Action<jQueryEvent> handler);

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters"), AlternateSignature]
        public extern jQueryExtensionsObject On(string eventName, string selector, Action<jQueryEvent> handler);

        // Note that this takes a double, unlike .Height(int); see TFSID: 556519
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public extern jQueryExtensionsObject OuterHeight(double height);
    }
}
