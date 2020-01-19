using System;

namespace Orion.Models.Rates.Bonds
{
    public class BondTransactionParameters : IBondTransactionParameters
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