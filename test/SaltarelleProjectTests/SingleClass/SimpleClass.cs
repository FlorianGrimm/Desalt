// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleClass.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace SimpleNamespace
{
    using System.Html;

    /// <summary>
    /// Just a few things to test the simple stuff.
    /// </summary>
    public class SimpleClass
    {
        private int width = 120;

        public SimpleClass()
        {
            // ctor
            this.width = 140;
        }

        public HtmlElement CreateElement()
        {
            var div = (HtmlElement)Document.CreateElement("div");
            div.InnerHTML = "Hello, world";
            div.Style.Width = this.width + "px";
            Document.Body.AppendChild(div);

            return div;
        }
    }
}
