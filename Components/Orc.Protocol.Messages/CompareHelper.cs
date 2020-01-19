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
using System.Reflection;


namespace Highlander.Orc.Messages
{

    #region CompareEventArgs

    public class CompareEventArgs
    {
        public CompareEventArgs()
        {
        }

        public CompareEventArgs(string fieldName, string object1Property,
                                string object2Property, int compareResult)
        {
            PropertyName = fieldName;
            Object1Property = object1Property;
            Object2Property = object2Property;
            CompareResult = compareResult;
        }

        public string PropertyName { get; set; } = string.Empty;

        public string Object1Property { get; set; } = string.Empty;

        public string Object2Property { get; set; } = string.Empty;

        public int CompareResult { get; set; }
    }

    #endregion

    public class CompareHelper
    {
        private int _nestedLevel;
        private int _result;

        public CompareEventArgs Outcome { get; private set; } = new CompareEventArgs();

        public void CompareObjects(object x, object y, out CompareEventArgs outcome)
        {
            CompareObjects(x, y);
            outcome = Outcome;
        }

        // Compares 2 objects recursively looking into all properties only
        // Fields are not taking into consideration
        private void CompareObjects(object x, object y)
        {
            if (_nestedLevel == 0)
            {
                _result = 0;
                Outcome = new CompareEventArgs();
            }
            _nestedLevel++;
            if (x != null || y != null)
            {
                CompareValues(x, y);
                if (_result == 0)
                    CompareProperties(x, y); // compareValue
                else
                {
                    if (x != null) Outcome = new CompareEventArgs("none", x.ToString(), y.ToString(), _result);
                }
            }
            _nestedLevel--;
        }

        private void CompareValues(object x, object y)
        {
            if (x == null && y == null)
            {
                _result = 0;
                return;
            }
            // in case if first object is null and second has more information 
            // treat them as equal in this particular case
            if (x == null)
            {
                _result = 0; // should be -1 for classical compare
                return;
            }
            if (y == null)
            {
                _result = 1;
                return;
            }

            _result = SupportsIComparable(x) ? Math.Abs(((IComparable)x).CompareTo(y)) : 0;
        }

        private void CompareProperties(object x, object y)
        {
            Type type = x.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (_result == 0)
                {
                    object[] propAttributes = property.GetCustomAttributes(true);
                    string typeName = GetPropertyCustomComparableType(propAttributes);
                    if (typeName != string.Empty)
                    {
                        Type newType = Type.GetType(typeName, true, true);
                        object valX = Convert.ChangeType(property.GetValue(x, null), newType);
                        object valY = Convert.ChangeType(property.GetValue(y, null), newType);
                        CompareValues(valX, valY);
                    }
                    else if (IsPropertyComparable(propAttributes))
                    {
                        object valX = property.GetValue(x, null);
                        object valY = property.GetValue(y, null);
                        if (IsInNetsNamespace(property.PropertyType.Namespace))
                            CompareObjects(valX, valY);
                        else
                            CompareValues(valX, valY);
                    }
                    if (_result != 0)
                    {
                        if (Outcome.PropertyName == string.Empty)
                        {
                            object valX = property.GetValue(x, null);
                            object valY = property.GetValue(y, null);
                            Outcome = new CompareEventArgs(property.Name, valX.ToString(), valY.ToString(), _result);
                        }
                        else
                            Outcome.PropertyName = property.Name + "/" + Outcome.PropertyName;
                    }
                }
            }
        }

        #region Subsidiary functions

        private static bool SupportsIComparable(object x)
        {
            Type objectType = x.GetType();
            TypeFilter filter = InterfaceFilter;
            Type[] foundInterfaces = objectType.FindInterfaces(filter, "System.IComparable");
            return foundInterfaces.Length > 0;
        }

        private static bool InterfaceFilter(Type typeObj, object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        private static bool IsPropertyComparable(object[] propAttributes)
        {
            if (propAttributes.Length == 0) return true;
            foreach (object propAttribute in propAttributes)
            {
                Type attrType = propAttribute.GetType();
                if (attrType == typeof(NonComparableAttribute))
                    return false;
            }
            return true;
        }

        private static string GetPropertyCustomComparableType(object[] propAttributes)
        {
            string type = string.Empty;
            if (propAttributes.Length == 0)
                return type;
            foreach (object propAttribute in propAttributes)
            {
                Type attrType = propAttribute.GetType();
                if (attrType == typeof(CustomComparableAttribute))
                {
                    CustomComparableAttribute attr = (CustomComparableAttribute)propAttribute;
                    type = attr.CustomCompareType;
                    break;
                }
            }
            return type;
        }

        private static bool IsInNetsNamespace(string namespaceName)
        {
            return
                (namespaceName.ToUpper().Contains("Nets") ||
                 namespaceName.ToUpper().Contains("NETS"));
        }

        #endregion
    }

    public class NonComparableAttribute : Attribute
    {
    }

    public class CustomComparableAttribute : Attribute
    {
        public CustomComparableAttribute(string type)
        {
            CustomCompareType = type;
        }

        public string CustomCompareType { get; }
    }
}
