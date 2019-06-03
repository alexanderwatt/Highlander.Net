using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class BusinessCentersResolver
    {
        public static void ResolveBusinessCenters(object objectGraph)
        {
            //BusinessDayAdjustments[] businessDayAdjustments = ;

            foreach (BusinessDayAdjustments businessDayAdjustment in GetBusinessDayAdjustments(objectGraph))
            {
                // "NONE" adjustments have neither businessCenters nor businessCentersReference.
                //
                if ((null != businessDayAdjustment) &&
                    (null == businessDayAdjustment.businessCenters) && !String.IsNullOrEmpty(businessDayAdjustment.businessCentersReference?.href))
                {
                    var businessCenters = ObjectLookupHelper.GetById<BusinessCenters>(objectGraph, businessDayAdjustment.businessCentersReference.href);
                    var businessCentersCloneWithNoId = BinarySerializerHelper.Clone(businessCenters);
                    businessCentersCloneWithNoId.id = null;
                    businessDayAdjustment.businessCenters = businessCentersCloneWithNoId;
                }
            }
        }

        public static void UnresolveBusinessCenters(object objectGraph)
        {
            IEnumerable<BusinessDayAdjustments> businessDayAdjustments = GetBusinessDayAdjustments(objectGraph);
            foreach (BusinessDayAdjustments businessDayAdjustment in businessDayAdjustments)
            {
                if (!String.IsNullOrEmpty(businessDayAdjustment.businessCentersReference?.href))
                {
                    businessDayAdjustment.businessCenters = null;
                }
            }
        }

        private static IEnumerable<BusinessDayAdjustments> GetBusinessDayAdjustments(object objectGraph)
        {
            return GetBusinessDayAdjustmentsRecursive(objectGraph).ToArray();
            //return businessDayAdjustmentses;
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
