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

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Orion.Util.Helpers;

#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class ParameterFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string FormatObject(object obj)
        {
            if (obj is Exception exception)
            {
                return FormatException(exception, false);
            }
            if (null == obj)
            {
                return "null";
            }
            if (obj is string)
            {
                return $@"""{obj}""";
            }
            if (obj is Array array)
            {
                return FormatArray(array);
            }
            return obj.ToString();
        }

        
        private static string FormatException(Exception ex, bool innerException)
        {
            StringBuilder sb = new StringBuilder();
            if (!innerException)
            {
                sb.Append("===Begin of exception details===");
                sb.AppendLine();
            }
            sb.AppendFormat("Type:{0}", ex.GetType());
            sb.AppendLine();
            sb.AppendFormat("Message:{0}", ex.Message);
            sb.AppendLine();
            sb.AppendFormat("Source:{0}", ex.Source);
            sb.AppendLine();
            sb.AppendFormat("StackTrace:{0}", ex.StackTrace);
            sb.AppendLine();
            sb.AppendFormat("TargetSite:{0}", ex.TargetSite);
            sb.AppendLine();
            if (ex.Data.Count > 0)
            {
                sb.Append("===Begin of exception data===");
                sb.AppendLine();
                int dataIndex = 0;
                foreach (DictionaryEntry dictionaryEntry in ex.Data)
                {
                    sb.AppendFormat("Item{0}", dataIndex);
                    sb.AppendFormat("Key: {0}", dictionaryEntry.Key);
                    if (null != dictionaryEntry.Value)
                    {
                        sb.AppendFormat("Value: {0}", dictionaryEntry.Value);
                    }
                    else 
                    {
                        sb.Append("Value: NULL");
                    }
                    sb.AppendLine();
                }
                sb.Append("===End of exception data===");
                sb.AppendLine();
            }
            if (null != ex.InnerException)
            {
                sb.AppendLine("InnerException:");
                sb.AppendLine(FormatException(ex.InnerException, true));
            }
            if (!innerException)
            {
                sb.Append("===End of exception details===");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string FormatArray(Array array)
        {
            StringBuilder stringBuilder = new StringBuilder();
            ArrayIndex arrayIndex = new ArrayIndex(array);
            int[] indexes = arrayIndex.GetCurrentIndexes();
            stringBuilder.Append('[', array.Rank);
            while (null != indexes)
            {
                object itemValue = arrayIndex.GetValue(array);
                stringBuilder.Append(FormatObject(itemValue));
                int numberOfShifts = 0;
                indexes = arrayIndex.GetNextIndexes(ref numberOfShifts);
                if (numberOfShifts > 0)
                {
                    string closingBrackets = new string(']', numberOfShifts);
                    string openingBrackets = new string('[', numberOfShifts);
                    stringBuilder.AppendLine(closingBrackets);
                    if (numberOfShifts != array.Rank)
                    {
                        stringBuilder.Append(openingBrackets);
                    }
                }
                else
                {
                    stringBuilder.Append(",\t");
                }
            }
            //  delete repetitive rows
            //
            string[] linesWithDuplicates = stringBuilder.ToString().Split(Environment.NewLine.ToCharArray());
            string previousLine = null;
            List<Pair<string, int>> result = new List<Pair<string, int>>();
            foreach (string possibleDuplicate in linesWithDuplicates)
            {
                if (String.IsNullOrEmpty(possibleDuplicate))
                {
                    continue;
                }
                if (previousLine == possibleDuplicate)
                {
                    result[result.Count - 1].Second = 1 + result[result.Count - 1].Second;
                    continue;
                }
                Pair<string, int> pair = new Pair<string, int>(possibleDuplicate, 1);
                result.Add(pair);
                previousLine = possibleDuplicate;
            }
            StringBuilder resultAsStringBuilder = new StringBuilder();
            foreach (Pair<string, int> pair in result)
            {
                if (1 == pair.Second)
                {
                    resultAsStringBuilder.Append(pair.First + Environment.NewLine); 
                }
                else
                {
                    resultAsStringBuilder.Append(pair.Second + " x " + pair.First + Environment.NewLine); 
                }
            }
            return resultAsStringBuilder.ToString();
        }
    }
}