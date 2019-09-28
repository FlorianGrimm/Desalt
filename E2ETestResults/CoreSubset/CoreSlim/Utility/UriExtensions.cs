// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="UriExtensions.cs" company="Tableau Software">
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
    using Tableau.JavaScript.Vql.TypeDefs;

    public static class UriExtensions
    {
        /// <summary>
        /// Parses the URI's query parameters and returns a map containing each key/value pair.  Each
        /// query parameter and value is also URI decoded.
        /// </summary>
        /// <param name="uri">The URI to decode, must contain a ? in order to indicate the start of params</param>
        /// <returns>A set of key value pairs</returns>
        public static JsDictionary<string, JsArray<string>> GetUriQueryParameters(URLStr uri)
        {
            var parameters = new JsDictionary<string, JsArray<string>>();

            if (Script.IsNullOrUndefined(uri)) { return parameters; }

            int indexOfQuery = ((string)uri).IndexOf("?");
            if (indexOfQuery < 0) { return parameters; }

            string query = ((string)uri).Substr(indexOfQuery + 1);
            int indexOfHash = query.IndexOf("#");
            if (indexOfHash >= 0)
            {
                query = query.Substr(0, indexOfHash);
            }

            if (string.IsNullOrEmpty(query)) { return parameters; }

            string[] paramPairs = query.Split("&");
            foreach (string pair in paramPairs)
            {
                string[] keyValue = pair.Split("=");
                string key = string.DecodeUriComponent(keyValue[0]);
                List<string> values;
                if (parameters.ContainsKey(key))
                {
                    values = parameters[key];
                }
                else
                {
                    values = new List<string>();
                    parameters[key] = values;
                }

                if (keyValue.Length > 1)
                {
                    values.Add(string.DecodeUriComponent(keyValue[1]));
                }
            }

            return parameters;
        }
    }
}
