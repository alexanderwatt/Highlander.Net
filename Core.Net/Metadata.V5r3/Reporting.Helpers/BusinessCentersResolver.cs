/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Serialisation;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class BusinessCentersResolver
    {
        public static void ResolveBusinessCenters(object objectGraph)
        {
            foreach (BusinessDayAdjustments businessDayAdjustment in GetBusinessDayAdjustments(objectGraph))
            {
                // "NONE" adjustments have neither businessCenters nor businessCentersReference.
                //
                if ((null != businessDayAdjustment) &&
                    (null == businessDayAdjustment.businessCenters) &&
                    (null != businessDayAdjustment.businessCentersReference) &&
                    (!String.IsNullOrEmpty(businessDayAdjustment.businessCentersReference.href)))
                {
                    var businessCenters = ObjectLookupHelper.GetById<BusinessCenters>(objectGraph, businessDayAdjustment.businessCentersReference.href);
                    var businessCentersCloneWithNoId = BinarySerializerHelper.Clone(businessCenters);
                    businessCentersCloneWithNoId.id = null;
                    businessDayAdjustment.businessCenters = businessCentersCloneWithNoId;
                }
            }
        }

        public static void UnresolvedBusinessCenters(object objectGraph)
        {
            IEnumerable<BusinessDayAdjustments> businessDayAdjustments = GetBusinessDayAdjustments(objectGraph);
            foreach (BusinessDayAdjustments businessDayAdjustment in businessDayAdjustments)
            {
                if (null != businessDayAdjustment.businessCentersReference)
                {
                    if (!String.IsNullOrEmpty(businessDayAdjustment.businessCentersReference.href))
                    {
                        businessDayAdjustment.businessCenters = null;
                    }
                }
            }
        }

        private static IEnumerable<BusinessDayAdjustments> GetBusinessDayAdjustments(object objectGraph)
        {
            return GetBusinessDayAdjustmentsRecursive(objectGraph).ToArray();
        }

        private static List<BusinessDayAdjustments> GetBusinessDayAdjustmentsRecursive(object objectGraph)
        {
            var list = new List<BusinessDayAdjustments>();         
            // Stop traversing once simple type (primitive type, string or non-reference type) is encountered.
            //
            if (
                objectGraph.GetType().IsPrimitive || //if a primitive type (i.e. int, long, double, etc)
                objectGraph is string || //if a string 
                !objectGraph.GetType().IsClass)             //if not a class (e.g. enum, struct)
            {
                return list;
            }
            foreach (PropertyInfo propertyInfo in objectGraph.GetType().GetProperties())
            {
                // If property of BusinessDayAdjustments type - add it into list
                //
                if (typeof(BusinessDayAdjustments) == propertyInfo.PropertyType)
                {
                    var value = (BusinessDayAdjustments)propertyInfo.GetValue(objectGraph, null);
                    list.Add(value);
                }
                else
                {
                    Debug.Assert(null != objectGraph);
                    object value = propertyInfo.GetValue(objectGraph, null);

                    if (null != value)
                    {
                        if (value.GetType().IsArray)//if array
                        {
                            // iterate thru the array 
                            //
                            var valueAsArray = (Array)value;
                            foreach (object arrayItem in valueAsArray)
                            {
                                if (null != arrayItem)
                                {
                                    list.AddRange(GetBusinessDayAdjustmentsRecursive(arrayItem));
                                }
                            }
                        }
                        else
                        {
                            list.AddRange(GetBusinessDayAdjustmentsRecursive(value));
                        }
                    }
                }
            }
            return list;                
        }
    }
}
