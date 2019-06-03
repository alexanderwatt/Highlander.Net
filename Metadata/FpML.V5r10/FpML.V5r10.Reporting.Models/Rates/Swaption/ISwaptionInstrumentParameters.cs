#region Usings

using System;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Swaption
{
    public interface ISwaptionInstrumentParameters
    {
        Decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the IsBoughtn flag.
        /// </summary>
        /// <value>The IsBought flag.</value>
        Boolean IsBought{ get; set; }

        /// <summary>
        /// Gets or sets the is a payers swaption flag.
        /// </summary>
        /// <value>The payers swaption flag.</value>
        Boolean IsCall{ get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        IVolatilitySurface VolatilitySurface { get; set; } // VolCube!

        /// <summary>
        /// Gets or sets the other npv.
        /// </summary>
        /// <value>The other npv</value>
        Decimal OtherNPV { get; set; }

        /// <summary>
        /// Gets or sets the swap delta.
        /// </summary>
        /// <value>The swap delta.</value>
        Decimal SwapAccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal? Premium { get; set; }

        /// <summary>
        /// Gets or sets the swap break even rate.
        /// </summary>
        /// <value>The swap break even rate.</value>
        Decimal SwapBreakEvenRate { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal? Volatility { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        Decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal Strike { get; set; }
    }
}