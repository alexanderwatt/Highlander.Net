using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;
using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
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
                    (null == businessDayAdjustment.businessCenters) &&
                    (null != businessDayAdjustment.businessCentersReference) &&
                    (!String.IsNullOrEmpty(businessDayAdjustment.businessCentersReference.href)))
                {
                    BusinessCenters businessCenters = ObjectLookupHelper.GetById<BusinessCenters>(objectGraph, businessDayAdjustment.businessCentersReference.href);
                    BusinessCenters businessCentersCloneWithNoId = BinarySerializerHelper.Clone<BusinessCenters>(businessCenters);
                    businessCentersCloneWithNoId.id = null;
                    businessDayAdjustment.businessCenters = businessCentersCloneWithNoId;
                }
            }
        }

        public static void UnresolveBusinessCenters(object objectGraph)
        {
            BusinessDayAdjustments[] businessDayAdjustments = GetBusinessDayAdjustments(objectGraph);
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

        private static BusinessDayAdjustments[]  GetBusinessDayAdjustments(object objectGraph)
        {
            return GetBusinessDayAdjustmentsRecursive(objectGraph).ToArray();
            //return businessDayAdjustmentses;
        }

        private static List<BusinessDayAdjustments> GetBusinessDayAdjustmentsRecursive(object objectGraph)
        {
            List<BusinessDayAdjustments> list = new List<BusinessDayAdjustments>();
            
            // Stop traversing once simple type (primitive type, string or non-reference type) is encountered.
            //
            if (
                objectGraph.GetType().IsPrimitive || //if a primitive type (i.e. int, long, double, etc)
                typeof(string) == objectGraph.GetType() || //if a string 
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
                    BusinessDayAdjustments value = (BusinessDayAdjustments)propertyInfo.GetValue(objectGraph, null);

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
                            Array valueAsArray = (Array)value;

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
