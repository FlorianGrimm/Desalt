// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="MiscUtil.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.TypedArrays;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
    using System.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using Tableau.JavaScript.CoreSlim;
    using Tableau.JavaScript.Vql.Bootstrap;
    using Tableau.JavaScript.Vql.TypeDefs;
    using UnderscoreJs;

    [NumericValues]
    public enum PathnameKey
    {
        WorkbookName = 2,
        SheetId = 3,
        AuthoringSheet = 4,
    }

    /// <summary>
    /// Contains utility methods for etc.
    /// </summary>
    public static class MiscUtil
    {
        public static string PathName
        {
            get
            {
                WindowInstance window = Utility.LocationWindow;
                return WindowHelper.GetLocation(window).Pathname;
            }
        }

        public static JsDictionary<PathnameKey, string> UrlPathnameParts
        {
            get
            {
                string pathname = MiscUtil.PathName;
                string siteRoot = TsConfig.SiteRoot;
                int index = pathname.IndexOf(siteRoot, 0);
                string actualPath = pathname.Substr(index + siteRoot.Length);

                string[] pathnameParts = actualPath.Split("/");

                JsDictionary<PathnameKey, string> pathnameProps = new JsDictionary<PathnameKey, string>();

                // TODO: change this to iterate over Enum.GetValues when we upgrade to script sharp version that supports this.
                pathnameProps[PathnameKey.WorkbookName] = pathnameParts[(int)PathnameKey.WorkbookName];
                pathnameProps[PathnameKey.SheetId] = pathnameParts[(int)PathnameKey.SheetId];
                pathnameProps[PathnameKey.AuthoringSheet] = pathnameParts[(int)PathnameKey.AuthoringSheet];
                return pathnameProps;
            }
        }

        /// <summary>
        /// Performs a shallow equals of two given objects.
        /// </summary>
        /// <param name="valueA">An object or <c>null</c></param>
        /// <param name="valueB">Another object or <c>null</c></param>
        /// <returns><c>true</c> if all keys are <c>===</c> between the two objects.</returns>
        public static bool ShallowEquals(object valueA, object valueB)
        {
            if (valueA == valueB) { return true; }
            if (valueA == null || valueB == null) { return false; }

            var keysA = Keys(valueA);
            var keysB = Keys(valueB);
            if (keysA.Length != keysB.Length)
            {
                return false;
            }
            for (int i = 0; i < keysA.Length; i++)
            {
                var key = keysA[i];
                if (!HasOwnProperty(valueB, key) || TypeUtil.GetField<object>(valueA, key) != TypeUtil.GetField<object>(valueB, key))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Percent encodes (also called URL encoding) the given string.  First the browser's encodeUriComponent()
        /// is called on the string and then any characters not included in <c>unreservedChars</c> are
        /// then encoded.  If you only want the browser standard behavior then use <seealso cref="string.EncodeUriComponent"/>,
        /// this method is only useful for encoding extra characters that the browser might not encode.
        /// </summary>
        /// <param name="valueToEncode">The string to be encoded</param>
        /// <param name="unreservedChars">A dictionary of unreserved string keys (single characters).  Any character
        /// not included as a key will be escaped.</param>
        /// <returns>The encoded string</returns>
        public static string PercentEncode(string valueToEncode, JsDictionary<string, string> unreservedChars)
        {
            valueToEncode = string.EncodeUriComponent(valueToEncode);

            if (Script.IsNullOrUndefined(unreservedChars)) { return valueToEncode; }

            StringBuilder sb = new StringBuilder();
            int i = 0;

            while (i < valueToEncode.Length)
            {
                string s = valueToEncode.Substr(i, 1);
                if (s == "%")
                {
                    // browser handled encoding, append unchanged
                    sb.Append(valueToEncode.Substr(i, 3));
                    i += 2;
                }
                else if (!unreservedChars.ContainsKey(s))
                {
                    sb.Append("%").Append(((int)s.CharCodeAt(0)).ToString(16).ToUpperCase());
                }
                else
                {
                    sb.Append(s);
                }
                i++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encodes string based on string encoding defined in wgapp\app\helpers\link_help.rb:encode_id(name)
        /// First, specific characters are url-encoded, and the resulting string is then completely url-encoded.
        /// </summary>
        /// <param name="valueToEncode">The string to be encoded</param>
        /// <returns>The encoded string</returns>
        public static string EncodeForWG(string valueToEncode)
        {
            JsDictionary<string, string> usernameValidChars = new JsDictionary<string, string>();

            Action<char, char> addCodes = (char from, char to) =>
            {
                for (char i = from; i <= to; i++)
                {
                    string s = string.FromCharCode(i);
                    usernameValidChars[s] = s;
                }
            };

            addCodes('a', 'z');
            addCodes('A', 'Z');
            addCodes('0', '9');
            addCodes('_', '_');
            addCodes('-', '-');

            valueToEncode = PercentEncode(valueToEncode, usernameValidChars);
            valueToEncode = PercentEncode(valueToEncode, null);
            return valueToEncode;
        }

        /// <summary>
        /// Tests whether a single argument or at least one of a list of arguments is null OR
        /// undefined.
        /// </summary>
        /// <param name="args">argument to be checked</param>
        /// <returns>true or false</returns>
        public static bool IsNullOrUndefined(object[] args)
        {
            if (Script.IsNullOrUndefined(args))
            {
                return true;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (Script.IsNullOrUndefined(args[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests whether an array or a string is either null, undefined, or empty
        /// (length is 0). You can pass one or more arguments to the function and it will
        /// return true if at least one of the arguments is null or empty.
        /// </summary>
        /// <param name="args">argument to be checked</param>
        /// <returns>true or false</returns>
        public static bool IsNullOrEmpty(object args)
        {
            if (Script.IsNullOrUndefined(args))
            {
                return true;
            }

            // B46804: corrected semantics for this function.
            JsDictionary<string, int> dict = JsDictionary<string, int>.GetDictionary(args);
            if (Script.IsValue(dict["length"]) && dict["length"] == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calling this on a null string would be either an error or vacuously impossible in real C#, but since this
        /// gets compiled to a static utility method in Saltarelle, it gives us a one-line way to check this condition
        /// rather than three-line 'if (string.isNullOrEmpty()) {}' conditionals everywhere.
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Overload of same IsNullOrEmpty for generic List objects
        /// <param name="list">the List object to check</param>
        /// </summary>
        /// <typeparam name="T">The object type contained by the List</typeparam>
        [IncludeGenericArguments(false)]
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Overload of same IsNullOrEmpty for generic JsArray objects
        /// <param name="array">the array object to check</param>
        /// </summary>
        /// <typeparam name="T">The object type contained by the List</typeparam>
        [IncludeGenericArguments(false)]
        public static bool IsNullOrEmpty<T>(this JsArray<T> array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// check if the given index is valid in the given array
        /// </summary>
        /// <param name="index">index of the array</param>
        /// <param name="arr">the array to check</param>
        /// <returns>true of false</returns>
        public static bool IsValidIndex(int index, Array arr)
        {
            return index >= 0 && index < arr.Length;
        }

        /// <summary>
        /// Checks if the given value is a non-null object.  Implementation cribbed from Underscore.
        /// </summary>
        /// <param name="o"></param>
        /// <returns>True if an object value</returns>
        [InlineCode("({o} === Object({o}))")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        public static bool IsObject(object o)
        {
            return false;
        }

        [InlineCode("{owner}.hasOwnProperty({field})")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        public static bool HasOwnProperty(object owner, string field)
        {
            return false;
        }

        /// <summary>
        /// Is given string a truthy value as defined by Tableau's query param truthy values?
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultIfMissing"></param>
        /// <returns>truthiness, or defaultIfMissing if the string is null or empty</returns>
        public static bool ToBoolean(string value, bool defaultIfMissing)
        {
            var positiveRegex = new Regex(@"^(yes|y|true|t|1)$", "i");

            if (IsNullOrEmpty(value))
            {
                return defaultIfMissing;
            }

            string[] match = value.Match(positiveRegex);
            return !IsNullOrEmpty(match);
        }

        /// <summary>
        /// Determines whether or not the user has the ability to view the data tab
        /// </summary>
        /// <returns>True if the data tab should be enabled, false if not</returns>
        public static bool ShouldShowDataTab()
        {
            return (TsConfig.AllowAddNewDatasource && FeatureFlags.IsEnabled(FeatureFlagIds.DataToTheWeb) &&
                (!TsConfig.IsMobile || (TsConfig.IsMobile && FeatureFlags.IsEnabled(FeatureFlagIds.DataToTheWebMobile))));
        }

        /// <summary>
        /// Replaces the query in the given URI with a new set of key/value pairs.
        /// </summary>
        /// <param name="uri">A uri</param>
        /// <param name="parameters">The set of query paramters to use</param>
        /// <returns>A uri with the query replaced(or appended)</returns>
        public static URLStr ReplaceUriQueryParameters(URLStr uri, JsDictionary<string, JsArray<string>> parameters)
        {
            if (parameters.Count == 0)
            {
                return uri;
            }

            StringBuilder newQueryString = new StringBuilder();
            bool first = true;
            Action appendSeparator = delegate
            {
                newQueryString.Append(first ? "?" : "&");
                first = false;
            };

            foreach (string key in Underscore.Keys(parameters))
            {
                List<string> vals = parameters[key];
                string keyEncoded = string.EncodeUriComponent(key);
                if (Script.IsNullOrUndefined(vals) || vals.Count == 0)
                {
                    appendSeparator();
                    newQueryString.Append(keyEncoded);
                }
                else
                {
                    foreach (string value in vals)
                    {
                        appendSeparator();
                        newQueryString.Append(keyEncoded).Append("=").Append(string.EncodeUriComponent(value));
                    }
                }
            }

            string hash = "";
            string baseUri = "";
            if (uri.ReinterpretAs<string>().Length > 0)
            {
                int indexOfQuery = ((string)uri).IndexOf("?");
                int indexOfHash = ((string)uri).IndexOf("#");

                int indexOfEnd = Math.Min(indexOfQuery < 0 ? ((string)uri).Length : indexOfQuery,
                    indexOfHash < 0 ? ((string)uri).Length : indexOfHash);
                baseUri = ((string)uri).Substr(0, indexOfEnd);
                hash = indexOfHash < 0 ? "" : ((string)uri).Substr(indexOfHash);
            }

            return baseUri + newQueryString + hash;
        }

        /// <summary>
        /// We can find that some of our bools are actually strings, like "false" or "true". c# won't catch this
        /// and makes us write a silly method to convert our bool to another bool.
        /// </summary>
        /// <param name="value">The bool that needs sanatized</param>
        /// <returns>Null/undefined if the value passed is null. Otherse it will pass back a parsed bool</returns>
        public static bool SanatizeBoolean(bool value)
        {
            if (Script.IsNullOrUndefined(value))
            {
                return value;
            }

            return value.ToString().ToLower() == "true";
        }

        /// <summary>
        /// Disposes the given object if non-null.
        /// </summary>
        /// <returns>returns null</returns>
        /// <typeparam name="T">The type of the class to dispose</typeparam>
        [IncludeGenericArguments(false)]
        public static T Dispose<T>(T d) where T : class, IDisposable
        {
            if (Script.IsValue(d))
            {
                d.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Disposes a list of disposables.
        /// </summary>
        /// <returns>returns null</returns>
        /// <typeparam name="T">The type of the class to dispose</typeparam>
        [IncludeGenericArguments(false)]
        public static List<T> Dispose<T>(List<T> d)
            where T : IDisposable
        {
            if (Script.IsValue(d))
            {
                foreach (var v in d)
                {
                    if (Script.IsValue(v))
                    {
                        v.Dispose();
                    }
                }
                d.Clear();
            }
            return null;
        }

        /// <summary>
        /// Safely calls Window.ClearTimeout for the provided handle, which was previously created using Window.SetTimeout.
        /// Returns null so that you can clear the timeout and assign in one statement, e.g.:
        /// myTimeout = MiscUtil.ClearTimeout(myTimeout);
        /// </summary>
        public static int? ClearTimeout(int? handle)
        {
            if (handle.HasValue)
            {
                Window.ClearTimeout(handle.Value);
            }
            return null;
        }

        public static int? ClearInterval(int? handle)
        {
            if (handle.HasValue)
            {
                Window.ClearInterval(handle.Value);
            }
            return null;
        }

        /// <summary>
        /// Returns a deep clone of the given object
        /// </summary>
        public static object CloneObject(object src)
        {
            string objStr = Json.Stringify(src, (k, v) =>
            {
                /*
                 * This is necessary because the runtime HUTT serializer sends over data
                 * in associative containers and javascript does not have a definition for handling them.
                 * This code snippet forces them to be interpreted as standard arrays for the purpose
                 * of serialization.
                 */
#if REMOVE_WHEN_SUPPORTED_IN_DESALT
                if (v is Uint32Array || v is Int32Array || v is Float64Array || v is Float32Array)
                {
                    // cannot call slice on v directly because typed arrays do not support slice in IE
                    // below should compile to `Array.prototype.slice.call(v);`
                    return ((dynamic)typeof(JsArray<object>).Prototype).slice.call(v);
                }
#endif
                return v;
            });

            return Json.Parse(objStr);
        }
    }
}
