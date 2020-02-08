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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Options;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.Reporting.Models.V5r3.ForeignExchange.FxOption
{
    public enum FxOptionType
    { Call, Put, Straddle, Strangle, Collar }

    public class FxOptionAnalytic : FxRateCashflowAnalytic// ModelAnalyticBase<IFxOptionLegParameters, FxOptionLegInstrumentMetrics>, IFxOptionLegInstrumentResults
    {
        #region Properties

        /// <summary>
        /// Gets or sets the FxOptionType.
        /// </summary>
        /// <value>The FxOptionType.</value>
        public FxOptionType OptionType { get; protected set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public List<Decimal> Strikes { get; protected set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public List<Decimal> Volatilities { get; protected set; }

        /// <summary>
        /// Gets or sets the Time To Index.
        /// </summary>
        /// <value>The Time To Index.</value>
        public List<Decimal> TimeToIndices { get; protected set; }

        /// <summary>
        /// Gets or sets the Time To expiry.
        /// </summary>
        /// <value>The Time To expiry.</value>
        public List<Decimal> ExpiryTimes { get; protected set; }

        public bool GetIsCall()
        {
            return OptionType == FxOptionType.Call;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// The params had better be there!
        /// </summary>
        public FxOptionAnalytic()
        { }

        /// <summary>
        /// Initiate a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="indexCurve">The rate curve to use for calculating the forward index.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="indexVolSurface">The index volatility surface.</param>
        /// <param name="expiryTime">The expiry time. </param>
        /// <param name="timeToIndex">The time to reset or expiry. </param>
        /// <param name="strike">The strike. </param>
        /// <param name="fxOptionType">The option type. </param>
        public FxOptionAnalytic(DateTime valuationDate, DateTime paymentDate, decimal strike, decimal expiryTime, decimal timeToIndex,
            IFxCurve reportingCurrencyFxCurve, ICurve indexCurve, IRateCurve discountCurve, IVolatilitySurface indexVolSurface, FxOptionType fxOptionType)
            : base(valuationDate, paymentDate, reportingCurrencyFxCurve, indexCurve, discountCurve)
        {
            OptionType = fxOptionType;
            Strikes = new List<decimal> { strike };
            ExpiryTimes = new List<decimal> { expiryTime };
            TimeToIndices = new List<decimal> { timeToIndex };
            Volatilities = new List<decimal> { (decimal)indexVolSurface.GetValue((double)timeToIndex, (double)strike) };
        }


        /// <summary>
        /// This assumes that the rest dates are consistent with the curve.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="paymentDate">The payment date. The same rest period is assumed as with the spot date.</param>
        /// <param name="indexCurve">The index curve should be already in the correct form for the fx.</param>
        /// <param name="currency2">Normally the foreign rate curve.</param>
        /// <param name="currency2PerCurrency1">The currency2PerCurrency1 flag. </param>
        /// <param name="currency1">Normally the domestic rate curve. </param>
        /// <param name="expiryTime">The expiry time. </param>
        /// <param name="timeToIndex">The time to reset or expiry. </param>
        /// <param name="indexVolSurface">The index volatility surface.</param>
        /// <param name="currency1Settlement">Is the currency1 the settlement currency? </param>
        /// <param name="strike">The strike. </param>
        /// <param name="reportingCurrencyFxCurve">The reporting current fx curve from settlement currency to reporting currency. It must already be normalised.</param>
        /// <param name="fxOptionType">The option type. </param>
        public FxOptionAnalytic(DateTime valuationDate, DateTime paymentDate, IFxCurve indexCurve, IRateCurve currency1, IRateCurve currency2,
            bool currency2PerCurrency1, bool currency1Settlement, decimal strike, decimal expiryTime, decimal timeToIndex, 
            IVolatilitySurface indexVolSurface, FxOptionType fxOptionType, IFxCurve reportingCurrencyFxCurve)
            : base(valuationDate, paymentDate, indexCurve, currency1, currency2, currency2PerCurrency1, currency1Settlement, reportingCurrencyFxCurve)
        {
            OptionType = fxOptionType;
            Strikes = new List<decimal> {strike};
            ExpiryTimes = new List<decimal> { expiryTime };
            TimeToIndices = new List<decimal> {timeToIndex};
            Volatilities = new List<decimal> {(decimal)indexVolSurface.GetValue((double)timeToIndex, (double)strike)};
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates the expected value.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue()
        {
            decimal result = AnalyticParameters.NotionalAmount * CalculateOptionValue() * GetMultiplier();
            return result;
        }

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns>The break even rate</returns>
        protected override Decimal EvaluateBreakEvenStrike()
        {
            return CalculateOptionStrike();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDelta0()
        {
            return AnalyticParameters.NotionalAmount * CalculateOptionDelta() * GetMultiplier();// +floorDelta;               
        }

        #endregion

        #region Methods

        protected virtual Decimal CalculateOptionValue(decimal forwardRate)
        {
            var result = OptionAnalytics.Opt(GetIsCall(), (double)forwardRate, (double)GetStrike(),
                                             (double)GetVolatility(), (double)GetExpiry());
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionValue()
        {
            var result = OptionAnalytics.Opt(GetIsCall(), (double)FloatingIndex, (double)GetStrike(),
                                             (double)GetVolatility(), (double)GetExpiry());
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionValue2()
        {
            var result = OptionAnalytics.Opt(GetIsCall(), (double)FloatingIndex - 0.0001, (double)GetStrike(),
                                             (double)GetVolatility(), (double)GetExpiry());
            return (decimal)result;
        }

        protected Decimal CalculateOptionStrike()
        {
            var result = 0.0m;
            if (AnalyticParameters.Premium != null)
            {
                var premium = (decimal)AnalyticParameters.Premium;
                result = (decimal)OptionAnalytics.OptSolveStrike(GetIsCall(), (double)FloatingIndex, (double)GetVolatility(),
                                                0.0, (double)AnalyticParameters.ExpiryYearFraction, (double)premium);
            }
            return result;
        }

        protected virtual Decimal CalculateOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(GetIsCall(), (double)FloatingIndex, (double)GetStrike(),
                                                       (double)GetVolatility(), (double)GetExpiry())[1];
            return (decimal)result;
        }

        public decimal GetStrike()
        {
            return Strikes[0];
        }

        public decimal GetVolatility()
        {
            return Volatilities[0];
        }

        public decimal GetExpiry()
        {
            return ExpiryTimes[0];
        }

        #endregion
 
    }
}