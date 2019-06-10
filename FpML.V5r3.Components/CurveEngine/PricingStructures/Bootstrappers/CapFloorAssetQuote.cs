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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Assets.Rates.CapsFloors;
using Orion.ModelFramework.PricingStructures;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class CapFloorAssetQuote : IObjectiveFunction
    {
        private PriceableCapRateAsset PriceableAsset { get; }
        protected DateTime BaseDate { get; set; }
        protected IDictionary<DateTime, Decimal> Vols { get; set; }
        protected InterpolationMethod InterpolationMethod { get; set; }
        protected bool Extrapolation { get; set; }
        protected double Tolerance { get; set; }
        private const double DefaultTolerance = 0.0000001d;
        public decimal Quote { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapFloorAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="vols">The volatilities.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        public CapFloorAssetQuote(PriceableCapRateAsset priceableAsset,
            IRateCurve discountCurve, IRateCurve forecastCurve, DateTime baseDate, 
            InterpolationMethod interpolation, bool extrapolation, IDictionary<DateTime, Decimal> vols)
            : this(priceableAsset, discountCurve, forecastCurve, baseDate, interpolation, extrapolation, vols, DefaultTolerance)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CapFloorAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="vols">The volatilities.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="discountCurve">The discount rate curve.</param> 
        /// <param name="forecastCurve">The forecast rate curve.</param> 
        public CapFloorAssetQuote(PriceableCapRateAsset priceableAsset, IRateCurve discountCurve, IRateCurve forecastCurve, 
            DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, IDictionary<DateTime, Decimal> vols, double tolerance)
        {
            PriceableAsset = priceableAsset;
            PriceableAsset.DiscountCurve = discountCurve;
            PriceableAsset.ForecastCurve = forecastCurve;
            BaseDate = baseDate;
            Vols = vols;
            InterpolationMethod = interpolation;
            Extrapolation = extrapolation;
            Tolerance = tolerance;
            //This is the flat volatility.
            var premium = PriceableAsset.CalculatePremium();
            if (premium != null)
            {
                Quote = (decimal)premium;
            }
        }

        /// <summary>
        /// Values the specified discount factor.
        /// </summary>
        /// <param name="volatility">volatility.</param>
        /// <returns></returns>
        public double Value(double volatility)
        {
            Vols[Vols.Keys.Last()] = (Decimal)volatility;
            return CalculateQuoteError();
        }

        ///<summary>
        /// This will need to be implemented if used in certain solvers.
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double Derivative(double x)
        {
            return 0.0;
        }

        /// <summary>
        /// An inital test to see if the guess was good..
        /// </summary>
        /// <returns></returns>
        public Boolean InitialValue()
        {
            double quoteError = CalculateQuoteError();
            return Math.Abs(quoteError) < Tolerance;
        }

        public virtual double CalculateQuoteError()
        {
            var curve = new SimpleCapletCurve(BaseDate, InterpolationMethod, Extrapolation, Vols);
            var impliedQuote = PriceableAsset.CalculateImpliedQuote(curve);
            return (double)(Quote - impliedQuote);
        }
    }
}