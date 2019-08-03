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
using System.Diagnostics;

namespace Orion.Analytics.Rates
{
    ///<summary>
    /// Defines the delegate type "Del". This is used to pass a reference to the
    /// method "Difference" to the optimiser as the objective function to be 
    /// minimised.
    ///</summary>
    ///<param name="x"> The variable to be varied to minimise the "Difference
    /// method. This corresponds to the parameter "CashForwardFN" in the the 
    /// "Difference" method.</param>
    ///<param name="list"> The object list containing all the other input 
    /// arguments required for the "Difference" method.</param>
    public delegate double Del(double x, params object[] list);

    ///<summary>
    /// The class stores the output  that is required from the optimisation.
    ///</summary>
    public class OptimisationOutput
    {
        ///<summary>
        ///</summary>
        public int NumIterations;
        ///<summary>
        ///</summary>
        public double X;
        ///<summary>
        ///</summary>
        public double Diff;

        ///<summary>
        /// The contructor takes the output values from the optimisation and 
        /// assigns them to the member variables.
        ///</summary>
        ///<param name="numIterations"> The number of iterations the 
        /// optimisation algorithm has taken to converge to the 
        /// solution.</param>
        ///<param name="x"> The input value which returns the zero for the
        /// objective function.</param>
        ///<param name="diff"> The value of the objective function at x.</param>
        public OptimisationOutput(int numIterations, double x, double diff)
        {
            NumIterations = numIterations;
            X = x;
            Diff = diff;
        }
    }

    class Optimiser
    {
        public static OptimisationOutput Secant(Del delF,
                                                double initialValue,
                                                double tolerance,
                                                int maxIterations,
                                                params object[] list)
        {
            // Finds the root of the function F using the secant method.

            var x = initialValue;
            var xMinus = 1.005 * x;
            var count = 0;
            while (Math.Abs(delF(x, list)) > tolerance && count < maxIterations)
            {
                var xPlus = x - (x - xMinus) * delF(x, list) / (delF(x, list) - delF(xMinus, list));
                xMinus = x;
                x = xPlus;
                count = count + 1;
            }
            var result = new OptimisationOutput(count, x, delF(x, list));

            return result;
        }
    }

    ///<summary>
    /// Calculates the BGM based futures convexity adjustment and returns the 
    /// adjusted cash forwards derived from the input futures rates.
    ///</summary>
    public static class CashForward
    {
        ///<summary>
        /// The main function to calculate the adjusted forward.
        ///</summary>
        ///<param name="futuresPrice"> Vector of futures prices.</param>
        ///<param name="volatility"> Vector of futures</param>
        ///<param name="correlation"> Correlation between forwards. Can be a
        /// scalar input or a correlation matrix.</param>
        ///<param name="shift"> The shift parameters from the BGM 
        /// calibration.</param>
        ///<param name="coverage"> The vector of time intervals between model 
        /// time nodes.</param>
        ///<param name="timeNodes"> The model time nodes</param>
        ///<returns> Output, a 2-D array of ouput data. The first column is the 
        /// vector of adjusted cash forwards. The second column is the error 
        /// from the optimisation (the implied futures rate minus the market 
        /// rate). The third column is the number of iterations taken by the
        /// optimisation routine to converge on each adjusted rate. In the first
        /// entry in the fourth and fifth column is the time taken to work 
        /// through the whole set of time nodes, and the program version.</returns>
        public static Double[,] CalculateCashForward(double[] futuresPrice,
                                                     double[,] volatility,
                                                     double[,] correlation,
                                                     double[] shift,
                                                     double[] coverage,
                                                     double[] timeNodes)
        {
            double[] prices = Utilities.PrependZero(futuresPrice);
            double[,] vols = Utilities.PrependZeros(volatility);
            double[] moves = Utilities.PrependZero(shift);
            var corrs = Utilities.BuildCorrelationMatrix(correlation, futuresPrice.Length, 1);

            const double versionNumber = 1.0;

            var time = new Stopwatch();
            time.Start();

            var optimisationOutput = CalculateCashForward(prices,
                                                          vols,
                                                          corrs,
                                                          moves,
                                                          coverage,
                                                          timeNodes);

            time.Stop();
            var output = new double[optimisationOutput.Length - 1, 5];
            var index = 0;
            foreach(var element in optimisationOutput)
            {
                if (index >= 1)
                {
                    output[index - 1, 0] = element.X;
                    output[index - 1, 1] = element.Diff;
                    output[index - 1, 2] = element.NumIterations;
                }
                index++;
            }
            var elapsedTime = time.Elapsed.TotalSeconds;
            output[0, 3] = elapsedTime;
            output[0, 4] = versionNumber;
            return output;
        }

        ///<summary>
        /// The main function to calculate the adjusted forward.
        ///</summary>
        ///<param name="futuresPrice"> Vector of futures prices.</param>
        ///<param name="volatility"> Vector of futures</param>
        ///<param name="correlation"> Correlation between forwards. Can be a
        /// scalar input or a correlation matrix.</param>
        ///<param name="shift"> The shift parameters from the BGM 
        /// calibration.</param>
        ///<param name="coverage"> The vector of time intervals between model 
        /// time nodes.</param>
        ///<param name="timeNodes"> The model time nodes</param>
        ///<returns> optOut, an array OptimisationOuput types, with one element 
        /// for each time node.</returns>
        public static OptimisationOutput[] CalculateCashForward(double[] futuresPrice,
                                                                double[,] volatility,
                                                                double[, ,] correlation,
                                                                double[] shift,
                                                                double[] coverage,
                                                                double[] timeNodes)
        {
            var terminalNode = futuresPrice.Length - 1;
            var optOut = new OptimisationOutput[terminalNode + 1];
            var cashForward = new double[terminalNode + 1];
            const double tolerance = 0.000000001;
            const int maxIterations = 100;
            const int approximationOrder = 3;

            Del delObjective = Difference;

            // This assumes that if vol is input as a column vector (for 
            // terminal node greater than 1) then the vol is meant to be 
            // constant for each time node.
            if (volatility.GetLength(1) == 1 &&
                volatility.GetLength(0) == terminalNode + 1 && terminalNode > 1)
            {
                volatility = Utilities.Transpose(volatility);
            }

            for (var j = 1; j <= terminalNode; j++)
            {
                var fixingNode = j;
                double initialValue = j > 1 ? cashForward[j - 1] : futuresPrice[j];

                var targetPrice = futuresPrice[j];

                optOut[j] = Optimiser.Secant(delObjective, initialValue,
                            tolerance, maxIterations, cashForward,
                            approximationOrder, fixingNode, volatility,
                            correlation, shift, coverage, timeNodes,
                            targetPrice);

                cashForward[j] = optOut[j].X;
            }


            return optOut;
        }

        ///<summary>
        /// Calculates the difference between the implied futures rate and the
        /// input rate.
        ///</summary>
        ///<param name="cashForwardFn"> The sample cash forward rate for the 
        /// time node under consideration</param>
        ///<param name="list"> The object list containing the other arguments 
        /// required in the calculation.</param>
        ///<returns> The difference between the futures rate implied by the 
        /// input parameters and the input futures rate.</returns>
        public static double Difference(double cashForwardFn, params object[] list)
        {
            var cashForward = (double[])list[0];
            var approximationOrder = (int)list[1];
            var fixingNode = (int)list[2];
            var volatility = (double[,])list[3];
            var correlation = (double[, ,])list[4];
            var shift = (double[])list[5];
            var coverage = (double[])list[6];
            var timeNodes = (double[])list[7];
            var targetPrice = (double)list[8];

            var sampleCashForward = new double[fixingNode + 1];

            for (var i = 1; i <= fixingNode - 1; i++)
            {
                sampleCashForward[i] = cashForward[i];
            }
            sampleCashForward[fixingNode] = cashForwardFn;
            var convexity = FuturesPrice.Convexity(sampleCashForward, volatility, correlation, shift, coverage, timeNodes,
                                                   approximationOrder, fixingNode);
            var price = FuturesPrice.Price(cashForwardFn, shift[fixingNode], convexity);
            
            return price - targetPrice;
        }

    }

    static class FuturesPrice
    {
        public static double Convexity(double[] cashForward,
                                       double[,] volatility, //either constant or piecewise constant
                                       double[, ,] correlation, //either constant or piecewise constant 
                                       //(i.e. either a constant correlation matrix, or a 3D array). 
                                       //If it is constant then take the 3rd dimension as being of length 1 
                                       //(so rho[i,j,k] = \rho_{i,j}(t), t\in (T_{k-1},T_k], t = 0 if k = 0
                                       double[] shift, // let shift[0] = 0
                                       double[] coverage,
                                       double[] timeNodes, // [T_0, T_1, ... T_N] \approx [0, 0.25, ..., ]
                                       int approximationOrder,
                                       int fixingNode)
        {
            //we calculate the convexity adjustment using the approximation in Jackel & Kawai (2005)
            //what we refer to as convexity corresponds to exp(epsilon) in their terminology.
            var integral = new double[fixingNode + 1];
            var shiftedCashForward = new double[fixingNode + 1];
            double sum1 = 0, sum2 = 0, innerSum2 = 0;

            for (var i = 0; i <= fixingNode; i++)
            {
                shiftedCashForward[i] = cashForward[i] + shift[i];
            }

            // Precalculate the integrals \int \xi(t,T_i) \xi(t,T_j) dt
            // These integrals appear in the convexity approximation formula

            if (volatility.GetLength(0) == 1 & correlation.GetLength(2) == 1)
                //the dimension of the volatility is taken as 2, if it is 1D (time-constant) then the 1st dimension has length 1
                //the dimension of the correlation is taken as 3, if it is 2D (time- constant) then the 3rd dimension has length 1
            {
                for (int j = 1; j <= fixingNode; j++)
                {
                    integral[j] = volatility[0, fixingNode] * timeNodes[j] * volatility[0, j] * correlation[fixingNode, j, 0];
                }
            }
            else if (volatility.GetLength(0) > 1 & correlation.GetLength(2) == 1)// the volatility is piecewise constant, correlation constant
            {
                for (int j = 1; j <= fixingNode; j++)
                {
                    for (int n = 1; n <= j; n++)
                    {
                        integral[j] += coverage[n - 1] * volatility[n, j] * volatility[n, fixingNode];
                    }
                    integral[j] *= correlation[fixingNode, j, 0];
                }
            }
            else if (volatility.GetLength(0) == 1 & correlation.GetLength(2) > 1)// the volatility is constant, correlation piecewise constant
            {
                for (int j = 1; j <= fixingNode; j++)
                {
                    for (int n = 1; n <= j; n++)
                    {
                        integral[j] += coverage[n - 1] * correlation[fixingNode, j, n];
                    }
                    integral[j] *= volatility[0, fixingNode] * volatility[0, j];
                }
            }
            else //both volatility and correlation piecewise constant
            {
                for (int j = 1; j <= fixingNode; j++)
                {
                    for (int n = 1; n < j; n++)
                    {
                        integral[j] += coverage[n - 1] * volatility[n, j] * volatility[n, fixingNode] * correlation[fixingNode, j, n];
                    }
                }
            }

            // Now can calculate the approximation to the convexity
            for (var k = 1; k <= approximationOrder; k++)
            {
                if (k >= 2) //sum2 only sums over k = 2 to n
                {
                    for (var j = 1; j <= fixingNode; j++)
                    {
                        sum1 += shiftedCashForward[j] * coverage[j] * Math.Pow(integral[j], k) / (Math.Pow((1 + cashForward[j] * coverage[j]), k) * Utilities.Factorial(k));
                        innerSum2 += shiftedCashForward[j] * coverage[j] * integral[j] / (1 + cashForward[j] * coverage[j]);
                    }
                    sum2 += Math.Pow(innerSum2, k) / Utilities.Factorial(k);
                    innerSum2 = 0;
                }
                else
                {
                    for (int j = 1; j <= fixingNode; j++)
                    {
                        sum1 += shiftedCashForward[j] * coverage[j] * integral[j] / (1 + cashForward[j] * coverage[j]);
                    }
                }
            }
            var epsilon = sum1 + 1.5 * sum2;
            //var convexity = Math.Exp(epsilon);

            return epsilon; //epsilon in the notation of Jackel & Kawai
        }

        public static double Price(double cashForward, double shift, double epsilon)
        {
            // the cash forward, shift and convexity are those corresponding to the fixing node of the futures contract
            return (cashForward + shift) * (1 + epsilon) - shift;

            /*
            // lognormal modification
            double convexity = Math.Exp(epsilon);
            return (cashForward + shift) * convexity - shift;
            */
        }

    }

    internal static class Utilities
    {
        public static int Factorial(int a)
        {
            //returns a! for a >= 0, 0 for a < 0.
            var fact = 1;
            if (a < 0)
            {
                return 0;
            }
            for (var i = 2; i <= a; i++)
            {
                fact *= i;
            }
            return fact;
        }

        public static double[,] Transpose(double[,] a)
        {
            //returns the transpose of a

            int m = a.GetLength(0);
            int n = a.GetLength(1);

            var b = new double[n,m];

            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    b[j, i] = a[i, j];
                }
            }
            return b;
        }

        public static double[, ,] BuildCorrelationMatrix(double[,] correlation, int terminalNode, int piecewiseIntervals)
        {
            //Builds the correlation matrix from the correlation input

            var correlationMatrix = new double[terminalNode + 1, terminalNode + 1, piecewiseIntervals];
            if (correlation.GetLength(0) == 1 && correlation.GetLength(1) == 1)
            {
                for (int i = 0; i <= terminalNode; i++)
                {
                    for (int j = 0; j <= terminalNode; j++)
                    {
                        for (int k = 0; k < piecewiseIntervals; k++)
                        {
                            if (i == 0 || j == 0)
                            {
                                correlationMatrix[i, j, k] = 0;
                            }
                            else
                            {
                                correlationMatrix[i, j, k] = correlation[0, 0];
                            }
                        }
                    }
                }
            }
            else if (correlation.GetLength(0) == terminalNode && correlation.GetLength(1) == terminalNode)
            {
                for (int i = 0; i <= terminalNode; i++)
                {
                    for (int j = 0; j <= terminalNode; j++)
                    {
                        for (int k = 0; k < piecewiseIntervals; k++)
                        {
                            if (i == 0 || j == 0)
                            {
                                correlationMatrix[i, j, k] = 0;
                            }
                            else
                            {
                                correlationMatrix[i, j, k] = correlation[i-1, j-1];
                            }
                        }
                    }
                }
            }
            else if ((correlation.GetLength(0) == 1 && correlation.GetLength(1) > 1) ||
                     correlation.GetLength(0) > 1 && correlation.GetLength(1) == 1)
            {
                // error
            }

            return correlationMatrix;
        }

        /// <summary>
        /// Insert a zero in the first entry.
        /// </summary>
        public static double[] PrependZero(IEnumerable<double> inputs)
        {
            var newList = new List<double> { 0 };
            newList.AddRange(inputs);
            return newList.ToArray();
        }

        /// <summary>
        /// Insert zeros as the first entries.
        /// </summary>
        public static double[,] PrependZeros(double[,] inputs)
        {
            var dimensions = new int[2];
            double[,] result;

            dimensions[0] = inputs.GetLength(0);
            dimensions[1] = inputs.GetLength(1);

            if (dimensions[0] == 1 && dimensions[1] > 1)
            {
                result = new double[1, dimensions[1] + 1];
                for (int i = 1; i <= dimensions[1]; i++)
                {
                    result[0, i] = inputs[0, i - 1];
                }
            }
            else if (dimensions[0] > 1 && dimensions[1] == 1)
            {
                result = new double[dimensions[0] + 1, 1];
                for (int i = 1; i <= dimensions[0]; i++)
                {
                    result[i, 0] = inputs[i - 1, 0];
                }
            }
            else
            {
                result = new double[dimensions[0] + 1, dimensions[1] + 1];
                for (int i = 1; i <= dimensions[0]; i++)
                {
                    for (int j = 1; j <= dimensions[1]; j++)
                    {
                        result[i, j] = inputs[i - 1, j - 1];
                    }
                }
            }
            return result;
        }
    }
}