using System;
using System.Runtime.Serialization;

namespace HLV5r3.Impl
{
  [Serializable]
  public class ExcelApiException : Exception
  {
    public static void Throw(string message)
    {
      var exp = new ExcelApiException(message);
      MethodErrors.Instance.Add(exp, "");
      throw exp;
    }

    public static void Throw(Exception e, string tag)
    {
      var exp = new ExcelApiException(e, tag);
      MethodErrors.Instance.Add(e, tag);
      throw exp;
    }

    public ExcelApiException(SerializationInfo info, StreamingContext context)
      :base(info, context)
    {
    }

    public ExcelApiException(string message)
      : base(message)
    {
    }
    public ExcelApiException(Exception exp, string message) 
      : base($"{exp.Message} ({message})")
    {
    }
  }
}
