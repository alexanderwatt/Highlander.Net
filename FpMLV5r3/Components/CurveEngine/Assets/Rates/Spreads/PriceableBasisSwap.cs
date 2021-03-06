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

#region Using directives

using System;
using Highlander.CurveEngine.V5r3.Assets.Rates.Swaps;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Spreads
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableBasisSwap : PriceableIRSwap
    {
        #region Properties

        /// <summary>
        /// The index on the base leg.
        /// </summary>
        public RateIndex BaseLegRateIndex { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBasisSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="spread">The spread.</param>
        public PriceableBasisSwap(DateTime baseDate, BasisSwapNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar, BasicQuotation spread)
            : base(baseDate, nodeStruct.MarginLeg, nodeStruct.SpotDate, nodeStruct.MarginLegCalculation, nodeStruct.MarginLegDateAdjustments,
            nodeStruct.MarginLegRateIndex, fixingCalendar, paymentCalendar, spread)
        {
            BaseLegRateIndex = nodeStruct.BaseLegRateIndex;
            MarketQuote = GetSpread(spread);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SwapAsset":
                    case "ClearedSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();//
                        break;
                }
            }
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
                                                               {
                                                                   YearFractions = YearFractions,
                                                                   StartDiscountFactor =
                                                                       GetDiscountFactor(interpolatedSpace,
                                                                                         AdjustedStartDate, BaseDate),
                                                                   Rate =
                                                                       CalculateImpliedQuote(interpolatedSpace) +
                                                                       MarketQuoteHelper
                                                                           .NormaliseGeneralPriceUnits(QuotationType, Spread,
                                                                               "DecimalRate").value
            };
            //3. Set the implied Rate with a spread
            //
            AnalyticResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel != null)
                AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.DiscountFactorAtMaturity });
            return AnalyticResults.DiscountFactorAtMaturity;
        }

        #region IPriceableRateSpreadAssetController

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="bq">The spread.</param>
        private static BasicQuotation GetSpread(BasicQuotation bq)
        {
            BasicQuotation spread;
            if (String.Compare(bq.measureType.Value, "MarketQuote", StringComparison.OrdinalIgnoreCase) == 0)
            {
                spread = BasicQuotationHelper.Create("Spread", bq.quoteUnits.Value);
                spread.value = bq.value;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
            return spread.measureType.Value == "Spread" ? spread : null;
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public new decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Spread.value;
        }

        #endregion
    }
}