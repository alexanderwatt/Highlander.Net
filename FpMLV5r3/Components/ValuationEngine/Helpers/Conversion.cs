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
using System.Linq;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Helpers.V5r3;

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    public class Conversion
    {
        /// <summary>
        /// Creates the money amounts.
        /// </summary>
        /// <param name="amounts">The amounts.</param>
        /// <param name="currencies">The currencies.</param>
        /// <returns></returns>
        internal static Money[] CreateMoneyAmounts(decimal[] amounts, string[] currencies)
        {
            var currencyAmounts = new List<Money>();
            int index = 0;
            foreach (decimal amount in amounts)
            {
                currencyAmounts.Add(MoneyHelper.GetAmount(amount, currencies[index]));
                index++;
            }
            return currencyAmounts.ToArray();
        }

        /// <summary>
        /// Creates the rate observations.
        /// </summary>
        /// <param name="observedRatesSpecified">The observed rates specified.</param>
        /// <param name="observedRates">The observed rates.</param>
        /// <returns></returns>
        internal static RateObservation[] CreateRateObservations(Boolean[] observedRatesSpecified, decimal[] observedRates)
        {
            var rateObservations = new List<RateObservation>();
            if (observedRates != null && observedRates.Length > 0)
            {
                int index = 0;
                foreach (Boolean observedRateSpecified in observedRatesSpecified)
                {
                    if (observedRateSpecified)
                    {
                        rateObservations.Add(new RateObservation { observedRate = observedRates[index], observedRateSpecified = true });
                    }
                    index++;
                }
            }
            return rateObservations.ToArray();
        }


        /// <summary>
        /// Converts from object to enum.
        /// </summary>
        /// <typeparam name="TTo">The type of to.</typeparam>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        internal static TTo[] ConvertFromObjectToEnum<TTo>(string[] array)
        {
            return array.Select(item => (TTo) Enum.Parse(typeof (TTo), item)).ToArray();
        }

        /// <summary>
        /// Converts from object to interval.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        internal static Period[] ConvertFromObjectToInterval(string[] array)
        {
            return array.Select(PeriodHelper.Parse).ToArray();
        }

        /// <summary>
        /// Converts from object to day count fraction.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        internal static DayCountFraction[] ConvertFromObjectToDayCountFraction(string[] array)
        {
            return array.Select(DayCountFractionHelper.Parse).ToArray();
        }

        /// <summary>
        /// Pads the items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="maxCapacity">The max capacity.</param>
        /// <returns></returns>
        internal static T[] PadItems<T>(T[] items, int maxCapacity)
        {
            Type type = typeof(T);
            if (!type.IsClass)
            {
                throw new ArgumentException("The generic type C must be a class");
            }
            if (type.GetConstructor(new Type[] { }) == null)
            {
                throw new ArgumentException("Failed to create instance of concrete class as there is no parameterless constructor");
            }
            var defaultItem = (T)Activator.CreateInstance(type);
            var itemsList = new List<T>();
            if (items == null || (maxCapacity > items.Length))
            {
                for (int index = 0; index < maxCapacity; index++)
                {
                    if (items != null && (items.Length > 0 && index < items.Length && items[index] != null))
                    {
                        itemsList.Add(items[index]);
                    }
                    else
                    {
                        itemsList.Add(defaultItem);
                    }
                }
            }
            return itemsList.ToArray();
        }

        /// <summary>
        /// Toes the title case.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        internal static string ToTitleCase(string inputString)
        {
            System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(inputString.ToLower());
        }

        internal static object ConvertListToArray(List<object> rValue, Type pType)
        {
            return ConvertListToTypeArray(rValue, pType.UnderlyingSystemType.GetElementType());
        }

        internal static object ConvertListToTypeArray(List<object> rValue, Type type)
        {
            object[] objArray = rValue.ToArray();
            object result;
            switch (type.Name)
            {
                case "Boolean":
                    result = Array.ConvertAll(objArray, Convert.ToBoolean);
                    break;
                case "DateTime":
                    result = Array.ConvertAll(objArray, Convert.ToDateTime);
                    break;
                case "Double":
                    result = Array.ConvertAll(objArray, Convert.ToDouble);
                    break;
                case "String":
                    result = Array.ConvertAll(objArray, Convert.ToString);
                    break;
                case "Decimal":
                    result = Array.ConvertAll(objArray, Convert.ToDecimal);
                    break;
                default:
                    result = objArray;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts the list to array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rValue">The r value.</param>
        /// <returns></returns>
        internal static T[] ConvertListToArray<T>(List<object> rValue)
        {
            return (T[])ConvertListToTypeArray(rValue, typeof(T));
        }

        /// <summary>
        /// Maps to object list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        internal static List<object> MapToObjectList<T>(T[] items)
        {
            List<object> result = null;
            if (items != null)
            {
                result = items.Select(t => (object) t).ToList();
            }
            return result;
        }


        /// <summary>
        /// Merges the specified first array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="firstArray">The first array.</param>
        /// <param name="secondArray">The second array.</param>
        /// <returns></returns>
        internal static T[] Merge<T>(T[] firstArray, T[] secondArray)
        {
            T[] result = { };
            List<T> list = null;
            if (firstArray != null)
            {
                list = new List<T>(firstArray);
                list.Sort();
            }
            if (secondArray != null && list != null)
            {
                foreach (T item in secondArray)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
                list.Sort();
            }
            if (list != null)
            {
                result = list.ToArray();
            }
            return result;
        }

        public static IDictionary<string, object[]> ValuesToObjectArray(IDictionary<string, object> dictionary)
        {
            IDictionary<string, object[]> results = new Dictionary<string, object[]>();
            foreach (string item in dictionary.Keys)
            {
                var newArray = new object[1];
                if (dictionary[item] != null && dictionary[item].GetType().IsArray)
                {
                    var array = (Array)dictionary[item];
                    newArray = new object[array.Length];
                    array.CopyTo(newArray, 0);
                }
                else
                {
                    newArray[0] = dictionary[item];
                }
                results.Add(item, newArray);
            }
            return results;
        }
    }
}