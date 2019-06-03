using System;


namespace Orion.Models.Rates.Swap
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