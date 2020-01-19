using System;

namespace Orion.EquitiesCore
    {
     /// <summary>
     /// 
     /// </summary>
     public class Greeks
        {
            /// <summary>
         /// 
         /// </summary>
         public double DT { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int Gridsteps { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Tau { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Sig { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Spot { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Delta { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Gamma { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Vega { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Theta { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Style { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Payoff { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Strike { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Smoothing { get; set; }

            private void SetParam(ITree spotT,Pricer priceT)
             {

             if ((spotT != null) && (priceT != null))
                 {
                 Gridsteps = spotT.Gridsteps;
                 Tau = spotT.Tau;
                 Sig = spotT.Sig;
                 Spot = spotT.Spot;
                 Strike = priceT.Strike;
                 Style = priceT.Style;
                 Payoff = priceT.Payoff;
                 Smoothing = priceT.Smoothing;
                 }
             }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="spotT"></param>
         /// <param name="priceT"></param>
         /// <param name="myZero"></param>
         /// <param name="myDiv"></param>
         public void MakeDeltaGamma(ITree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
             {
             SetParam(spotT,priceT);
             var type = spotT.GetType();        
             ITree treeD = (ITree)Activator.CreateInstance(type);                             
             treeD.Gridsteps = Gridsteps+2;
             treeD.Spot = Spot;
             treeD.Sig = Sig;
             treeD.Tau = Tau * (1 + 2 / Gridsteps);
             treeD.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(treeD);
             double[] s = new double[3];
             double[] c = new double[3];
             for (int i = 0; i <= 2; ++i)
                 {
                 s[i] = treeD.GetSpotMatrix(2, i);
                 c[i] = priceT.GetPriceMatrix(2, i);
                 }
             //Delta = (S[0] * (2 * S[1] - S[0]) * (C[1] - C[2]) + 
             //    (S[1] * S[1]) * (C[2] - C[0]) + S[2] * (2 * S[1] - S[2]) * (C[0] - C[1]))
             //       / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);
             //Gamma = 2 * (S[0] * (C[1] - C[2]) + S[1] * (C[2] - C[0]) + S[2] * (C[0] - C[1]))
             //           / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);
             double u = treeD.GetUp(0);
             double d = treeD.GetDn(0);
             double est1 = (c[2] - c[0]) / (s[2] - s[0]);
             double est2 = (priceT.GetPriceMatrix(4, 4) - priceT.GetPriceMatrix(4, 0)) / (treeD.GetSpotMatrix(4, 4) - treeD.GetSpotMatrix(4, 0));
             double est3 = (priceT.GetPriceMatrix(6, 6) - priceT.GetPriceMatrix(6, 0)) / (treeD.GetSpotMatrix(6, 6) - treeD.GetSpotMatrix(6, 0));
             double temp = (Math.Pow(u, 2) + Math.Pow(d, 2) + Math.Pow(u, 2) * Math.Pow(d, 2)) * est1 -
                           (Math.Pow(u, 2) + Math.Pow(d, 2) + 1) * est2 + est3;
             Delta = 1/Math.Pow(u,2)/Math.Pow(d,2)*temp;
             //Delta = 1/
             }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="spotT"></param>
         /// <param name="priceT"></param>
         /// <param name="myZero"></param>
         /// <param name="myDiv"></param>
         public void MakeVega(ITree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
             {

             SetParam(spotT, priceT);
             var type = spotT.GetType();
             ITree t1 = (ITree)Activator.CreateInstance(type);
             ITree t2 = (ITree)Activator.CreateInstance(type);                       
             t1.Tau = Tau;
             t1.Gridsteps = Gridsteps;
             t1.Sig = .99 * Sig;
             t1.Spot = Spot;
             t1.MakeGrid(myZero,myDiv);
             priceT.MakeGrid(t1);
             double p1 = priceT.Price();
             t2.Tau = Tau;
             t2.Gridsteps = Gridsteps;
             t2.Sig = 1.01* Sig;
             t2.Spot = Spot;
             t2.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(t2);
             double p2 = priceT.Price();
             if (Sig != 0)
                 {
                 Vega = 0.01 * (p2 - p1) / (2 * 0.01 * Sig);
                 }
             }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="spotT"></param>
         /// <param name="priceT"></param>
         /// <param name="myZero"></param>
         /// <param name="myDiv"></param>
         public void MakeTheta(ITree spotT, Pricer priceT, ZeroCurve myZero, DivList myDiv)
             {
             SetParam(spotT, priceT);
              var type = spotT.GetType();
             ITree t1 = (ITree)Activator.CreateInstance(type);
             ITree t2 = (ITree)Activator.CreateInstance(type);    
             t1.Tau = Tau;
             t1.Gridsteps = Gridsteps;
             t1.Sig = Sig;
             t1.Spot = Spot;
             t1.MakeGrid(myZero, myDiv);
             priceT.MakeGrid(t1);
             double p1 = priceT.Price();
                 DivList shiftDiv = new DivList {Divpoints = myDiv.Divpoints};
                 shiftDiv.MakeArrays();
             for (int idx =0;idx<myDiv.Divpoints;idx++)
             {
                 shiftDiv.SetD(idx,myDiv.GetD(idx),myDiv.GetT(idx)-1/365.0); 
             }
             double t = Tau - 1.00 / 365.00;
             t2.Tau = t;    
             t2.Gridsteps = Gridsteps;
             t2.Sig = Sig;
             t2.Spot = Spot;
             t2.MakeGrid(myZero, shiftDiv);          
             priceT.MakeGrid(t2);
             double p2 = priceT.Price();
             Theta = (p2 - p1);           
             }
          }
     }
