export const enum CssPosition {
  Static = 'static',
  Relative = 'relative',
  Absolute = 'absolute',
  Fixed = 'fixed',
  Inherit = 'inherit',
}

export const enum CssSizeKeyword {
  BorderBox = 'border-box',
  ContentBox = 'content-box',
  Available = 'available',
  MinContent = 'min-content',
  MaxContent = 'max-content',
  FitContent = 'fit-content',
  Complex = 'complex',
  Auto = 'auto',
}

export const enum CSSTextOverflow {
  Clip = 'clip',
  Ellipsis = 'ellipsis',
  Inherit = 'inherit',
  Initial = 'initial',
}

export const enum CssOverflow {
  Visible = 'visible',
  Hidden = 'hidden',
  Scroll = 'scroll',
  Auto = 'auto',
}

export const enum CssPointerEvent {
  Auto = 'auto',
  None = 'none',
  VisiblePainted = 'visiblePainted',
  VisibleFill = 'visibleFill',
  VisibleStroke = 'visibleStroke',
  Visible = 'visible',
  Painted = 'painted',
  Fill = 'fill',
  Stroke = 'stroke',
  All = 'all',
  Inherit = 'inherit',
}

export const enum CssVisibility {
  Visible = 'visible',
  Hidden = 'hidden',
  Collapse = 'collapse',
}

export const enum CssDisplay {
  None = 'none',
  Inline = 'inline',
  Block = 'block',
  Flex = 'flex',
  InlineBlock = 'inline-block',
  TableCell = 'table-cell',
  Table = 'table',
}

export const enum CssTextAlign {
  Left = 'left',
  Right = 'right',
  Center = 'center',
  Justify = 'justify',
}

export const enum CssWhiteSpace {
  Normal = 'normal',
  Nowrap = 'nowrap',
  Pre = 'pre',
  PreWrap = 'pre-wrap',
  PreLine = 'pre-line',
}

export const enum CssTextDecorationLine {
  None = 'none',
  Underline = 'underline',
  Overline = 'overline',
  LineThrough = 'line-through',
}

/**
 * Utility class for giving us a compiler safe way of specifying CSS attributes to be passed to
 * {@link jQueryObject.CSS}.  All of this should compile away and at runtime we're just
 * creating JS objects.
 * Please add new CSS properties to this class as you need them.
 * @example The following code:
 * `
 * this.Template.DomRoot.CSS(new CssDictionary
 * {
 * Width = CssSize.Px(zoneVm.Width),
 * Height = zoneVm.Height.AsPx(),
 * Top = zoneVm.Y.AsPx(),
 * Left = zoneVm.X.AsPx(),
 * });
 * `
 * would compile down to:
 * `
 * this.get_template().get_domRoot().css({
 * width: zoneVm.get_width() + 'px',
 * height: zoneVm.get_height() + 'px',
 * top: zoneVm.get_y() + 'px',
 * left: zoneVm.get_x() + 'px'
 * });
 * `
 */
export class CssDictionary {
  public position: Object<CssPosition, string>;

  public top: Object<CssSize, string>;

  public left: Object<CssSize, string>;

  public right: Object<CssSize, string>;

  public bottom: Object<CssSize, string>;

  public width: Object<CssSize, CssSizeKeyword, string>;

  public height: Object<CssSize, CssSizeKeyword, string>;

  public margin: Object<CssSize, string>;

  public margin-top: Object<CssSize, string>;

  public margin-left: Object<CssSize, string>;

  public margin-bottom: Object<CssSize, string>;

  public margin-right: Object<CssSize, string>;

  public max-height: Object<CssSize, string>;

  public z-index: string;

  public background-color: string;

  public color: string;

  public opacity: string;

  public font-family: string;

  public font-size: Object<CssSize, string>;

  public font-style: string;

  public font-weight: string;

  public text-decoration: Object<CssTextDecorationLine, string>;

  public text-decoration-line: Object<CssTextDecorationLine, string>;

  public white-space: Object<CssWhiteSpace, string>;

  public padding-top: Object<CssSize, string>;

  public padding-right: Object<CssSize, string>;

  public padding-bottom: Object<CssSize, string>;

  public padding-left: Object<CssSize, string>;

  public border: string;

  public box-shadow: string;

  public border-width: Object<CssSize, string>;

  public border-top-width: Object<CssSize, string>;

  public border-left-width: Object<CssSize, string>;

  public border-bottom-width: Object<CssSize, string>;

  public border-right-width: Object<CssSize, string>;

  public border-color: string;

  public border-top-color: string;

  public border-left-color: string;

  public border-bottom-color: string;

  public border-right-color: string;

  public border-style: string;

  public border-top-style: string;

  public border-left-style: string;

  public border-bottom-style: string;

  public border-right-style: string;

  public text-align: Object<CssTextAlign, string>;

  public line-height: string;

  public box-sizing: CssSizeKeyword;

  public display: Object<CssDisplay, string>;

  public pointer-events: Object<CssPointerEvent, string>;

  public visibility: Object<CssVisibility, string>;

  public textOverflow: Object<CSSTextOverflow, string>;

  public overflow: Object<CssOverflow, string>;

  public overflow-x: Object<CssOverflow, string>;

  public overflow-y: Object<CssOverflow, string>;

  public flex-grow: string;

  public flex-shrink: string;

  public flex-basis: Object<CssSize, string>;

  public order: string;

  public transform: string;

  public transform-origin: string;

  public constructor() { }
}

export class CssSize {
  private constructor() { }

  public static op_Implicit(s: CssSize): string {
    return s;
  }
}

export class CssHelpers {
}
