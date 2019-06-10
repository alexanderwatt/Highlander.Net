/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orion.Util.Helpers;

#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// Helper class which provides typed list/array conversions 
    /// </summary>
    public static class ObjectToArrayOfPropertiesConverter
    {
        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfMembers">The list of members.</param>
        /// <returns></returns>
        /// <param name="error"></param>
        public static T CreateObject<T>(List<object> listOfMembers, out bool error) //where T : class, new() 
        {
            error = true;
            //  check special case - simple types
            //
            if (typeof(IConvertible).IsAssignableFrom(typeof(T)) & 1 == listOfMembers.Count)
            {
                var resultConvertible = (T)Convert.ChangeType(listOfMembers[0], typeof(T));//throws exception
                error = false;
                return resultConvertible;
            }
            //T result = new T();
            var result = (T)Activator.CreateInstance(typeof (T));
            bool atLeastOneMemberHasBeenConverted = false;
            FieldInfo[] fields = typeof(T).GetFields();
            if (0 != fields.Length)//if at least one public FIELD exists - use it.
            {
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (listOfMembers.Count < fields.Length)
                    {
                        string message =
                            $"There is not enough data ('{listOfMembers.Count}' items) to create object of type '{typeof(T)}' ('{fields.Length}' fields)";
                        throw new Exception(message);
                    }
                    object fieldValue = listOfMembers[i];
                    // check for nulls
                    //
                    if (null != fieldValue)
                    {
                        FieldInfo fieldInfo = fields[i];
                        try
                        {
                            object convertedObjectFromRange = ReflectionHelper.ChangeType(fieldValue, fieldInfo.FieldType);
                            if (null != convertedObjectFromRange)
                            {
                                atLeastOneMemberHasBeenConverted = true;
                                fieldInfo.SetValue(result, convertedObjectFromRange);
                            }
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (Exception ex)
                        {
                            string message =
                                $"Error: field {typeof(T).Name}.{fieldInfo.Name} ({fieldInfo.FieldType}) is not assignable from {fieldValue} ({fieldValue.GetType()})";
                            var exception = new Exception(message, ex);
                            exception.Data.Add("NotAssignableFrom", fieldValue.ToString());
                            throw exception;
                        }
                    }
                }
            }
            else//otherwise - try iterate over public PROPERTIES
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                if (0 == properties.Length)
                {
                    throw new Exception();//type have neither public fields nor public properties
                }
                for (int i = 0; i < properties.Length; ++i)
                {
                    if (listOfMembers.Count < properties.Length)
                    {
                        string message =
                            $"There is not enough data ('{listOfMembers.Count}' items) to create object of type '{typeof(T)}' ('{properties.Length}' properties)";
                        throw new Exception(message);
                    }
                    object propertyValue = listOfMembers[i];
                    // check for nulls
                    //
                    if (null != propertyValue)
                    {
                        PropertyInfo propertyInfo = properties[i];
                        try
                        {
                            object convertedObjectFromRange = ReflectionHelper.ChangeType(propertyValue, propertyInfo.PropertyType);
                            if (null != convertedObjectFromRange)
                            {
                                atLeastOneMemberHasBeenConverted = true;
                                propertyInfo.SetValue(result, convertedObjectFromRange, null);
                            }
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (Exception ex)
                        {
                            string message =
                                $"Error: property {typeof(T).Name}.{propertyInfo.Name} ({propertyInfo.PropertyType}) is not assignable from {propertyValue} ({propertyValue.GetType()})";
                            var exception = new Exception(message, ex);
                            exception.Data.Add("NotAssignableFrom", propertyValue.ToString());
                            throw exception;
                        }
                    }
                }
            }
            //return atLeastOneMemberHasBeenConverted ? result : null;
            if (atLeastOneMemberHasBeenConverted)
            {
                error = false;
            }           
            return result;
        }

        /// <summary>
        /// Creates the list from horizontal array range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayRange">The array range.</param>
        /// <returns></returns>
        public static List<T> CreateListFromHorizontalArrayRange<T>(object[,] arrayRange)// where T : class, new()
        {
            var listOfListOfFields = new List<List<object>>();
            for (int rowNumber = arrayRange.GetLowerBound(0); rowNumber <= arrayRange.GetUpperBound(0); ++rowNumber)
            {
                var listOfFields = new List<object>();
                bool wholeRowIsEmpty = true;
                for (int columnNumber = arrayRange.GetLowerBound(1); columnNumber <= arrayRange.GetUpperBound(1); ++columnNumber)
                {
                    object fieldValue = arrayRange[rowNumber, columnNumber];
                    if (null != fieldValue)
                    {
                        listOfFields.Add(fieldValue);
                        wholeRowIsEmpty = false;
                    }
                }
                if (AreAllFieldEmptyStrings(listOfFields))//"", "", "" case
                {
                    wholeRowIsEmpty = true;
                }
                if (!wholeRowIsEmpty)
                {
                    listOfListOfFields.Add(listOfFields);
                }
            }
            return CreateListOfObjectsFromListOfListOfFields<T>(listOfListOfFields);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listOfFields"></param>
        /// <returns></returns>
        public static bool AreAllFieldEmptyStrings(List<object> listOfFields)
        {
            foreach (object fieldAsObject in listOfFields)
            {
                if (fieldAsObject is string s)
                {
                    var fieldAsString = s;
                    if (!String.IsNullOrEmpty(fieldAsString))
                    {
                        return false;//at least one field is not an empty string                        
                    }
                }
                else
                {
                    return false;//at least one field is not string at all
                }
            }
            return true;
        }

        /// <summary>
        /// Creates the list from vertical array range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayRange">The array range.</param>
        /// <returns></returns>
        public static List<T> CreateListFromVerticalArrayRange<T>(object[,] arrayRange)// where T : class, new()
        {
            var listOfListOfFields = new List<List<object>>();
            for (int columnNumber = arrayRange.GetLowerBound(1); columnNumber <= arrayRange.GetUpperBound(1); ++columnNumber)
            {
                var listOfFields = new List<object>();
                bool wholeRowIsEmpty = true;
                for (int rowNumber = arrayRange.GetLowerBound(0); rowNumber <= arrayRange.GetUpperBound(0); ++rowNumber)
                {
                    object fieldValue = arrayRange[rowNumber, columnNumber];
                    if (null != fieldValue)
                    {
                        listOfFields.Add(fieldValue);
                        wholeRowIsEmpty = false;
                    }
                }
                if (AreAllFieldEmptyStrings(listOfFields))//"", "", "" case
                {
                    wholeRowIsEmpty = true;
                }
                if (!wholeRowIsEmpty)
                {
                    listOfListOfFields.Add(listOfFields);
                }
            }
            return CreateListOfObjectsFromListOfListOfFields<T>(listOfListOfFields);
        }

        /// <summary>
        /// Creates the list of objects from list of list of fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfListOfFields">The list of list of fields.</param>
        /// <returns></returns>
        public static List<T> CreateListOfObjectsFromListOfListOfFields<T>(List<List<object>> listOfListOfFields)// where T : class, new()
        {
            var result = new List<T>();
            foreach(var listOfProperties in listOfListOfFields)
            {
                var obj = CreateObject<T>(listOfProperties, out var error);
                if (!error)
                {
                    if (null != obj)
                    {
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// NB: returns 'horizontal array'.
        /// Should only be used for Lists of complex classes and not simple types.
        /// Works both for both lists and single objects.
        /// </summary>
        private static List<List<object>> ConvertObjectToListOfListsOfFields<T>(T obj)
        {
            var result = new List<List<object>>();
            int numberOfObjects = 1;
            FieldInfo[] fields          = typeof(T).GetFields();
            PropertyInfo[] properties   = typeof(T).GetProperties();
            if (obj is IList list1)
            {
                numberOfObjects = list1.Count;
                object arrayItemObject = list1[0];
                fields = arrayItemObject.GetType().GetFields();
                properties = arrayItemObject.GetType().GetProperties();
            }
            if (0 != fields.Length)
            {
                for (int objectNumber = 0; objectNumber < numberOfObjects; ++objectNumber)
                {
                    object objectToExtractValuesFrom = obj;
                    if (obj is IList list)
                    {                       
                        objectToExtractValuesFrom = list[objectNumber];
                    }
                    var objectAsListOfFields = fields.Select(t => t.GetValue(objectToExtractValuesFrom)).ToList();
                    result.Add(objectAsListOfFields);
                }
            }
            else if (0 != properties.Length)
            {
                for (int objectNumber = 0; objectNumber < numberOfObjects; ++objectNumber)
                {
                    object objectToExtractValuesFrom = obj;
                    if (obj is IList list)
                    {
                        objectToExtractValuesFrom = list[objectNumber];
                    }
                    var objectAsListOfProperties = fields.Select((t, i) => properties[i].GetValue(objectToExtractValuesFrom, null)).ToList();
                    result.Add(objectAsListOfProperties);
                }
            }
            else
            {
                throw new Exception();
            }
            return result;
        }


        #region Vertical range methods

        /// <summary>
        /// <example>
        /// class A
        /// {
        ///     int     b;
        ///     double  c;
        ///     string  d;
        /// }
        /// 
        /// A a = new A();
        /// will be converted to 
        /// 
        /// array of objects - ar[,] such as 
        /// 
        /// a.b -> ar[0, 0] 
        /// a.c -> ar[1, 0]
        /// a.d -> ar[2, 0]
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object[,] ConvertObjectToVerticalArrayRange<T>(T obj)
        {
            List<List<object>> lists = ConvertObjectToListOfListsOfFields(obj);
            var result = new object[lists[0].Count, lists.Count];
            for (int columnNumber = 0; columnNumber < lists[0].Count; ++columnNumber)
            {
                for (int rowNumber = 0; rowNumber < lists.Count; ++rowNumber)
                {
                    result[columnNumber, rowNumber] = lists[rowNumber][columnNumber];
                }
            }
            return result;
        }

        /// <summary>
        /// Converts the list to vertical array range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static object[,] ConvertListToVerticalArrayRange<T>(List<T> obj)
        {
            List<List<object>> lists = ConvertObjectToListOfListsOfFields(obj);
            var result = new object[lists[0].Count, lists.Count];
            for (int columnNumber = 0; columnNumber < lists[0].Count; ++columnNumber)
            {
                for (int rowNumber = 0; rowNumber < lists.Count; ++rowNumber)
                {
                    result[columnNumber, rowNumber] = lists[rowNumber][columnNumber];
                }
            }
            return result;
        }

        #endregion

        #region Horizontal range methods

        /// <summary>
        /// <example>
        /// class A
        /// {
        ///     int     b;
        ///     double  c;
        ///     string  d;
        /// }
        /// 
        /// A a = new A();
        /// will be converted to 
        /// 
        /// array of objects - ar[,] such as 
        /// 
        /// a.b -> ar[0, 0] 
        /// a.c -> ar[0, 1]
        /// a.d -> ar[0, 2]
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object[,] ConvertObjectToHorizontalArrayRange<T>(T obj)
        {
            List<List<object>> lists = ConvertObjectToListOfListsOfFields(obj);
            if (0 == lists.Count)
            {
                var emptyResult = new object[lists.Count, 0];
                return emptyResult;
            }
            var result = new object[lists.Count, lists[0].Count];
            for (int rowNumber = 0; rowNumber < lists.Count; ++rowNumber)
            {
                for (int columnNumber = 0; columnNumber < lists[0].Count; ++columnNumber)
                {
                    result[rowNumber, columnNumber] = lists[rowNumber][columnNumber];
                }
            }
            return result;
        }


        /// <summary>
        /// Converts the list to horizontal array range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static object[,] ConvertListToHorizontalArrayRange<T>(List<T> obj)
        {
            List<List<object>> lists = ConvertObjectToListOfListsOfFields(obj);
            var result = new object[lists.Count, lists[0].Count];
            for (int rowNumber = 0; rowNumber < lists.Count; ++rowNumber)
            {
                for (int columnNumber = 0; columnNumber < lists[0].Count; ++columnNumber)
                {
                    result[rowNumber, columnNumber] = lists[rowNumber][columnNumber];
                }
            }
            return result;
        }

        #endregion
    }
}