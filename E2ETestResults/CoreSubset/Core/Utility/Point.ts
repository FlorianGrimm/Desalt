/**
 * Basic X,Y point.  Use PointUtil to manipulate.
 */
export class Point {
  public x: number;

  public y: number;

  public constructor(x: number, y: number) {
    this.x = x;
    this.y = y;
  }
}

/**
 * A variation of Point record that uses floats instead of ints.
 */
export class PointD {
  public x: number;

  public y: number;

  public constructor(x: number, y: number) {
    this.x = x;
    this.y = y;
  }
}
