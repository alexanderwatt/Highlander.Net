#region Usings

using System;
using Orion.Analytics.Rates;

#endregion

namespace Orion.Analytics.Equities 
{

  public class Tree
  {
    #region Private Members

      public int Gridsteps { get; set; }
      public double Tau { get; set; }
      public double Sig { get; set; }
      public double Spot { get; set; }
      public double Spotstar { get; set; }
      public double[,] SpotMatrix { get; set; }
      public double[] up { get; set; }
      public double[] dn { get; set; }
      public double[] r { get; set; }
      public double[] div { get; set; }
      public double[] divtime { get; set; }
      public double flatRate { get; set; }
      public bool flatFlag = true;

    public Tree()
    {

    }

    public Tree(double t, double vol, double spot, int steps, bool flag)
    {
        Tau = t;
        Sig = vol;
        Spot = spot;
        Gridsteps = steps;
        flatFlag = flag;
    }

    #endregion

    #region Methods

    //get forward rate item
    public double GetR(int idx)
    {
        if (idx < Gridsteps)
          {
            return r[idx];
          }
        return 0.0;
    }

    //set the forward rate
    public void SetR(int idx, double value)
    {
      if (r != null)
      {
        if (idx < Gridsteps)
        {
          r[idx] = value;
        }
      }
    }

    //set the div rate
    public void set_div(int idx, double value, double t)
    {
      div[idx] = value;
      divtime[idx] = t;
    }

    //get div rate item
    public double get_div(int idx)
    {
      return div[idx];
    }

    //get div rate item
    public double get_divtime(int idx)
    {
      return divtime[idx];
    }

    //get up item
    public double get_up(int idx)
    {
      if (idx < Gridsteps)
      {
        return up[idx];
      }
      else
      {
        return 0.0;
      }
    }

    //set the up itm
    public void set_up(int idx, double value)
    {
      if (idx < Gridsteps)
      {
        up[idx] = value;
      }
    }

    //get dn item
    public double get_dn(int idx)
    {
      if (idx < Gridsteps)
      {
        return dn[idx];
      }
      else
      {
        return 0.0;
      }
    }

    //set the dn itm
    public void set_dn(int idx, double value)
    {
      if (idx < Gridsteps)
      {
        dn[idx] = value;
      }
    }

    #endregion

    #region Public Methods

    //get SpotMatrix item
    public double GetSpotMatrix(int idx, int jdx)
    {
      return SpotMatrix[idx, jdx];
    }

    //set SpotMatrix item
    public void SetSpotMatrix(int idx, int jdx, double value)
    {
      if ((idx <= Gridsteps) && (jdx <= Gridsteps))
      {
        SpotMatrix[idx, jdx] = value;
      }
    }


    //public MakeGrid
    public void MakeGrid(ZeroCurve myZero, DivList myDivList)
    {
      EmptyArrays();
      MakeArrays();
      MakeSpotStar(myZero, myDivList);
      MakeDivArray(myZero, myDivList);
      FillForwardRate(myZero);
      FillUpDown(Sig);

      //make the spot grid
      double sv = Spotstar;
      SetSpotMatrix(0, 0, sv+get_div(0)) ;

      //now march foraward in time
      for (int idx = 1; idx <= Gridsteps; idx++)
      {
        for (int jdx = 0; jdx <= idx; jdx++)
        {
          sv = Spotstar * Math.Pow(get_up(idx - 1), jdx) * Math.Pow(get_dn(idx - 1), idx - jdx);
          sv += (idx == Gridsteps) ? 0.0 : get_div(idx);
           //get_div(idx);
          SetSpotMatrix(idx, jdx, sv);
        }
      }
    }

    #endregion

    #region Private Methods

    //create grid
    private void MakeArrays()
    {
      if (SpotMatrix == null)
      {
        SpotMatrix = new double[Gridsteps + 1, Gridsteps + 1];
        up = new double[Gridsteps];
        dn = new double[Gridsteps];
        r = new double[Gridsteps];
        div = new double[Gridsteps];
        divtime = new double[Gridsteps];
      }
    }

    //empty grid
    private void EmptyArrays()
    {
      SpotMatrix = null;
      up = null;
      dn = null;
      div = null;
    }

    //fill grid forward rates
    private void FillForwardRate(ZeroCurve myZero)
    {

      double dt = Tau / Gridsteps;
      if (myZero != null)
      {
        if (flatFlag)
            flatRate = myZero.LinInterp(Tau);
        for (int idx = 0; idx < Gridsteps; idx++)
        {
            SetR(idx, flatFlag ? flatRate : myZero.ForwardRate(idx * dt, (idx + 1) * dt));
        }
      }
    }

    //create the up/down arrays
    private void FillUpDown(double sig)
    {
      if (up != null)
      {
          double dt = Tau / Gridsteps;
        for (int idx = 0; idx < Gridsteps; idx++)
        {
          double a = (1.0 + Math.Exp((2 * GetR(idx) + sig * sig) * dt));
          double upval = (a + Math.Sqrt(a * a - 4.0 * Math.Exp(2.0 * GetR(idx) * dt)))
              / (2.0 * Math.Exp(GetR(idx) * dt));
          set_up(idx, upval);
          set_dn(idx, 1.0 / upval);
        }
      }
    }

    //public spotStar
    private void MakeSpotStar(ZeroCurve myZero, DivList myDivList)
    {
      Spotstar = Spot;

      if ((myDivList != null) && (myZero != null))
      {
        for (int idx = 0; idx < myDivList.Divpoints; idx++)
        {
            if (myDivList.GetT(idx) <= Tau)
          {
            double d1 = myDivList.GetD(idx);
            double r1 = myZero.LinInterp(myDivList.GetT(idx));
            double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
            Spotstar -= d1 * t1;
          }
        }
      }
    }

    //public spotStar
    private void MakeDivArray(ZeroCurve myZero, DivList myDivList)
    {
        double dt = Tau / Gridsteps;
      if ((myDivList != null) && (myZero != null))
      {
        for (int idx = 0; idx < Gridsteps; idx++)
        {
          double temp = 0.0;
          for (int kdx = 0; kdx < myDivList.Divpoints; kdx++)
          {
              if ((myDivList.GetT(kdx) > idx * dt) && (myDivList.GetT(kdx) < Tau))
            {

              temp += myDivList.GetD(kdx) * Math.Exp(-myZero.ForwardRate(idx * dt, myDivList.GetT(kdx)) *
                 (myDivList.GetT(kdx) - idx * dt));
            }
          }
          set_div(idx, temp, dt * idx);
        }
      }
      else  //missing either div or zero, load in 0 for _div on tree
      {
        for (int idx = 0; idx <= Gridsteps; idx++)
        {
          set_div(idx, 0.0, dt * idx);
        }
      }
    }


    #endregion
  }



}
