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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.Reporting.V5r3;

#endregion


namespace Highlander.CurveEngine.V5r3.Assets.Property
{
    ///<summary>
    ///</summary>
    public abstract class PriceableLeaseAssetController : AssetControllerBase, IPriceableLeaseAssetController
    {
        public decimal Price { get; set; }

        public decimal StartAmount { get; set; }

        public decimal ChangeToBase { get; set; }

        public abstract Highlander.Reporting.V5r3.Lease GetLease();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public abstract DateTime GetNextLeasePaymentDate();

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        ///<summary>
        ///</summary>
        public ILeaseAssetResults CalculationResults { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ILeaseAssetParameters AnalyticModelParameters { get; protected set; }

        // Analytics
        public IModelAnalytic<ILeaseAssetParameters, LeaseMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// The issuer name.
        /// </summary>
        public bool IsPayerBase { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime FirstPaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public Period ReviewFrequency { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextReviewDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextPaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastPaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// The daycount fraction.
        /// </summary>
        public DayCountFraction DayCount { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextLeasePaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastLeasePaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime RiskMaturityDate { get; set; }


        ///<summary>
        ///</summary>
        public int LeasePaymentNumber { get; set; }

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
        public decimal BaseAmount { get; set; }

        ///<summary>
        ///</summary>
        public Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        public Period Frequency { get; set; }

        ///<summary>
        ///</summary>
        public Period LeaseTenor { get; set; }

        ///<summary>
        ///</summary>
        public BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

    }
}