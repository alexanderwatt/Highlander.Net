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
