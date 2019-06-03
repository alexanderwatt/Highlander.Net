#region Usings

using System;

#endregion

namespace FpML.V5r3.Confirmation
{
    /// <summary>
    /// Helper class for financial dates
    /// </summary>
    static public class AdjustableOrAdjustedDateHelper
    {
        public static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create("AdjustedDate", adjustedDate);
            var items = new object[1]; 
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType[1];
            itemsElementName[0] = ItemsChoiceType.adjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateUnadjustedDate(DateTime unadjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create("UnadjustedDate", unadjustedDate);
            var items = new object[1];
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType[1];
            itemsElementName[0] = ItemsChoiceType.unadjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }
    }
}