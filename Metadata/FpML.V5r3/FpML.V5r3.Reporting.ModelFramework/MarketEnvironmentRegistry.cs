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
        public static IList<string> Markets
        {
            get
            {
                if (_marketEnvironments != null)
                    lock (_marketEnvironments)
                    {
                        return new List<string>(_marketEnvironments.Keys);
                    }
                return null;
            }
        }

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
                        var market = (MarketEnvironment) marketEnvironment;
                        items.Add(market.Id, market.PricingStructureIds);
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// Pricing the structure ids by market.
        /// </summary>
        /// <param name="marketEnvironmentId">The market environment id.</param>
        /// <returns></returns>
        public static IList<string> PricingStructureIdsByMarket(string marketEnvironmentId)
        {
            MarketEnvironment marketEnvironment = Get(marketEnvironmentId);
            var items = marketEnvironment != null ? marketEnvironment.PricingStructureIds : new List<string>();
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