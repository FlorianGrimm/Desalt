/**
 * Rectangle record for a (left, top) based clipRect. Four script fields: l, t, w, h.
 */
export class Rect {
  public l: number;

  public t: number;

  public w: number;

  public h: number;

  public constructor(left: number, top: number, width: number, height: number) {
    this.l = left;
    this.t = top;
    this.w = width;
    this.h = height;
  }
}

/**
 * Rectangle record for a (x, y) based clipRect. Four script fields: x, y, w, h.
 */
export class RectXY {
  public x: number;

  public y: number;

  public w: number;

  public h: number;

  public constructor(x: number, y: number, width: number, height: number) {
    this.x = x;
    this.y = y;
    this.w = width;
    this.h = height;
  }
}

/**
 * Rectangle record for a (x, y) based rect. Four script fields: x, y, w, h.
 */
export class DoubleRectXY {
  public x: number;

  public y: number;

  public w: number;

  public h: number;

  public constructor(x: number, y: number, width: number, height: number) {
    this.x = x;
    this.y = y;
    this.w = width;
    this.h = height;
  }
}

/**
 * BBoxRectD record for a bounding box rect. Four script fields: x, y, w, h.
 */
export class BBoxRectD {
  public minX: number;

  public minY: number;

  public maxX: number;

  public maxY: number;

  public constructor(minX: number, minY: number, maxX: number, maxY: number) {
    this.minX = minX;
    this.minY = minY;
    this.maxX = maxX;
    this.maxY = maxY;
  }
}
