using System;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Swaption instrument
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableInterestRateSwaption<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the floating payer party reference.
        /// </summary>
        /// <value>The floating payer party reference.</value>
        string BuyerPartyReference { get; }

        /// <summary>
        /// Gets the fixed payer party reference.
        /// </summary>
        /// <value>The fixed payer party reference.</value>
        string SellerPartyReference { get; }

        /// <summary>
        /// Gets the premium payment dates.
        /// </summary>
        /// <value>The payment dates.</value>
        DateTime[] PremiumPaymentDates { get; }

        /// <summary>
        /// Gets the premium payment amounts.
        /// </summary>
        /// <value>The payment amounts.</value>
        Decimal[] PremiumPaymentAmounts { get; }

        /// <summary>
        /// Gets the exercise dates.
        /// </summary>
        /// <value>The exercise dates.</value>
        DateTime[] ExerciseDates { get; }

        /// <summary>
        /// Gets or sets the priceable swap instrument.
        /// </summary>
        /// <value>The priceable swap instrument.</value>
        IPriceableInstrumentController<Swap> PriceableSwapInstrument{ get; }
    }
}