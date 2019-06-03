#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    /// <summary>
    /// Helper class for financial dates
    /// </summary>
    static public class AdjustableOrAdjustedDateHelper
    {
        public static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create(ItemsChoiceType.adjustedDate.ToString(), adjustedDate);
            var items = new object[1]; 
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType[1];
            itemsElementName[0] = ItemsChoiceType.adjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create(ItemsChoiceType.adjustedDate.ToString(), adjustedDate);
            object[] items;
            ItemsChoiceType[] itemsElementName;
            if (businessDayAdjustments != null)
            {
                items = new object[2];
                items[0] = identifiedDate;
                items[1] = businessDayAdjustments;
                itemsElementName = new ItemsChoiceType[2];
                itemsElementName[0] = ItemsChoiceType.adjustedDate;
                itemsElementName[1] = ItemsChoiceType.dateAdjustments;
            }
            else
            {
                items = new object[1];
                items[0] = identifiedDate;
                itemsElementName = new ItemsChoiceType[1];
                itemsElementName[0] = ItemsChoiceType.adjustedDate;
            }
            date.Items = items;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateUnadjustedDate(DateTime unadjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create(ItemsChoiceType.unadjustedDate.ToString(), unadjustedDate);
            var items = new object[1];
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType[1];
            itemsElementName[0] = ItemsChoiceType.unadjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateUnadjustedDate(DateTime unadjustedDate, string businessDayConventionAsString, string businessCentersAsString)
        {
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConventionAsString, businessCentersAsString);
            return CreateUnadjustedDate(unadjustedDate, businessDayAdjustments);
        }

        public static AdjustableOrAdjustedDate CreateUnadjustedDate(DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = IdentifiedDateHelper.Create(ItemsChoiceType.unadjustedDate.ToString(), unadjustedDate);
            object[] items;
            ItemsChoiceType[] itemsElementName;
            if (businessDayAdjustments != null)
            {
                items = new object[2];
                items[0] = identifiedDate;
                items[1] = businessDayAdjustments;
                itemsElementName = new ItemsChoiceType[2];
                itemsElementName[0] = ItemsChoiceType.unadjustedDate;
                itemsElementName[1] = ItemsChoiceType.dateAdjustments;
            }
            else
            {
                items = new object[1];
                items[0] = identifiedDate;
                itemsElementName = new ItemsChoiceType[1];
                itemsElementName[0] = ItemsChoiceType.unadjustedDate;
            }
            date.Items = items;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate Create(DateTime? unadjustedDate, DateTime? adjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var date = new AdjustableOrAdjustedDate();
            var items = new List<object>();
            var itemsElementName = new List<ItemsChoiceType>();
            if (unadjustedDate == null && adjustedDate == null)
                return date;
            if (unadjustedDate != null)
            {
                items.Add(IdentifiedDateHelper.Create(ItemsChoiceType.unadjustedDate.ToString(), (DateTime)unadjustedDate));
                itemsElementName.Add(ItemsChoiceType.unadjustedDate);
            }
            if (adjustedDate != null)
            {
                items.Add(IdentifiedDateHelper.Create(ItemsChoiceType.adjustedDate.ToString(), (DateTime)adjustedDate));
                itemsElementName.Add(ItemsChoiceType.adjustedDate);
            }
            if (businessDayAdjustments != null)
            {
                items.Add(businessDayAdjustments);
                itemsElementName.Add(ItemsChoiceType.dateAdjustments);
            }
            date.Items = items.ToArray();
            date.ItemsElementName = itemsElementName.ToArray();
            return date;
        }

        public static Boolean Contains(AdjustableOrAdjustedDate adjustableOrAdjustedDate, ItemsChoiceType item, out object dateOrBusinessConvention)
        {
            dateOrBusinessConvention = null;
            if (adjustableOrAdjustedDate.Items == null)
            {
                return false;
            }
            if (adjustableOrAdjustedDate.ItemsElementName != null)
            {
                for (var i = 0; i < adjustableOrAdjustedDate.ItemsElementName.Length; i++)
                {
                    if (adjustableOrAdjustedDate.ItemsElementName[i] != item) continue;
                    dateOrBusinessConvention = adjustableOrAdjustedDate.Items[i];
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}