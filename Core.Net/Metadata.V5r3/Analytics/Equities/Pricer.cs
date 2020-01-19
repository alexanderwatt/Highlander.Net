/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Equities 
{
    public class Pricer
    {
        #region Private Members

        public double[,] PriceMatrix {get; set;}

        public string Style {get; set;}
        
        public string Payoff {get; set;}
        
        public double Strike {get; set;}
        
        public double[] R {get; set;}
        
        public double[] P {get; set;}
        
        public int GridSteps {get; set;}
        
        public string Smoothing {get; set;}

        
          public Pricer()
          {
          }

          public Pricer(double strike, string pay, string smoothing, string sty)
          {
          Strike = strike;
          Payoff = pay;
          Smoothing = smoothing;
          Style = sty;
          }

          #endregion

      #region Methods

    //Get forward rate
    public double GetR(int idx)
    {
        return idx < GridSteps ? R[idx] : 0.0;
    }

    //Set the forward rate
    public void SetR(int idx, double value)
    {
      if (idx < GridSteps)
      {
        R[idx] = value;
      }
    }

    //get the prob.
    public double GetP(int idx)
    {
        return idx <= GridSteps ? P[idx] : 0.0;
    }

    //set the prob.
    public void SetP(int idx, double value)
    {
      if (idx <= GridSteps)
      {
        P[idx] = value;
      }
    }

    #endregion

    #region Private Methods

    //Create grid
    private void MakeArrays()
    {
        if (PriceMatrix != null) return;
        PriceMatrix = new double[GridSteps + 1, GridSteps + 1];
      R = new double[GridSteps];
    }

    //empty grid
    private void EmptyArrays()
    {
      PriceMatrix = null;
      R = null;
    }

    //Fill rate vector using a tree object.
    private void FillRates(Tree spotTree)
    {
        if (spotTree == null) return;
        R = new double[GridSteps];
        for (int idx = 0; idx < GridSteps; idx++)
        {
            SetR(idx, spotTree.GetR(idx));
        }
    }
    
    //Fill the prob vector using rates and Tree up and down vectors.
    private void FillP(Tree spotTree)
    {
      P = new double[GridSteps];
      double dt = spotTree.Tau / spotTree.GridSteps;
      for (int i = 0; i < GridSteps; i++)
      {
          var temp = (Math.Exp(spotTree.GetR(i) * dt) - spotTree.GetDn(i)) /
                        (spotTree.GetUp(i) - spotTree.GetDn(i));
          SetP(i, temp);
      }
    }

    //Assign terminal payoff condition.
    private void TermValue(Tree spotTree)
    {
        if (spotTree != null)
        {
            for (int j = 0; j <= GridSteps; j++)
            {
                double temp;
                Payoff = Payoff.ToLower();
                if ((Payoff == "c") | (Payoff == "call"))
                {
                    temp = Math.Max(spotTree.GetSpotMatrix(GridSteps, j) - Strike, 0);
                    SetPriceMatrix(GridSteps, j, temp);
                }
                else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                {
                    temp = Math.Max(Strike - spotTree.GetSpotMatrix(GridSteps, j), 0);
                    SetPriceMatrix(GridSteps, j, temp);

                }
            }
        }
    }

      //Assign values at Gridsteps-1 node.
      private void PreTermValue(Tree spotTree)
      {
          if (spotTree != null) 
          {
              double dt = spotTree.Tau / spotTree.GridSteps;
              int idx = GridSteps-1;
              for (int j = 0; j <= GridSteps - 1; j++)
              {
                  double temp;
                  if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
                  {
                      if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                      {
                          temp = Math.Max(Math.Exp(-spotTree.GetR(idx) * dt) * (GetP(idx) *
                                                                                GetPriceMatrix(idx+1, j + 1) + (1 - GetP(idx)) *
                                                                                GetPriceMatrix(idx+1, j)), spotTree.GetSpotMatrix(idx, j)
                                                                                                           - Strike);
                          SetPriceMatrix(idx , j, temp);
                      }
                      else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                      {
                          temp = Math.Max(Math.Exp(-spotTree.GetR(idx) * dt) * (GetP(idx) *
                                                                                GetPriceMatrix(idx+1, j + 1) + (1 - GetP(idx)) *
                                                                                GetPriceMatrix(idx+1, j)), -spotTree.GetSpotMatrix(idx, j)
                                                                                                           + Strike);
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
      private void Smooth(string str, Tree spotTree)
      {
        if (str != null)
        {
            double dt = spotTree.Tau / spotTree.GridSteps;
            if ((str.ToLower() == "y") | (str.ToLower() == "yes"))
            {
                int centre = 0;
                int j;
                int idx = GridSteps - 1;
                int k;
                for (k = 1; spotTree.GetSpotMatrix(idx, k - 1) <= Strike &&
                            spotTree.GetSpotMatrix(idx, k) <= Strike &&
                            k <= GridSteps - 1; k++)
                {
                }
                if (k == 1)
                {
                    centre = 2;
                }
                else if (k >= GridSteps - 1)
                {
                    centre = GridSteps - 2;
                }
                else if ((k <= GridSteps - 2) && (k > 1))
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
                    double temp = 0;
                    if ((Payoff.ToLower() == "c") || (Payoff.ToLower() == "call"))
                    {
                        //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                        //, dt, Strike, get_r(idx), SpotTree.sig, "C");
                        double fwd = spotTree.GetSpotMatrix(idx, j) - spotTree.GetDIV(idx);
                        fwd *= Math.Exp(spotTree.GetR(idx) * dt);
                        temp = BSprice(fwd, dt, Strike, spotTree.GetR(idx), spotTree.Sig, "C");
                        SetPriceMatrix(idx, j, temp);
                    }
                    else if ((Payoff.ToLower() == "p") || (Payoff.ToLower() == "put"))
                    {
                        //temp = BSprice(SpotTree.Get_SpotMatrix(idx, j)
                        //, dt, Strike, get_r(idx), SpotTree.sig, "P");
                        double fwd = spotTree.GetSpotMatrix(idx, j) - spotTree.GetDIV(idx);
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
      public void SetPriceMatrix(int idx, int jdx, double value)
      {
        if ((idx <= GridSteps) && (jdx <= GridSteps))
        {
          PriceMatrix[idx, jdx] = value;
        }
      }

      //get SpotMatrix item
      public double GetPriceMatrix(int idx, int jdx)
      {
        return PriceMatrix[idx, jdx];
      }

    public double BSprice(double fwd, double tau1, double strike1, double rate1,
            double sigma1, string payoff)
    {
        int s1 = 0;
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
    public void MakeGrid(Tree spotTree)
    {
      GridSteps = spotTree.GridSteps;
      EmptyArrays();
      MakeArrays();
      FillRates(spotTree);
      TermValue(spotTree);
      FillP(spotTree);
      PreTermValue(spotTree);
      Smooth(Smoothing, spotTree);

      //Iterate backward through tree to price American or Euro calls or puts.

      double temp = 0;
      double dt = spotTree.Tau / spotTree.GridSteps;

      if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
        for (int i = GridSteps - 2; i >= 0; i--)
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
        for (int i = GridSteps - 2; i >= 0; i--)
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

    public double Price()
    {
        if (PriceMatrix != null)
        {
            return GetPriceMatrix(0, 0);
        }
        return -1.0;
    }

      #endregion
    }
}