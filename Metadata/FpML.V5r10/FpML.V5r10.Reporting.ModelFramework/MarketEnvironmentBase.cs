#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
  ///<summary>
    ///</summary>
    [Serializable]
    public abstract class MarketEnvironmentBase : IMarketEnvironment
    {
        /// <summary>
        /// Gets the pricing structure ids.
        /// </summary>
        /// <value>The pricing structure ids.</value>
        public IList<string> PricingStructureIds
        {
            get
            {
                return GetPricingStructureIds();
            }
        }

        protected MarketEnvironmentBase(string id)
        {
            Id = id;
        }

        public virtual IList<IMarketEnvironment> GetChildren()
        {
            return null;
        }

      ///<summary>
      /// Returns the market.
      ///</summary>
      ///<returns></returns>
      public abstract Market GetMarket();

      /// <summary>
      /// Gets the pricing structure.
      /// </summary>
      /// <param name="name">The name.</param>
      /// <returns></returns>
      public virtual IPricingStructure GetPricingStructure(string name)
        {
            IDictionary<string, IPricingStructure> ps = GetPricingStructures();
            if (ps.ContainsKey(name))
                return ps[name];
            throw new ArgumentNullException("name", string.Format("The pricing structure '{0}' does not exist in the '{1}' market", name, Id));
        }

        public virtual IDictionary<string, IPricingStructure> GetPricingStructures()
        {
            return null;
        }

        public abstract Market GetFpMLData();

        public virtual object GetProperty(string name)
        {
            return GetProperties()[name];
        }

        public virtual IDictionary<string, object> GetProperties()
        {
            return null;
        }

        public string Id { get; set; }

        /// <summary>
        /// Gets the pricing structure ids.
        /// </summary>
        /// <returns></returns>
        private string[] GetPricingStructureIds()
        {
            var items = new List<string>();
            IDictionary<string, IPricingStructure> pricingStructures = GetPricingStructures();

            if (pricingStructures != null && pricingStructures.Count > 0)
            {
                items = new List<string>(pricingStructures.Keys);
            }
            return items.ToArray();
        }
   }
}