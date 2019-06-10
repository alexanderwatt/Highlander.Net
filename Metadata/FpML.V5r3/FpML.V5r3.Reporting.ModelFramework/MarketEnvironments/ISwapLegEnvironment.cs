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

using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.PricingStructures;

namespace Orion.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface ISwapLegEnvironment : IMarketEnvironment
    {
        ///<summary>
        /// Gets the relevant commodity curve.
        ///</summary>
        ///<returns></returns>
        ICommodityCurve GetCommodityCurve();

        ///<summary>
        /// Gets the relevant fx rate for local currency to reporting currency conversion.
        ///</summary>
        ///<returns></returns>
        IFxCurve GetReportingCurrencyFxCurve();

        /// <summary>
        /// Gets the forecast rate curve.
        /// </summary>
        /// <returns></returns>
        IRateCurve GetForecastRateCurve();

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        IRateCurve GetDiscountRateCurve();

        ///<summary>
        /// Gets the relevant fx rate for local currency to reporting currency conversion.
        ///</summary>
        ///<returns></returns>
        IFxCurve GetReportingCurrencyFxCurve2();

        ///<summary>
        /// Gets the relevant discount curve.
        ///</summary>
        ///<returns></returns>
        IRateCurve GetDiscountRateCurve2();


        /// <summary>
        /// Gets the volatility surface. THis may need to be extended to cubes.
        /// </summary>
        /// <returns></returns>
        IVolatilitySurface GetVolatilitySurface();

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        Pair<FxCurve, FxCurveValuation> GetCommodityCurveFpML();

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        Pair<FxCurve, FxCurveValuation> GetReportingCurrencyFxCurveFpML();

        /// <summary>
        /// Gets the forecast rate curve.
        /// </summary>
        /// <returns></returns>
        Pair<YieldCurve, YieldCurveValuation> GetForecastRateCurveFpML();

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        Pair<YieldCurve, YieldCurveValuation> GetDiscountRateCurveFpML();

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        Pair<FxCurve, FxCurveValuation> GetReportingCurrencyFxCurve2FpML();

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        Pair<YieldCurve, YieldCurveValuation> GetDiscountRateCurve2FpML();

        /// <summary>
        /// Gets the volatility surface. THis may need to be extended to cubes.
        /// </summary>
        /// <returns></returns>
        Pair<PricingStructure, PricingStructureValuation> GetVolatilitySurfaceFpML();
    }
}