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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HLV5r3.Impl
{
  public static class RangeConversion
  {
    public static List<T> RowsToList<T>(object[,] matrix)
    {
      var result = new List<T>();
      for (int idx = matrix.GetLowerBound(0) + 1; idx <= matrix.GetUpperBound(0); idx++)
        result.Add(RowToObject<T>(matrix, idx));
      return result;
    }

    public static List<T> ColumnsToList<T>(object[,] matrix)
    {
      var result = new List<T>();
      for (int idx = matrix.GetLowerBound(1) + 1; idx <= matrix.GetUpperBound(1); idx++)
        result.Add(ColumnToObject<T>(matrix, idx));
      return result;
    }

    public static T RowToObject<T>(object[,] matrix, int rowIndex)
    {
      var result = (T)Activator.CreateInstance(typeof(T));
      int nameIndex = matrix.GetLowerBound(0);
      for (int idx = matrix.GetLowerBound(1); idx <= matrix.GetUpperBound(1); idx++)
        SetValue(result, matrix[nameIndex, idx] as string, matrix[rowIndex, idx]);
      return result;
    }

    public static T ColumnToObject<T>(object[,] matrix, int colIndex)
    {
      var result = (T)Activator.CreateInstance(typeof(T));
      int nameIndex = matrix.GetLowerBound(1);
      for (int idx = matrix.GetLowerBound(0); idx <= matrix.GetUpperBound(0); idx++)
        SetValue(result, matrix[idx, nameIndex] as string, matrix[idx, colIndex]);
      return result;
    }

    public static void SetValue(object target, string name, object value)
    {
      try
      {
        Type type = target.GetType();
        FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (field != null)
        {
          var values = new[] { ChangeType(value, field.FieldType) };
          type.InvokeMember(field.Name, BindingFlags.SetField, null, target, values);
        }
        else
        {
          PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
          if (property != null)
            property.SetValue(target, ChangeType(value, property.PropertyType), new object[] { });
          else
            ExcelApiException.Throw(string.Format(ExcelApiResource.ErrorClassMemberNotExist, target.GetType().FullName, name));
        }
      }
      catch (Exception exp)
      {
        string message = string.Format(ExcelApiResource.ErrorInvalidPropertyValue,  "RangeConversion.SetValue", name, value);
        MethodErrors.Instance.Add(exp, message, false);
      }
    }

    public static object ChangeType(object value, Type type)
    {
      object result = null;
      if (!RangeFunctions.IsError(value))
      {
        if (type.IsEnum)
        {
            var enumValue = (string)Convert.ChangeType(value, typeof(string));
            if (enumValue != null) result = Enum.Parse(type, enumValue);
        }
        else
        {
          result = Convert.ChangeType(value, type);
        }
      }
      return result;
    }
  }
}
