﻿using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;


namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// List of supported coupon stream types
    /// </summary>
    public enum CouponStreamType
    {
        ///<summary>
        ///</summary>
        GenericFixedRate,
        ///<summary>
        ///</summary>
        GenericFloatingRate,
        ///<summary>
        ///</summary>
        StructuredRate, 
        ///<summary>
        ///</summary>
        ComplexRate, 
        ///<summary>
        ///</summary>
        CapRate, 
        ///<summary>
        ///</summary>
        FloorRate, 
        ///<summary>
        ///</summary>
        CollarRate
    };

    ///// <summary>
    ///// List of supported principal types
    ///// </summary>
    //public enum PrincipalExchangeType
    //{
    //    ///<summary>
    //    ///</summary>
    //    None, 
    //    ///<summary>
    //    ///</summary>
    //    Initial, 
    //    ///<summary>
    //    ///</summary>
    //    Final, 
    //    ///<summary>
    //    ///</summary>
    //    InitialAndFinal, 
    //    ///<summary>
    //    ///</summary>
    //    LinearAmortising, 
    //    ///<summary>
    //    ///</summary>
    //    Collection
    //};

    /// <summary>
    /// IPriceableInterestRateStream
    /// </summary>
    public interface IPriceableInterestRateStream<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets a value indicating whether [paying fixed rate].
        /// </summary>
        /// <value><c>true</c> if [paying fixed rate]; otherwise, <c>false</c>.</value>
        Boolean PayingFixedRate { get; }


        /// <summary>
        /// Gets a value indicating whether [payer is base party].
        /// </summary>
        /// <value><c>true</c> if [payer is base party]; otherwise, <c>false</c>.</value>
        Boolean PayerIsBaseParty { get; }

        /// <summary>
        /// Gets the payer.
        /// </summary>
        /// <value>The payer.</value>
        string Payer { get; }

        /// <summary>
        /// Gets the receiver.
        /// </summary>
        /// <value>The receiver.</value>
        string Receiver{ get; }

        /// <summary>
        /// Gets the coupon steam type.
        /// </summary>
        /// <value>The coupon stream type.</value>
        CouponStreamType CouponStreamType { get; set; }

        /// <summary>
        /// Gets the principal type.
        /// </summary>
        /// <value>The principal type.</value>
        PrincipalExchanges PrincipalExchanges { get; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        List<InstrumentControllerBase> PriceableCoupons { get;}

        /// <summary>
        /// Gets or sets the priceable principal exchanges.
        /// </summary>
        /// <value>The priceable principal exchanges.</value>
        List<InstrumentControllerBase> PriceablePrincipalExchanges { get; }

        /// <summary>
        /// Gets the calculation.
        /// </summary>
        /// <value>The calculation.</value>
        Calculation Calculation { get; }

        /// <summary>
        /// Gets the stream start dates.
        /// </summary>
        /// <value>The stream start dates.</value>
        IList<DateTime> StreamStartDates { get; }

        /// <summary>
        /// Gets the stream end dates.
        /// </summary>
        /// <value>The stream end dates.</value>
        IList<DateTime> StreamEndDates { get; }

        /// <summary>
        /// Gets the stream payment dates.
        /// </summary>
        /// <value>The stream payment dates.</value>
        IList<DateTime> StreamPaymentDates { get; }

        /// <summary>
        /// Gets the adjusted stream dates indicators.
        /// </summary>
        /// <value>The adjusted stream dates indicators.</value>
        IList<Boolean> AdjustedStreamDatesIndicators { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newDiscountCurveName">New name of the discount curve.</param>
        void UpdateDiscountCurveName(string newDiscountCurveName);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketting.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}