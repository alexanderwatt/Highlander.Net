
using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

namespace Orion.CurveEngine.Markets
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class SimpleCommodityMarketEnvironment : SimpleMarketEnvironment, ISimpleCommodityMarketEnvironment
    {

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        public SimpleCommodityMarketEnvironment()
            : base("Unidentified")
        {}

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="id"></param>
        public SimpleCommodityMarketEnvironment(string id):base(id)
        {}


        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="market">The market</param>
        public SimpleCommodityMarketEnvironment(Market market)//TODO implement a class factory conversion to IPricingStructure.
            : base(market.id)
        {
        }


        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="market">The market. This market only contains one rate curves.</param>
        ///<param name="pricingStructureProperties">The properties order for each pricing structure.</param>
        public SimpleCommodityMarketEnvironment(Market market, NamedValueSet pricingStructureProperties)
            : base(market.id)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetForecastRateCurve()
        {
            var curve = SearchForPricingStructureType("ForecastCurve");

            if (curve != null)
            {
                return (IRateCurve)curve;
            }
            return (IRateCurve)SearchForPricingStructureType("DiscountCurve");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetDiscountRateCurve()
        {
            var curve = SearchForPricingStructureType("DiscountCurve");

            if (curve != null)
            {
                return (IRateCurve)curve;
            }
            return (IRateCurve)SearchForPricingStructureType("ForecastCurve");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IVolatilitySurface GetVolatilitySurface()
        {
            return (IVolatilitySurface)SearchForPricingStructureType("VolatilitySurface");
        }

        /// <summary>
        /// Gets the forecast rate curve.
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetForecastRateCurveFpML()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the discount rate curve.
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetDiscountRateCurveFpML()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the volatility surface. THis may need to be extended to cubes.
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetVolatilitySurfaceFpML()
        {
            throw new System.NotImplementedException();
        }

        #region Implementation of ISimpleCommodityMarketEnvironment

        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <returns></returns>
        public ICommodityCurve GetCommodityCurve()
        {
            return (ICommodityCurve)PricingStructures[PricingStructureIdentifier];
        }

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetCommodityCurveProperties()
        {
            return TheProperties;
        }

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        public Pair<FxCurve, FxCurveValuation> GetCommodityCurveFpML()
        {
            return new Pair<FxCurve, FxCurveValuation>((FxCurve)TheMarket.Items[0], (FxCurveValuation)TheMarket.Items1[0]);
        }

        #endregion
    }
}