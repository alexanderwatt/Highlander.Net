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

using System;
using System.Collections;
using System.Diagnostics;
using Highlander.Numerics.Processes;

#endregion

namespace Highlander.Numerics.Lattices
{
    /// <summary>
    /// Lattice-method abstract base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="Lattice"/> relies on one or several trees 
    /// (each one approximating a diffusion process) to price an
    /// instance of the <see cref="DiscretizedAsset"/> class. 
    /// Trees are instances of classes derived from <see cref="Tree"/>,
    /// and define the branching between nodes and transition probabilities.
    /// </para>
    /// </remarks>
    public abstract class Lattice : NumericalMethod 
    {
        protected Lattice(ITimeGrid timeGrid, int n) : base(timeGrid)
        {
            this.n = n;
            if( n <= 0 ) 
                throw new ArgumentException( 
                    "LatNotZero");

            _statePrices = new ArrayList {new[] {1.0}};
            _statePricesLimit = 0;
        }

        private readonly int n;
        // Arrow-Debrew state prices
        private readonly ArrayList _statePrices;
        private int _statePricesLimit;

        /// <summary>
        /// Computes the present value of an asset using Arrow-Debrew prices.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public double PresentValue(IDiscretizedAsset asset)
        {
            int i = TimeGrid.FindIndex(asset.Time);
            if ( i > _statePricesLimit )
                ComputeStatePrices(i);
            // return DotProduct(asset.Values, statePrices[i]);
            var prices = (double[])_statePrices[i];
            Debug.Assert(
                asset.Values.Length == prices.Length,
                "arrays with different sizes cannot be multiplied.");
            double val = 0.0;
            for( int j=0; j<prices.Length; j++)
                val += asset.Values[j] * prices[j];
            return val;
            // return inner_product(asset.Values, statePriceValues(i), 0.0);
        }
/*
		private double inner_product(IEnumerable a, IEnumerable b, double val)
		{
			IEnumerator ita = a.GetEnumerator();
			IEnumerator itb = b.GetEnumerator();
			while( ita.MoveNext() && itb.MoveNext() )
				val += (double)ita.Current * (double)itb.Current;
			return val;
		}
*/
        #region NumericalMethod interface

        /// <summary>
        /// Initialize a DiscretizedAsset object.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="time"></param>
        public override void Initialize(IDiscretizedAsset asset, double time)
        {
            int i = TimeGrid.FindIndex(time);
            asset.Time = time;
            asset.Reset(Count(i));
        }

        /// <summary>
        /// Roll-back a DiscretizedAsset object until a certain time.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="toTime"></param>
        public override void Rollback(IDiscretizedAsset asset, double toTime)
        {
            double from = asset.Time;	// Can't Rollback to the future:
            if( from<toTime )
                throw new ArgumentException( "LatBackFut");

            int iFrom = TimeGrid.FindIndex(from);
            int iTo = TimeGrid.FindIndex(toTime);

            for (int i=iFrom-1; i>=iTo; i--) 
            {
                var newValues = new double[ Count(i) ];
                Stepback(i, asset.Values, newValues);
                asset.Time = TimeGrid[i];
                asset.Values = newValues;
                asset.AdjustValues();
            }
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        public abstract int Count(int i);

        /// <summary>
        /// Discount factor at time <i>t<sub>i</sub></i> and 
        /// node indexed by <paramref name="index"/>.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract double Discount(int i, int index);

        /// <summary>
        /// Descendant.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        protected abstract int Descendant(int i, int index, int branch);

        /// <summary>
        /// Probability.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="index"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        protected abstract double Probability(int i, int index, int branch);


        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        public double[] StatePrices(int i)
        {
            if ( i > _statePricesLimit )
                ComputeStatePrices(i);
            return (double[])_statePrices[i];
        }

        private double[] StatePriceValues(int i)
        {
            return (double[])_statePrices[i];
        }

        protected void ComputeStatePrices(int until)
        {
            for (int i=_statePricesLimit; i<until; i++) 
            {
                var v = new double[ Count(i+1) ];
                _statePrices.Add(v);
                for (int j=0; j<Count(i); j++) 
                {
                    double disc = Discount(i,j);
                    double statePrice = StatePriceValues(i)[j];
                    for (int l=0; l<n; l++) 
                    {
                        StatePriceValues(i+1)[Descendant(i,j,l)] +=
                            statePrice*disc*Probability(i,j,l);
                    }
                }
            }
            _statePricesLimit = until;
        }

        protected virtual void Stepback(int i, double[] values, double[] newValues)
        {
            for (int j=0; j<Count(i); j++) 
            {
                double value = 0.0;
                for (int l=0; l<n; l++) 
                    value += Probability(i,j,l) * values[Descendant(i,j,l)];
                value *= Discount(i,j);
                newValues[j] = value;
            }

        }
    }
}