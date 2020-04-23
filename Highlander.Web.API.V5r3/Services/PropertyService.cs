﻿using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Models;
using System.Collections.Generic;

namespace Highlander.Web.API.V5r3.Services
{
    public class PropertyService
    {
        private readonly PricingCache cache;
        private readonly Reference<ILogger> logger;

        public PropertyService(PricingCache cache, Reference<ILogger> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public IEnumerable<string> GetPropertyTradeIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, cache.NameSpace);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = cache.QueryTradeIds(properties);
            logger.Target.LogInfo("Queried property trade ids.");
            return trades;
        }

        public string CreatePropertyTrade(PropertyTradeViewModel model)
        {
            var properties = new NamedValueSet();
            //TODO
            //Add the create property to this!!
            //This way the property transaction isw closer to the Lendhaus model.
            var result = cache.CreatePropertyTradeWithProperties(model.TradeId, true, model.Purchaser, model.Seller, model.TradeTimeUtc, model.EffectiveTimeUtc,
                model.PurchaseAmount, model.PaymentTimeUtc, model.PropertyType, model.Currency, model.PropertyId, model.TradingBook, properties);
            logger.Target.LogInfo("Created property trade id: {0}", result);
            return result;
        }

        public IEnumerable<string> GetPropertyAssetIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = cache.QueryPropertyAssetIds(properties);
            logger.Target.LogInfo("Queried property assets.");
            return trades;
        }

        public string CreatePropertyAsset(PropertyAssetViewModel model)
        {
            var result = cache.CreatePropertyAsset(model.PropertyId, model.PropertyType, model.ShortName, model.StreetIdentifier, model.StreetName, model.Suburb, model.City,
    model.PostalCode, model.State, model.Country, model.NumBedrooms.ToString(), model.NumBathrooms.ToString(), model.NumParking.ToString(), model.Currency, model.Description, null);
            logger.Target.LogInfo($"Created property id: {result}");
            return result;
        }

        public PropertyNodeStruct GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            var instrument = cache.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
            return instrument?.Data as PropertyNodeStruct;
        }

        public PricingStructureData GetValue(string id)
        {
            var pricingStructure = cache.GetPricingStructure(id);
            return pricingStructure;
        }

        public void ClearCache()
        {
            cache.Clear();
            return;
        }
    }
}