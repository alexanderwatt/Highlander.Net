/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Class which recursively traverses an object
    /// </summary>
    public static class ObjectLookupHelper
    {
        private const string IdFieldName = "id";

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectGraph">The object graph.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static T GetById<T>(object objectGraph, string id)
        {
            return (T)GetByIdRecursive(objectGraph, id);
        }

        /// <summary>
        /// Finds object in the objects graph by it's id (string "id" property) and returns it.
        /// If object with specified id is not found - returns null.
        /// <remarks>
        /// Method traverse object graph recursively, checking type's PUBLIC properties only.
        /// </remarks>
        /// </summary>
        /// <param name="objectGraph">The object graph.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static object GetByIdRecursive(object objectGraph, string id)
        {
            // Stop traversing once simple type (primitive type, string or non-reference type) is encountered.
            //
            if (
                objectGraph.GetType().IsPrimitive || //if a primitive type (i.e. int, long, double, etc)
                objectGraph is string || //if a string 
                !objectGraph.GetType().IsClass)             //if not a class (e.g. enum, struct)
            {
                return null;
            }
            foreach (PropertyInfo propertyInfo in objectGraph.GetType().GetProperties())
            {
                if (IdFieldName == propertyInfo.Name)
                {
                    object value = propertyInfo.GetValue(objectGraph, null);
                    if (null == value)
                    {
                        return null;
                    }
                    return id == (string)value ? objectGraph : null;
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
                                    object arrayItemValue = GetByIdRecursive(arrayItem, id);
                                    if (null != arrayItemValue)
                                    {
                                        return arrayItemValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            object objValue = GetByIdRecursive(value, id);
                            if (null != objValue)
                            {
                                return objValue;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Objects the property exists.
        /// </summary>
        /// <param name="theObject">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Boolean ObjectPropertyExists(object theObject, string propertyName)
        {
            Type theObjectType = theObject.GetType();
            PropertyInfo p = theObjectType.GetProperty(propertyName);
            return p != null;
        }

        /// <summary>
        /// Gets the object property.
        /// </summary>
        /// <param name="theObject">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static PropertyInfo GetObjectProperty(object theObject, string propertyName)
        {
            Type theObjectType = theObject.GetType();
            PropertyInfo p = theObjectType.GetProperty(propertyName);
            //if (p == null)
            //{
            //    throw new ArgumentNullException($"property {propertyName} does not exist on {theObjectType.Name}");
            //}
            return p;
        }

        /// <summary>
        /// Gets the object properties.
        /// </summary>
        /// <param name="theObject">The object.</param>
        /// <returns></returns>
        public static PropertyInfo[] GetObjectProperties(object theObject)
        {
            Type theObjectType = theObject.GetType();
            return theObjectType.GetProperties();
        }

        /// <summary>
        /// Gets the objects property value.
        /// </summary>
        /// <param name="theObject">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static object GetPropertyValue(object theObject, string propertyName)
        {
            PropertyInfo p = GetObjectProperty(theObject, propertyName);
            if (p == null) return null;
            return p.GetValue(theObject, null);
        }

        /// <summary>
        /// Sets the objects property value.
        /// </summary>
        /// <param name="theObject">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public static void SetPropertyValue(object theObject, string propertyName, object value)
        {
            PropertyInfo property = GetObjectProperty(theObject, propertyName);
            property.SetValue(theObject, value, null);
        }
    }
}