#region Using directives

using System;
using System.Collections.Generic;
using Orion.ModelFramework.MarketEnvironments;

#endregion

namespace Orion.ModelFramework
{
    ///<summary>
    ///</summary>
    public static class MarketEnvironmentRegistry//TODO change all of this.
    {
        private static Dictionary<string, IMarketEnvironment> _marketEnvironments = Initialise();

        /// <summary>
        /// Gets the market to pricing structure map.
        /// </summary>
        /// <value>The market to pricing structure map.</value>
        public static IDictionary<string, IList<string>> MarketToPricingStructureMap => BuildMarketToPricingStructureMap();

        /// <summary>
        /// Gets the markets.
        /// </summary>
        /// <value>The markets.</value>
        public static IList<string> Markets => new List<string>(_marketEnvironments.Keys);

        /// <summary>
        /// Adds the specified market environment.
        /// </summary>
        /// <param name="marketEnvironment">The market environment.</param>
        public static void Add(IMarketEnvironment marketEnvironment)
        {
            lock (_marketEnvironments)
            {
                if (_marketEnvironments.ContainsKey(marketEnvironment.Id))
                {
                    _marketEnvironments.Remove(marketEnvironment.Id);
                }
                _marketEnvironments.Add(marketEnvironment.Id, marketEnvironment);
            }
        }

        /// <summary>
        /// Builds the market to pricing structure map.
        /// </summary>
        /// <returns></returns>
        private static IDictionary<string, IList<string>> BuildMarketToPricingStructureMap()
        {
            IDictionary<string, IList<string>> items = new Dictionary<string, IList<string>>();
            if (_marketEnvironments != null)
            {
                lock (_marketEnvironments)
                {
                    foreach (var marketEnvironment in _marketEnvironments.Values)
                    {
                        var marketEnvionment = (MarketEnvironment) marketEnvironment;
                        items.Add(marketEnvionment.Id, marketEnvionment.PricingStructureIds);
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// Pricings the structure ids by market.
        /// </summary>
        /// <param name="marketEnvironmentId">The market environment id.</param>
        /// <returns></returns>
        public static IList<string> PricingStructureIdsByMarket(string marketEnvironmentId)
        {
            MarketEnvironment marketEnvionment = Get(marketEnvironmentId);
            var items = marketEnvionment != null ? marketEnvionment.PricingStructureIds : new List<string>();
            return items;
        }

        /// <summary>
        /// Removes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public static void Remove(string id)
        {
            lock (_marketEnvironments)
            {
                if (_marketEnvironments.ContainsKey(id))
                {
                    _marketEnvironments.Remove(id);
                }
                else
                {
                    throw new ArgumentNullException($"{id} market does not exist");
                }
            }
        }

        /// <summary>
        /// Gets the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MarketEnvironment Get(string id)
        {
            MarketEnvironment marketEnvironment = null;
            lock (_marketEnvironments)
            {
                if (_marketEnvironments.ContainsKey(id))
                {
                    marketEnvironment = (MarketEnvironment)_marketEnvironments[id];
                }
            }
            return marketEnvironment;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, IMarketEnvironment> Initialise()
        {
            return new Dictionary<string, IMarketEnvironment>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public static void Reset()
        {
            if (_marketEnvironments != null)
            {
                lock (_marketEnvironments)
                {
                    _marketEnvironments = Initialise();
                }
            }
        }
    }

}