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
    public class CapletBootstrapResults
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="CapletBootstrapResults"/>.
        /// No data is passed to the constructor because its only purpose is
        /// to instantiate a default instance of the data structure that will
        /// store the results of the Caplet bootstrap.
        /// </summary>
        public CapletBootstrapResults()
        {
            _capletBootstrapResults =
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
            if (!_capletBootstrapResults.ContainsKey(expiry))
            {
                // Add the result
                _capletBootstrapResults.Add(expiry, capletVolatility);
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
            if (!_capletBootstrapResults.ContainsKey(expiry))
            {
                // Add the result
                _capletBootstrapResults.Add(expiry, (decimal)capletVolatility);
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

            if (_capletBootstrapResults.ContainsKey(expiry))
            {
                capletVolatility = _capletBootstrapResults[expiry];
            }

            return capletVolatility;
        }

        /// <summary>
        /// Gets the results of the Caplet bootstrap.
        /// </summary>
        /// <value>SortedList in which the keys are expiries and the
        /// values are the bootstrap Caplet volatilities.</value>
        public SortedList<DateTime, decimal> Results
        {
            get { return _capletBootstrapResults; }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Data structure to store the bootstrap Caplet volatility at a
        /// particular expiry key.
        /// </summary>
        private readonly SortedList<DateTime, decimal> _capletBootstrapResults;

        #endregion
    }
}