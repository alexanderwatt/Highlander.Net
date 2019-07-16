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

#endregion

namespace FpML.V5r3.Reporting
{
    public partial class Market
    {
        /// <summary>
        /// Gets and sets the main QuotedAssetSet.
        /// </summary>
        public QuotedAssetSet GetQuotedAssetSet() 
        {
            // since this actually is not used in Highlander, we need to extract the correct QAS.
            if (benchmarkQuotes != null)
            {
                return benchmarkQuotes;
            }
            if (Items1 == null) return null;
            var psv = Items1[0];
            if (psv is YieldCurveValuation ycv)
            {
                return ycv.inputs;
            }
            if (psv is FxCurveValuation fxv)
            {
                return fxv.spotRate;
            }
            if (psv is CreditCurveValuation crv)
            {
                return crv.inputs;
            }
            var volMatrix = psv as VolatilityMatrix;
            return volMatrix?.inputs;
        }

        /// <summary>
        /// Returns a term curve.
        /// This means the discount factor curve for a YieldCurve,
        /// an fx forward curve for an Fx curve,
        /// a recovery rate curve for a credit curve. 
        /// A volatility matrix is not handled.
        /// </summary>
        /// <returns></returns>
        public TermCurve GetTermCurve()
        {
            // we need to extract the curve.
            if (Items1 == null) return null;
            var psv = Items1[0];
            var ycv = psv as YieldCurveValuation;
            if (ycv?.zeroCurve != null)
            {
                return ycv.zeroCurve.rateCurve ?? ycv.discountFactorCurve;
            }
            if (psv is FxCurveValuation fxv)
            {
                return fxv.fxForwardCurve;
            }
            if (psv is CreditCurveValuation crv)
            {
                return crv.recoveryRateCurve;
            }
            //This only works for volatility curves.
            if (psv is VolatilityMatrix vm)
            {
                var points = vm.dataPoints?.point;
                var termPoints = new List<TermPoint>();
                if (points != null && vm.baseDate?.Value is DateTime baseDate)
                {
                    foreach (var point in points)
                    {
                        var tenor = point.coordinate[0]?.expiration[0];
                        var period = tenor?.Items[0];
                        if (period is Period numDaysPeriod)
                        {
                            var newDate = baseDate.AddDays(Convert.ToInt16(numDaysPeriod.periodMultiplier));
                            var time = new TimeDimension {Items = new object[1]};
                            time.Items[0] = newDate;
                            var termPoint = new TermPoint
                            {
                                term = time,
                                mid = point.value,
                                midSpecified = true
                            };
                            termPoints.Add(termPoint);
                        }
                    }
                }
                var termCurve = new TermCurve {point = termPoints.ToArray()};
                return termCurve;
            }
            return null;
        }

        /// <summary>
        /// Returns a term curve.
        /// This means the discount factor curve for a YieldCurve,
        /// an fx forward curve for an Fx curve,
        /// a recovery rate curve for a credit curve. 
        /// A volatility matrix is not handled.
        /// </summary>
        /// <returns></returns>
        public ZeroRateCurve GetZeroCurve()
        {
            // we need to extract the curve.
            if (Items1 == null) return null;
            var psv = Items1[0];
            var ycv = psv as YieldCurveValuation;
            return ycv?.zeroCurve;
        }

        /// <summary>
        /// Returns a multi dimensional surface.
        /// This means a volatility matrix 
        /// </summary>
        /// <returns></returns>
        public MultiDimensionalPricingData GetMultiCurve()
        {
            // we need to extract the curve.
            if (Items1 == null) return null;
            var psv = Items1[0];
            var volMatrix = psv as VolatilityMatrix;
            return volMatrix?.dataPoints;
        }
    }
}
