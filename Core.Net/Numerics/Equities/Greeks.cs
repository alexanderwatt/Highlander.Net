/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using Highlander.Numerics.Rates;

#endregion

namespace Highlander.Numerics.Equities 
{
     public class Greeks
     {
         public double dt { get; set; }

         public int GridSteps = 0;

         public double Tau { get; set; }

         public double Sigma { get; set; }

         public double Spot { get; set; }

         public string Style { get; set; }

         public string Payoff { get; set; }

         public double Strike { get; set; }

         public string Smoothing { get; set; }

         public double Delta { get; set; }

         public double Gamma { get; set; }

         public double Vega { get; set; }

         public double Theta { get; set; }

         private void SetParam(Tree spotT, Pricer priceT)
         {
             if ((spotT != null) && (priceT != null))
             {
                 GridSteps = spotT.GridSteps;
                 Tau = spotT.Tau;
                 Sigma = spotT.Sig;
                 Spot = spotT.Spot;
                 Strike = priceT.Strike;
                 Style = priceT.Style;
                 Payoff = priceT.Payoff;
                 Smoothing = priceT.Smoothing;
             }
         }

         public void MakeDeltaGamma(Tree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
         {
            SetParam(spotT,priceT);
             Tree treeD = new Tree
             {
                 GridSteps = GridSteps + 2,
                 Spot = Spot,
                 Sig = Sigma,
                 Tau = Tau * (1 + 2 / GridSteps)
             };
             treeD.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(treeD);
             double[] s = new double[3];
             double[] c = new double[3];
             for (int i = 0; i <= 2; ++i)
             {
                 s[i] = treeD.GetSpotMatrix(2, i);
                 c[i] = priceT.GetPriceMatrix(2, i);
             }
             Delta = (s[0] * (2 * s[1] - s[0]) * (c[1] - c[2]) + 
                 (s[1] * s[1]) * (c[2] - c[0]) + s[2] * (2 * s[1] - s[2]) * (c[0] - c[1]))
                    / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);

             Gamma = 2 * (s[0] * (c[1] - c[2]) + s[1] * (c[2] - c[0]) + s[2] * (c[0] - c[1]))
                        / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);
         }

         public void MakeVega(Tree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
         {
             SetParam(spotT, priceT);
             Tree t1 = new Tree(), t2 = new Tree();
             t1.Tau = Tau;
             t1.GridSteps = GridSteps;
             t1.Sig = .99 * Sigma;
             t1.Spot = Spot;
             t1.MakeGrid(myZero,myDiv);
             priceT.MakeGrid(t1);
             double p1 = priceT.Price();
             t2.Tau = Tau;
             t2.GridSteps = GridSteps;
             t2.Sig = 1.01* Sigma;
             t2.Spot = Spot;
             t2.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(t2);
             double p2 = priceT.Price();
             if (Sigma != 0)
             {
                 Vega = 0.01 * (p2 - p1) / (2 * 0.01 * Sigma);
             }
         }

         public void MakeTheta(Tree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
         {
             SetParam(spotT, priceT);
             Tree t1 = new Tree(), t2 = new Tree();
             t1.Tau = Tau;
             t1.GridSteps = GridSteps;
             t1.Sig = Sigma;
             t1.Spot = Spot;
             t1.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(t1);
             double p1 = priceT.Price();
             double t = Tau - 1.00 / 365.00;
             t2.Tau = t;    
             t2.GridSteps = GridSteps;
             t2.Sig = Sigma;
             t2.Spot = Spot;
             t2.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(t2);
             double p2 = priceT.Price();
             Theta = (p2 - p1);       
         }
     }
}
