#region Usings

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Constants;
using Orion.Util.Helpers;

#endregion

namespace Orion.CurveEngine.Markets
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class SwapLegEnvironment : MarketEnvironment, ISwapLegEnvironment
    {

        ///<summary>
        /// A simple market environment can only contain a maximum of 5 curves:
        /// A forecast rate curve, a discount curve, a commodity curve, 
        /// a reporting currency fx curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        public SwapLegEnvironment()
            : base("Unidentified")
        {}

        ///<summary>
        /// Gets the relevant fcommodity curve.
        ///</summary>
        ///<returns></returns>
        public ICommodityCurve GetCommodityCurve()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.CommodityIndexCurve.ToString());
            return (ICommodityCurve) curve;
        }

        ///<summary>
        /// Gets the relevant fx rate for local currency to reporting currency conversion.
        ///</summary>
        ///<returns></returns>
        public IFxCurve GetReportingCurrencyFxCurve()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.ReportingCurrencyFxCurve.ToString());
            return (IFxCurve)curve;
        }

        ///<summary>
        /// Gets the relevant fx rate for local currency to reporting currency conversion.
        ///</summary>
        ///<returns></returns>
        public IFxCurve GetReportingCurrencyFxCurve2()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.ReportingCurrencyFxCurve2.ToString());
            return (IFxCurve)curve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetForecastRateCurve()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.ForecastCurve.ToString());
            if (curve != null)
            {
                return (IRateCurve)curve;
            }
            return (IRateCurve)SearchForPricingStructureType(InterestRateStreamPSTypes.DiscountCurve.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetDiscountRateCurve()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.DiscountCurve.ToString());
            if (curve != null)
            {
                return (IRateCurve)curve;
            }
            return (IRateCurve)SearchForPricingStructureType(InterestRateStreamPSTypes.ForecastCurve.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetDiscountRateCurve2()
        {
            var curve = SearchForPricingStructureType(InterestRateStreamPSTypes.DiscountCurve2.ToString());
            if (curve != null)
            {
                return (IRateCurve)curve;
            }
            return (IRateCurve)SearchForPricingStructureType(InterestRateStreamPSTypes.ForecastCurve.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IVolatilitySurface GetVolatilitySurface()
        {
            return (IVolatilitySurface)SearchForPricingStructureType(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString());
        }

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        public Pair<FxCurve, FxCurveValuation> GetCommodityCurveFpML()
        {
            var curve = GetCommodityCurve().GetFpMLData();
            var result = new Pair<FxCurve, FxCurveValuation>((FxCurve)curve.First, (FxCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        public Pair<FxCurve, FxCurveValuation> GetReportingCurrencyFxCurveFpML()
        {
            var curve = GetReportingCurrencyFxCurve().GetFpMLData();
            var result = new Pair<FxCurve, FxCurveValuation>((FxCurve)curve.First, (FxCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the reporting currency fx curve.
        /// </summary>
        /// <returns></returns>
        public Pair<FxCurve, FxCurveValuation> GetReportingCurrencyFxCurve2FpML()
        {
            var curve = GetReportingCurrencyFxCurve2().GetFpMLData();
            var result = new Pair<FxCurve, FxCurveValuation>((FxCurve)curve.First, (FxCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the forecast rate curve.
        /// </summary>
        /// <returns></returns>
        public Pair<YieldCurve, YieldCurveValuation> GetForecastRateCurveFpML()
        {
            var curve = GetForecastRateCurve().GetFpMLData();
            var result = new Pair<YieldCurve, YieldCurveValuation>((YieldCurve)curve.First, (YieldCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        public Pair<YieldCurve, YieldCurveValuation> GetDiscountRateCurveFpML()
        {
            var curve = GetDiscountRateCurve().GetFpMLData();
            var result = new Pair<YieldCurve, YieldCurveValuation>((YieldCurve)curve.First, (YieldCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        public Pair<YieldCurve, YieldCurveValuation> GetDiscountRateCurve2FpML()
        {
            var curve = GetDiscountRateCurve2().GetFpMLData();
            var result = new Pair<YieldCurve, YieldCurveValuation>((YieldCurve)curve.First, (YieldCurveValuation)curve.Second);
            return result;
        }

        /// <summary>
        /// Gets the volatility surface. THis may need to be extended to cubes.
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetVolatilitySurfaceFpML()
        {
            var curve = GetDiscountRateCurve2().GetFpMLData();
            var result = new Pair<PricingStructure, PricingStructureValuation>(curve.First, curve.Second);
            return result;
        }


    }
}