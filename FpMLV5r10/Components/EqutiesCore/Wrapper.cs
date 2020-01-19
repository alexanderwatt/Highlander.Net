using System;

namespace Orion.EquitiesCore
{
  /// <summary>
  /// 
  /// </summary>
  public class Wrapper
  {

      /// <summary>
      /// 
      /// </summary>
      /// <param name="today"></param>
      /// <param name="dates"></param>
      /// <param name="amts"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public ZeroCurve UnpackZero(DateTime today, DateTime[] dates, double[] amts)
      {
          int n1 = dates.Length;
          int n2 = dates.Length;
          if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
          var zc = new ZeroCurve {Ratepoints = n1};
          zc.MakeArrays();
          int kdx = 0;
          for (int idx=0; idx<n1;idx++)
          {
              double time = dates[idx].Subtract(today).Days/365.0;
              double rate = amts[idx];            
              zc.SetR(kdx, rate, time);
              kdx++;          
          }         
          return zc;
      }

      int _kdx;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="today"></param>
      /// <param name="expiry"></param>
      /// <param name="dates"></param>
      /// <param name="amts"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public DivList UnpackDiv(DateTime today, DateTime expiry, DateTime[] dates, double[] amts)
      {
          int n1 = dates.Length;
          int n2 = dates.Length;
          double timetoexp = expiry.Subtract(today).Days / 365.0;
          if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
          var dl = new DivList {Divpoints = n1};
          dl.MakeArrays();
          for (int idx = 0; idx < n1; idx++)
          {
              double time = dates[idx].Subtract(today).Days / 365.0;
              double rate = amts[idx];
              if (time > 0 & time <= timetoexp)
              {
                  dl.SetD(_kdx, rate, time);
                  _kdx++;
              }
          }
          return dl;
      }
  }
}
