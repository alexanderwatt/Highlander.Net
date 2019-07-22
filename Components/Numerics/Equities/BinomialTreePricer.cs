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

using System;
using Highlander.Numerics.Distributions;
using Highlander.Numerics.Lattices;

namespace Highlander.Numerics.Equities
{
    public class BinomialTreePricer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BinomialTreePricer"/> class.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="isPut">if set to <c>true</c> [is put].</param>
        /// <param name="tau">The tau.</param>
        /// <param name="vol">The vol.</param>
        /// <param name="steps">The steps.</param>
        /// <param name="flatIntRate">if set to <c>true</c> [flat int rate].</param>
        /// <param name="style">The style.</param>
        /// <param name="smoothing">The smoothing.</param>
        /// <param name="rateDays">The rate Days.</param>
        /// <param name="rateAmounts">The rate Amounts.</param>
        /// <param name="dividendDays">The dividend Days.</param>
        /// <param name="dividendAmounts">The dividend Amounts.</param>
        /// <param name="binomialTree">The binomial tree.</param>
        public BinomialTreePricer(double spot,  double strike,  Boolean isPut,  double tau, double vol, int steps, bool flatIntRate, 
                                  String style, string smoothing, int[] rateDays, double[] rateAmounts, int[] dividendDays, double[] dividendAmounts, Type binomialTree)
        {                                     
            _treeType = binomialTree ;
            Tree = (EquityBinomialTree)Activator.CreateInstance(binomialTree, spot,tau,vol,steps,true,rateDays,rateAmounts,dividendDays,dividendAmounts);
            Strike = strike;
            Payoff = isPut ? "Put" : "Call";        
            _volatility = vol;
            Smoothing = smoothing;
            Style = style;    
            _rateDays = rateDays;
            _rateAmounts = rateAmounts;
            _dividendDays = dividendDays;
            _dividendAmounts = dividendAmounts;
            _flatFlag = flatIntRate;
            MakeGrid(); 
              
        }

        #region Private Members

        /// <summary>
        /// Price tree
        /// </summary>
        private double[,] _priceMatrix;

        /// <summary>
        /// Tree type implementing EquityBinomialTree
        /// </summary>
        private readonly Type _treeType;
        /// <summary>
        /// Use flat forward rate across every node point
        /// </summary>
        private readonly bool _flatFlag;
        /// <summary>
        /// Continuously compounded zero curve days
        /// </summary>
        private readonly int[] _rateDays;
        /// <summary>
        /// Continuously compounded zero curve rates
        /// </summary>
        private readonly double[] _rateAmounts;
        /// <summary>
        /// Dividend strip days
        /// </summary>
        private readonly int[] _dividendDays;
        /// <summary>
        /// Dividend strip divs
        /// </summary>
        private readonly double[] _dividendAmounts;
        /// <summary>
        /// Volatility of the option
        /// </summary>
        private readonly double _volatility;

        /// <summary>
        /// rv
        /// </summary>
        private readonly NormalDistribution _normalDistribution = new NormalDistribution(0, 1);

        #endregion

        #region Accessors

    
        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <value>The style.</value>
        public string Style { get; }


        /// <summary>
        /// Gets the payoff.
        /// </summary>
        /// <value>The payoff.</value>
        public string Payoff { get; private set; }


        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public double Strike { get; }


        /// <summary>
        /// Gets the smoothing.
        /// </summary>
        /// <value>The smoothing.</value>
        public string Smoothing { get; }

        /// <summary>
        /// Gets the tree.
        /// </summary>
        /// <value>The tree.</value>
        public EquityBinomialTree Tree { get; }

        #endregion

        #region Private Methods

     
        /// <summary>
        /// Get_s the P.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        private double GetP(int idx)
        {
            if (idx <= Tree.Columns)
            {
                return Tree.Probability(idx, 0, 0);
            }
            return 0.0;
        }
      
        /// <summary>
        /// Makes the arrays.
        /// </summary>
        private void MakeArrays()
        {
            if (_priceMatrix == null)
            {
                _priceMatrix = new double[Tree.Columns + 1, Tree.Columns + 1];               
            }
        }

        /// <summary>
        /// Empties the arrays.
        /// </summary>
        private void EmptyArrays()
        {
            _priceMatrix = null;    
        }

        /// <summary>
        /// Assign terminal payoff condition.
        /// </summary>
        private void TermValue()
        {
            int n = Tree.Columns;
            if (Tree != null)
            {
                for (int j = 0; j <= n; j++)
                {
                    double temp;
                    Payoff = Payoff.ToLower();
                    if ((Payoff == "c") | (Payoff == "call"))
                    {
                        temp = Math.Max(Tree.Underlying(n, j) - Strike, 0);
                        SetPriceMatrix(n, j, temp);
                    }
                    else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                    {
                        temp = Math.Max(Strike - Tree.Underlying(n, j), 0);
                        SetPriceMatrix(n, j, temp);
                    }
                }
            }
        }


        /// <summary>
        /// Assign values at Grid steps-1 node.
        /// </summary>
        private void PreTermValue()
        {
            if (Tree != null)
            {
                int n = Tree.Columns;
                double dt = Tree.Time / n;
                int idx = n - 1;
                for (int j = 0; j <= n - 1; j++)
                {
                    double temp;
                    if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
                    {
                        if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                        {
                            temp = Math.Max(Math.Exp(-Tree.Rate(idx) * dt) * (GetP(idx) *
                                GetPriceMatrix(idx + 1, j + 1) + (1 - GetP(idx)) *
                                GetPriceMatrix(idx + 1, j)), Tree.Underlying(idx,j) - Strike);
                            SetPriceMatrix(idx, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                        {
                            temp = Math.Max(Math.Exp(-Tree.Rate(idx) * dt) * (GetP(idx) *
                               GetPriceMatrix(idx + 1, j + 1) + (1 - GetP(idx)) *
                               GetPriceMatrix(idx + 1, j)), -Tree.Underlying(idx, j) + Strike);
                            SetPriceMatrix(idx, j, temp);
                        }
                    }
                    else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
                    {
                        temp = Math.Exp(-Tree.Rate(idx) * dt) * (GetP(idx) *
                               GetPriceMatrix(idx + 1, j + 1) + (1 - GetP(idx)) *
                               GetPriceMatrix(idx + 1, j));
                        SetPriceMatrix(idx, j, temp);

                    }
                }
            }
        }

        /// <summary>
        ///Here we Smooth the 3 -closest nodes to the strike at the second last time step. This involves
        ///replacing the tree calculated price with the BS price. This has minimal effect on the price
        ///but significant effect on smoothness of spatial derivatives especially second order like gamma
        /// </summary>
        /// <param name="str">The STR.</param>
        private void Smooth(string str)
        {
            double tau = Tree.Time;
            int n = Tree.Columns;
            if (str != null)
            {
                double dt = tau / Tree.Columns;
                if ((str.ToLower() == "y") | (str.ToLower() == "yes"))
                {
                    int centre = 0;
                    int j;
                    int idx = Tree.Columns - 1;
                    int k;
                    for (k = 1; ((Tree.Underlying(idx, k - 1) <= Strike) &&
                                ((Tree.Underlying(idx, k) <= Strike)) &&
                                ((k <= n - 1))); k++)
                    {
                    }
                    if (k == 1)
                    {
                        centre = 2;
                    }
                    else if (k >= n - 1)
                    {
                        centre = n - 2;
                    }
                    else if ((k <= n - 2) && (k > 1))
                    {
                        if (Math.Abs(Tree.Underlying(idx, k - 2) / Strike - 1) >
                            Math.Abs(Tree.Underlying(idx, k + 1) / Strike - 1))
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
                        if ((Payoff.ToLower() == "c") || (Payoff.ToLower() == "call"))
                        {
                            //temp = Black Scholes price(tree.Get_SpotMatrix(idx, j)
                            //, dt, Strike, get_r(idx), tree.sig, "C");
                            double fwd = Tree.Underlying(idx, j) - Tree.Dividend(idx);
                            fwd *= Math.Exp(Tree.Rate(idx) * dt);
                            temp = BlackScholesPrice(fwd, dt, Strike, Tree.Rate(idx), Tree.Volatility, "C");
                            SetPriceMatrix(idx, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") || (Payoff.ToLower() == "put"))
                        {
                            //temp = Black Scholes price(tree.Get_SpotMatrix(idx, j)
                            //, dt, Strike, get_r(idx), tree.sig, "P");
                            double fwd = Tree.Underlying(idx, j) - Tree.Dividend(idx);
                            fwd *= Math.Exp(Tree.Rate(idx) * dt);
                            temp = BlackScholesPrice(fwd, dt, Strike, Tree.Rate(idx), Tree.Volatility, "P");
                            SetPriceMatrix(idx, j, temp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set_s the price matrix.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="jdx">The JDX.</param>
        /// <param name="value">The value.</param>
        private void SetPriceMatrix(int idx, int jdx, double value)
        {
            int n = Tree.Columns;
            if ((idx <= n) && (jdx <= n))
            {
                _priceMatrix[idx, jdx] = value;
            }
        }

        /// <summary>
        /// Bs the price.
        /// </summary>
        /// <param name="fwd">The FWD.</param>
        /// <param name="tau1">The tau1.</param>
        /// <param name="strike1">The strike1.</param>
        /// <param name="rate1">The rate1.</param>
        /// <param name="sigma1">The sigma1.</param>
        /// <param name="payoff">The payoff.</param>
        /// <returns></returns>
        private double BlackScholesPrice(double fwd, double tau1, double strike1, double rate1,
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
            var n1 = _normalDistribution.CumulativeDistribution(s1 * d1);
            var n2 = _normalDistribution.CumulativeDistribution(s1 * d2);
            return s1 * (fwd * n1 - strike1 * n2) * Math.Exp(-rate1 * tau1);
        }

        /// <summary>
        /// Makes the grid.
        /// </summary>
        private void MakeGrid()
        {
            EmptyArrays();
            MakeArrays();           
            TermValue();       
            PreTermValue();
            Smooth(Smoothing);
            //Iterate backward through tree to price American or Euro calls or puts.
            int n = Tree.Columns;
            double tau = Tree.Time;
            double temp;
            double dt = tau / Tree.Columns;
            if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
            {                
                for (int i = n - 2; i >= 0; i--)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                        {
                            temp = Math.Max(Math.Exp(-Tree.Rate(i) * dt) * (GetP(i) *
                                GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                                GetPriceMatrix(i + 1, j)), Tree.Underlying(i, j)
                                - Strike);
                            SetPriceMatrix(i, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                        {
                            temp = Math.Max(Math.Exp(-Tree.Rate(i) * dt) * (GetP(i) *
                               GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                               GetPriceMatrix(i + 1, j)), Strike - Tree.Underlying(i, j));
                            SetPriceMatrix(i, j, temp);
                        }
                    }
                }
            }
            else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
            {
                for (int i = n - 2; i >= 0; i--)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        temp = Math.Exp(-Tree.Rate(i) * dt) * (GetP(i) *
                               GetPriceMatrix(i + 1, j + 1) + (1 - GetP(i)) *
                               GetPriceMatrix(i + 1, j));
                        SetPriceMatrix(i, j, temp);
                    }
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get_s the price matrix.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="jdx">The JDX.</param>
        /// <returns></returns>
        public double GetPriceMatrix(int idx, int jdx)
        {
            return _priceMatrix[idx, jdx];
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            if (_priceMatrix != null)
            {
                return GetPriceMatrix(0, 0);
            }
            return -1.0;
        }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <returns></returns>
        public double GetDelta()
        {       
            double spot = Tree.Underlying(0,0);                
            int steps = Tree.Columns + 2;
            double tau = Tree.Time * (1 + 2 / steps);
            BinomialTreePricer clone = new BinomialTreePricer(spot,Strike, Payoff.ToLower()[0] == 'p', tau,_volatility,steps,_flatFlag,Style,Smoothing,_rateDays,_rateAmounts,_dividendDays,_dividendAmounts,_treeType);
            double[] s = new double[3];
            double[] c = new double[3];
            for (int i = 0; i <= 2; ++i)
            {
                s[i] = clone.Tree.Underlying(2, i);
                c[i] = clone.GetPriceMatrix(2, i);
            }
            double delta = (s[0] * (2 * s[1] - s[0]) * (c[1] - c[2]) +
             (s[1] * s[1]) * (c[2] - c[0]) + s[2] * (2 * s[1] - s[2]) * (c[0] - c[1]))
                / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);

            return delta;
        }

        /// <summary>
        /// Gets the gamma.
        /// </summary>
        /// <returns></returns>
        public double GetGamma()
        {
            double spot = Tree.Underlying(0, 0);
            int steps = Tree.Columns + 2;
            double tau = Tree.Time * (1 + 2 / steps);
            BinomialTreePricer clone = new BinomialTreePricer(spot, Strike, Payoff.ToLower()[0] == 'p', tau, _volatility, steps, _flatFlag, Style, Smoothing, _rateDays, _rateAmounts, _dividendDays, _dividendAmounts, _treeType);
            double[] s = new double[3];
            double[] c = new double[3];
            for (int i = 0; i <= 2; ++i)
            {
                s[i] = clone.Tree.Underlying(2, i);
                c[i] = clone.GetPriceMatrix(2, i);
            }
            double gamma = 2 * (s[0] * (c[1] - c[2]) + s[1] * (c[2] - c[0]) + s[2] * (c[0] - c[1]))
                    / (s[1] - s[0]) / (s[2] - s[0]) / (s[2] - s[1]);
            return gamma;
        }


        /// <summary>
        /// Gets the vega.
        /// </summary>
        /// <returns></returns>
        public double GetVega()
        {
            double spot = Tree.Underlying(0, 0);
            double tau = Tree.Time;
            BinomialTreePricer lhs1 = new BinomialTreePricer(spot, Strike, Payoff.ToLower()[0] == 'p', tau, 0.99*_volatility, Tree.Columns, _flatFlag, Style, Smoothing, _rateDays, _rateAmounts, _dividendDays, _dividendAmounts, _treeType);
            BinomialTreePricer lhs2 = new BinomialTreePricer(spot, Strike, Payoff.ToLower()[0] == 'p', tau, 1.01 * _volatility, Tree.Columns, _flatFlag, Style, Smoothing, _rateDays, _rateAmounts, _dividendDays, _dividendAmounts, _treeType);
            double p1 = lhs1.GetPrice();
            double p2 = lhs2.GetPrice();
            double vega = 0.0;
            if (_volatility != 0)
            {
                vega = 0.01 * (p2 - p1) / (2 * 0.01 * _volatility);
            }
            return vega;
        }

        /// <summary>
        /// Gets the theta.
        /// </summary>        
        /// <returns></returns>
        public double GetTheta()
        {
            double spot = Tree.Underlying(0, 0);
            double tau = Tree.Time;
            int n1 = _dividendDays.Length;
            int n2 = _rateDays.Length;
            BinomialTreePricer lhs1 = new BinomialTreePricer(spot, Strike, Payoff.ToLower()[0] == 'p', tau, _volatility, Tree.Columns, _flatFlag, Style, Smoothing, _rateDays, _rateAmounts, _dividendDays, _dividendAmounts, _treeType);
            int[] divDays1 = new int[n1];                       
            for (int idx = 0; idx < n1; idx++)
            {
                divDays1[idx] = Math.Max(_dividendDays[idx] - 1, 0) ;
            }
            int[] rateDays1 = new int[n2];
            for (int idx = 0; idx < n2; idx++)
            {
                rateDays1[idx] = Math.Max(_rateDays[idx] - 1, 0);
            }
            BinomialTreePricer lhs2 = new BinomialTreePricer(spot, Strike, Payoff.ToLower()[0] == 'p', tau-1.0/365.0, _volatility, Tree.Columns, _flatFlag, Style, Smoothing, rateDays1, _rateAmounts, divDays1, _dividendAmounts, _treeType);
            double p1 = lhs1.GetPrice();
            double p2 = lhs2.GetPrice();
            double theta = lhs2.GetPrice() - lhs1.GetPrice();
            return theta;
        }

        #endregion

    }
}
