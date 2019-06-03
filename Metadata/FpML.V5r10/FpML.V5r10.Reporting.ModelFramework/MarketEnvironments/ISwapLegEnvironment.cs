
using Orion.Util.Helpers;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

namespace FpML.V5r10.Reporting.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface ISwapLegEnvironment : IMarketEnvironment
    {
        ///<summary>
        /// Gets the relevant fcommodity curve.
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