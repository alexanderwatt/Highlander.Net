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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Equity;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;

namespace Highlander.CurveEngine.V5r3.Assets.Equity
{
    /// <summary>
    /// PriceableBondAssetController
    /// </summary>
    public abstract class PriceableEquityAssetController : AssetControllerBase, IPriceableEquityAssetController
    {
        public abstract EquityAsset GetEquity();

        ///<summary>
        ///</summary>
        public IEquityAssetResults CalculationResults { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IEquityAssetParameters AnalyticModelParameters { get; protected set; }

        // Analytics
        public IModelAnalytic<IEquityAssetParameters, EquityMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        public Decimal QuoteValue { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SettlementDate { get; set; }

        /// <summary>
        /// THe equity valuation curve.
        /// </summary>
        public string EquityCurveName { get; set; }

        /// <summary>
        /// The multiplier to be set
        /// </summary>
        public decimal Multiplier { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastRegDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        public DateTime Next2DividendDate { get; set; }

        ///<summary>
        ///</summary>
        public bool IsXD { get; set; }

        ///<summary>
        ///</summary>
        public RelativeDateOffset ExDividendDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public RelativeDateOffset SettlementDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public IBusinessCalendar SettlementDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        public IBusinessCalendar PaymentDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        public Decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        public Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal IndexAtMaturity { get; set; }
    }
}