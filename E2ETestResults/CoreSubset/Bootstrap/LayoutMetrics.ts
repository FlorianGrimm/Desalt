import { ClientUIMetricType, tsConfig } from 'TypeDefs';

import 'mscorlib';

/**
 * UIMetricType enums
 */
export enum UIMetricType {
  scrollbar = 0,
  qfixed = 1,
  qslider = 2,
  qreadout = 3,
  cfixed = 4,
  citem = 5,
  hfixed = 6,
  hitem = 7,
  cmslider = 8,
  cmdropdown = 9,
  cmpattern = 10,
  rdate = 11,
  rdatep = 12,
  capply = 13,
  cmtypeinsearch = 14,
  ccustomitem = 15,
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
  private static defaultDb: Object<UIMetricType, Metric>;

  private static metricToClientMetric: Object<UIMetricType, ClientUIMetricType> = {};

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    LayoutMetrics.defaultDb = {};
    LayoutMetrics.defaultDb[UIMetricType.scrollbar] = new Metric(17, 17);
    LayoutMetrics.defaultDb[UIMetricType.qfixed] = new Metric(0, 5);
    LayoutMetrics.defaultDb[UIMetricType.qslider] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.qreadout] = new Metric(0, 20);
    LayoutMetrics.defaultDb[UIMetricType.cfixed] = new Metric(0, 6);
    LayoutMetrics.defaultDb[UIMetricType.citem] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.ccustomitem] = new Metric(0, 9);
    LayoutMetrics.defaultDb[UIMetricType.hfixed] = new Metric(0, 21);
    LayoutMetrics.defaultDb[UIMetricType.hitem] = new Metric(0, 18);
    LayoutMetrics.defaultDb[UIMetricType.cmslider] = new Metric(0, 49);
    LayoutMetrics.defaultDb[UIMetricType.cmdropdown] = new Metric(0, 29);
    LayoutMetrics.defaultDb[UIMetricType.cmpattern] = new Metric(0, 29);
    LayoutMetrics.defaultDb[UIMetricType.capply] = new Metric(0, 21);
    LayoutMetrics.defaultDb[UIMetricType.cmtypeinsearch] = new Metric(0, 22);
    LayoutMetrics.defaultDb[UIMetricType.rdate] = new Metric(0, 28);
    if (tsConfig.is_mobile) {
      LayoutMetrics.defaultDb[UIMetricType.scrollbar] = new Metric(0, 0);
    }
    LayoutMetrics.metricToClientMetric[UIMetricType.scrollbar] = ClientUIMetricType.scrollbar-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.qfixed] = ClientUIMetricType.q-filter-fixed-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.qslider] = ClientUIMetricType.q-filter-slider-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.qreadout] = ClientUIMetricType.q-filter-readout-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.cfixed] = ClientUIMetricType.c-filter-fixed-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.citem] = ClientUIMetricType.c-filter-item-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.capply] = ClientUIMetricType.c-filter-apply-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.hfixed] = ClientUIMetricType.h-filter-fixed-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.hitem] = ClientUIMetricType.h-filter-item-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.cmslider] = ClientUIMetricType.cm-slider-filter-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.cmdropdown] = ClientUIMetricType.cm-dropdown-filter-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.cmpattern] = ClientUIMetricType.cm-pattern-filter-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.rdate] = ClientUIMetricType.r-date-filter-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.rdatep] = ClientUIMetricType.r-date-p-filter-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.cmtypeinsearch] = ClientUIMetricType.cm-type-in-search-metric;
    LayoutMetrics.metricToClientMetric[UIMetricType.ccustomitem] = ClientUIMetricType.c-filter-custom-item-metric;
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
    (<any>this)[UIMetricType.scrollbar] = new Metric(scrollbarSize.w, scrollbarSize.h);
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
    let sb: StringBuilder = new StringBuilder();
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

export class Metric {
  public w: number;

  public h: number;

  public constructor(w: number, h: number) {
    this.w = w;
    this.h = h;
  }
}
