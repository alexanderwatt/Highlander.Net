using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Numerics.Lattices;
using Orion.Numerics.Distributions.Continuous;

namespace Orion.Analytics.Equities
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
        /// <param name="smoo">The smoo.</param>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="binomialTree">The binomial tree.</param>
        public BinomialTreePricer(double spot,  double strike,  Boolean isPut,  double tau, double vol, int steps, bool flatIntRate, 
                                  String style, string smoo, int[] rtdays, double[] rtamts, int[] divdays, double[] divamts, Type binomialTree)
        {                                     
            _treeType = binomialTree ;
            _assetTree = (EquityBinomialTree)Activator.CreateInstance(binomialTree, spot,tau,vol,steps,true,rtdays,rtamts,divdays,divamts);
            _Strike = strike;
            _Payoff = isPut ? "Put" : "Call";        
            _vol = vol;
            _Smoothing = smoo;
            _Style = style;    
            _ratedays = rtdays;
            _rateamts = rtamts;
            _divdays = divdays;
            _divamts = divamts;
            _flatFlag = flatIntRate;
            MakeGrid(); 
              
        }

        #region Private Members

        /// <summary>
        /// Price tree
        /// </summary>
        private double[,] _PriceMatrix;
        /// <summary>
        /// Option style American or European
        /// </summary>
        private string _Style;
        /// <summary>
        /// Pay off style Call or Put
        /// </summary>
        private string _Payoff;
        /// <summary>
        /// Strike
        /// </summary>
        private double _Strike;
        /// <summary>
        /// Apply Black Scholes smoothing to second last 
        /// node. Y or N
        /// </summary>
        private string _Smoothing;
        /// <summary>
        /// Asset simulation tree to be used in deriving the 
        /// price tree
        /// </summary>
        private EquityBinomialTree _assetTree;
        /// <summary>
        /// Tree type implementing EquityBinomialTree
        /// </summary>
        private Type _treeType;
        /// <summary>
        /// Use flat forward rate across every node point
        /// </summary>
        private bool _flatFlag;
        /// <summary>
        /// Continuously compounded zero curve days
        /// </summary>
        private int[] _ratedays;
        /// <summary>
        /// Continuously compounded zero curve rates
        /// </summary>
        private double[] _rateamts;
        /// <summary>
        /// Dividend strip days
        /// </summary>
        private int[] _divdays;
        /// <summary>
        /// Dividend strip divs
        /// </summary>
        private double[] _divamts;
        /// <summary>
        /// Volatility of the option
        /// </summary>
        private double _vol;


        /// <summary>
        /// rv
        /// </summary>
        private NormalDistribution _nd = new NormalDistribution(0, 1);

        #endregion
           

        #region Accessors

    
        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <value>The style.</value>
        public string Style
        {
            get { return _Style; }           
        }   

       
        /// <summary>
        /// Gets the payoff.
        /// </summary>
        /// <value>The payoff.</value>
        public string Payoff
        {
            get { return _Payoff; }           
        }

    
        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public double Strike
        {
            get { return _Strike; }       
        }

    
        /// <summary>
        /// Gets the smoothing.
        /// </summary>
        /// <value>The smoothing.</value>
        public string Smoothing
        {
            get { return _Smoothing; }
        }                 
                 
        /// <summary>
        /// Gets the tree.
        /// </summary>
        /// <value>The tree.</value>
        public EquityBinomialTree Tree
        {
            get { return _assetTree; }
        }

        #endregion


        #region Private Methods

     
        /// <summary>
        /// Get_s the P.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <returns></returns>
        private double Get_P(int idx)
        {
            if (idx <= _assetTree.Columns)
            {
                return _assetTree.Probability(idx, 0, 0);
            }
            return 0.0;
        }
      
        /// <summary>
        /// Makes the arrays.
        /// </summary>
        private void MakeArrays()
        {
            if (_PriceMatrix == null)
            {
                _PriceMatrix = new double[_assetTree.Columns + 1, _assetTree.Columns + 1];               
            }
        }

       
        /// <summary>
        /// Empties the arrays.
        /// </summary>
        private void EmptyArrays()
        {
            _PriceMatrix = null;    
        }
        
     
        /// <summary>
        /// Assign terminal payoff condition.
        /// </summary>
        private void TermValue()
        {
            int n = _assetTree.Columns;
            if (_assetTree != null)
            {
                for (int j = 0; j <= n; j++)
                {
                    double temp=0.0;
                    _Payoff = _Payoff.ToLower();
                    if ((Payoff == "c") | (Payoff == "call"))
                    {
                        temp = Math.Max(_assetTree.Underlying(n, j) - Strike, 0);
                        Set_PriceMatrix(n, j, temp);
                    }
                    else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                    {
                        temp = Math.Max(Strike - _assetTree.Underlying(n, j), 0);
                        Set_PriceMatrix(n, j, temp);

                    }
                }
            }
        }



     
        /// <summary>
        /// Assign values at Gridsteps-1 node.
        /// </summary>
        private void PreTermValue()
        {
            if (_assetTree != null)
            {
                int n = _assetTree.Columns;
                double temp = 0;
                double dt = _assetTree.Time / n;
                int idx = n - 1;

                for (int j = 0; j <= n - 1; j++)
                {
                    if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
                    {
                        if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                        {
                            temp = Math.Max(Math.Exp(-_assetTree.Rate(idx) * dt) * (Get_P(idx) *
                                Get_PriceMatrix(idx + 1, j + 1) + (1 - Get_P(idx)) *
                                Get_PriceMatrix(idx + 1, j)), _assetTree.Underlying(idx,j) - Strike);
                            Set_PriceMatrix(idx, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                        {
                            temp = Math.Max(Math.Exp(-_assetTree.Rate(idx) * dt) * (Get_P(idx) *
                               Get_PriceMatrix(idx + 1, j + 1) + (1 - Get_P(idx)) *
                               Get_PriceMatrix(idx + 1, j)), -_assetTree.Underlying(idx, j) + Strike);
                            Set_PriceMatrix(idx, j, temp);
                        }
                    }
                    else if ((Style.ToLower() == "e") | (Style.ToLower() == "european"))
                    {
                        temp = Math.Exp(-_assetTree.Rate(idx) * dt) * (Get_P(idx) *
                               Get_PriceMatrix(idx + 1, j + 1) + (1 - Get_P(idx)) *
                               Get_PriceMatrix(idx + 1, j));
                        Set_PriceMatrix(idx, j, temp);

                    }


                }
            }
        }

        
        /// <summary>
        ///Here we Smooth the 3 -closest nodes to the strike at the second last timestep. This involves
        ///replacing the tree calculated price with the BS price. This has minimal effect on the price
        ///but significant effect on smoothness of spatial derivatives especially second order like gamma
        /// </summary>
        /// <param name="str">The STR.</param>
        private void Smooth(string str)
        {
            double _tau = _assetTree.Time;
            int n = _assetTree.Columns;

            if (str != null)
            {
                double dt = _tau / _assetTree.Columns;



                if ((str.ToLower() == "y") | (str.ToLower() == "yes"))
                {
                    int Centre = 0;
                    int k, j;
                    int idx = _assetTree.Columns - 1;
                    k = 1;


                    for (k = 1; ((_assetTree.Underlying(idx, k - 1) <= Strike) &&
                                ((_assetTree.Underlying(idx, k) <= Strike)) &&
                                ((k <= n - 1))); k++)
                    {
                    }
                    if (k == 1)
                    {
                        Centre = 2;
                    }
                    else if (k >= n - 1)
                    {
                        Centre = n - 2;
                    }
                    else if ((k <= n - 2) && (k > 1))
                    {
                        if (Math.Abs(_assetTree.Underlying(idx, k - 2) / Strike - 1) >
                            Math.Abs(_assetTree.Underlying(idx, k + 1) / Strike - 1))
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
                            //temp = BSprice(tree.Get_SpotMatrix(idx, j)
                            //, dt, Strike, get_r(idx), tree.sig, "C");
                            double fwd = _assetTree.Underlying(idx, j) - _assetTree.Dividend(idx);
                            fwd *= Math.Exp(_assetTree.Rate(idx) * dt);
                            temp = BSprice(fwd, dt, Strike, _assetTree.Rate(idx), _assetTree.Volatility, "C");
                            Set_PriceMatrix(idx, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") || (Payoff.ToLower() == "put"))
                        {
                            //temp = BSprice(tree.Get_SpotMatrix(idx, j)
                            //, dt, Strike, get_r(idx), tree.sig, "P");
                            double fwd = _assetTree.Underlying(idx, j) - _assetTree.Dividend(idx);
                            fwd *= Math.Exp(_assetTree.Rate(idx) * dt);
                            temp = BSprice(fwd, dt, Strike, _assetTree.Rate(idx), _assetTree.Volatility, "P");
                            Set_PriceMatrix(idx, j, temp);
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
        private void Set_PriceMatrix(int idx, int jdx, double value)
        {
            int n = _assetTree.Columns;
            if ((idx <= n) && (jdx <= n))
            {
                _PriceMatrix[idx, jdx] = value;
            }
        }



        /// <summary>
        /// Bs the sprice.
        /// </summary>
        /// <param name="Fwd">The FWD.</param>
        /// <param name="Tau1">The tau1.</param>
        /// <param name="Strike1">The strike1.</param>
        /// <param name="Rate1">The rate1.</param>
        /// <param name="Sigma1">The sigma1.</param>
        /// <param name="Payoff">The payoff.</param>
        /// <returns></returns>
        private double BSprice(double Fwd, double Tau1, double Strike1, double Rate1,
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
            n1 =_nd.CumulativeDistribution(S1 * d1);
            n2 =_nd.CumulativeDistribution(S1 * d2);
            return S1 * (Fwd * n1 - Strike1 * n2) * Math.Exp(-Rate1 * Tau1);
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

            int n = _assetTree.Columns;
            double _tau = _assetTree.Time;
            double temp = 0;
            double dt = _tau / _assetTree.Columns;

            
            if ((Style.ToLower() == "a") | (Style.ToLower() == "american"))
            {                
                for (int i = n - 2; i >= 0; i--)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        if ((Payoff.ToLower() == "c") | (Payoff.ToLower() == "call"))
                        {
                            temp = Math.Max(Math.Exp(-_assetTree.Rate(i) * dt) * (Get_P(i) *
                                Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                                Get_PriceMatrix(i + 1, j)), _assetTree.Underlying(i, j)
                                - Strike);
                            Set_PriceMatrix(i, j, temp);
                        }
                        else if ((Payoff.ToLower() == "p") | (Payoff.ToLower() == "put"))
                        {
                            temp = Math.Max(Math.Exp(-_assetTree.Rate(i) * dt) * (Get_P(i) *
                               Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                               Get_PriceMatrix(i + 1, j)), Strike - _assetTree.Underlying(i, j));
                            Set_PriceMatrix(i, j, temp);
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
                        temp = Math.Exp(-_assetTree.Rate(i) * dt) * (Get_P(i) *
                               Get_PriceMatrix(i + 1, j + 1) + (1 - Get_P(i)) *
                               Get_PriceMatrix(i + 1, j));
                        Set_PriceMatrix(i, j, temp);
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
        public double Get_PriceMatrix(int idx, int jdx)
        {
            return _PriceMatrix[idx, jdx];
        }


        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            if (_PriceMatrix != null)
            {
                return Get_PriceMatrix(0, 0);
            }
            else
            {
                return -1.0;
            }
        }


        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <returns></returns>
        public double GetDelta()
        {       
            double spot = _assetTree.Underlying(0,0);                
            int steps = _assetTree.Columns + 2;
            double tau = _assetTree.Time * (1 + 2 / steps);                 

            BinomialTreePricer clone = new BinomialTreePricer(spot,_Strike, _Payoff.ToLower()[0] == 'p', tau,_vol,steps,_flatFlag,_Style,_Smoothing,_ratedays,_rateamts,_divdays,_divamts,_treeType);                                    

            double[] S = new double[3];
            double[] C = new double[3];

            for (int i = 0; i <= 2; ++i)
            {
                S[i] = clone.Tree.Underlying(2, i);
                C[i] = clone.Get_PriceMatrix(2, i);
            }

            double delta = (S[0] * (2 * S[1] - S[0]) * (C[1] - C[2]) +
             (S[1] * S[1]) * (C[2] - C[0]) + S[2] * (2 * S[1] - S[2]) * (C[0] - C[1]))
                / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);

            return delta;


        }
        /// <summary>
        /// Gets the gamma.
        /// </summary>
        /// <returns></returns>
        public double GetGamma()
        {
            double spot = _assetTree.Underlying(0, 0);
            int steps = _assetTree.Columns + 2;
            double tau = _assetTree.Time * (1 + 2 / steps);
         
            BinomialTreePricer clone = new BinomialTreePricer(spot, _Strike, _Payoff.ToLower()[0] == 'p', tau, _vol, steps, _flatFlag, _Style, _Smoothing, _ratedays, _rateamts, _divdays, _divamts, _treeType);                                    

            double[] S = new double[3];
            double[] C = new double[3];
            for (int i = 0; i <= 2; ++i)
            {
                S[i] = clone.Tree.Underlying(2, i);
                C[i] = clone.Get_PriceMatrix(2, i);
            }

            double gamma = 2 * (S[0] * (C[1] - C[2]) + S[1] * (C[2] - C[0]) + S[2] * (C[0] - C[1]))
                    / (S[1] - S[0]) / (S[2] - S[0]) / (S[2] - S[1]);

            return gamma;
        }


        /// <summary>
        /// Gets the vega.
        /// </summary>
        /// <returns></returns>
        public double GetVega()
        {
         
            double spot = _assetTree.Underlying(0, 0);
            double _tau = _assetTree.Time;

            BinomialTreePricer lhs1 = new BinomialTreePricer(spot, _Strike, _Payoff.ToLower()[0] == 'p', _tau, 0.99*_vol, _assetTree.Columns, _flatFlag, _Style, _Smoothing, _ratedays, _rateamts, _divdays, _divamts, _treeType);
            BinomialTreePricer lhs2 = new BinomialTreePricer(spot, _Strike, _Payoff.ToLower()[0] == 'p', _tau, 1.01 * _vol, _assetTree.Columns, _flatFlag, _Style, _Smoothing, _ratedays, _rateamts, _divdays, _divamts, _treeType);                                                 

            double P1 = lhs1.GetPrice();       

            double P2 = lhs2.GetPrice();

            double vega = 0.0;

            if (_vol != 0)
            {
                vega = 0.01 * (P2 - P1) / (2 * 0.01 * _vol);
            }
            return vega;

        }

        /// <summary>
        /// Gets the theta.
        /// </summary>        
        /// <returns></returns>
        public double GetTheta()
        {
            double spot = _assetTree.Underlying(0, 0);
            double _tau = _assetTree.Time;
            int n1 = _divdays.Length;
            int n2 = _ratedays.Length;
            BinomialTreePricer lhs1 = new BinomialTreePricer(spot, _Strike, _Payoff.ToLower()[0] == 'p', _tau, _vol, _assetTree.Columns, _flatFlag, _Style, _Smoothing, _ratedays, _rateamts, _divdays, _divamts, _treeType);
            int[] _divdays1 = new int[n1];                       
            for (int idx = 0; idx < n1; idx++)
            {
                _divdays1[idx] = Math.Max(_divdays[idx] - 1, 0) ;
            }
            int[] _ratedays1 = new int[n2];
            for (int idx = 0; idx < n2; idx++)
            {
                _ratedays1[idx] = Math.Max(_ratedays[idx] - 1, 0);
            }

            BinomialTreePricer lhs2 = new BinomialTreePricer(spot, _Strike, _Payoff.ToLower()[0] == 'p', _tau-1.0/365.0, _vol, _assetTree.Columns, _flatFlag, _Style, _Smoothing, _ratedays1, _rateamts, _divdays1, _divamts, _treeType);      

            double p1 = lhs1.GetPrice();
            double p2 = lhs2.GetPrice();

            double theta = lhs2.GetPrice() - lhs1.GetPrice();

            return theta;

        }

        #endregion



    }
}
