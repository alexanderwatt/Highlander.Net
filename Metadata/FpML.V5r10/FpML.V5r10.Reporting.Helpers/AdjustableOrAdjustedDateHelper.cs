/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    /// <summary>
    /// Helper class for financial dates
    /// </summary>
    public static class AdjustableOrAdjustedDateHelper
    {
        public static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.adjustedDate.ToString(), adjustedDate);
            var items = new object[1]; 
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType1[1];
            itemsElementName[0] = ItemsChoiceType1.adjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.adjustedDate.ToString(), adjustedDate);
            object[] items;
            ItemsChoiceType1[] itemsElementName;
            if (businessDayAdjustments != null)
            {
                items = new object[2];
                items[0] = identifiedDate;
                items[1] = businessDayAdjustments;
                itemsElementName = new ItemsChoiceType1[2];
                itemsElementName[0] = ItemsChoiceType1.adjustedDate;
                itemsElementName[1] = ItemsChoiceType1.dateAdjustments;
            }
            else
            {
                items = new object[1];
                items[0] = identifiedDate;
                itemsElementName = new ItemsChoiceType1[1];
                itemsElementName[0] = ItemsChoiceType1.adjustedDate;
            }
            date.Items = items;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate CreateUnadjustedDate(DateTime unadjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.unadjustedDate.ToString(), unadjustedDate);
            var items = new object[1];
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType1[1];
            itemsElementName[0] = ItemsChoiceType1.unadjustedDate;
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
            var identifiedDate = ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.unadjustedDate.ToString(), unadjustedDate);
            object[] items;
            ItemsChoiceType1[] itemsElementName;
            if (businessDayAdjustments != null)
            {
                items = new object[2];
                items[0] = identifiedDate;
                items[1] = businessDayAdjustments;
                itemsElementName = new ItemsChoiceType1[2];
                itemsElementName[0] = ItemsChoiceType1.unadjustedDate;
                itemsElementName[1] = ItemsChoiceType1.dateAdjustments;
            }
            else
            {
                items = new object[1];
                items[0] = identifiedDate;
                itemsElementName = new ItemsChoiceType1[1];
                itemsElementName[0] = ItemsChoiceType1.unadjustedDate;
            }
            date.Items = items;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        public static AdjustableOrAdjustedDate Create(DateTime? unadjustedDate, DateTime? adjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var date = new AdjustableOrAdjustedDate();
            var items = new List<object>();
            var itemsElementName = new List<ItemsChoiceType1>();
            if (unadjustedDate == null && adjustedDate == null)
                return date;
            if (unadjustedDate != null)
            {
                items.Add(ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.unadjustedDate.ToString(), (DateTime)unadjustedDate));
                itemsElementName.Add(ItemsChoiceType1.unadjustedDate);
            }
            if (adjustedDate != null)
            {
                items.Add(ProductTypeHelper.IdentifiedDateHelper.Create(ItemsChoiceType1.adjustedDate.ToString(), (DateTime)adjustedDate));
                itemsElementName.Add(ItemsChoiceType1.adjustedDate);
            }
            if (businessDayAdjustments != null)
            {
                items.Add(businessDayAdjustments);
                itemsElementName.Add(ItemsChoiceType1.dateAdjustments);
            }
            date.Items = items.ToArray();
            date.ItemsElementName = itemsElementName.ToArray();
            return date;
        }

        public static Boolean Contains(AdjustableOrAdjustedDate adjustableOrAdjustedDate, ItemsChoiceType1 item, out object dateOrBusinessConvention)
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