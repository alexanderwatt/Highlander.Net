/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace Orion.Analytics.Options
{
    /// <summary>
    /// Class that encapsulates the data structure and functionality to
    /// store the results of a bootstrap to construct Caplet volatilities.
    /// Results are stored as (strike, (expiry, Caplet volatility)), where
    /// "strike" is the primary key. Each strike "expands" to reveal a list of
    /// expiries and corresponding Caplet volatilities.
    /// No data validation is performed by the class because it contains no
    /// business logic.
    /// </summary>
    public class VolCurveBootstrapResults
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="CapletBootstrapResults"/>.
        /// No data is passed to the constructor because its only purpose is
        /// to instantiate a default instance of the data structure that will
        /// store the results of the Caplet bootstrap.
        /// </summary>
        public VolCurveBootstrapResults()
        {
            Results =
                new SortedList<DateTime, decimal>();
        }

        #endregion

        #region Public Methods to Add and Get Results

        /// <summary>
        /// Adds a result to the container that stores all results of a
        /// particular Caplet bootstrap.
        /// If an attempt is made to add a Caplet volatility at an existing
        /// expiry, then this result is ignored and not added.
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        /// <param name="capletVolatility">Caplet volatility.</param>
        public void AddResult(DateTime expiry, decimal capletVolatility)
        {
            // Check if the key is present in the current results.
            if (!Results.ContainsKey(expiry))
            {
                // Add the result
                Results.Add(expiry, capletVolatility);
            }            
        }

        /// <summary>
        /// Adds a result to the container that stores all results of a
        /// particular Caplet bootstrap.
        /// If an attempt is made to add a Caplet volatility at an existing
        /// expiry, then this result is ignored and not added.
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        /// <param name="capletVolatility">Caplet volatility.</param>
        public void AddResult(DateTime expiry, double capletVolatility)
        {
            // Check if the key is present in the current results.
            if (!Results.ContainsKey(expiry))
            {
                // Add the result
                Results.Add(expiry, (decimal)capletVolatility);
            }
        }

        /// <summary>
        /// Gets a result from the container that stores all the results of
        /// the Caplet bootstrap.
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        /// <returns>Caplet volatility. If the expiry is not found, the
        /// method returns -1.</returns>
        public decimal GetResult(DateTime expiry)
        {
            // Initialise the return variable to its default value.
            var capletVolatility = -1.0m;

            if (Results.ContainsKey(expiry))
            {
                capletVolatility = Results[expiry];
            }
            return capletVolatility;
        }

        /// <summary>
        /// Gets the results of the Caplet bootstrap.
        /// </summary>
        /// <value>SortedList in which the keys are expiries and the
        /// values are the bootstrap Caplet volatilities.</value>
        public SortedList<DateTime, decimal> Results { get; set; } 

        #endregion
    }
}