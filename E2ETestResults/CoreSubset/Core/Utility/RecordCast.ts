import { DoubleRectanglePresModel, RectanglePresModel, SizePresModel } from 'TypeDefs';

import { DoubleRectXY, RectXY } from './Rect';

import { Size } from './Size';

/**
 * Use this class to treat records (basic property bag objects) like other records.
 * You want to do so because the records provide the same properties/interface.
 * Does not make copies or use memory.  (We care less about the function overhead.)
 * Generally, casting to smaller records (sub-sets of properties) is safe, while casting larger is not...
 */
export class RecordCast {
  public static rectPresModelAsRectXY(rpm: RectanglePresModel): RectXY {
    return rpm === null ? null : new RectXY(rpm.x, rpm.y, rpm.w, rpm.h);
  }

  public static doubleRectPresModelAsDoubleRectXY(rpm: DoubleRectanglePresModel): DoubleRectXY {
    return rpm === null ? null : new DoubleRectXY(rpm.doubleLeft, rpm.doubleTop, rpm.width, rpm.height);
  }

  public static sizeAsSizePresModel(sz: Size): SizePresModel {
    return sz === null ? null : new SizePresModel();
  }
}
