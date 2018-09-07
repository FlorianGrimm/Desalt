import { ClientUIMetricType, tsConfig } from 'TypeDefs';

import 'mscorlib';

/**
 * UIMetricType enums
 */
export const enum UIMetricType {
  Scrollbar = 'scrollbar',
  QFixed = 'qfixed',
  QSlider = 'qslider',
  QReadout = 'qreadout',
  CFixed = 'cfixed',
  CItem = 'citem',
  HFixed = 'hfixed',
  HItem = 'hitem',
  CmSlider = 'cmslider',
  CmDropdown = 'cmdropdown',
  CmPattern = 'cmpattern',
  RDate = 'rdate',
  RDateP = 'rdatep',
  CApply = 'capply',
  CmTypeInSearch = 'cmtypeinsearch',
  CCustomItem = 'ccustomitem',
}

/**
 * Contains the metrics for scrollbars, filters, and other items used to
 * determine the initial layouts. Also contains utility methods for working with
 * the hard-coded values.
 * 
 * These values are currently maintained in TWO locations.
 * C++: SetPortSizeCommand::DefaultMetrics() and
 * Saltarelle: Tableau.JavaScript.Vql.Bootstrap.LayoutMetrics.LayoutMetrics.
 * Make sure to keep both of them in sync.
 */
export class LayoutMetrics {
  private static defaultDb: { [key: string]: Metric };

  private static metricToClientMetric: { [key: string]: ClientUIMetricType } = {};

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    LayoutMetrics.defaultDb = {};
    LayoutMetrics.defaultDb[UIMetricType.Scrollbar] = new Metric(17, 17);
    LayoutMetrics.defaultDb[UIMetricType.QFixed] = new Metric(0, 5);
    LayoutMetrics.defaultDb[UIMetricType.QSlider] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.QReadout] = new Metric(0, 20);
    LayoutMetrics.defaultDb[UIMetricType.CFixed] = new Metric(0, 6);
    LayoutMetrics.defaultDb[UIMetricType.CItem] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.CCustomItem] = new Metric(0, 9);
    LayoutMetrics.defaultDb[UIMetricType.HFixed] = new Metric(0, 21);
    LayoutMetrics.defaultDb[UIMetricType.HItem] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.CmSlider] = new Metric(0, 49);
    LayoutMetrics.defaultDb[UIMetricType.CmDropdown] = new Metric(0, 29);
    LayoutMetrics.defaultDb[UIMetricType.CmPattern] = new Metric(0, 29);
    LayoutMetrics.defaultDb[UIMetricType.CApply] = new Metric(0, 21);
    LayoutMetrics.defaultDb[UIMetricType.CmTypeInSearch] = new Metric(0, 22);
    LayoutMetrics.defaultDb[UIMetricType.RDate] = new Metric(0, 28);
    if (tsConfig.is_mobile) {
      LayoutMetrics.defaultDb[UIMetricType.Scrollbar] = new Metric(0, 0);
    }
    LayoutMetrics.metricToClientMetric[UIMetricType.Scrollbar] = ClientUIMetricType.ScrollbarMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.QFixed] = ClientUIMetricType.QFilterFixedMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.QSlider] = ClientUIMetricType.QFilterSliderMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.QReadout] = ClientUIMetricType.QFilterReadoutMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CFixed] = ClientUIMetricType.CFilterFixedMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CItem] = ClientUIMetricType.CFilterItemMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CApply] = ClientUIMetricType.CFilterApplyMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.HFixed] = ClientUIMetricType.HFilterFixedMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.HItem] = ClientUIMetricType.HFilterItemMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CmSlider] = ClientUIMetricType.CmSliderFilterMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CmDropdown] = ClientUIMetricType.CmDropdownFilterMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CmPattern] = ClientUIMetricType.CmPatternFilterMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.RDate] = ClientUIMetricType.RDateFilterMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.RDateP] = ClientUIMetricType.RDatePFilterMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CmTypeInSearch] = ClientUIMetricType.CmTypeInSearchMetric;
    LayoutMetrics.metricToClientMetric[UIMetricType.CCustomItem] = ClientUIMetricType.CFilterCustomItemMetric;
  }

  /**
   * Initializes a new instance of the LayoutMetrics class.
   * @param scrollbarSize If not supplied the scrollbar sizes are measured, which can be expensive.
   */
  public constructor(scrollbarSize: Metric) {
    this.cloneDefaultDb();
    if (ss.isNullOrUndefined(scrollbarSize)) {
      let size: number = LayoutMetrics.getScrollbarSize();
      scrollbarSize = new Metric(size, size);
    }
    (<any>this)[UIMetricType.Scrollbar] = new Metric(scrollbarSize.w, scrollbarSize.h);
  }

  public static clone(other: LayoutMetrics): LayoutMetrics {
    let obj: LayoutMetrics = <LayoutMetrics>new LayoutMetrics(null);
    for (const entry of LayoutMetrics.defaultDb) {
      let v: Metric = <Metric>(<any>other)[entry.key];
      if (ss.isValue(v)) {
        (<any>obj)[entry.key] = v;
      }
    }
    return obj;
  }

  public static getScrollbarSize(): number {
    if (tsConfig.is_mobile) {
      return 0;
    }
    let outer: HTMLElement = <HTMLElement>document.createElement('div');
    let style: CSSStyleDeclaration = outer.style;
    style.width = '100px';
    style.height = '100px';
    style.overflow = 'scroll';
    style.position = 'absolute';
    style.top = '0px';
    style.filter = 'alpha(opacity=0)';
    style.opacity = '0';
    style.left = '0px';
    let inner: HTMLElement = <HTMLElement>document.createElement('div');
    inner.style.width = '400px';
    inner.style.height = '400px';
    outer.appendChild(inner);
    document.body.appendChild(outer);
    let width: number = outer.offsetWidth - outer.clientWidth;
    document.body.removeChild(outer);
    outer.removeChild(inner);
    outer = inner = null;
    width = width > 0 ? width : 9;
    return width;
  }

  public toJson(): string {
    let sb: ss.StringBuilder = new ss.StringBuilder();
    sb.append('{\n');
    let length: number = 0;
    for (const entry of LayoutMetrics.defaultDb) {
      length++;
    }
    let i: number = 0;
    for (const entry of LayoutMetrics.defaultDb) {
      if (ss.isValue((<any>this)[entry.key])) {
        let m: Metric = <Metric>(<any>this)[entry.key];
        sb.append('\t\"' + entry.key + '\": {\n');
        sb.append('\t\t\"w\": ' + m.w + ',\n');
        sb.append('\t\t\"h\": ' + m.h + '\n');
        sb.append('\t}');
        if (i < length - 1) {
          sb.append(',');
        }
        sb.append('\n');
      }
      ++i;
    }
    sb.append('}');
    return sb.toString();
  }

  private cloneDefaultDb(): void {
    for (const entry of LayoutMetrics.defaultDb) {
      (<any>this)[entry.key] = entry.value;
    }
  }
}

// Call the static constructor
LayoutMetrics.__ctor();

export class Metric extends Object {
  public w: number;

  public h: number;

  public constructor(w: number, h: number) {
    this.w = w;
    this.h = h;
  }
}
