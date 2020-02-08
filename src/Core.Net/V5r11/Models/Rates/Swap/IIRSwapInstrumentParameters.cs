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

namespace Highlander.Reporting.Models.V5r3.Rates.Swap
{
    public interface IIRSwapInstrumentParameters// : IRateInstrumentParameters
    {
        string Leg1Currency { get; set; }
        string Leg2Currency { get; set; }
        string ReportingCurrency { get; set; }
        decimal FixedRateOrStrike { get; set; }
        Decimal MarketQuote { get; set; }
        Decimal DeltaR { get; set; }
        Decimal HistoricalDeltaR { get; set; }
        Decimal AccrualFactor { get; set; }
        Decimal FloatingNPV  { get; set; }
        Decimal NPV  { get; set; }
        Boolean IsPayFixedInd { get; set; }
        Boolean IsDiscounted { get; set; }
        Decimal PayStreamAccrualFactor { get; set; }
        Decimal PayStreamNPV { get; set; }
        Decimal PayStreamFloatingNPV { get; set; }
        Decimal ReceiveStreamFloatingNPV { get; set; }
        Decimal ReceiveStreamNPV { get; set; }
        Decimal ReceiveStreamAccrualFactor { get; set; }
        Decimal TargetNPV { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        Decimal[] PayerPaymentDiscountFactors { get; set; }

        ////// Payer
        //Decimal[] PayerCouponNotionals { get; set; }
        Decimal[] PayerCouponYearFractions { get; set; }
        //Decimal[] PayerPaymentDiscountFactors { get; set; }
        //Decimal[] PayerPresentValues { get; set; }
        //Decimal PayerPrincipalExchange { get; set; }

        ////// Receiver
        //Decimal[] ReceiverCouponNotionals { get; set; }
        Decimal[] ReceiverCouponYearFractions { get; set; }
        //Decimal[] ReceiverPaymentDiscountFactors { get; set; }
        //Decimal[] ReceiverPresentValues { get; set; }
        //Decimal ReceiverPrincipalExchange { get; set; }
        //Decimal ReceiverToPayerSpotRate { get; set; }
    }
}