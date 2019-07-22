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

namespace FpML.V5r10.Reporting.Models.Rates.Swap
{
    public class SwapInstrumentParameters : IIRSwapInstrumentParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        public string Leg1Currency { get; set; }
        public string Leg2Currency { get; set; }
        public string ReportingCurrency { get; set; }
        public decimal MarketQuote { get; set; }
        public decimal FixedRateOrStrike { get; set; }
        public decimal DeltaR { get; set; }
        public decimal HistoricalDeltaR { get; set; }
        public decimal AccrualFactor { get; set; }
        public decimal FloatingNPV { get; set; }
        public decimal NPV { get; set; }
        public Boolean IsPayFixedInd { get; set; }
        public Boolean IsDiscounted { get; set; }
        public decimal PayStreamAccrualFactor { get; set; }
        public decimal PayStreamNPV { get; set; }
        public decimal PayStreamFloatingNPV { get; set; }
        public decimal ReceiveStreamFloatingNPV { get; set; }
        public decimal ReceiveStreamNPV { get; set; }
        public decimal ReceiveStreamAccrualFactor { get; set; }
        public Decimal TargetNPV { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        public Decimal[] PayerPaymentDiscountFactors { get; set; }

        ////// Payer
        //public Decimal[] PayerCouponNotionals { get; set; }
        public Decimal[] PayerCouponYearFractions { get; set; }
        //public Decimal[] PayerPaymentDiscountFactors { get; set; }
        //public Decimal[] PayerPresentValues { get; set; }
        //public Decimal PayerPrincipalExchange { get; set; }

        ////// Receiver
        //public Decimal[] ReceiverCouponNotionals { get; set; }
        public Decimal[] ReceiverCouponYearFractions { get; set; }
        //public Decimal[] ReceiverPaymentDiscountFactors { get; set; }
        //public Decimal[] ReceiverPresentValues { get; set; }
        //public Decimal ReceiverPrincipalExchange { get; set; }

        //public Decimal ReceiverToPayerSpotRate { get; set; }

    }
}