﻿/*
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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableBondAssetController : IPriceableAssetController
    {
        /// <summary>
        /// The issuer name.
        /// </summary>
        string Issuer { get; set; }

        ///<summary>
        ///</summary>
        Bond GetBond();

        ///<summary>
        ///</summary>
        DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        bool IsYTMQuote { get; set; }

        ///<summary>
        ///</summary>
        decimal QuoteValue { get; set; }

        ///<summary>
        ///</summary>
        DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime MaturityDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime NextCouponDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime LastCouponDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime LastRegCoupDate { get; set; }

        ///<summary>
        ///</summary>
        int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        DateTime Next2CoupDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime FirstAccrualDate { get; set; }

        ///<summary>
        ///</summary>
        int CoupNum { get; set; }

        ///<summary>
        ///</summary>
        bool IsXD { get; set; }

        ///<summary>
        ///</summary>
        RelativeDateOffset ExDividendDateOffset { get; set; }

        ///<summary>
        ///</summary>
        RelativeDateOffset SettlementDateOffset { get; set; }

        ///<summary>
        ///</summary>
        IBusinessCalendar SettlementDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        IBusinessCalendar PaymentDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        DateTime[] UnAdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        DateTime[] AdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        int Frequency { get; set; }

        /// <summary>
        /// The purchase price as a dirty price.
        /// </summary>
        decimal PurchasePrice { get; set; }

        ///<summary>
        ///</summary>
        BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        decimal[] GetYearFractions();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        decimal GetAccruedFactor();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        decimal GetRemainingAccruedFactor();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        bool IsExDiv();

        ///<summary>
        ///</summary>
        decimal GetCouponRate();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        DateTime GetLastCouponDate();

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<returns></returns>
        int GetAccrualDays(DateTime valuationDate);

        ///<summary>
        ///</summary>
        ///<returns></returns>
        DateTime GetNextCouponDate();

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        decimal Multiplier { get; set; }
    }
}