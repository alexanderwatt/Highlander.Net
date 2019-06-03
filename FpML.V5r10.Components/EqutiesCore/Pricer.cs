using System;

namespace Orion.EquitiesCore
{
  /// <summary>
  /// 
  /// </summary>
  public class Pricer
  {

    #region Private Members

    private double[,] _priceMatrix;
      private double[] _r;
    private double[] _p;

      /// <summary>
    /// 
    /// </summary>
    public Pricer()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strike"></param>
    /// <param name="pay"></param>
    /// <param name="smoo"></param>
    /// <param name="sty"></param>
    public Pricer(double strike, string pay, string smoo, string sty)
    {
      Strike = strike;
      Payoff = pay;
      Smoothing = smoo;
      Style = sty;
    }

    #endregion

    #region Accessors

    //Set, get string style
    /// <summary>
    /// 
    /// </summary>
    public string Style { get; set; }

      //Get, set the gridsteps
    /// <summary>
    /// 
    /// </summary>
    public int Gridsteps { get; set; }

      //Set, get payoff
    /// <summary>
    /// 
    /// </summary>
    public string Payoff { get; set; }

      //Set, get the strike
    /// <summary>
    /// 
    /// </summary>
    public double Strike { get; set; }

      //Set, get the smoothing
    /// <summary>
    /// 
    /// </summary>
    public string Smoothing { get; set; }

      //Get forward rate
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetR(int idx)
    {
        return idx < Gridsteps ? _r[idx] : 0.0;
    }

    //Set the forward rate
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    public void SetR(int idx, double value)
    {
        if (idx >= Gridsteps) return;
        _r[idx] = value;
    }

    //get the prob.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetP(int idx)
    {
        return idx <= Gridsteps ? _p[idx] : 0.0;
    }

    //set the prob.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    public void SetP(int idx, double value)
    {
      if (idx <= Gridsteps)
      {
        _p[idx] = value;
      }
    }

    #endregion

    #region Private Methods

    //Create grid
    private void MakeArrays()
    {
        if (_priceMatrix != null) return;
        _priceMatrix = new double[Gridsteps + 1, Gridsteps + 1];
        _r = new double[Gridsteps];
    }

    //empty grid
    private void EmptyArrays()
    {
      _priceMatrix = null;
      _r = null;
    }


    //Fill rate vector using a tree object.
    private void FillRates(ITree spotTree)
    {
      if (spotTree != null)
      {
        _r = new double[Gridsteps];
        for (int idx = 0; idx < Gridsteps; idx++)
        {
          SetR(idx, spotTree.GetR(idx));
        }
      }
    }
    
    //Fill the prob vector using rates and Tree up and down vectors.
    private void FillP(ITree spotTree)
    {
      _p = new double[Gridsteps];
        double dt = spotTree.Tau / spotTree.Gridsteps;
      for (int i = 0; i < Gridsteps; i++)
      {
          var temp = (Math.Exp(spotTree.GetR(i) * dt) - spotTree.GetDn(i)) /
                        (spotTree.GetUp(i) - spotTree.GetDn(i));
          SetP(i, temp);
      }

    }

    //Assign terminal payoff condition.
    private void TermValue(ITree spotTree)
    {
        if (spotTree == null) return;
        for (int j = 0; j <= Gridsteps; j++)
        {
            double temp;
            Payoff = Payoff.ToLower();
            if ((Payoff == "c") | (Payoff == "call"))
            {
                temp = Math.Max(spotTree.GetSpotMatrix(Gridsteps, j) - Strike, 0);
                SetPriceMatrix(Gridsteps, j, temp);
            }
            else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
            {
                temp = Math.Max(Strike - spotTree.GetSpotMatrix(Gridsteps, j), 0);
                SetPriceMatrix(Gridsteps, j, temp);

            }
        }
    }
   
      //Assign values at Gridsteps-1 node.
      private void PreTermValue(ITree spotTree)
      {
      if (spotTree != null) 
          {
              double dt = spotTree.Tau / spotTree.Gridsteps;
          int idx = Gridsteps-1;
          for (int j = 0; j <= Gridsteps - 1; j++)
          {
              double temp;
              if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
                  {
                  if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                      {
                          temp = Math.Max(Math.Exp(-spotTree.GetR(idx) * dt) * (GetP(idx) *
                              GetPriceMatrix(idx + 1, j + 1) + (1 - GetP(idx)) *
                              GetPriceMatrix(idx + 1, j)), spotTree.GetSpotMatrix(idx, j) - Strike);                          
                      SetPriceMatrix(idx , j, temp);
                      }
                  else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                      {
                          temp = Math.Max(Math.Exp(-spotTree.GetR(idx) * dt) * (GetP(idx) *
                             GetPriceMatrix(idx + 1, j + 1) + (1 - GetP(idx)) *
                             GetPriceMatrix(idx + 1, j)), -spotTree.GetSpotMatrix(idx, j) + Strike);
                      SetPriceMatrix(idx , j, temp);
                      }
                  }
              else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
                  {
                  temp = Math.Exp(-spotTree.GetR(idx) * dt) * (GetP(idx) *
                         GetPriceMatrix(idx+1, j + 1) + (1 - GetP(idx )) *
                         GetPriceMatrix(idx+1, j));
                  SetPriceMatrix(idx , j, temp);

                  }
          }
          }
      }

      //Smoothing as describe in Orc quant guide.
      private void Smooth(string str, ITree spotTree)
      {
        if (str != null)
        {
          double dt = spotTree.Tau / spotTree.Gridsteps;
          if ((str.ToLower() == "y") | (str.ToLower() == "yes"))
          {
            int centre = 0;
            int k, j;
            int idx = Gridsteps - 1;
              for (k = 1; spotTree.GetSpotMatrix(idx, k - 1) <= Strike &&
                          spotTree.GetSpotMatrix(idx, k) <= Strike &&
                          k <= Gridsteps - 1; k++)
            {
            }
            if (k == 1)
            {
              centre = 2;
            }
            else if (k >= Gridsteps - 1)
            {
              centre = Gridsteps - 2;
            }
            else if (k <= Gridsteps - 2 && (k > 1))
            {
              if (Math.Abs(spotTree.GetSpotMatrix(idx, k - 2) / Strike - 1) >
                  Math.Abs(spotTree.GetSpotMatrix(idx, k + 1) / Strike - 1))
              {
                centre = k;
              }
              else
              {
                centre = k - 1;
              }
            }
            for (j = centre - 1; j <= centre + 1; j++)
            {
              double temp;
              if (Payoff.ToLower() == "c" || (Payoff.ToLower() == "call"))
              {
                //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                //, dt, Strike, get_r(idx), SpotTree.sig, "C");
                double fwd = spotTree.GetSpotMatrix(idx, j) - spotTree.GetDiv(idx);
                fwd *= Math.Exp(spotTree.GetR(idx) * dt);
                temp = BSprice(fwd, dt, Strike, spotTree.GetR(idx), spotTree.Sig, "C");
                SetPriceMatrix(idx, j, temp);
              }
              else if ((Payoff.ToLower() == "p") || (Payoff.ToLower() == "put"))
              {
                //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                //, dt, Strike, get_r(idx), SpotTree.sig, "P");
                double fwd = spotTree.GetSpotMatrix(idx, j) - spotTree.GetDiv(idx);
                fwd *= Math.Exp(spotTree.GetR(idx) * dt);
                temp = BSprice(fwd, dt, Strike, spotTree.GetR(idx), spotTree.Sig, "P");
                SetPriceMatrix(idx, j, temp);
              }
            }
          }
        }
      }

    #endregion

    #region Public Methods

      //set SpotMatrix item
      /// <summary>
      /// 
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="jdx"></param>
      /// <param name="value"></param>
      public void SetPriceMatrix(int idx, int jdx, double value)
      {
          if ((idx > Gridsteps) || (jdx > Gridsteps)) return;
          _priceMatrix[idx, jdx] = value;
      }

      //get SpotMatrix item
      /// <summary>
      /// 
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="jdx"></param>
      /// <returns></returns>
      public double GetPriceMatrix(int idx, int jdx)
      {
        return _priceMatrix[idx, jdx];
      }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fwd"></param>
    /// <param name="tau1"></param>
    /// <param name="strike1"></param>
    /// <param name="rate1"></param>
    /// <param name="sigma1"></param>
    /// <param name="payoff"></param>
    /// <returns></returns>
    public double BSprice(double fwd, double tau1, double strike1, double rate1,
            double sigma1, string payoff)
    {
        int s1;
      if ((payoff.ToLower() == "p") | (payoff.ToLower() == "put"))
      {
        s1 = -1;
      }
      else
      {
        s1 = 1;
      }
      var d1 = (Math.Log(fwd / strike1) + 0.5 * sigma1 * sigma1 * tau1)
                  / sigma1 / Math.Pow(tau1, 0.5);
      var d2 = d1 - sigma1 * Math.Pow(tau1, 0.5);
      var n1 = MathFunction.CND(s1 * d1);
      var n2 = MathFunction.CND(s1 * d2);
      return s1 * (fwd * n1 - strike1 * n2) * Math.Exp(-rate1 * tau1);      
    }

    //Make grid passing in a tree object.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="spotTree"></param>
    public void MakeGrid(ITree spotTree)
    {
      Gridsteps = spotTree.Gridsteps;
      EmptyArrays();
      MakeArrays();
      FillRates(spotTree);
      TermValue(spotTree);
      FillP(spotTree);
      PreTermValue(spotTree);
      Smooth(Smoothing, spotTree);
      //Iterate backward through tree to price American or Euro calls or puts.
      double temp;
      double dt = spotTree.Tau / spotTree.Gridsteps;

      if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
        for (int i = Gridsteps - 2; i >= 0; i--)
        {
          for (int j = 0; j <= i; j++)
          {
            if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
            {
              temp = Math.Max(Math.Exp(-spotTree.GetR(i) * dt) * (GetP(i) *
                  GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                  GetPriceMatrix(i + 1, j)), spotTree.GetSpotMatrix(i, j)
                  - Strike);
              SetPriceMatrix(i, j, temp);
            }
            else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
            {
              temp = Math.Max(Math.Exp(-spotTree.GetR(i) * dt) * (GetP(i) *
                 GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                 GetPriceMatrix(i + 1, j)), Strike - spotTree.GetSpotMatrix(i, j));
              SetPriceMatrix(i, j, temp);
            }
          }
        }
      else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
      {
        for (int i = Gridsteps - 2; i >= 0; i--)
        {
          for (int j = 0; j <= i ; j++)
          {
            temp = Math.Exp(-spotTree.GetR(i) * dt) * (GetP(i) *
                   GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                   GetPriceMatrix(i + 1, j));
            SetPriceMatrix(i, j, temp);
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public double Price()
    {
        return _priceMatrix != null ? GetPriceMatrix(0, 0) : -1.0;
    }

      #endregion
  }
}