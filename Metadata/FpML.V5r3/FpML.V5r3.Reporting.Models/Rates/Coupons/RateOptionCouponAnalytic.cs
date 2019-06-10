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
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Rates.Coupons
{
    public class RateOptionCouponAnalytic : FloatingRateCouponAnalytic
    {
        #region Properties

        /// <summary>
        /// Gets or sets the isCollar flag.
        /// </summary>
        /// <value>The isCollar flag.</value>
        public Boolean IsCollar { get; protected set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public Decimal Strike { get; protected set; }

        /// <summary>
        /// Gets or sets the collar second strike.
        /// </summary>
        /// <value>The collar second strike.</value>
        public Decimal CollarFloorStrike { get; protected set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal Volatility { get; protected set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal CollarFloorVolatility { get; protected set; }

        /// <summary>
        /// Gets or sets the Time To Index.
        /// </summary>
        /// <value>The Time To Index.</value>
        public Decimal TimeToIndex { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The params had better be there!
        /// </summary>
        public RateOptionCouponAnalytic()
        { }

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="timeToIndex">The timeToIndex. Not necessarily the time to expiry. This is used for surface interpolation..</param>
        /// <param name="strike">The strike.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <param name="indexVolSurface">The index volatility surface.</param>
        public RateOptionCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate, 
            decimal timeToIndex, decimal strike, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve, IVolatilitySurface indexVolSurface)
            : base(valuationDate, startDate, endDate, paymentDate, reportingCurrencyFxCurve, discountCurve, forecastCurve)
        {
            Strike = strike;
            TimeToIndex = timeToIndex;
            Volatility = (decimal)indexVolSurface.GetValue((double)timeToIndex, (double)strike);
        }

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="startDate">The start date of the coupon.</param>
        /// <param name="endDate">The end date of the coupon.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="timeToIndex">The timeToIndex. Not necessarily the time to expiry. This is used for surface interpolation..</param>
        /// <param name="capStrike">The cap strike.</param>
        /// <param name="floorStrike">The floor strike.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <param name="indexVolSurface">The index volatility surface.</param>
        public RateOptionCouponAnalytic(DateTime valuationDate, DateTime startDate, DateTime endDate, DateTime paymentDate,
            decimal timeToIndex, decimal capStrike, decimal floorStrike, IFxCurve reportingCurrencyFxCurve, IRateCurve discountCurve, IRateCurve forecastCurve, IVolatilitySurface indexVolSurface)
            : this(valuationDate, startDate, endDate, paymentDate, timeToIndex, capStrike, reportingCurrencyFxCurve, discountCurve, forecastCurve, indexVolSurface)
        {
            IsCollar = true;
            CollarFloorStrike = floorStrike;
            CollarFloorVolatility = (decimal)indexVolSurface.GetValue((double)timeToIndex, (double)floorStrike);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDelta1()
        {
            var temp = AnalyticParameters.PeriodAsTimesPerYear * AnalyticParameters.Rate;
            return EvaluateNPV() * -AnalyticParameters.CurveYearFraction / (1 + temp) / 10000;
        }

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns>The break even rate</returns>
        protected virtual Decimal EvaluateBreakEvenStrike()
        {
            return CalculateOptionStrike();
        }

        /// <summary>
        /// Evaluates the expected value2.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue2()//TODO This is not correct yet!
        {
            decimal result;
            var floor = 0.0m;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateCollarOptionValue2() * GetMultiplier();
                }
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionValue2() * GetMultiplier() + floor;               
            }
            else
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier()
                    / (COne + CalculateCollarOptionValue2() * AnalyticParameters.YearFraction);
                }
                result = AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier()
                    / (COne + CalculateOptionValue2() * AnalyticParameters.YearFraction) + floor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue()
        {
            decimal result;
            var floor = 0.0m;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateCollarOptionValue() * GetMultiplier();
                }
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionValue() * GetMultiplier() + floor;
            }
            else
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier()
                    / (COne + CalculateCollarOptionValue() * AnalyticParameters.YearFraction);
                }
                result = AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier() 
                    / (COne + CalculateOptionValue() * AnalyticParameters.YearFraction) + floor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateExpectedValue(decimal breakEvenRate)
        {
            decimal result;
            var floor = 0.0m;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateCollarOptionValue(breakEvenRate) * GetMultiplier();
                }
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionValue(breakEvenRate) * GetMultiplier() + floor;
            }
            else
            {
                if (IsCollar)
                {
                    floor = -AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier()
                    / (COne + CalculateCollarOptionValue(breakEvenRate) * AnalyticParameters.YearFraction);
                }
                result = AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier()
                    / (COne + CalculateOptionValue(breakEvenRate) * AnalyticParameters.YearFraction) + floor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDelta0()
        {
            decimal result;
            var floorDelta = 0.0m;
            if (AnalyticParameters.DiscountType == DiscountType.None)
            {
                if (IsCollar)
                {
                    floorDelta = -AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateCollarOptionDelta() * GetMultiplier();
                }
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionDelta() * GetMultiplier() + floorDelta;               
            }
            else
            {
                if (IsCollar)
                {
                    floorDelta = -AnalyticParameters.NotionalAmount * GetMultiplier() + AnalyticParameters.NotionalAmount * GetMultiplier() / (COne + CalculateCollarOptionDelta() * AnalyticParameters.YearFraction);
                }
                result = AnalyticParameters.NotionalAmount * GetMultiplier() - AnalyticParameters.NotionalAmount * GetMultiplier() / (COne + CalculateOptionDelta() * AnalyticParameters.YearFraction) + floorDelta;
            }
            return result;
        }

        #endregion

        #region Methods

        protected virtual Decimal CalculateCollarOptionValue(decimal forwardRate)
        {
            var result = OptionAnalytics.Opt(!AnalyticParameters.IsCall, (double)forwardRate + (double)AnalyticParameters.Spread, (double)CollarFloorStrike,
                                             (double)CollarFloorVolatility, (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionValue(decimal forwardRate)
        {
            var result = OptionAnalytics.Opt(AnalyticParameters.IsCall, (double)forwardRate + (double)AnalyticParameters.Spread, (double)GetStrike(),
                                             (double)GetVolatility(), (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionValue()
        {
            var result = OptionAnalytics.Opt(AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread, (double)GetStrike(),
                                             (double)GetVolatility(), (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionValue2()
        {
            var result = OptionAnalytics.Opt(AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread - 0.0001, (double)GetStrike(),
                                             (double)GetVolatility(), (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateCollarOptionValue()
        {
            var result = OptionAnalytics.Opt(!AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread, (double)CollarFloorStrike,
                                             (double)CollarFloorVolatility, (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateCollarOptionValue2()
        {
            var result = OptionAnalytics.Opt(!AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread - 0.0001, (double)CollarFloorStrike,
                                             (double)CollarFloorVolatility, (double)AnalyticParameters.ExpiryYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateCollarOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread, (double)CollarFloorStrike,
                                                       (double)CollarFloorVolatility, (double)AnalyticParameters.ExpiryYearFraction)[1];
            return (decimal)result;
        }

        protected Decimal CalculateOptionStrike()
        {
            var result = 0.0m;
            if (AnalyticParameters.SwapAccrualFactor != 0.0m && AnalyticParameters.Premium != 0.0m)
            {
                var premium = AnalyticParameters.Premium / Math.Abs(AnalyticParameters.SwapAccrualFactor);
                result = (decimal)OptionAnalytics.OptSolveStrike(AnalyticParameters.IsCall, (double)EvaluateBreakEvenRate(), (double)Volatility,
                                                0.0, (double)AnalyticParameters.ExpiryYearFraction, (double)premium);
            }
            return result;
        }

        protected virtual Decimal CalculateOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(AnalyticParameters.IsCall, (double)AnalyticParameters.Rate + (double)AnalyticParameters.Spread, (double)GetStrike(),
                                                       (double)GetVolatility(), (double)AnalyticParameters.ExpiryYearFraction)[1];
            return (decimal)result;
        }

        public decimal GetStrike()
        {
            var strike = AnalyticParameters.Strike;
            if (strike != null)
            {
                return (decimal)strike;
            }
            return Strike;
        }

        public decimal GetVolatility()
        {
            var volatility = AnalyticParameters.Volatility;
            if (volatility != null)
            {
                return (decimal)volatility;
            }
            return Volatility;
        }

        #endregion
    }
}