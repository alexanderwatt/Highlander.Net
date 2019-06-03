
using System;
using System.Collections.Generic;

namespace Orion.Models.Rates.Options
{
    public interface IRateOptionAssetResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal ImpliedStrike { get; }

        /// <summary>
        /// Gets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        Decimal FlatVolatility { get; }

        /// <summary>
        /// Gets the forward rates have been calculated from the discount ffactors provided.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double> ForwardRates { get; }

        /// <summary>
        /// Gets the npvs.
        /// </summary>
        /// <value>The pvs.</value>
        List<double> NPV { get; }

        /// <summary>
        /// Gets the implied quotes.
        /// </summary>
        /// <value>The implied quotes.</value>
        List<double> ImpliedQuote { get; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        List<double> ExpectedValue { get; }

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        List<double> RawValue { get; }

        /// <summary>
        /// Gets the accrual factors.
        /// </summary>
        /// <value>The accrual factors.</value>
        List<double> AccrualFactor { get; }

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        List<double> Delta0 { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        List<double> Delta1 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        List<double> Gamma0 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        List<double> Gamma1 { get; }

        /// <summary>
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        List<double> Vega0 { get; }

        /// <summary>
        /// Gets the first derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        List<double> Theta0 { get; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        List<double> VolatilityAtExpiry { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        List<double> MarketQuote { get; }
    }
}