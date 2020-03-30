import { Point } from './Point';

import { Rect } from './Rect';

/**
 * Represents interface to the viewport of the browser.
 */
export interface IBrowserViewport {
  readonly dimensions: Rect;
  /**
   * Translates the point (in document coordinates) into viewport coordinates.
   * @param p 
   * @returns The point relative to the given viewport
   */
  translatePositionToViewport(p: Point): Point;
  /**
   * Gets the amount of visible room around a given point.  Visible room is the amount
   * of space in each cardinal direction that is within the current viewport.
   * Warning: if the point itself is not visible, the results may not be meaningful.
   * @param position A point in document space
   * @param padding The amount of padding to include on all sides.  Defaults to 0
   */
  getVisibleRoom(position: Point, padding: number = 0): VisibleRoom;
  /**
   * Gets a rectangle in document space enclosing a given point that expresses the portion of the document visible in the browser viewport.
   * Warning: if the point itself is not visible, the results may not be meaningful.
   * @param point A point in document space
   */
  getDocumentViewport(point: Point): Rect;
}

export class VisibleRoom {
  public roomAbove: number;

  public roomBelow: number;

  public roomLeft: number;

  public roomRight: number;

  public constructor(roomAbove: number, roomBelow: number, roomLeft: number, roomRight: number) {
    this.roomAbove = roomAbove;
    this.roomBelow = roomBelow;
    this.roomLeft = roomLeft;
    this.roomRight = roomRight;
  }
}
