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

namespace Highlander.Utilities.Helpers
{
    public static class RangeFunctions
    {
        public static double[] DateTime2Double(DateTime[] values)
        {
            var da = new double[values.GetLength(0)];
            for (int idx = 0; idx < values.GetLength(0); idx++)
                da[idx] = values[idx].ToOADate();
            return da;
        }

        public static double[] Extract(object obj, int count)
        {
            var array = (Array) obj;
            int rstart = array.GetLowerBound(0);
            //int rend = array.GetUpperBound(0);
            var result = new double[count];
            for (int idx = rstart; idx < rstart + count; idx++)
            {
                result[idx - rstart] = Convert<double>(array.GetValue(idx));
            }
            return result;
        }

        public static double[] Extract(object obj)
        {
            var array = (Array) obj;
            int rstart = array.GetLowerBound(0);
            int rend = array.GetUpperBound(0);
            var result = new double[rend - rstart + 1];
            for (int idx = rstart; idx <= rend; idx++)
            {
                result[idx - rstart] = Convert<double>(array.GetValue(idx));
            }
            return result;
        }

        public static void Move(double[] from, object to)
        {
            var array = (Array) to;
            int start = array.GetLowerBound(0);
            int length = from.Length;
            for (int idx = start; idx < start + length; idx++)
                array.SetValue(from[idx - start], idx);
        }

        private static T[] AsHorizontalArray<T>(object[,] ranges)
        {
            int start = ranges.GetLowerBound(1);
            int end = ranges.GetUpperBound(1);
            int cstart = ranges.GetLowerBound(0);
            var result = new T[ranges.GetLength(1)];
            for (int idx = start; idx <= end; idx++)
            {
                result[idx - start] = Convert<T>(ranges[cstart, idx]);
            }
            return result;
        }

        private static T[] AsVerticalArray<T>(object[,] ranges)
        {
            int start = ranges.GetLowerBound(0);
            int end = ranges.GetUpperBound(0);
            int cstart = ranges.GetLowerBound(1);
            var result = new T[ranges.GetLength(0)];
            for (int idx = start; idx <= end; idx++)
            {
                result[idx - start] = Convert<T>(ranges[idx, cstart]);
            }
            return result;
        }

        private static T Convert<T>(object v)
        {
            object result = v;
            if (typeof(T).Name == "Double" || typeof(T).Name == "Int32")
            {
                if (result == null)
                    result = 0;
                else if (result is DateTime)
                    result = ((DateTime) result).ToOADate();
                else if (result is string && ((string) result).Trim().Length == 0)
                    result = 0;
            }
            try
            {
                result = (T) System.Convert.ChangeType(result, typeof(T));
            }
            catch (System.Exception)
            {
                // ignored
            }
            return (T) result;
        }
    }
}
