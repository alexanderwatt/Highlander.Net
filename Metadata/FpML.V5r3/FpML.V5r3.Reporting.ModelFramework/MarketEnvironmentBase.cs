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
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework
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
        public IList<string> PricingStructureIds => GetPricingStructureIds();

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
            throw new ArgumentNullException(nameof(name),
                $"The pricing structure '{name}' does not exist in the '{Id}' market");
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