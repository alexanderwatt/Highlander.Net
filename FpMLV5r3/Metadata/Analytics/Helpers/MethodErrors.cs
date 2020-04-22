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
using Highlander.Reporting.Analytics.V5r3.Maths.Collections;
using Highlander.Utilities.Helpers;

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
  [Serializable]
  public class MethodErrors
  {
    /// <summary>
    /// 
    /// </summary>
    public static MethodErrors Instance => _instance ?? (_instance = new MethodErrors());

      //----------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public int Count => _callErrors.Count;

      /// <summary>
      /// 
      /// </summary>
      public int ErrorCount
    {
      get
      {
        int result = 0;
        for (int idx = 0; idx < Count; idx++)
        {
          if (!this[idx].AsWarning) result++;
        }
        return result;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int WarningCount
    {
      get
      {
        int result = 0;
        for (int idx = 0; idx < Count; idx++)
        {
          if (this[idx].AsWarning) result++;
        }
        return result;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string ErrorLines
    {
      get
      {
        string result = "";
        for (int idx = 0; idx < Count; idx++)
        {
          if (!this[idx].AsWarning)
          {
            if (result.Length > 0) result += Environment.NewLine;
            result += this[idx].Message;
          }
        }
        return result;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string WarningLines
    {
      get
      {
        string result = "";
        for (int idx = 0; idx < Count; idx++)
        {
          if (this[idx].AsWarning)
          {
            if (result.Length > 0) result += Environment.NewLine;
            result += this[idx].Message;
          }
        }
        return result;
      }
    }

    //----------------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    public MethodError this[int idx] => _callErrors[idx];

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      public void Add(MethodError item)
    {
      _callErrors.Add(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    public void Add(MethodErrors items)
    {
      _callErrors.Add(items._callErrors);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="tag"></param>
    public void Add(Exception exp, string tag)
    {
      Add(exp, tag, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="tag"></param>
    /// <param name="asWarning"></param>
    public void Add(Exception exp, string tag, bool asWarning)
    {
      if (exp?.GetBaseException() != null)
        exp = exp.GetBaseException();
      Add(exp?.Message, exp?.StackTrace, tag, asWarning);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="stackTrace"></param>
    /// <param name="tag"></param>
    public void Add(string message, string stackTrace, string tag)
    {
      Add(message, stackTrace, tag, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="stackTrace"></param>
    /// <param name="tag"></param>
    /// <param name="asWarning"></param>
    public void Add(string message, string stackTrace, string tag, bool asWarning)
    {
        var err = new MethodError
        {
            DomainName = AppDomain.CurrentDomain.FriendlyName,
            Message = message,
            StackTrace = stackTrace ?? "",
            Tag = tag ?? "",
            AsWarning = asWarning
        };
        Add(err);
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveAll()
    {
      _callErrors.RemoveAll();
    }

    private readonly Vector<MethodError> _callErrors = new Vector<MethodError>();
    [ThreadStatic] private static MethodErrors _instance;
  }
}
