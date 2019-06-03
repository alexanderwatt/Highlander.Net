#region Usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// 
    /// </summary>
  public class ZeroCurve
  {
    private int _nratepoints = 0;
    private double[] _r; //Rate Vector
    private double[] _t; // Corresponding time vector
      


    //set the number of div points
      /// <summary>
      /// 
      /// </summary>
    public int ratepoints
    {
      get { return _nratepoints; }
      set { _nratepoints = value; }
    }

    //get rate item
      /// <summary>
      /// 
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
    public double get_r(int idx)
    {
      return _r[idx];
    }

    //get time item
      /// <summary>
      /// 
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
    public double get_t(int idx)
    {
      return _t[idx];
    }

    //set the  rate and time 
      /// <summary>
      /// 
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="r"></param>
      /// <param name="t"></param>
    public void set_r(int idx, double r, double t)
    {
      _r[idx] = r;
      _t[idx] = t;
    }

   
    //make the arrays
      /// <summary>
      /// 
      /// </summary>
    public void makeArrays()
    {
      if (_r == null)
      {
        _r = new double[ratepoints];
        _t = new double[ratepoints];
      }
    }

    //empty the arrays
      /// <summary>
      /// 
      /// </summary>
    public void emptyArrays()
    {
      _r = null;
      _t = null;
    }


    //Linear interp
      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <returns></returns>
    public double linInterp(double x)
    {
      if (x < get_t(0))
      {
        return get_r(0);
      }

      else if (x >= get_t(ratepoints - 1))
      {
        return get_r(ratepoints - 1);
      }

      else 
      for (int idx = 0; idx < ratepoints - 1; idx++)
      {
        if( (x >= get_t(idx)) && (x < get_t(idx+1)) )
        {
          double temp = get_r(idx) + ( (get_r(idx+1) - get_r(idx) ) * (x - get_t(idx)) /
            (get_t(idx+1) - get_t(idx)) );
          return temp;
        }
       }
     return 0.0;
     }

    //compute the forward rate between t1 and t2
      /// <summary>
      /// 
      /// </summary>
      /// <param name="t1"></param>
      /// <param name="t2"></param>
      /// <returns></returns>
    public double forwardRate(double t1, double t2)
    {
      if (t2 > t1)
      {
        double temp = (t2 * linInterp(t2) - t1 * linInterp(t1)) / (t2 - t1);
        return temp;
      }
      else
      {
        return 0.0;
      }
    }

  }
}
