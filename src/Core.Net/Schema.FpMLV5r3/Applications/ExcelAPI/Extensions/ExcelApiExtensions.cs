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

using System.Linq;
using System.Security;

namespace HLV5r3.Extensions
{
  /// <summary>
  /// 
  /// </summary>
  public static class ExcelApiExtensions
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="ch"></param>
    /// <returns></returns>
    public static int CountChar(this string value, char ch)
    {
        return value.Count(t => t == ch);
    }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      public static string EscapeXml(this string value)
    {
      return SecurityElement.Escape(value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static object Default<T>()
    {
      return default(T);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int IndexOf<T>(this T [] values, T value)
    {
      int result = -1;
      for (int idx = 0; idx < values.Length ; idx++)
      {
        if (values[idx].Equals(value))
        {
          result = idx;
          break;
        }
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chars"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int IndexOf(this char[] chars, char[] value)
    {
      int vdx = 0;
      int result = -1;
      int vLast = value.Length - 1;
      int cdx = 0;
      while (cdx < chars.Length)
      {
        if (chars[cdx] == value[vdx])
        {
          if (vdx == 0)
            result = cdx;
          else if (vdx == vLast)
            break;

          vdx++;
          cdx++;
        }
        else if (result != -1)
        {
          result = -1;
          vdx = 0;
        }
        else
        {
          vdx = 0;
          cdx++;
        }
      }
      return cdx == chars.Length ? -1 : result;
    }
  }
}
