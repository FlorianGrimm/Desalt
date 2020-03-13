// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlFragmentParser.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.Xml;

    /// <summary>
    /// An XML parser that is designed to parse small fragments of XML such as those that appear in
    /// documentation comments.
    /// </summary>
    /// <remarks>
    /// Heavily borrowed from the Roslyn source at <see href="http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Shared/Utilities/XmlFragmentParser.cs"/>.
    /// </remarks>
    internal partial class XmlFragmentParser
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly XmlReaderSettings s_xmlSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
        };

        private XmlReader? _xmlReader;
        private readonly Reader _textReader = new Reader();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Parse the given XML fragment. The given callback is executed until either the end of the
        /// fragment is reached or an exception occurs.
        /// </summary>
        /// <typeparam name="TArg">
        /// Type of an additional argument passed to the <paramref name="callback"/> delegate.
        /// </typeparam>
        /// <param name="xmlFragment">The fragment to parse.</param>
        /// <param name="callback">Action to execute while there is still more to read.</param>
        /// <param name="arg">Additional argument passed to the callback.</param>
        /// <remarks>
        /// It is important that the <paramref name="callback"/> action advances the <see
        /// cref="XmlReader"/>, otherwise parsing will never complete.
        /// </remarks>
        public void ParseFragment<TArg>(string xmlFragment, Action<XmlReader, TArg> callback, TArg arg)
        {
            _textReader.SetText(xmlFragment);

            if (_xmlReader == null)
            {
                _xmlReader = XmlReader.Create(_textReader, s_xmlSettings);
            }

            try
            {
                while (!ReachedEnd)
                {
                    if (BeforeStart)
                    {
                        // skip over the synthetic root element and first node
                        _xmlReader.Read();
                    }
                    else
                    {
                        callback(_xmlReader, arg);
                    }
                }

                // read the final EndElement to reset things for the next user
                _xmlReader.ReadEndElement();
            }
            catch
            {
                // the reader is in a bad state, so dispose of it and recreate a new one next time we
                // get called
                _xmlReader.Dispose();
                _xmlReader = null;
                _textReader.Reset();
                throw;
            }
        }

        // Depth 0 = Document root
        // Depth 1 = Synthetic wrapper, "CurrentElement"
        // Depth 2 = Start of user's fragment.
        private bool BeforeStart => _xmlReader?.Depth < 2;

        private bool ReachedEnd =>
            _xmlReader?.Depth == 1 &&
            _xmlReader.NodeType == XmlNodeType.EndElement &&
            _xmlReader.LocalName == Reader.CurrentElementName;
    }
}
