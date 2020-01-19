using System;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// A pricer for collars interface
    /// </summary>
    public interface ICollarPricer
    {
        /// <summary>
        /// Finds the price.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindPrice(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost call strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostCallStrike(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost put strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostPutStrike(Stock stock, ZeroAUDCurve zeroRateCurve);

        /// <summary>
        /// Finds the zero cost strike.
        /// </summary>
        /// <param name="optionType">Type of the option.</param>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        Double FindZeroCostStrike(OptionType optionType, Stock stock, ZeroAUDCurve zeroRateCurve);
    }
}
