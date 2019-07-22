#region Usings

using System;

#endregion

namespace Orion.Analytics.Equities 
{
  public class Pricer
  {

    #region Private Members

    public double[,] PriceMatrix {get; set;}
    public string Style {get; set;}
    public string Payoff {get; set;}
    public double Strike {get; set;}
    public double[] r {get; set;}
    public double[] p {get; set;}
    public int Gridsteps {get; set;}
    public string Smoothing {get; set;}

    public Pricer()
    {

    }

    public Pricer(double strike, string pay, string smoo, string sty)
    {
      Strike = strike;
      Payoff = pay;
      Smoothing = smoo;
      Style = sty;
    }

    #endregion

    #region Methods

    //Get forward rate
    public double get_r(int idx)
    {
      if (idx < Gridsteps)
      {
        return r[idx];
      }
      else
      {
        return 0.0;
      }
    }

    //Set the forward rate
    public void set_r(int idx, double value)
    {
      if (idx < Gridsteps)
      {
        r[idx] = value;
      }
    }

    //get the prob.
    public double Get_P(int idx)
    {
      if (idx <= Gridsteps)
      {
        return p[idx];
      }
      return 0.0;
    }

    //set the prob.
    public void Set_P(int idx, double value)
    {
      if (idx <= Gridsteps)
      {
        p[idx] = value;
      }
    }

    #endregion


    #region Private Methods

    //Create grid
    private void MakeArrays()
    {
      if (PriceMatrix == null)
      {
        PriceMatrix = new double[Gridsteps + 1, Gridsteps + 1];
        r = new double[Gridsteps];
      }
    }

    //empty grid
    private void EmptyArrays()
    {
      PriceMatrix = null;
      r = null;
    }


    //Fill rate vector using a tree object.
    private void FillRates(Tree SpotTree)
    {

      if (SpotTree != null)
      {
        r = new double[Gridsteps];
        for (int idx = 0; idx < Gridsteps; idx++)
        {
          set_r(idx, SpotTree.GetR(idx));
        }
      }
    }
    
    //Fill the prob vector using rates and Tree up and down vectors.
    private void FillP(Tree SpotTree)
    {
      p = new double[Gridsteps];
      double temp = 0;
      double dt = SpotTree.Tau / SpotTree.Gridsteps;
      for (int i = 0; i < Gridsteps; i++)
      {
        temp = (Math.Exp(SpotTree.GetR(i) * dt) - SpotTree.get_dn(i)) /
                (SpotTree.get_up(i) - SpotTree.get_dn(i));
        Set_P(i, temp);
      }

    }

    

    //Assign terminal payoff condition.
    private void TermValue(Tree SpotTree)
    {
      if (SpotTree != null)
      {
        for (int j = 0; j <= Gridsteps; j++)
        {
          double temp;
          Payoff = Payoff.ToLower();
          if ((Payoff == "c") | (Payoff == "call"))
          {
            temp = Math.Max(SpotTree.GetSpotMatrix(Gridsteps, j) - Strike, 0);
            Set_PriceMatrix(Gridsteps, j, temp);
          }
          else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
          {
            temp = Math.Max(Strike - SpotTree.GetSpotMatrix(Gridsteps, j), 0);
            Set_PriceMatrix(Gridsteps, j, temp);

          }
        }
      }
    }

      //Assign values at Gridsteps-1 node.
      private void PreTermValue(Tree SpotTree)
      {
      if (SpotTree != null) 
          {

          double temp = 0;
          double dt = SpotTree.Tau / SpotTree.Gridsteps;
          int idx = Gridsteps-1;

          for (int j = 0; j <= Gridsteps - 1; j++)
              {
              if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
                  {
                  if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                      {
                      temp = Math.Max(Math.Exp(-SpotTree.GetR(idx) * dt) * (Get_P(idx) *
                          Get_PriceMatrix(idx+1, j + 1) + (1 - Get_P(idx)) *
                          Get_PriceMatrix(idx+1, j)), SpotTree.GetSpotMatrix(idx, j)
                          - Strike);
                      Set_PriceMatrix(idx , j, temp);
                      }
                  else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                      {
                      temp = Math.Max(Math.Exp(-SpotTree.GetR(idx) * dt) * (Get_P(idx) *
                         Get_PriceMatrix(idx+1, j + 1) + (1 - Get_P(idx)) *
                         Get_PriceMatrix(idx+1, j)), -SpotTree.GetSpotMatrix(idx, j)
                         + Strike);
                      Set_PriceMatrix(idx , j, temp);
                      }
                  }
              else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
                  {
                  temp = Math.Exp(-SpotTree.GetR(idx) * dt) * (Get_P(idx) *
                         Get_PriceMatrix(idx+1, j + 1) + (1 - Get_P(idx )) *
                         Get_PriceMatrix(idx+1, j));
                  Set_PriceMatrix(idx , j, temp);

                  }
              }
          }
      }

      //Smoothing as describe in Orc quant guide.
      private void Smooth(string str, Tree SpotTree)
      {
        if (str != null)
        {
            double dt = SpotTree.Tau / SpotTree.Gridsteps;



          if ((str.ToLower() == "y") | (str.ToLower() == "yes"))
          {
            int Centre = 0;
            int k, j;
            int idx = Gridsteps - 1;
            k = 1;


            for (k = 1; ((SpotTree.GetSpotMatrix(idx, k - 1) <= Strike) &&
                        ((SpotTree.GetSpotMatrix(idx, k) <= Strike)) &&
                        ((k <= Gridsteps - 1))); k++)
            {
            }
            if (k == 1)
            {
              Centre = 2;
            }
            else if (k >= Gridsteps - 1)
            {
              Centre = Gridsteps - 2;
            }
            else if ((k <= Gridsteps - 2) && (k > 1))
            {
              if (Math.Abs(SpotTree.GetSpotMatrix(idx, k - 2) / Strike - 1) >
                  Math.Abs(SpotTree.GetSpotMatrix(idx, k + 1) / Strike - 1))
              {
                Centre = k;
              }
              else
              {
                Centre = k - 1;
              }
            }

            for (j = Centre - 1; j <= Centre + 1; j++)
            {
              double temp = 0;

              if ((Payoff.ToLower() == "c") || (Payoff.ToLower() == "call"))
              {
                //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                //, dt, Strike, get_r(idx), SpotTree.sig, "C");
                double fwd = SpotTree.GetSpotMatrix(idx, j) - SpotTree.get_div(idx);
                fwd *= Math.Exp(SpotTree.GetR(idx) * dt);
                temp = BSprice(fwd, dt, Strike, SpotTree.GetR(idx), SpotTree.Sig, "C");
                Set_PriceMatrix(idx, j, temp);
              }
              else if ((Payoff.ToLower() == "p") || (Payoff.ToLower() == "put"))
              {
                //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                //, dt, Strike, get_r(idx), SpotTree.sig, "P");
                double fwd = SpotTree.GetSpotMatrix(idx, j) - SpotTree.get_div(idx);
                fwd *= Math.Exp(SpotTree.GetR(idx) * dt);
                temp = BSprice(fwd, dt, Strike, SpotTree.GetR(idx), SpotTree.Sig, "P");
                Set_PriceMatrix(idx, j, temp);
              }
            }

          }
        }
      }


    #endregion


    #region Public Methods

      //set SpotMatrix item
      public void Set_PriceMatrix(int idx, int jdx, double value)
      {
        if ((idx <= Gridsteps) && (jdx <= Gridsteps))
        {
          PriceMatrix[idx, jdx] = value;
        }
      }

      //get SpotMatrix item
      public double Get_PriceMatrix(int idx, int jdx)
      {
        return PriceMatrix[idx, jdx];
      }

    public double BSprice(double Fwd, double Tau1, double Strike1, double Rate1,
            double Sigma1, string Payoff)
    {
      double d1, d2, n1, n2;
      int S1 = 0;
      if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
      {
        S1 = -1;
      }
      else
      {
        S1 = 1;
      }

      d1 = (Math.Log(Fwd / Strike1) + 0.5 * Sigma1 * Sigma1 * Tau1)
          / Sigma1 / Math.Pow(Tau1, 0.5);
      d2 = d1 - Sigma1 * Math.Pow(Tau1, 0.5);
      n1 = MathFunction.CND(S1 * d1);
      n2 = MathFunction.CND(S1 * d2);
      return S1 * (Fwd * n1 - Strike1 * n2) * Math.Exp(-Rate1 * Tau1);      
    }



    //Make grid passing in a tree object.
    public void MakeGrid(Tree SpotTree)
    {
      Gridsteps = SpotTree.Gridsteps;
      EmptyArrays();
      MakeArrays();
      FillRates(SpotTree);
      TermValue(SpotTree);
      FillP(SpotTree);
      PreTermValue(SpotTree);
      Smooth(Smoothing, SpotTree);

      //Iterate backward through tree to price American or Euro calls or puts.

      double temp = 0;
      double dt = SpotTree.Tau / SpotTree.Gridsteps;

      if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
        for (int i = Gridsteps - 2; i >= 0; i--)
        {
          for (int j = 0; j <= i; j++)
          {
            if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
            {
              temp = Math.Max(Math.Exp(-SpotTree.GetR(i) * dt) * (Get_P(i) *
                  Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                  Get_PriceMatrix(i + 1, j)), SpotTree.GetSpotMatrix(i, j)
                  - Strike);
              Set_PriceMatrix(i, j, temp);
            }
            else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
            {
              temp = Math.Max(Math.Exp(-SpotTree.GetR(i) * dt) * (Get_P(i) *
                 Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                 Get_PriceMatrix(i + 1, j)), Strike - SpotTree.GetSpotMatrix(i, j));
              Set_PriceMatrix(i, j, temp);
            }
          }
        }
      else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
      {
        for (int i = Gridsteps - 2; i >= 0; i--)
        {
          for (int j = 0; j <= i ; j++)
          {
            temp = Math.Exp(-SpotTree.GetR(i) * dt) * (Get_P(i) *
                   Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                   Get_PriceMatrix(i + 1, j));
            Set_PriceMatrix(i, j, temp);
          }
        }
      }
    }

    public double Price()
    {
      if (PriceMatrix != null)
      {
        return Get_PriceMatrix(0, 0);
      }
      else
      {
        return -1.0;
      }
    }

      #endregion

  }
}