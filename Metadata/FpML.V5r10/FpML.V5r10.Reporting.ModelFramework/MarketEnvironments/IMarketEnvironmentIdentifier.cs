﻿using System;

namespace FpML.V5r10.Reporting.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IMarketEnvironmentIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets the Market.
        /// </summary>
        /// <value>The Market.</value>
        string Market { get; }

        ///<summary>
        /// Gets the market date.
        ///</summary>
        DateTime? Date { get; }
    }
}