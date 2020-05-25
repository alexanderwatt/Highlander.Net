using Highlander.Codes.V5r3;
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

        public PropertyService(string nameSpace, Reference<ILogger> logger)
        {
            this.cache = new PricingCache(nameSpace, false);
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

        public string CreatePropertyTrade(PropertyTradeViewModel model, string transactionId)
        {
            var properties = new NamedValueSet();
            properties.Set(Constants.Constants.TransactionIdProperty, transactionId);

            //TODO
            //Add the create property to this!!
            //This way the property transaction is closer to the Lendhaus model.
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

        public string CreatePropertyAsset(PropertyAssetViewModel model, string transactionId)
        {
            var props = new NamedValueSet();
            props.Set(Constants.Constants.TransactionIdProperty, transactionId);
            var result = cache.CreatePropertyAsset(model.PropertyId, model.PropertyType, model.ShortName, model.StreetIdentifier, model.StreetName, model.Suburb, model.City,
    model.PostalCode, model.State, model.Country, model.NumBedrooms.ToString(), model.NumBathrooms.ToString(), model.NumParking.ToString(), model.Currency, model.Description, props);
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

        public int CleanTransaction(string transactionId)
        {
            var props = new NamedValueSet();
            props.Set(Constants.Constants.TransactionIdProperty, transactionId);
            var properties = cache.DeletePropertyAssetsByQuery(props);
            var trades = cache.DeleteTradesByQuery(props);
            //TODOS
            //var reports = cache.DeleteReportsByQuery(props);
            return properties + trades;
        }
    }
}