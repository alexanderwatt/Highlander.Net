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
        private readonly PricingCache _cache;
        private readonly Reference<ILogger> _logger;

        public PropertyService(string nameSpace, Reference<ILogger> logger)
        {
            this._cache = new PricingCache(nameSpace, false);
            this._logger = logger;
        }

        public IEnumerable<string> GetPropertyTradeIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.NameSpace, _cache.NameSpace);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _cache.QueryTradeIds(properties);
            _logger.Target.LogInfo("Queried property trade ids.");
            return trades;
        }

        public string CreatePropertyTrade(PropertyTradeViewModel model, string transactionId)
        {
            var properties = new NamedValueSet();
            properties.Set(Constants.Constants.TransactionIdProperty, transactionId);

            //TODO
            //Add the create property to this!!
            //This way the property transaction is closer to the Lendhaus model.
            var result = _cache.CreatePropertyTradeWithProperties(model.TradeId, true, model.Purchaser, model.Seller, model.TradeTimeUtc, model.EffectiveTimeUtc,
                model.PurchaseAmount, model.PaymentTimeUtc, model.PropertyType, model.Currency, model.PropertyId, model.TradingBook, properties);
            _logger.Target.LogInfo("Created property trade id: {0}", result);
            return result;
        }

        public IEnumerable<string> GetPropertyAssetIds()
        {
            var properties = new NamedValueSet();
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trades = _cache.QueryPropertyAssetIds(properties);
            _logger.Target.LogInfo("Queried property assets.");
            return trades;
        }

        public string CreatePropertyAsset(PropertyAssetViewModel model, string transactionId)
        {
            var props = new NamedValueSet();
            props.Set(Constants.Constants.TransactionIdProperty, transactionId);
            var result = _cache.CreatePropertyAsset(model.PropertyId, model.PropertyType, model.ShortName, model.StreetIdentifier, model.StreetName, model.Suburb, model.City,
    model.PostalCode, model.State, model.Country, model.NumBedrooms.ToString(), model.NumBathrooms.ToString(), model.NumParking.ToString(), model.Currency, model.Description, props);
            _logger.Target.LogInfo($"Created property id: {result}");
            return result;
        }

        public PropertyNodeStruct GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            var instrument = _cache.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
            return instrument?.Data as PropertyNodeStruct;
        }

        public PricingStructureData GetValue(string id)
        {
            var pricingStructure = _cache.GetPricingStructure(id);
            return pricingStructure;
        }

        public int CleanTransaction(string transactionId)
        {
            var props = new NamedValueSet();
            props.Set(Constants.Constants.TransactionIdProperty, transactionId);
            var properties = _cache.DeletePropertyAssetsByQuery(props);
            var trades = _cache.DeleteTradesByQuery(props);
            //TODOS
            //var reports = cache.DeleteReportsByQuery(props);
            return properties + trades;
        }
    }
}