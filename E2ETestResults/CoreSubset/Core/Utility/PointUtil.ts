import { DoubleUtil } from './DoubleUtil';

import { jQueryPosition } from 'jQuery';

import 'mscorlib';

import { Param } from './Param';

import { Point, PointD } from './Point';

import { PointPresModel } from 'TypeDefs';

/**
 * This class provides methods that operate on Point records.
 */
export class PointUtil {
  public static fromPresModel(pointPM: PointPresModel): Point {
    if (ss.isNullOrUndefined(pointPM)) {
      return null;
    }
    return new Point(pointPM.x, pointPM.y);
  }

  public static toPresModel(pt: Point): PointPresModel {
    if (ss.isNullOrUndefined(pt)) {
      return null;
    }
    let pointPM: PointPresModel = new PointPresModel();
    return pointPM;
  }

  /**
   * Converts a {@link jQueryPosition} to a {@link Point}.
   * @param position The position to convert.
   * @returns A new {@link Point} representing the same coordinates as .
   */
  public static fromPosition(position: jQueryPosition): Point {
    return new Point(DoubleUtil.roundToInt(position.left), DoubleUtil.roundToInt(position.top));
  }

  /**
   * Converts a {@link PointD} to a {@link Point}.
   */
  public static fromPointD(pointD: PointD): Point {
    return new Point(DoubleUtil.roundToInt(pointD.x), DoubleUtil.roundToInt(pointD.y));
  }

  /**
   * Adds first and second and returns the result. Normally this would be an operator overload, but Script#
   * doesn't support them.
   * @param first The first point.
   * @param second The second point.
   * @returns The sum of first and second.
   */
  public static add(first: Point, second: Point): Point {
    if (ss.isNullOrUndefined(first) || ss.isNullOrUndefined(second)) {
      return first;
    }
    return new Point(first.x + second.x, first.y + second.y);
  }

  /**
   * Subtracts first - second and returns the result. Normally this would be an operator overload, but Script#
   * doesn't support them.
   * @param first The first point.
   * @param second The second point.
   * @returns The difference of first - second.
   */
  public static subtract(first: Point, second: Point): Point {
    return new Point(first.x - second.x, first.y - second.y);
  }

  /**
   * Multiplys first * second and returns the product of the X components and Y components.
   * @param first The first point.
   * @param second The second point.
   * @returns The product of first * second.
   */
  public static multiply(first: Point, second: Point): Point {
    return new Point(first.x * second.x, first.y * second.y);
  }

  /**
   * Gets the distance between the first and second points.
   * @param first The first point.
   * @param second The second point.
   * @returns The distance between first and second
   */
  public static distance(first: Point, second: Point): number {
    Param.verifyValue(first, 'first');
    Param.verifyValue(second, 'second');
    let diffX: number = first.x - second.x;
    let diffY: number = first.y - second.y;
    return Math.sqrt((diffX * diffX) + (diffY * diffY));
  }

  /**
   * Quick distance test between two points
   * @param first The first point.
   * @param second The second point.
   * @param distance Maximum distance between the two points.
   * @returns true if points are less than or equal to the specified distance
   */
  public static isWithinDistance(first: Point, second: Point, distance: number): boolean {
    let diffX: number = first.x - second.x;
    let diffY: number = first.y - second.y;
    return ((diffX * diffX) + (diffY * diffY)) <= (distance * distance);
  }

  /**
   * Tests if two points are equal.
   * @param p The first point to test
   * @param p2 The second point to test
   * @returns True if p and p2 are not null and p.x,y equal p2.x,y
   */
  public static equals(p: Point, p2: Point): boolean {
    return ss.isValue(p) && ss.isValue(p2) && p2.x === p.x && p2.y === p.y;
  }

  public static timesScalar(p: Point, scalar: number): Point {
    return new Point(p.x * scalar, p.y * scalar);
  }
}

/**
 * This class provides methods that operate on PointD records.
 */
export class PointDUtil {
  /**
   * Subtracts first - second and returns the result. Normally this would be an operator overload, but Script#
   * doesn't support them.
   * @param first The first point.
   * @param second The second point.
   * @returns The difference of first - second.
   */
  public static subtract(first: PointD, second: PointD): PointD {
    return new PointD(first.x - second.x, first.y - second.y);
  }

  public static timesScalar(p: Point, scalar: number): PointD {
    return new PointD(p.x * scalar, p.y * scalar);
  }

  public static round(p: PointD): Point {
    return new Point(Math.round(p.x), Math.round(p.y));
  }
}
