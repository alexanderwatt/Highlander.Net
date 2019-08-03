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
using HLV5r3.Extensions;
using Microsoft.Office.Interop.Excel;

namespace HLV5r3.Impl
{
  public static class RangeFunctions
  {
    public static T[] AsArray<T>(object range)
    {
      object[,] ranges = Range2Matrix(range);
        var result = ranges.GetUpperBound(0) > ranges.GetLowerBound(0) ? AsVerticalArray<T>(ranges) : AsHorizontalArray<T>(ranges);
      return result;
    }
    public static sbyte[] AsBytes(object range)
    {
      sbyte[] result;
      object objRange = AsAny(range);
      if (objRange is string s)
      {
        result = new sbyte[1];
        result[0] = (sbyte)s[0];
      }
      else
      {
        string [] values = AsArray<string>(range);
        result = new sbyte[values.Length];
        for (int idx = 0; idx < values.Length; idx++)
        {
          result[idx] = values[idx].Length > 0 ? (sbyte)values[idx][0] : (sbyte)0;
        }
      }
      return result;
    }

    public static T[,] AsMatrix<T>(object range)
    {
      object[,] ranges = Range2Matrix(range);
      int rstart = ranges.GetLowerBound(0);
      int rend = ranges.GetUpperBound(0);
      int cstart = ranges.GetLowerBound(1);
      int cend = ranges.GetUpperBound(1);
      var result = new T[ranges.GetLength(0), ranges.GetLength(1)];
      for (int idx = rstart; idx <= rend; idx++)
      {
        for (int jdx = cstart; jdx <= cend; jdx++)
        {
          result[idx - rstart, jdx - cstart] = Convert<T>(ranges[idx, jdx]);
        }
      }
      return result;
    }
    public static double[] DateTime2Double(DateTime[] values)
    {
      var da = new double[values.GetLength(0)];
      for (int idx = 0; idx < values.GetLength(0); idx++)
        da[idx] = values[idx].ToOADate();
      return da;
    }

    public static double[] Extract(object obj, int count)
    {
      var array = (Array)obj;
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
      var array = (Array)obj;
      int rstart = array.GetLowerBound(0);
      int rend = array.GetUpperBound(0);
      var result = new double[rend - rstart + 1];
      for (int idx = rstart; idx <= rend; idx++)
      {
        result[idx - rstart] = Convert<double>(array.GetValue(idx));
      }
      return result;
    }

    public static void Move (double [] from, object to)
    {
      var array = (Array)to;
      int start = array.GetLowerBound(0);
      int length = from.Length;
      for (int idx = start; idx < start + length; idx++)
        array.SetValue(from[idx - start], idx);
    }

    public static int AsInt(object comObj)
    {
      object result = AsAny(comObj);
      if (result is double d)
        result = (int)d;
      return (int) result;
    }

    public static double AsDouble(object comObj)
    {
      object result = AsAny(comObj);
      if (result is int i)
        result = (double)i;
      else if (result is long l)
        result = (double)l;
      return (double)result;
    }

    public static DateTime AsDateTime(object comObj)
    {
      return (DateTime)AsAny(comObj);
    }

    public static string AsString(object comObj)
    {
      return (string)AsAny(comObj);
    }

    public static bool AsBool(object comObj)
    {
      object result = AsAny(comObj);
      if (result is int i)
        result = i != 0;
      else if (result is long l)
        result = l != 0;
      else if (result is double d)
        result = d != 0;
      return (bool) result;
    }

    public static bool IsDateTime(object comObj)
    {
      return AsAny(comObj) is DateTime;
    }

    public static bool IsRange(object comObj)
    {
      return comObj is Range;
    }

    public static object AsAny(object comObj)
    {
      object result;
      if (comObj is Missing)
        result = null;
      else if (comObj is Range)
        result = AsAny((Range)comObj);
      else
        result = comObj;
      return result;
    }

    public static object AsAny(Range range)
    {
      object result;
      result = range.Value2;
      if (result is double)
      {
        if (NumberFormat(range).ToLower().Contains("yy"))
          result = DateTime.FromOADate((double)result);
      }
      else if (result is Array)
      {
        result = Range2Matrix(range);
      }
      else if (range.HasArray != null
            && range.HasFormula is bool
            && ((bool)range.HasFormula)
            && result == null)
      {
        if (NumberFormat(range).ToLower().Contains("yy"))
        {
          result = DateTime.Parse((string)range.Text);
        }
        else if (range.Text is string)
        {
          result = range.Text;
        }
      }
      return result;
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

      if (typeof(T).Name == "Double" || typeof(T).Name == "Int32" )
      {
        if (result == null)
          result = 0;
        else if (result is DateTime)
          result = ((DateTime)result).ToOADate();
        else if (result is string && ((string)result).Trim().Length == 0)
          result = 0;
      }
      try
      {
        result = (T)System.Convert.ChangeType(result, typeof(T));
      }
        catch (Exception)
        {
            // ignored
        }
        return (T)result;
    }

    public static object[,] Range2Matrix(object objRange)
    {
      var range = (Range)objRange;
      var values = (object[,])range.get_Value(Type.Missing);
      var formulas = (object[,])range.Formula;

      if (values.GetUpperBound(0) != formulas.GetUpperBound(0) ||
          values.GetUpperBound(1) != formulas.GetUpperBound(1) ||
          values.GetLowerBound(0) != formulas.GetLowerBound(0) ||
          values.GetLowerBound(1) != formulas.GetLowerBound(1))
      {
        ExcelApiException.Throw(string.Format(ExcelApiResource.ErrorRangeToMatrix, GetExcelAddress(range)));
      }

      for (int i = values.GetLowerBound(0); i <= values.GetUpperBound(0); ++i)
      {
        for (int j = values.GetLowerBound(1); j <= values.GetUpperBound(1); ++j)
        {
          string formula = (string)formulas[i, j];
          if (IsError(formula))
          {
            values[i, j] = null;
          }
        }
      }
      return values;
    }

    public static string GetExcelAddress(object comObject)
    {
      string result = "";
      if (comObject != null && comObject is Range)
      {
        result = ((Range)(comObject)).get_Address(false, false, XlReferenceStyle.xlA1, true, false);
      }
      return result;
    }

    private static string NumberFormat(Range value)
    {
      string result = "";
      try
      {
        result = value.NumberFormat.ToString();
      }
        catch
        {
            // ignored
        }
        return result;
    }
    
    /// #NULL!   0x800a07d0  -2146826288
    /// #DIV/0!  0x800a07d7  -2146826281 
    /// #VALUE!  0x800a07df  -2146826273 
    /// #REF!    0x800a07e7  -2146826265
    /// #NAME?   0x800a07ed  -2146826259
    /// #NUM!    0x800a07f4  -2146826252
    /// #N/A     0x800a07fa  -2146826246
    private static readonly int[] ErrValues = { -2146826288, -2146826281, -2146826273, -2146826265, -2146826259, -2146826252, -2146826246};
    private static readonly string[] ErrStrings = { "#NULL!", "#DIV/0!", "#VALUE!", "#REF!", "#NAME?", "#NUM!", "#N/A" };
  
    public static bool IsError(object value)
    {
      return IndexOfError(value) >= 0;
    }

    public static string ErrorString(object value)
    {
      int idx = IndexOfError(value);
      return idx == -1 ? null : ErrStrings[idx];
    }

    private static int IndexOfError(object value)
    {
      int result = -1;
      if (value is int)
      {
        result = ErrValues.IndexOf((int)value);
      }
      else if (value is Range)
      {
        var range = (Range)value;
        if (range.Value2 is int)
          result = ErrValues.IndexOf((int)range.Value2);
      }
      else
      {
        result = ErrStrings.IndexOf(value as string);
      }
      return result;
    }
  }
}
