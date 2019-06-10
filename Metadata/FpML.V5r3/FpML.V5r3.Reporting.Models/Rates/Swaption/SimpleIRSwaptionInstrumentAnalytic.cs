/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using Orion.Analytics.Options;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Rates.Swaption
{
    public class SimpleIRSwaptionInstrumentAnalytic : ModelAnalyticBase<ISwaptionInstrumentParameters, SwaptionInstrumentMetrics>, ISwaptionInstrumentResults
    {
        protected const Decimal COne = 1.0m;
        protected const Decimal BasisPoint = 10000.0m;

        #region Properties

        /// <summary>
        /// Gets or sets the analytical delta.
        /// </summary>
        /// <value>The analytical delta.</value>
        public Decimal? AnalyticalDelta { get; protected set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public Decimal? Premium { get; protected set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public Decimal Strike { get; protected set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public Decimal Volatility { get; protected set; }

        /// <summary>
        /// Gets or sets the Time To Index.
        /// </summary>
        /// <value>The Time To Index.</value>
        public Decimal TimeToIndex { get; protected set; }

        /// <summary>
        /// Gets or sets the valuation fx rate from the payment currency to the reporting currency.
        /// </summary>
        /// <value>The valuation fx rate from the payment currency to the reporting currency.</value>
        public Decimal ToReportingCurrencyRate { get; protected set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote => EvaluateOptionPremium();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => AnalyticParameters.MarketQuote;

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE => new decimal[] { };

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public Decimal[] PCETerm => new Decimal[] { };

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public Decimal NPV
        {
            get 
            {
                var result = AnalyticParameters.OtherNPV; 
                if (AnalyticParameters.IsBought) return result + Math.Abs(AnalyticParameters.SwapAccrualFactor) * EvaluateOptionPremium() * 10000;
                return result + Math.Abs(AnalyticParameters.SwapAccrualFactor) * EvaluateOptionPremium() * -10000; 
            }
        }

        /// <summary>
        /// Gets the BES.
        /// </summary>
        /// <value>The BES.</value>
        public decimal BreakEvenStrike => EvaluateOptionStrike();

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        public Decimal Delta
        {
            get
            {
                if (AnalyticParameters.IsBought) return EvaluateOptionDelta();
                return -EvaluateOptionDelta();
            }
        }

        /// <summary>
        /// Gets the strike delta.
        /// </summary>
        /// <value>The strike delta.</value>
        public Decimal StrikeDelta
        {
            get
            {
                if (AnalyticParameters.IsBought) return EvaluateStrikeDelta();
                return -EvaluateStrikeDelta();
            }
        }

        /// <summary>
        /// Gets the fixed leg accrual factor of a fixed/float swap.
        /// </summary>
        /// <value>The fixed leg accrual factor of a fixed/float swap.</value>
        public Decimal FixedLegAccrualFactor => AnalyticParameters.SwapAccrualFactor;

        #endregion

        #region Constructor

        public SimpleIRSwaptionInstrumentAnalytic()
        {
            ToReportingCurrencyRate = 1.0m;
        }

        ///// <param name="isCall">The isCall flag. If [true] then the the option is a call.</param>
        ///// <param name="rate">The rate.</param>
        ///// <param name="timeToExpiry">The time To Expiry.</param>
        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="timeToIndex">The timeToIndex. Not necessarily the time to expiry. This is used for surface interpolation..</param>
        /// <param name="strike">The strike.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="indexVolSurface">THe volatility surface to use. </param>
        ///// <param name="discountCurve">The rate curve to use for discounting.</param>
        public SimpleIRSwaptionInstrumentAnalytic(DateTime valuationDate, //DateTime paymentDate, bool isCall, 
            decimal timeToIndex, decimal strike, 
            //decimal timeToExpiry, decimal rate, 
            IFxCurve reportingCurrencyFxCurve, IVolatilitySurface indexVolSurface) //IRateCurve discountCurve, /// <param name="paymentDate">The payment date of the cash flow.</param>
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            TimeToIndex = timeToIndex;
            Strike = strike;
            Volatility = (decimal)indexVolSurface.GetValue((double)timeToIndex, (double)strike);
        }

        #endregion

        #region Methods

        protected Decimal EvaluateOptionPremium()
        {
            return CalculateOptionValue();
        }

        protected Decimal EvaluateOptionStrike()
        {
            return CalculateOptionStrike();
        }

        protected Decimal EvaluateOptionDelta()
        {
            return CalculateOptionDelta();
        }

        protected Decimal EvaluateStrikeDelta()
        {
            return CalculateStrikeDelta();
        }

        /// <summary>
        /// Evaluating the fx rate.
        /// </summary>
        /// <returns>The fx rate</returns>
        protected decimal EvaluateReportingCurrencyFxRate(DateTime valuationDate, IFxCurve fxCurve)
        {
            var result = 1.0m;
            if (fxCurve != null)
            {
                result = (decimal)fxCurve.GetForward(valuationDate, valuationDate);
            }
            return result;
        }

        protected Decimal CalculateOptionValue()
        {
            var result = OptionAnalytics.Opt(AnalyticParameters.IsCall, (double)AnalyticParameters.SwapBreakEvenRate, (double)AnalyticParameters.Strike,
                                             (double)Volatility, (double)AnalyticParameters.TimeToExpiry);
            return (decimal)result;
        }

        protected Decimal CalculateOptionStrike()
        {
            //if (AnalyticParameters.OtherNPV != null)
            //{
                var premium = Math.Abs(AnalyticParameters.OtherNPV) / Math.Abs(AnalyticParameters.SwapAccrualFactor)/ 10000;
                var result = (decimal)OptionAnalytics.OptSolveStrike(AnalyticParameters.IsCall, (double)AnalyticParameters.SwapBreakEvenRate, (double)Volatility,
                                                                         0.0, (double)AnalyticParameters.TimeToExpiry, (double)premium);
            //}
            return result;
        }

        protected Decimal CalculateOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(AnalyticParameters.IsCall, (double)AnalyticParameters.SwapBreakEvenRate, (double)AnalyticParameters.Strike,
                                                       (double)Volatility, (double)AnalyticParameters.TimeToExpiry)[1];
            return (decimal)result;
        }

        protected Decimal CalculateStrikeDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(AnalyticParameters.IsCall, (double)AnalyticParameters.SwapBreakEvenRate, (double)AnalyticParameters.Strike,
                                                       (double)Volatility, (double)AnalyticParameters.TimeToExpiry)[6];
            return (decimal)result;
        }

        public static Decimal CalculateOptionDelta(bool isCall, decimal swapBreakEvenRate, decimal strike, double timeToExpiry, double timeToIndex, IVolatilitySurface indexVolSurface)
        {
            var volatility = (decimal)indexVolSurface.GetValue(timeToIndex, (double)strike);
            var result = OptionAnalytics.OptWithGreeks(isCall, (double)swapBreakEvenRate, (double)strike, (double)volatility, timeToExpiry)[1];
            return (decimal)result;
        }

        public static Decimal CalculateStrikeDelta(bool isCall, decimal swapBreakEvenRate, decimal strike, double timeToExpiry, double timeToIndex, IVolatilitySurface indexVolSurface)
        {
            var volatility = (decimal)indexVolSurface.GetValue(timeToIndex, (double)strike);
            var result = OptionAnalytics.OptWithGreeks(isCall, (double)swapBreakEvenRate, (double)strike, (double)volatility, timeToExpiry)[6];
            return (decimal)result;
        }

        public decimal GetOptionDelta()
        {
            if (AnalyticalDelta != null)
            {
                return (decimal)AnalyticalDelta;
            }
            AnalyticalDelta = CalculateOptionDelta();
            return (decimal)AnalyticalDelta;
        }

        #endregion
    }
}