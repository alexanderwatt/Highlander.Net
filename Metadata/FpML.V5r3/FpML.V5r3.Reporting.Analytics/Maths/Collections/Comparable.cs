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

namespace Orion.Analytics.Maths.Collections
{
  public interface IComparable<T> : IComparer<T>
  {
    bool IsAscending();
    int Compare(T x);
  }

  //------------------------------------------------------------------------------------------------
  public class StringAscending : IComparable<string>
  {
    public StringAscending()
    {
    }

    public StringAscending(string rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return true;
    }

    public int Compare(string lhs, string rhs)
    {
      return String.CompareOrdinal(lhs, rhs);
    }

    public int Compare(string lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly string _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class StringAscendingNoCase : IComparable<string>
  {
    public StringAscendingNoCase()
    {
    }

    public StringAscendingNoCase(string rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return true;
    }

    public int Compare(string lhs, string rhs)
    {
      return String.Compare(lhs, rhs, StringComparison.OrdinalIgnoreCase);
    }

    public int Compare(string lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly string _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class StringDescending : IComparable<string>
  {
    public StringDescending()
    {
    }

    public StringDescending(string rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return false;
    }

    public int Compare(string lhs, string rhs)
    {
      return String.CompareOrdinal(rhs, lhs);
    }

    public int Compare(string lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly string _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class StringDescendingNoCase : IComparable<string>
  {
    public StringDescendingNoCase()
    {
    }

    public StringDescendingNoCase(string rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return false;
    }

    public int Compare(string lhs, string rhs)
    {
      return String.Compare(rhs, lhs, StringComparison.OrdinalIgnoreCase);
    }

    public int Compare(string lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly string _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class IntAscending : IComparable<int>
  {
    public IntAscending()
    {
    }

    public IntAscending(int rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return true;
    }

    public int Compare(int lhs, int rhs)
    {
      return lhs - rhs;
    }

    public int Compare(int lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly int _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class IntDescending : IComparable<int>
  {
    public IntDescending()
    {
    }

    public IntDescending(int rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return false;
    }

    public int Compare(int lhs, int rhs)
    {
      return rhs - lhs;
    }

    public int Compare(int lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly int _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class DateTimeAscending : IComparable<DateTime>
  {
    public DateTimeAscending()
    {
    }

    public DateTimeAscending(DateTime rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return true;
    }

    public int Compare(DateTime lhs, DateTime rhs)
    {
      return DateTime.Compare(lhs, rhs);
    }

    public int Compare(DateTime lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly DateTime _mRhs;
  }

  //------------------------------------------------------------------------------------------------
  public class DateTimeDescending : IComparable<DateTime>
  {
    public DateTimeDescending()
    {
    }

    public DateTimeDescending(DateTime rhs)
    {
      _mRhs = rhs;
    }

    public bool IsAscending() 
    {
      return false;
    }

    public int Compare(DateTime lhs, DateTime rhs)
    {
      return DateTime.Compare(rhs, lhs);
    }

    public int Compare(DateTime lhs)
    {
      return Compare(lhs, _mRhs);
    }

    private readonly DateTime _mRhs;
  }
}