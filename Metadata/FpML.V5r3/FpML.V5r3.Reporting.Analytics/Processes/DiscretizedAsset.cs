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
using System.Collections;

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Discretized asset class used by numerical methods.
    /// </summary>
    /// <remarks>
    /// This abstract base class is a representation of the price of a
    /// derivative at a specific time. It is roughly an array of values, 
    /// each value being associated to a state of the underlying stochastic
    /// variables. For the moment, it is only used when working with trees, 
    /// but it should be quite easy to make a use of it in finite-differences
    /// methods. 
    /// The two main points, when deriving classes, are:
    /// <list type="number">
    /// <item><description>
    /// Define the initialisation procedure 
    /// (e.g. terminal payoff for european stock options).
    /// </description></item>
    /// <item><description>
    /// Define the method adjusting values, when necessary, at each time steps 
    /// (e.g. apply the step condition for american or bermudan options).
    /// </description></item>
    /// </list>
    /// </remarks>
    public abstract class DiscretizedAsset : IDiscretizedAsset
    {

        private const double EPSILON = Double.Epsilon;	// TODO: maybe this is to small!

        protected DiscretizedAsset(INumericalMethod method)
        {
            Method = method;
        }

        #region IDiscretizedAsset implementation

        public double Time { get; set; }

        public double[] Values { get; set; }

        public double this[int i]
        { 
            get 
            { 
                if ( Values == null )
                    throw new InvalidOperationException(
                        "NumDiscrAsVal");
                return Values[i]; 
            }
        }

        public INumericalMethod Method { get; }

        public abstract void Reset(int size);

        public virtual void AdjustValues() 
        {}

        public virtual void AddTimes(/*std::list<Time>&*/ IList times) 
        {}

        #endregion

        protected bool IsOnTime(double time)
        {
            ITimeGrid grid = Method.TimeGrid;
            double gridTime = grid[grid.FindIndex(time)];
            return ( Math.Abs(gridTime - Time) < EPSILON);
        }
    }
}