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

using System;
using System.Collections.Generic;
using Highlander.Equities;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.Solvers;

namespace Highlander.Reporting.Analytics.V5r3.Equities
{
    public class AmOptionAnalytics : ICloneable, IObjectiveFunction
    {
        const double Eps = 1e-4;
        const int Maxits = 40;

        private double _premium;
        private readonly ITree _propAssetTree;
        private PriceTree _priceTree;

        /// <summary>
        /// 
        /// </summary>
        public double DT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int GridSteps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Expiry { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        public RateCurve RateCurve { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<Dividend> DivCurve { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="today"></param>
        /// <param name="expiry"></param>
        /// <param name="spot"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="paystyle"></param>
        /// <param name="payoff"></param>
        /// <param name="rateCurve"></param>
        /// <param name="divCurve"></param>
        /// <param name="gridSteps"></param>
        public AmOptionAnalytics(DateTime today, DateTime expiry, double spot, double strike, double vol,  string paystyle, string payoff, RateCurve rateCurve, List<Dividend> divCurve, int gridSteps)
        {
            Spot = spot;
            Strike = strike;
            Sig = vol;
            Today = today;
            Expiry = expiry;
            Payoff = payoff;
            Style = paystyle;
            RateCurve = rateCurve;
            DivCurve = divCurve;
            GridSteps = gridSteps;
            double tau= Expiry.Subtract(Today).Days / 365.0;
            _propAssetTree = new PropAssetTree(tau, Sig, Spot, GridSteps, true, Today,RateCurve,DivCurve);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Price()
        {
            _propAssetTree.Spot = Spot;
            _propAssetTree.Sig = Sig;
            _propAssetTree.Tau = Expiry.Subtract(Today).Days / 365.0;
            _propAssetTree.GridSteps = GridSteps;   
            _propAssetTree.MakeGrid();          
            //create PriceTree
            _priceTree = new PriceTree(Strike, Payoff, "Y", Style);
            _priceTree.MakeGrid(_propAssetTree);
            double pr = _priceTree.Price();
            return pr;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public new string ToString()
        {
            return "PriceWrt";
        }

        /// <summary>
        /// Definition of the objective function.
        /// </summary>
        /// <returns>
        /// The value of the objective function, <i>f(x)</i>.
        /// </returns>
        public double Value(double sig)
        {
            Sig = sig;
            return Price() - _premium;
        }

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="sig"></param>
        /// <returns>
        /// The value of the derivative, <i>f'(x)</i>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        public double Derivative(double sig)
        {
            MakeVega();
            return Vega;
        }

        /// <summary>
        /// 
        /// </summary>
        public void MakeDeltaGamma()
        {
            var type = _propAssetTree.GetType();
            var treeD = (ITree)Activator.CreateInstance(type);
            double tau = Expiry.Subtract(Today).Days / 365.0;
            treeD.GridSteps = GridSteps + 2;
            treeD.Spot = Spot;
            treeD.Sig = Sig;
            treeD.Tau = tau * (1 + 2 / GridSteps);
            treeD.MakeGrid();
            _priceTree.MakeGrid(treeD);
            var s = new double[3];
            var c = new double[3];
            for (int i = 0; i <= 2; ++i)
            {
                s[i] = treeD.GetSpotMatrix(2, i);
                c[i] = _priceTree.GetPriceMatrix(2, i);
            }
            Delta = (s[0] * (2 * s[1] - s[0]) * (c[1] - c[2]) +
                (s[1] * s[1]) * (c[2] - c[0]) + s[2] * (2 * s[1] - s[2]) * (c[0] - c[1]))
                   / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);
            Gamma = 2 * (s[0] * (c[1] - c[2]) + s[1] * (c[2] - c[0]) + s[2] * (c[0] - c[1]))
                       / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);

        }

        /// <summary>
        /// 
        /// </summary>
        public void MakeVega()
        {         
            var type = _propAssetTree.GetType();
            double tau = Expiry.Subtract(Today).Days / 365.0;
            double sig1 = .99 * Sig;         
            var t1 = (ITree)Activator.CreateInstance(type, tau, sig1,Spot, GridSteps, true, Today, RateCurve, DivCurve);           
            t1.MakeGrid();
            _priceTree.MakeGrid(t1);
            double p1 = _priceTree.Price();                                 
            double sig2 = 1.01 * Sig;
            ITree t2 = t1;
            t2.Sig = sig2;
            t2.MakeGrid();
            _priceTree.MakeGrid(t2);
            double p2 = _priceTree.Price();
            if (Sig != 0)
            {
                Vega = 0.01 * (p2 - p1) / (2 * 0.01 * Sig);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void MakeTheta()
        {      
            var type = _propAssetTree.GetType();
            var t1 = (ITree)Activator.CreateInstance(type);
            var t2 = (ITree)Activator.CreateInstance(type);
            t1.Tau = Expiry.Subtract(Today).Days / 365.0;
            t1.GridSteps = GridSteps;
            t1.Sig = Sig;
            t1.Spot = Spot;
            t1.MakeGrid();
            _priceTree.MakeGrid(t1);
            double p1 = _priceTree.Price();
            double tau = Expiry.Subtract(Today).Days / 365.0;
            double t = tau - 1.00 / 365.00;
            t2.Tau = t;
            t2.GridSteps = GridSteps;
            t2.Sig = Sig;
            t2.Spot = Spot;
            t2.MakeGrid();
            _priceTree.MakeGrid(t2);
            double p2 = _priceTree.Price();
            Theta = (p2 - p1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="premium"></param>
        /// <param name="fwdPrice"></param>
        /// <returns></returns>
        public double OptSolveVol(double premium, double fwdPrice)
        {
            //Solver1D solver = new Ridder();
            //this._Premium = prem;
            //double x = solver.Solve(this, EPS, 0.20, EPS);
            //return x;
            return Newton(premium, fwdPrice);
        }

        private double Newton(double premium, double fwdPrice)
        {
            double dVol;
            var cp = Payoff.ToLower()=="c" ? 1 : -1;
            int days = Expiry.Subtract(Today).Days;
            double t = days/365.0;
            var priceClone = (AmOptionAnalytics)Clone();          
            if (fwdPrice <= 0 || Strike <= 0 || t <= 0 || premium <= 0) return 0;         
            //double df = Convert.ToDouble(_rateCurve.getDf(days));
            if (premium < Math.Max(cp*(fwdPrice - priceClone.Strike), 0)) throw new System.Exception("No solution for volatility");
            int idx = 0;
            do 
            {                
                var price = priceClone.Price();
                priceClone.MakeVega();
                var vega = priceClone.Vega;
                if (vega == 0 || idx > Maxits) throw new System.Exception("No volatility solution");
                dVol = (price-premium) / (100*vega);
                priceClone.Sig -= dVol;
                if (priceClone.Sig < 0) priceClone.Sig = 0;
                idx++;
            } while (Math.Abs(dVol) > Eps);
            return priceClone.Sig;
        }

        private double Bisection(double premium, double fwdPrice)
        {
            double right = 0.75;
            double left = 0.35;
            double mid;
            var cp = Payoff.ToLower()=="c" ? 1 : -1;
            int days = Expiry.Subtract(Today).Days;
            double t = days/365.0;
            var priceClone = (AmOptionAnalytics)Clone();            
            if (fwdPrice <= 0 || Strike <= 0 || t <= 0 || premium <= 0) return 0;        
            double df = Convert.ToDouble(RateCurve.GetDf(days));
            if (premium < Math.Max(cp * df * (fwdPrice - priceClone.Strike), 0)) throw new Exception("No solution for volatility");
            do
            {
                mid = (right + left) / 2;
                priceClone.Sig = left;
                double fleft = priceClone.Price() - premium;              
                priceClone.Sig = right;
                double fright = priceClone.Price() - premium;
                priceClone.Sig = mid;
                double fmid = priceClone.Price() - premium;
                if (fleft * fmid < 0)
                    right = mid;
                else if (fright * fmid < 0)
                    left = mid;
            } while (Math.Abs(right - left) > 2 * Eps);
            return mid;                
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
