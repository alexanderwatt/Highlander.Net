using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Assets
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceableComplexRateCoupon<AMP, AMR> : IPriceableRateCoupon<AMP, AMR>
    {
        /// <summary>
        /// Gets or sets a value indicating whether [use observed rate].
        /// </summary>
        /// <value><c>true</c> if [use observed rate]; otherwise, <c>false</c>.</value>
        Boolean UseObservedRates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires reset].
        /// </summary>
        /// <value><c>true</c> if [requires reset]; otherwise, <c>false</c>.</value>
        Boolean RequiresReset { get; }

        /// <summary>
        /// Gets the name of the forward curve.
        /// </summary>
        /// <value>The name of the forward curve.</value>
        String ForecastCurveName { get; }

        /// <summary>
        /// Gets a value indicating whether [rate observation specified].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [rate observation specified]; otherwise, <c>false</c>.
        /// </value>
        Boolean RateObservationsSpecified { get; }

        /// <summary>
        /// Gets the rate observation.
        /// </summary>
        /// <value>The rate observation.</value>
        IList<RateObservation> RateObservations { get; }

        /// <summary>
        /// Gets the index of the forecast rate.
        /// </summary>
        /// <value>The index of the forecast rate.</value>
        IList<ForecastRateIndex> ForecastRateIndexes { get; }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <value>The reset date.</value>
        IList<DateTime> ResetDates { get; }

        /// <summary>
        /// Gets the reset relative to.
        /// </summary>
        /// <value>The reset relative to.</value>
        ResetRelativeToEnum ResetRelativeTo { get; }

        /// <summary>
        /// Gets the margin.
        /// </summary>
        /// <value>The margin.</value>
        IList<Decimal> Margins { get; }

        /// <summary>
        /// Updates the name of the forward curve.
        /// </summary>
        /// <param name="newForwardCurveName">New name of the forward curve.</param>
        void UpdateForwardCurveName(string newForwardCurveName);
    }
}