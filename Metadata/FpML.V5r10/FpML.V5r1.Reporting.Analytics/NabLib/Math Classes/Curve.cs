using System;
using System.Net;

namespace nab.QR.PricingModelNab1
{
  public class Curve
  {
    private double[] rates;
    private double[] times;


    public Curve(long n, double[] t, double[] r)
    {
      rates = new double[n + 1];
      times = new double[n + 1];

      for (long idx = 0; idx <= n; idx++)
      {
        rates[idx] = r[idx];
        times[idx] = t[idx];
      }
    }

    public Curve(long n, double[] t, double r)
    {
      rates = new double[n + 1];
      times = new double[n + 1];

      for (long idx = 0; idx <= n; idx++)
      {
        rates[idx] = r;
        times[idx] = t[idx];
      }
    }



    public double GetRate(int idx)
    {
      return rates[idx];
    }

    public void SetRate(int idx, double val)
    {
      if (idx < rates.Length)
        rates[idx] = val;
    }


    public double Interpolate(double x)
    {
      double temp = 0.0;

      int n = times.Length;
      if (n == 0) return temp;

      if (x <= times[0])
      {
        temp = rates[0];
      }
      else if (x > times[n - 1])
      {
        temp = rates[n - 1];
      }
      else
      {
        for (int idx = 0; idx < n - 1; idx++)
        {
          if (x > times[idx] && x <= times[idx + 1])
          {
            temp = rates[idx + 1];
            break;
          }
        }
      }
      return temp;
    }

    public double LinInterpolate(double x)
    {
      double temp = 0.0;

      int n = times.Length;
      if (n == 0) return temp;

      if (x <= times[0])
      {
        temp = rates[0];
      }
      else if (x > times[n - 1])
      {
        temp = rates[n - 1];
      }
      else
      {
        for (int idx = 0; idx < n - 1; idx++)
        {
          if ((x > times[idx]) && (x <= times[idx + 1]))
          {
            temp = rates[idx] + (rates[idx + 1] - rates[idx]) * (x - times[idx])
              / (times[idx + 1] - times[idx]);
            break;
          }
        }
      }
      return temp;
    }

    public double LinVarInterpolate(double x)
    {
      double temp = 0.0;

      int n = times.Length;
      if (n == 0) return temp;

      if (x <= times[0])
      {
        temp = rates[0];
      }
      else if (x > times[n - 1])
      {
        temp = rates[n - 1];
      }
      else
      {
        for (int idx = 0; idx < n - 1; idx++)
        {
          if ((x > times[idx]) && (x <= times[idx + 1]))
          {
            double up = rates[idx + 1] * rates[idx + 1] * times[idx + 1];
            double lo = rates[idx] * rates[idx] * times[idx];
            temp = lo + (up - lo) * (x - times[idx]) / (times[idx + 1] - times[idx]);
            temp = Math.Sqrt(temp / x);
            break;
          }
        }
      }
      return temp;
    }
  }
}
