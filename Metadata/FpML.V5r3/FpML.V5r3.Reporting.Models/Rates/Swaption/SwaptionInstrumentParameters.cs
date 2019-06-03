#region Usings

using System;
using Orion.Models.Rates.Swap;
using Orion.Models.Rates.Swaption;
using Orion.Models.Rates.Coupons;
using Orion.Models.Generic.Cashflows;
using System.Collections.Generic;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Rates.Swaption
{
    public class SwaptionInstrumentParameters : ISwaptionInstrumentParameters
    {
        public Decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public Decimal? Premium { get; set; }

        /// <summary>
        /// Gets or sets the IsBoughtn flag.
        /// </summary>
        /// <value>The IsBought flag.</value>
        public Boolean IsBought { get; set; }

        /// <summary>
        /// Gets or sets the is a payers swaption flag.
        /// </summary>
        /// <value>The payers swaption flag.</value>
        public bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        public IVolatilitySurface VolatilitySurface { get; set; } // VolCube!

        /// <summary>
        /// Gets or sets the other npv.
        /// </summary>
        /// <value>The other npv</value>
        public Decimal OtherNPV { get; set; }

        /// <summary>
        /// Gets or sets the swap delta.
        /// </summary>
        /// <value>The swap delta.</value>
        public decimal SwapAccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the swap break even rate.
        /// </summary>
        /// <value>The swap break even rate.</value>
        public decimal SwapBreakEvenRate { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal? Volatility { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public decimal Strike { get; set; }
    }
}