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
using Orion.ModelFramework.Assets;
using Orion.CurveEngine.PricingStructures.Bootstrappers;

using FpML.V5r3.Reporting;
#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// Creates an interpolation class.
    /// </summary>
    public class PDHHelper
    {
        /// <summary>
        /// Generate a set of perturbed rate curves by just changing the market quote values of the priceable assets and
        /// thus not requiring the use of the cache or business calendars.
        /// THIS ONLY WORKS FOR MARKET QUOTES.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="perturbation"></param>
        /// <param name="priceableRateAssets"></param>
        /// <param name="extrapolationPermitted"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<TermCurve> GenerateCurvesWithPerturbedMarketQuotes(DateTime baseDate, Decimal perturbation, List<IPriceableRateAssetController> priceableRateAssets,
            bool extrapolationPermitted, InterpolationMethod interpolationMethod, double tolerance)
        {
            var termCurves = new List<TermCurve>();
            //Modify the quotes.
            if (priceableRateAssets != null)
            {
                foreach (var rateAsset in priceableRateAssets)
                {
                    //Set up the new term curve.
                    var termCurve = new TermCurve
                    {
                        extrapolationPermitted = extrapolationPermitted, interpolationMethod = interpolationMethod
                    };
                    //Perturb the asset quote.
                    var quote = rateAsset.MarketQuote.value;
                    var perturbedQuote = rateAsset.MarketQuote.value + perturbation;
                    rateAsset.MarketQuote.value = perturbedQuote;
                    termCurve.point = RateBootstrapper.Bootstrap(priceableRateAssets, baseDate, extrapolationPermitted,
                                        interpolationMethod, tolerance);
                    //Reset the old value.
                    rateAsset.MarketQuote.value = quote;
                    //Add the perturbed term curve
                    termCurves.Add(termCurve);              
                }
            }
            return termCurves;
        }

        /// <summary>
        /// Generate a set of perturbed rate curves by just changing the market quote values of the priceable assets and
        /// thus not requiring the use of the cache or business calendars.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="value"></param>
        /// <param name="assetPosition"></param>
        /// <param name="priceableRateAssets"></param>
        /// <param name="extrapolationPermitted"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static TermCurve PerturbSpecificAsset(DateTime baseDate, Decimal value, int assetPosition, List<IPriceableRateAssetController> priceableRateAssets,
            bool extrapolationPermitted, InterpolationMethod interpolationMethod, double tolerance)
        {
            //Set up the new term curve.
            var termCurve = new TermCurve
            {
                extrapolationPermitted = extrapolationPermitted, interpolationMethod = interpolationMethod
            };
            //Modify the quotes.
            if (priceableRateAssets != null && assetPosition <= priceableRateAssets.Count)
            {
                //Perturb the asset quote.
                var quote = priceableRateAssets[assetPosition].MarketQuote.value;
                var perturbedQuote = priceableRateAssets[assetPosition].MarketQuote.value + value;
                priceableRateAssets[assetPosition].MarketQuote.value = perturbedQuote;
                termCurve.point = RateBootstrapper.Bootstrap(priceableRateAssets, baseDate, extrapolationPermitted,
                                    interpolationMethod, tolerance);
                //Reset the old value.
                priceableRateAssets[assetPosition].MarketQuote.value = quote;
            }
            return termCurve;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="values"></param>
        /// <param name="priceableRateAssets"></param>
        /// <param name="extrapolationPermitted"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static TermCurve PerturbAllAssets(DateTime baseDate, Decimal[] values, List<IPriceableRateAssetController> priceableRateAssets,
            bool extrapolationPermitted, InterpolationMethod interpolationMethod, double tolerance)
        {
            //Set up the new term curve.
            var termCurve = new TermCurve
            {
                extrapolationPermitted = extrapolationPermitted, interpolationMethod = interpolationMethod
            };
            //Modify the quotes.
            if (priceableRateAssets != null)
            {
                var quotes = new List<decimal>();
                var numControllers = priceableRateAssets.Count;
                var valuesArray = new decimal[numControllers];
                if (values.Length == numControllers)
                {
                    valuesArray = values;
                }
                if (values.Length < numControllers)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        valuesArray[i] = values[i];
                    }
                    for (var i = values.Length; i < numControllers; i++)
                    {
                        valuesArray[i] = values[values.Length - 1];
                    }
                }
                else
                {
                    for (var i = 0; i < numControllers; i++)
                    {
                        valuesArray[i] = values[i];
                    }
                }
                var index = 0;
                foreach (var rateController in priceableRateAssets)
                {
                    //Perturb the asset quote.
                    var quote = rateController.MarketQuote.value;
                    quotes.Add(quote);
                    var perturbedQuote = quote + valuesArray[index];
                    rateController.MarketQuote.value = perturbedQuote;
                    index++;
                }
                termCurve.point =
                RateBootstrapper.Bootstrap(priceableRateAssets, baseDate,
                                                extrapolationPermitted,
                                                interpolationMethod, tolerance);

                index = 0;
                foreach (var rateController in priceableRateAssets)
                {
                    //Reset the asset quote.
                    rateController.MarketQuote.value = quotes[index];
                    index++;
                }
            }
            return termCurve;
        }
    }
}