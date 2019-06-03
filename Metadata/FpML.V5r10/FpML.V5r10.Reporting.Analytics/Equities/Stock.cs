using System;

namespace Orion.Analytics.Equities 
{
  public class Stock : IComparable
  {
      public double Dollars { get; set; }
      public string StockName { get; set; }
    
    public Stock(String stockName, Double dollars)
    {
      StockName = stockName;
      Dollars = dollars;
    }

    int IComparable.CompareTo(object obj)
    {
      var c = (Stock)obj;
      if (Dollars < c.Dollars)
        return 1;
        if (Dollars == c.Dollars)
            return 0;
        return -1;
    }
  }
}
