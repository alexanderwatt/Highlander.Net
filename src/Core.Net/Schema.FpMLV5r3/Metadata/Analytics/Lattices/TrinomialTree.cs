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
using Highlander.Reporting.Analytics.V5r3.Processes;

namespace Highlander.Reporting.Analytics.V5r3.Lattices
{
    /// <summary>
    /// Recombining trinomial tree class.
    /// </summary>
    /// <remarks>
    /// This class defines a recombining trinomial tree approximating a
    /// a diffusion.
    /// <para>
    /// Warning: The diffusion term of the SDE must be independent of
    ///	         the underlying process.
    ///	</para>
    ///	<para>
    ///	When the underlying stochastic process has a mean-reverting pattern,
    ///	it is usually better to use a trinomial tree instead of a binomial tree. 
    ///	<see cref="TrinomialTree"/> is constructed using a 
    ///	<see cref="IDiffusionProcess">diffusion process</see> and a
    ///	<see cref="ITimeGrid">time-grid</see>. 
    ///	The goal is to build a recombining trinomial tree that will
    ///	discretize, at a finite set of times, the possible evolutions of a random
    ///	variable <i>y</i> satisfying <i>dy<sub>t</sub> = 
    ///	mu(t, y<sub>t</sub>) dt + sigma(t, y<sub>t</sub>) dW<sub>t</sub></i>.
    ///	At each node, there is a probability <i>p<sub>u</sub></i>, 
    ///	<i>p<sub>m</sub></i> and <i>p<sub>d</sub></i> to go through respectively 
    ///	the upper, the middle and the lower branch.
    ///	</para>
    ///	        <!--
    ///        
    ///			These probabilities must satisfy
    ///			\f[
    ///				p_{u}y_{i+1,k+1}+p_{m}y_{i+1,k}+p_{d}y_{i+1,k-1}=E_{i,j}
    ///			\f]
    ///			and
    ///			\f[
    ///				p_u y_{i+1,k+1}^2 + p_m y_{i+1,k}^2 + p_d y_{i+1,k-1}^2 = 
    ///				V^2_{i,j}+E_{i,j}^2,
    ///			\f]
    ///			where k (the index of the node at the end of the middle branch)
    ///			is the index of the node which is the nearest to the expected future
    ///			value, \f$ E_{i,j}=\mathbf{E}\left( y(t_{i+1})|y(t_{i})=y_{i,j}\right) \f$
    ///			and \f$ V_{i,j}^{2}=\mathbf{Var}\{y(t_{i+1})|y(t_{i})=y_{i,j}\} \f$.
    ///			If we suppose that the variance is only dependant on time 
    ///			\f$ V_{i,j}=V_{i} \f$ and set \f$ y_{i+1} \f$ to \f$ V_{i}\sqrt{3} \f$,
    ///			we find that
    ///			\f[
    ///				p_{u} = \frac{1}{6}+\frac{(E_{i,j}-y_{i+1,k})^{2}}{6V_{i}^{2}} +
    ///						\frac{E_{i,j}-y_{i+1,k}}{2\sqrt{3}V_{i}},
    ///			\f]
    ///			\f[
    ///				p_{m} = \frac{2}{3}-\frac{(E_{i,j}-y_{i+1,k})^{2}}{3V_{i}^{2}},
    ///			\f]
    ///			\f[
    ///				p_{d} = \frac{1}{6}+\frac{(E_{i,j}-y_{i+1,k})^{2}}{6V_{i}^{2}} -
    ///						\frac{E_{i,j}-y_{i+1,k}}{2\sqrt{3}V_{i}}.
    ///			\f]
    ///
    ///		-->
    /// </remarks>
    public class TrinomialTree : Tree 
    {
        ///<summary>
        ///</summary>
        ///<param name="process"></param>
        ///<param name="timeGrid"></param>
        public TrinomialTree(IDiffusionProcess process,
                             ITimeGrid timeGrid) : this(process, timeGrid, false)
        {}

        ///<summary>
        ///</summary>
        ///<param name="process"></param>
        ///<param name="timeGrid"></param>
        ///<param name="isPositive"></param>
        public TrinomialTree(IDiffusionProcess process,
                             ITimeGrid timeGrid, bool isPositive) : base(timeGrid.Count)
        {
            TimeGrid = timeGrid;
            _x0 = process.X0;
            _dx = new double[timeGrid.Count];
            // dx[0] = 0.0;
            int nTimeSteps = timeGrid.Count - 1;
            int jMin = 0;
            int jMax = 0;
            _branchings = new TrinomialBranching[nTimeSteps];
            for (int i=0; i<nTimeSteps; i++) 
            {
                double  t = timeGrid[i];
                double dt = timeGrid.Dt(i);
                //Variance must be independent of x
                double v2 = process.Variance(t, 0.0, dt);
                double v  = Math.Sqrt(v2);
                _dx[i+1] = v*Math.Sqrt(3.0);		// Add dx
                var branching = 
                    new TrinomialBranching(jMax-jMin+1);
                int jNewMin = int.MaxValue;
                int jNewMax = int.MinValue;
                for (int j=jMin; j<=jMax; j++) 
                {
                    double x = _x0 + j*_dx[i];
                    double m = process.Expectation(t, x, dt);
                    var temp = (int)Math.Floor((m-_x0)/_dx[i+1] + 0.5);
                    if (isPositive) 
                        while (_x0 + (temp-1)*_dx[i+1] <= 0) 
                            temp++;
                    if( temp < jNewMin ) jNewMin = temp;
                    if( temp > jNewMax ) jNewMax = temp;
                    double e = m - (_x0 + temp*_dx[i+1]);
                    double e2 = e*e;
                    double e3 = e*Math.Sqrt(3.0);
                    int j0 = j - jMin;
                    branching.K[j0] = temp;
                    branching.Probs[0,j0] = (1.0 + e2/v2 - e3/v) / 6.0;
                    branching.Probs[1,j0] = (2.0 - e2/v2) / 3.0;
                    branching.Probs[2,j0] = (1.0 + e2/v2 + e3/v) / 6.0;
                }
                branching.JMin = jNewMin-1;
                branching.JMax = jNewMax+1;
                _branchings[i] = branching;	// Add branching
                jMin = branching.JMin;
                jMax = branching.JMax;
            }
        }

        private readonly double _x0;
        private readonly double[] _dx;
        private readonly TrinomialBranching[] _branchings;

        ///<summary>
        ///</summary>
        public ITimeGrid TimeGrid { get; }

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        public double Dx(int i)
        { 
            return _dx[i]; 
        }

        public override double Underlying(int i, int index)
        {
            if (i==0) return _x0;
            int jMin = _branchings[i-1].JMin;
            return _x0 + Dx(i)*(jMin+index);		// todo: Dx(i) = dx[i] ??
        }

        public override int Count(int i)
        {
            if (i==0) return 1;
            return _branchings[i-1].JMax - 
                   _branchings[i-1].JMin + 1;
        }

        public override int Descendant(int i, int index, int branch)
        {
            return _branchings[i].Descendant(index, branch);
        }

        public override double Probability(int i, int index, int branch)
        {
            return _branchings[i].Probability(index, branch);
        }

        /// <summary>
        /// Branching scheme for a trinomial node.
        /// </summary>
        /// <remarks>
        /// Each node has three descendants, with the middle branch linked
        /// to the node which is closest to the expectation of the variable.
        /// </remarks>
        private class TrinomialBranching 
        {
            public TrinomialBranching(int n)
            {
                K = new int[n];
                Probs = new double[3,n];
            }

            public readonly int[] K;
            public readonly double[,] Probs;
            public int JMin = int.MaxValue;
            public int JMax = int.MinValue;

            public int Descendant(int index, int branch)
            {
                return (K[index] - JMin) - 1 + branch;
            }
            public double Probability(int index, int branch)
            {
                return Probs[branch,index];
            }
        }
    }
}