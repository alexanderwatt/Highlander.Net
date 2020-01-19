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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// Base class which defines the data required by all asset controllers (i.e. Type A Models)
    /// </summary>
    public sealed class AssetControllerData : IAssetControllerData
    {
        /// <summary>
        /// Gets or sets the basic asset valuation.
        /// </summary>
        /// <value>The basic asset valuation.</value>
        public BasicAssetValuation BasicAssetValuation { get; set; }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        public IMarketEnvironment MarketEnvironment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetControllerData"/> class.
        /// </summary>
        public AssetControllerData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetControllerData"/> class.
        /// </summary>
        /// <param name="basicAssetValuation">The basic asset valuation.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="market">The market.</param>
        public AssetControllerData(BasicAssetValuation basicAssetValuation, DateTime valuationDate, IMarketEnvironment market)
        {
            BasicAssetValuation = basicAssetValuation;
            ValuationDate = valuationDate;
            MarketEnvironment = market;
        }

        /// <summary>
        /// Creates the asset controller data.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public IAssetControllerData CreateAssetControllerData(string[] metrics, DateTime baseDate, IMarketEnvironment market)
        {
            var bav = new BasicAssetValuation();
            var quotes = new BasicQuotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                var quotation = new BasicQuotation
                                    {
                                        valueSpecified = true,
                                        value = 0.0m,
                                        measureType = new AssetMeasureType
                                                          {
                                                              Value =
                                                                  metric
                                                          }
                                    };
                quotes[index] = quotation;
                index++;
            }
            bav.quote = quotes;
            return new AssetControllerData(bav, baseDate, market);
        }
    }
}