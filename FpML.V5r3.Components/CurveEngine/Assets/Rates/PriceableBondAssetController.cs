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

#region Usings

using System;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.Models.Assets;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// PriceableBondAssetController
    /// </summary>
    public abstract class PriceableBondAssetController : AssetControllerBase, IPriceableBondAssetController
    {
        public abstract Bond GetBond();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract DateTime GetNextCouponDate();

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        ///<summary>
        ///</summary>
        public IBondAssetResults CalculationResults { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IBondAssetParameters AnalyticModelParameters { get; protected set; }

        // Analytics
        public IModelAnalytic<IBondAssetParameters, BondMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        public bool IsYTMQuote { get; set; }

        ///<summary>
        ///</summary>
        public decimal QuoteValue { get; set; }

        /// <summary>
        /// The issuer name.
        /// </summary>
        public string Issuer { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime MaturityDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextCouponDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastCouponDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastRegCoupDate { get; set; }

        ///<summary>
        ///</summary>
        public int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        public DateTime Next2CoupDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime FirstAccrualDate { get; set; }

        ///<summary>
        ///</summary>
        public int CoupNum { get; set; }

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
        public DateTime[] UnAdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        public DateTime[] AdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        public decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        public Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        public int Frequency { get; set; }

        /// <summary>
        /// The purchase price as a dirty price.
        /// </summary>
        public decimal PurchasePrice { get; set; }

        ///<summary>
        ///</summary>
        public BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public abstract decimal[] GetYearFractions();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract decimal GetAccruedFactor();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract decimal GetRemainingAccruedFactor();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract bool IsExDiv();

        ///<summary>
        ///</summary>
        public abstract decimal GetCouponRate();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract DateTime GetLastCouponDate();

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<returns></returns>
        public abstract int GetAccrualDays(DateTime valuationDate);
    }
}