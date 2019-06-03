#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.MarketEnvironments
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class MarketEnvironment : MarketEnvironmentBase
    {
        public decimal Perturbation { get; set; }

        ///<summary>
        /// The simple market containing only one pricing structure and pricingstructurevaluation.
        ///</summary>
        public Market TheMarket { get; set; }


        ///<summary>
        /// The properties.
        ///</summary>
        public Boolean NotYetConverted { get; set; }

        ///<summary>
        ///</summary>
        public MarketEnvironment()
            : base("Unidentified")
        {
            Perturbation = 10;
        }

        ///<summary>
        ///</summary>
        ///<param name="id"></param>
        public MarketEnvironment(string id):base(id)
        {
            Perturbation = 10;
        }

        ///<summary>
        ///</summary>
        ///<param name="name"></param>
        ///<param name="pricingStructure"></param>
        public void AddPricingStructure(string name, IPricingStructure pricingStructure)
        {
            if (pricingStructure != null)
            {
                if (PricingStructures.ContainsKey(name))
                {
                    PricingStructures.Remove(name);
                }
                PricingStructures.Add(name, pricingStructure);
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="name"></param>
        public void RemovePricingStructure(string name)
        {
            if (PricingStructures.ContainsKey(name))
            {
                PricingStructures.Remove(name);
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="pricingStructureName"></param>
        ///<param name="riskName"> </param>
        ///<param name="perturbation"></param>
        private IList<IPricingStructure> GenerateRiskMarket(string pricingStructureName, string riskName, decimal perturbation)
        {
            var riskCurves = new List<IPricingStructure>();
            var temp = SearchForPricingStructureType(pricingStructureName) as IRateCurve;
            if (temp != null)
            {
                //build the riskMarket
                riskCurves = temp.CreateCurveRiskSet(perturbation);
                var market = new MarketEnvironment { Id = pricingStructureName + '.' + riskName };
                foreach (var rateCurve in riskCurves)
                {
                    var id = rateCurve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset", true);
                    market.AddPricingStructure( id, rateCurve);
                }
                RiskMarkets.Add(pricingStructureName + '.' + riskName, market);
            }
            return riskCurves;
        }

        ///<summary>
        ///</summary>
        ///<param name="name"></param>
        public void RemoveMarket(string name)
        {
            if (RiskMarkets.ContainsKey(name))
            {
                RiskMarkets.Remove(name);
            }
        }

        ///<summary>
        /// Returns the market.
        ///</summary>
        ///<returns></returns>
        public override Market GetMarket()
        {
            var market = GetFpMLData();
            return market;
        }

        public override IDictionary<string, IPricingStructure> GetPricingStructures()
        {
            return PricingStructures;
        }

        protected readonly IDictionary<string, IPricingStructure> PricingStructures = new Dictionary<string, IPricingStructure>();

        protected readonly IDictionary<string, IMarketEnvironment> RiskMarkets = new Dictionary<string, IMarketEnvironment>();

        public override Market GetFpMLData()
        {
            var market = new Market { id = Id };
            var ps = new List<PricingStructure>();
            var psv = new List<PricingStructureValuation>();
            foreach (var element in PricingStructures)
            {
                var curve = element.Value.GetFpMLData();
                ps.Add(curve.First);
                psv.Add(curve.Second);
            }
            market.Items = ps.ToArray();
            market.Items1 = psv.ToArray();
            return market;
        }

        public override IDictionary<string, object> GetProperties()
        {
            IDictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var pricingStructure in PricingStructures)
            {
                var id = pricingStructure.Value.GetPricingStructureId().Id;
                var prop = pricingStructure.Value.GetPricingStructureId().Properties;
                properties.Add(id, prop);
            }
            return properties;
        }

        public override IList<IMarketEnvironment> GetChildren()
        {
            return (IList<IMarketEnvironment>) RiskMarkets.Values;
        }

        ///<summary>
        /// Gets the properties linked to the spefied pricing structure.
        ///</summary>
        ///<param name="identifier"></param>
        ///<returns></returns>
        public override object GetProperty(string identifier)
        {
            var result = new object();
            if(PricingStructures.ContainsKey(identifier))
            {
                result = PricingStructures[identifier];
            }
            return result;
        }

        ///<summary>
        /// Finds the request pricing structure.
        ///</summary>
        ///<param name="name"></param>
        ///<returns></returns>
        public IPricingStructure SearchForPricingStructureType(string name)
        {
            IPricingStructure pricingStructure;
            if(!PricingStructures.TryGetValue(name, out pricingStructure))
            {
                throw new ApplicationException(String.Format("The pricing structure with name '{0}' was not found.", name));
            }
            return pricingStructure;
        }

        ///<summary>
        /// Finds the request pricing structure.
        ///</summary>
        ///<param name="pricingStructureName"></param>
        ///<param name="riskName"> </param>
        ///<returns></returns>
        public ICollection<IPricingStructure> SearchForPerturbedPricingStructures(string pricingStructureName, string riskName)
        {
            IMarketEnvironment marketEnvironment;
            var marketExist = RiskMarkets.TryGetValue(pricingStructureName + '.' + riskName, out marketEnvironment);
            if (!marketExist)
            {
                return GenerateRiskMarket(pricingStructureName, riskName, Perturbation);
            }
            var pricingStructures = marketEnvironment.GetPricingStructures().Values;
            return pricingStructures;
        }
    }
}