using System;
using Orion.Analytics.Maths.Collections;

namespace HLV5r3.Impl
{
  [Serializable]
  public class MethodErrors
  {
    public static MethodErrors Instance => _instance ?? (_instance = new MethodErrors());

      //----------------------------------------------------------------------------------------------
    public int Count => _callErrors.Count;

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
    public MethodError this[int idx] => _callErrors[idx];

      public void Add(MethodError item)
    {
      _callErrors.Add(item);
    }

    public void Add(MethodErrors items)
    {
      _callErrors.Add(items._callErrors);
    }

    public void Add(Exception exp, string tag)
    {
      Add(exp, tag, false);
    }

    public void Add(Exception exp, string tag, bool asWarning)
    {
      if (exp?.GetBaseException() != null)
        exp = exp.GetBaseException();
      Add(exp.Message, exp.StackTrace, tag, asWarning);
    }

    public void Add(string message, string stackTrace, string tag)
    {
      Add(message, stackTrace, tag, false);
    }

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

    public void RemoveAll()
    {
      _callErrors.RemoveAll();
    }

    private readonly Vector<MethodError> _callErrors = new Vector<MethodError>();
    [ThreadStatic] private static MethodErrors _instance;
  }
}
