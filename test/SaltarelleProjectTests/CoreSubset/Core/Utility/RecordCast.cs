// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RecordCast.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using TypeDefs;

    /// <summary>
    /// Use this class to treat records (basic property bag objects) like other records.
    /// You want to do so because the records provide the same properties/interface.
    /// Does not make copies or use memory.  (We care less about the function overhead.)
    /// Generally, casting to smaller records (sub-sets of properties) is safe, while casting larger is not...
    /// </summary>
    public static class RecordCast
    {
        public static RectXY RectPresModelAsRectXY(RectanglePresModel rpm)
        {
            return rpm == null ? null : new RectXY(rpm.Left, rpm.Top, rpm.Width, rpm.Height);
        }

        public static DoubleRectXY DoubleRectPresModelAsDoubleRectXY(DoubleRectanglePresModel rpm)
        {
            return rpm == null ? null : new DoubleRectXY(rpm.Left, rpm.Top, rpm.Width, rpm.Height);
        }

        public static SizePresModel SizeAsSizePresModel(Size sz)
        {
            return sz == null ? null : new SizePresModel { Width = sz.Width, Height = sz.Height };
        }
    }
}
