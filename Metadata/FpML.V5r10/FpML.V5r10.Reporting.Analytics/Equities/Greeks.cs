#region Usings

using Orion.Analytics.Rates;

#endregion

namespace Orion.Analytics.Equities 
    {
     public class Greeks
        {
         public double dt { get; set; }
         public int Gridsteps = 0;
         public double tau { get; set; }
         public double sig { get; set; }
         public double spot { get; set; }

         public string Style { get; set; }
         public string Payoff { get; set; }
         public double Strike { get; set; }
         public string Smoothing { get; set; }

         public double Delta { get; set; }
         public double Gamma { get; set; }
         public double Vega { get; set; }
         public double Theta { get; set; }

         private void SetParam(Tree SpotT,Pricer PriceT)
             {

             if ((SpotT != null) && (PriceT != null))
                 {
                 Gridsteps = SpotT.Gridsteps;
                 tau = SpotT.Tau;
                 sig = SpotT.Sig;
                 spot = SpotT.Spot;
                 Strike = PriceT.Strike;
                 Style = PriceT.Style;
                 Payoff = PriceT.Payoff;
                 Smoothing = PriceT.Smoothing;
                 }
             }

         public void MakeDeltaGamma(Tree SpotT, Pricer PriceT, ZeroCurve myZero, DivList myDiv)
             {
             SetParam(SpotT,PriceT);
                 Tree treeD = new Tree
                 {
                     Gridsteps = Gridsteps + 2,
                     Spot = spot,
                     Sig = sig,
                     Tau = tau * (1 + 2 / Gridsteps)
                 };
                 treeD.MakeGrid(myZero, myDiv);
             PriceT.MakeGrid(treeD);

             double[] S = new double[3];
             double[] C = new double[3];
             for (int i = 0; i <= 2; ++i)
                 {
                 S[i] = treeD.GetSpotMatrix(2, i);
                 C[i] = PriceT.Get_PriceMatrix(2, i);
                 }
             Delta = (S[0] * (2 * S[1] - S[0]) * (C[1] - C[2]) + 
                 (S[1] * S[1]) * (C[2] - C[0]) + S[2] * (2 * S[1] - S[2]) * (C[0] - C[1]))
                    / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);

             Gamma = 2 * (S[0] * (C[1] - C[2]) + S[1] * (C[2] - C[0]) + S[2] * (C[0] - C[1]))
                        / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);

             }

         public void MakeVega(Tree SpotT, Pricer PriceT, ZeroCurve myZero, DivList myDiv)
             {


             SetParam(SpotT, PriceT);
             Tree T1 = new Tree(), T2 = new Tree();

             T1.Tau = tau;
             T1.Gridsteps = Gridsteps;
             T1.Sig = .99 * sig;
             T1.Spot = spot;
             T1.MakeGrid(myZero,myDiv);

             PriceT.MakeGrid(T1);
             double P1 = PriceT.Price();

             T2.Tau = tau;
             T2.Gridsteps = Gridsteps;
             T2.Sig = 1.01* sig;
             T2.Spot = spot;
             T2.MakeGrid(myZero, myDiv);

             PriceT.MakeGrid(T2);
             double P2 = PriceT.Price();

             if (sig != 0)
                 {
                 Vega = 0.01 * (P2 - P1) / (2 * 0.01 * sig);
                 }

             }

         public void MakeTheta(Tree SpotT, Pricer PriceT, ZeroCurve myZero, DivList myDiv)
             {
             SetParam(SpotT, PriceT);
             Tree T1 = new Tree(), T2 = new Tree();

             T1.Tau = tau;
             T1.Gridsteps = Gridsteps;
             T1.Sig = sig;
             T1.Spot = spot;
             T1.MakeGrid(myZero, myDiv);

             PriceT.MakeGrid(T1);
             double P1 = PriceT.Price();

             double t = tau - 1.00 / 365.00;
             T2.Tau = t;    
             T2.Gridsteps = Gridsteps;
             T2.Sig = sig;
             T2.Spot = spot;
             T2.MakeGrid(myZero, myDiv);

             PriceT.MakeGrid(T2);
             double P2 = PriceT.Price();

             Theta = (P2 - P1);       
             }
          }
     }
