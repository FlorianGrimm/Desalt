// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CssDictionary.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    using jQueryApi;

    [Imported, NamedValues]
    public enum CssPosition
    {
        Static,
        Relative,
        Absolute,
        Fixed,
        Inherit
    }

    [Imported, NamedValues]
    public enum CssSizeKeyword
    {
        [ScriptName("border-box")]
        BorderBox,
        [ScriptName("content-box")]
        ContentBox,
        Available,
        [ScriptName("min-content")]
        MinContent,
        [ScriptName("max-content")]
        MaxContent,
        [ScriptName("fit-content")]
        FitContent,
        Complex,
        Auto
    }

    [Imported, NamedValues]
    public enum CSSTextOverflow
    {
        Clip,
        Ellipsis,
        Inherit,
        Initial
    }

    [Imported, NamedValues]
    public enum CssOverflow
    {
        Visible,
        Hidden,
        Scroll,
        Auto
    }

    [Imported]
    [NamedValues]
    public enum CssPointerEvent
    {
        Auto,
        None,
        VisiblePainted,
        VisibleFill,
        VisibleStroke,
        Visible,
        Painted,
        Fill,
        Stroke,
        All,
        Inherit
    }

    [Imported, NamedValues]
    public enum CssVisibility
    {
        Visible,
        Hidden,
        Collapse
    }

    [Imported, NamedValues]
    public enum CssDisplay
    {
        None,
        Inline,
        Block,
        Flex,
        [ScriptName("inline-block")]
        InlineBlock,
        [ScriptName("table-cell")]
        TableCell,
        Table
    }

    [Imported, NamedValues]
    public enum CssTextAlign
    {
        Left,
        Right,
        Center,
        Justify
    }

    [Imported, NamedValues]
    public enum CssWhiteSpace
    {
        Normal,
        Nowrap,
        Pre,
        [ScriptName("pre-wrap")]
        PreWrap,
        [ScriptName("pre-line")]
        PreLine
    }

    [Imported, NamedValues]
    public enum CssTextDecorationLine
    {
        None,
        Underline,
        Overline,
        [ScriptName("line-through")]
        LineThrough
    }

    /// <summary>
    /// Utility class for giving us a compiler safe way of specifying CSS attributes to be passed to
    /// <see cref="jQueryObject.CSS(JsDictionary)"/>.  All of this should compile away and at runtime we're just
    /// creating JS objects.
    /// Please add new CSS properties to this class as you need them.
    /// </summary>
    /// <example>
    /// The following code:
    /// <code>
    /// this.Template.DomRoot.CSS(new CssDictionary
    /// {
    ///     Width = CssSize.Px(zoneVm.Width),
    ///     Height = zoneVm.Height.AsPx(),
    ///     Top = zoneVm.Y.AsPx(),
    ///     Left = zoneVm.X.AsPx(),
    /// });
    /// </code>
    /// would compile down to:
    /// <code>
    /// this.get_template().get_domRoot().css({
    ///     width: zoneVm.get_width() + 'px',
    ///     height: zoneVm.get_height() + 'px',
    ///     top: zoneVm.get_y() + 'px',
    ///     left: zoneVm.get_x() + 'px'
    /// });
    /// </code>
    /// </example>
    [Imported, Serializable]
    public class CssDictionary
    {
        public TypeOption<CssPosition, string> Position;

        public TypeOption<CssSize, string> Top;

        public TypeOption<CssSize, string> Left;

        public TypeOption<CssSize, string> Right;

        public TypeOption<CssSize, string> Bottom;

        public TypeOption<CssSize, CssSizeKeyword, string> Width;

        public TypeOption<CssSize, CssSizeKeyword, string> Height;

        public TypeOption<CssSize, string> Margin;

        [ScriptName("margin-top")]
        public TypeOption<CssSize, string> MarginTop;

        [ScriptName("margin-left")]
        public TypeOption<CssSize, string> MarginLeft;

        [ScriptName("margin-bottom")]
        public TypeOption<CssSize, string> MarginBottom;

        [ScriptName("margin-right")]
        public TypeOption<CssSize, string> MarginRight;

        [ScriptName("max-height")]
        public TypeOption<CssSize, string> MaxHeight;

        [ScriptName("z-index")]
        public string ZIndex;

        [ScriptName("background-color")]
        public string BackgroundColor;

        public string Color;

        public string Opacity;

        [ScriptName("font-family")]
        public string FontFamily;

        [ScriptName("font-size")]
        public TypeOption<CssSize, string> FontSize;

        [ScriptName("font-style")]
        public string FontStyle;

        [ScriptName("font-weight")]
        public string FontWeight;

        [ScriptName("text-decoration")]
        public TypeOption<CssTextDecorationLine, string> TextDecoration;

        [ScriptName("text-decoration-line")]
        public TypeOption<CssTextDecorationLine, string> TextDecorationLine;

        [ScriptName("white-space")]
        public TypeOption<CssWhiteSpace, string> WhiteSpace;

        // PhantomJS converts padding to padding-top, padding-right and etc
        // use explicit padding properties to avoid unit test surprise failures
        [ScriptName("padding-top")]
        public TypeOption<CssSize, string> PaddingTop;

        [ScriptName("padding-right")]
        public TypeOption<CssSize, string> PaddingRight;

        [ScriptName("padding-bottom")]
        public TypeOption<CssSize, string> PaddingBottom;

        [ScriptName("padding-left")]
        public TypeOption<CssSize, string> PaddingLeft;

        public string Border;

        [ScriptName("box-shadow")]
        public string BoxShadow;

        [ScriptName("border-width")]
        public TypeOption<CssSize, string> BorderWidth;

        [ScriptName("border-top-width")]
        public TypeOption<CssSize, string> BorderTopWidth;

        [ScriptName("border-left-width")]
        public TypeOption<CssSize, string> BorderLeftWidth;

        [ScriptName("border-bottom-width")]
        public TypeOption<CssSize, string> BorderBottomWidth;

        [ScriptName("border-right-width")]
        public TypeOption<CssSize, string> BorderRightWidth;

        [ScriptName("border-color")]
        public string BorderColor;

        [ScriptName("border-top-color")]
        public string BorderTopColor;

        [ScriptName("border-left-color")]
        public string BorderLeftColor;

        [ScriptName("border-bottom-color")]
        public string BorderBottomColor;

        [ScriptName("border-right-color")]
        public string BorderRightColor;

        [ScriptName("border-style")]
        public string BorderStyle;

        [ScriptName("border-top-style")]
        public string BorderTopStyle;

        [ScriptName("border-left-style")]
        public string BorderLeftStyle;

        [ScriptName("border-bottom-style")]
        public string BorderBottomStyle;

        [ScriptName("border-right-style")]
        public string BorderRightStyle;

        [ScriptName("text-align")]
        public TypeOption<CssTextAlign, string> TextAlign;

        [ScriptName("line-height")]
        public string LineHeight;

        [ScriptName("box-sizing")]
        public CssSizeKeyword BoxSizing;

        public TypeOption<CssDisplay, string> Display;

        [ScriptName("pointer-events")]
        public TypeOption<CssPointerEvent, string> PointerEvents;

        public TypeOption<CssVisibility, string> Visibility;

        public TypeOption<CSSTextOverflow, string> TextOverflow;

        public TypeOption<CssOverflow, string> Overflow;

        [ScriptName("overflow-x")]
        public TypeOption<CssOverflow, string> OverflowX;

        [ScriptName("overflow-y")]
        public TypeOption<CssOverflow, string> OverflowY;

        [ScriptName("flex-grow")]
        public string FlexGrow;

        [ScriptName("flex-shrink")]
        public string FlexShrink;

        [ScriptName("flex-basis")]
        public TypeOption<CssSize, string> FlexBasis;

        [ScriptName("order")]
        public string Order;

        [ScriptName("transform")]
        public string Transform;

        [ScriptName("transform-origin")]
        public string TransformOrigin;

        [ObjectLiteral]
        public CssDictionary() { }

        [InlineCode("{d}")]
        public static implicit operator JsDictionary(CssDictionary d)
        {
            return d;
        }

        [InlineCode("{d}")]
        public static implicit operator JsDictionary<string, string>(CssDictionary d)
        {
            return d;
        }
    }

    [Imported, Serializable]
    public sealed class CssSize
    {
        private CssSize() { }

        [ScriptSkip]
        public static implicit operator string(CssSize s)
        {
            return s;
        }
    }

    [Imported]
    public static class CssHelpers
    {
        [InlineCode("{i} + 'px'"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "i")]
        public static CssSize AsPx(this int i)
        {
            return null;
        }

        [InlineCode("{i} + 'px'"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "i")]
        public static CssSize AsPx(this uint i)
        {
            return null;
        }

        [InlineCode("{i} + 'px'"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "i")]
        public static CssSize AsPx(this double i)
        {
            return null;
        }

        [InlineCode("{i} + 'pt'"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "i")]
        public static CssSize AsPt(this int i)
        {
            return null;
        }

        [InlineCode("{i} + 'pt'"), SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "i")]
        public static CssSize AsPt(this uint i)
        {
            return null;
        }

        [InlineCode("'scale(' + {x} + ',' + {y} + ')'"), SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "x"), SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "y")]
        public static string AsTransformScale(float x, float y)
        {
            return null;
        }
    }
}
