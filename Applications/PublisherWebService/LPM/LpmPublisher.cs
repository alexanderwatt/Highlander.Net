using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orion.Constants;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Identifiers;
using nab.QDS.Build;
using nab.QDS.Core.V34;
using nab.QDS.FpML.V47;
//using National.QRSC.ObjectCache;
using nab.QDS.Core.Common;
using nab.QDS.Util.Logging;
using nab.QDS.Util.NamedValues;
using nab.QDS.Util.RefCounting;
using nab.QDS.Util.Serialisation;

namespace Orion.PublisherWebService.Lpm
{
    public class LpmPublisher
    {
        //private static readonly string MessengerId = string.Format("{0}-{1}",
        //                                             Assembly.GetExecutingAssembly().GetName().Name,
        //                                             Assembly.GetExecutingAssembly().GetName().Version);

        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private readonly Reference<ILogger> _loggerRef;
        private readonly ICoreCache _cache;

        public LpmPublisher(Reference<ILogger> loggerRef, ICoreCache cache)
        {
            _loggerRef = loggerRef;
            if (cache != null)
            {
                _cache = cache;
            }
            else if (_cache == null)
            {
                //var logger = new TraceLogger(false);
                //string configId;
                //switch (BuildEnv)//Only Dev.
                //{
                //    case EnvId.DEV_Development:
                //        configId = "Default_DEV_Server";
                //        break;
                //    case EnvId.SIT_SystemTest:
                //        configId = "Default_SIT_Server";
                //        break;
                //    case EnvId.STG_StagingLive:
                //        configId = "Default_STG_Server";
                //        break;
                //    default:
                //        throw new InvalidOperationException("Unhandled environment:" +
                //                                            ServerStore.Client.ClientInfo.BuildEnv);
                //}
                try
                {
                    CoreClientFactory factory = new CoreClientFactory(_loggerRef)
                        .SetEnv(BuildEnv.ToString())
                        .SetApplication(Assembly.GetExecutingAssembly())
                        .SetProtocols(WcfConst.AllProtocolsStr)
                        .SetServers("localhost");
                    _cache = factory.Create();
                }
                catch (Exception excp)
                {
                    _loggerRef.Target.Log(excp);
                }
                //runTime = RuntimeFactory.CreateClient(logger, MessengerId, configId);
            }
        }

        //private void CleanUp()
        //{
        //    _loggerRef.Target.LogInfo("Stopped.");
        //    DisposeHelper.SafeDispose(ref _cache);
        //}

        public void Publish(Market market, NamedValueSet inputProperties, TimeSpan lifetime)
        {
            // Convert the properties back to old style
            NamedValueSet revertedProperties = RevertProperties(inputProperties, market, lifetime);
            // Convert the market back to old style
            Market revertedMarket = RevertMarket(market, revertedProperties);
            // Get the ID
            string identifier = revertedProperties.GetString("UniqueIdentifier", true);
            // Send the message
            _cache.SaveObject(revertedMarket, identifier, revertedProperties, lifetime);
            //SendMessage(revertedMarket, identifier, revertedProperties, lifetime);
        }

        private static NamedValueSet RevertProperties(NamedValueSet inputProperties, Market market, TimeSpan lifetime)
        {
            var properties = new NamedValueSet(inputProperties);
            NamedValueCopy(properties, "MarketName", "Market");
            NamedValueCopy(properties, "Market", "TimeState");
            var yieldCurveValuation = market.Items1[0] as YieldCurveValuation;
            if (yieldCurveValuation == null)
            {
                NamedValueCopy(properties, "Source", "Owner");
                properties.Set("SurfaceType", "VolatilitySurface");
            }
            else
            {
                NamedValueCopy(properties, "PricingStructureType", "StructureType");
                NamedValueCopy(properties, "StructureType", "CurveType");
                const string owner = "SydSwapDesk";
                string buildDateTimeString = yieldCurveValuation.baseDate.Value.ToString("dd/MM/yyyy");
                string structureType = properties.GetString("StructureType", true);
                string timeState = properties.GetString("TimeState", true);
                string currency = properties.GetString("Currency", true);
                string index = properties.GetString("Index", true);
                string indexTenor = properties.GetString("IndexTenor", true);
                string validity = properties.GetString("Validity", true);
                string identifier = string.Format("{0}.{1}{2}.{3}.{4}.{5}.{6}",
                                                  timeState, currency, index, indexTenor, validity, owner, buildDateTimeString);
                string domainId = string.Format("Orion.Market.{0}.{1}.{2}-{3}-{4}",
                                                timeState, structureType, currency, index, indexTenor);
                NamedValueApply(properties, "DomainId", domainId);
                NamedValueApply(properties, "Owner", owner);
                NamedValueApply(properties, "BuildDateTimeString", buildDateTimeString);
                NamedValueApply(properties, "EventName", identifier);
                NamedValueApply(properties, "IndexName", string.Format("{0}-{1}", currency, index));
                properties.Set("BaseDate", yieldCurveValuation.baseDate.Value);
                properties.Set("BuildDateTime", yieldCurveValuation.buildDateTime);
                NamedValueApply(properties, "Identifier", identifier);
            }
            properties.Set("ExpiryIntervalInMins", lifetime.TotalMinutes);
            properties.Set("LPMCopy", true);
            return properties;
        }

        private Market RevertMarket(Market inputMarket, NamedValueSet inputProperties)
        {
            Market market = Translate(inputMarket);
            market.id = null;
            if ((market.Items != null && market.Items.Length > 0))
            {
                if (market.Items[0] is YieldCurve)
                {
                    var yieldCurve = (YieldCurve)market.Items[0];
                    yieldCurve.id = inputProperties.GetString("Identifier", true);
                    yieldCurve.algorithm = "Base algorithm";
                    yieldCurve.forecastRateIndex.floatingRateIndex.Value = inputProperties.GetString("IndexName", true);
                    if (market.Items1.Length <= 0 || !(market.Items1[0] is YieldCurveValuation))
                    {
                        return market;
                    }
                    string currency = PropertyHelper.ExtractCurrency(inputProperties);
                    var yieldCurveValuation = (YieldCurveValuation)market.Items1[0];
                    yieldCurveValuation.id = inputProperties.GetString("Identifier", true);
                    yieldCurveValuation.definitionRef = null;
                    int assetCount = 0;
                    int futureAssetCount = 1;
                    foreach (Asset asset in yieldCurveValuation.inputs.instrumentSet)
                    {
                        string id = "";
                        // Need to redo the IDs as the parts are used
                        string[] hrefParts = asset.id.Split('-');
                        switch (hrefParts[1])
                        {
                            case "Deposit":
                                id = "Deposit-" + hrefParts[0] + "-" + hrefParts[2];
                                break;
                            case "IRFuture":
                                id = "Future-" + hrefParts[0] + "-IR" + futureAssetCount.ToString("00");
                                futureAssetCount++;
                                break;
                            case "IRSwap":
                                id = "SimpleIRSwap-" + hrefParts[0] + "-" + hrefParts[2];
                                break;
                        }
                        //asset.instrumentId = new InstrumentId[1];
                        //asset.instrumentId[0] = new InstrumentId { Value = id };
                        asset.id = id + "-" + assetCount.ToString("000");

                        assetCount++;
                    }
                    // Need to convert Rate to IRFuturesPrice
                    foreach (BasicAssetValuation assetQuote in yieldCurveValuation.inputs.assetQuote)
                    {
                        string[] hrefParts = assetQuote.objectReference.href.Split('-');
                        if (hrefParts[1] == "IRFuture")
                        {
                            assetQuote.quote[0].quoteUnits.Value = "IRFuturesPrice";
                            assetQuote.quote[0].value = 100 * (1 - assetQuote.quote[0].value);
                        }
                    }
                    // Change the Discount Factors to how they used to be
                    if (yieldCurveValuation.discountFactorCurve.point.Count() > 3)
                    {
                        DateTime baseDate = yieldCurveValuation.baseDate.Value;
                        yieldCurveValuation.discountFactorCurve =
                            ConstructDiscountFactors(_loggerRef.Target, _cache, yieldCurveValuation.discountFactorCurve, baseDate, currency);
                    }
                }
                else if (market.Items[0] is VolatilityRepresentation)
                {
                    var valuation = (VolatilityMatrix)market.Items1[0];
                    DateTime baseDate = valuation.baseDate.Value;

                    string source = inputProperties.GetString("Source", true);
                    string currency = inputProperties.GetString("Currency", true);
                    string type = inputProperties.GetString("PricingStructureType", true).Replace("LPM", "").Replace("Lpm", "").Replace("Curve", "");
                    string suffix = string.Format("{0}-{1}-{2:dd/MM/yyyy}", currency, source, baseDate);
                    var representation = (VolatilityRepresentation)market.Items[0];
                    representation.id = string.Format("{0}-{1}", type, suffix);
                    representation.name = suffix.Replace('-', '.');
                    market.id = string.Format("Market - {0}", representation.id);
                    valuation.objectReference.href = representation.id;
                }
            }
            return market;
        }

        internal static TermCurve ConstructDiscountFactors(ILogger logger, ICoreCache cache, TermCurve inputCurve, DateTime baseDate, string currency)
        {
            List<DateTime> dates = inputCurve.point.Select(a => (DateTime)a.term.Items[0]).ToList();
            List<decimal> values = inputCurve.point.Select(a => a.mid).ToList();
            var properties = new NamedValueSet();
            properties.Set(CurveProp.PricingStructureType, "RateCurve");
            properties.Set(CurveProp.Market, "ConstructDiscountFactors");
            properties.Set(CurveProp.IndexTenor, "0M");
            properties.Set(CurveProp.Currency1, currency);
            properties.Set(CurveProp.IndexName, "XXX-XXX");
            properties.Set(CurveProp.Algorithm, "FastLinearZero");
            properties.Set(CurveProp.BaseDate, baseDate);
            //var curveId = new RateCurveIdentifier(properties);
            //var algorithmHolder = new PricingStructureAlgorithmsHolder(logger, cache, curveId.PricingStructureType, curveId.Algorithm);
            var curve = new RateCurve(properties, null, dates, values);
            var termPoints = new List<TermPoint>();
            for (DateTime date = dates.First(); date <= dates.Last(); date = date.AddMonths(1))
            {
                var discountFactor = (decimal)curve.GetDiscountFactor(date);
                var timeDimension = new TimeDimension();
                XsdClassesFieldResolver.TimeDimension_SetDate(timeDimension, date);
                var termPoint = new TermPoint
                                                 {
                                                     mid = discountFactor,
                                                     midSpecified = true,
                                                     term = timeDimension
                                                 };
                termPoints.Add(termPoint);
            }
            var termCurve = new TermCurve { point = termPoints.ToArray() };
            return termCurve;
        }

        private static Market Translate(Market source)
        {
            string xml = XmlSerializerHelper.SerializeToString(source);
            // change the fpml version
            xml = xml.Replace("http://www.fpml.org/2009/FpML-4-7", "http://www.fpml.org/2007/FpML-4-3");
            var newMarket = XmlSerializerHelper.DeserializeFromString<Market>(xml);
            return newMarket;
        }

        /// <summary>
        /// Set the value if it is not yet set
        /// </summary>
        private static void NamedValueApply(NamedValueSet properties, string key, string value)
        {
            string existingValue = properties.GetString(key, String.Empty);
            if (string.IsNullOrEmpty(existingValue))
            {
                properties.Set(key, value);
            }
        }

        private static void NamedValueCopy(NamedValueSet properties, string existingKey, string newKey)
        {
            string newValue = properties.GetString(existingKey, String.Empty);
            if (!string.IsNullOrEmpty(newValue))
            {
                properties.Set(newKey, newValue);
            }
        }
        
        //private void SendMessage(Market market, string messageName, NamedValueSet messageProperties, TimeSpan messageLifeTime)
        //{
        //    DateTime expiryTime = DateTime.Now.Add(messageLifeTime);
        //    runTime.PublishEvent(market, messageName, messageProperties, expiryTime);
        //}
    }
}
