
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
    public class SimpleRateMarketEnvironment : SimpleMarketEnvironment, ISimpleRateMarketEnvironment
    {

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        public SimpleRateMarketEnvironment()
            : base("Unidentified")
        {}

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="id"></param>
        public SimpleRateMarketEnvironment(string id):base(id)
        {}


        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="market">The market</param>
        public SimpleRateMarketEnvironment(Market market)//TODO implement a class factory conversion to IPricingStructure.
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
        public SimpleRateMarketEnvironment(Market market, NamedValueSet pricingStructureProperties)
            : base(market.id)
        {
        }

        #region Implementation of ISimpleRateMarketEnvironment

        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetRateCurve()
        {
            var curve = SearchForPricingStructureType("DiscountCurve");

            return (IRateCurve)curve;
        }

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetRateCurveProperties()
        {
            return TheProperties;
        }

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        public Pair<YieldCurve, YieldCurveValuation> GetRateCurveFpML()
        {
            return new Pair<YieldCurve, YieldCurveValuation>((YieldCurve)TheMarket.Items[0], (YieldCurveValuation)TheMarket.Items1[0]);
        }

        #endregion
    }
}