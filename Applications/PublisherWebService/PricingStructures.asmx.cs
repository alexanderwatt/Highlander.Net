using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Services;
using Core.Common;
using Core.V34;
using Orion.Analytics.Stochastics.Volatilities;
using Orion.Build;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Cubes;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.PricingStructures.LPM;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using nab.QDS.FpML.V47;
using Orion.Identifiers;
using Orion.ModelFramework;

namespace Orion.PublisherWebService
{
    /// <summary>
    /// Summary description for PricingStructures
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class PricingStructures : WebService
    {
        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private readonly Reference<ILogger> _loggerRef;
        public readonly ICoreCache Cache;

        public Reference<ILogger> Logger { get { return _loggerRef; } }

        public PricingStructures()
        {
            _loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            CoreClientFactory factory = new CoreClientFactory(_loggerRef)
                .SetEnv(BuildEnv.ToString())
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr)
                .SetServers("localhost");
            Cache = factory.Create();
        }

        public PricingStructures(ICoreCache cache)
            : this(Reference<ILogger>.Create(new TraceLogger(true)), cache)
        {}

        public PricingStructures(Reference<ILogger> loggerRef, ICoreCache cache)
        {
            _loggerRef = loggerRef;
            if (cache != null)
            {
                Cache = cache;
            }
            else if (Cache == null)
            {
                try
                {
                    CoreClientFactory factory = new CoreClientFactory(_loggerRef)
                        .SetEnv(BuildEnv.ToString())
                        .SetApplication(Assembly.GetExecutingAssembly())
                        .SetProtocols(WcfConst.AllProtocolsStr)
                        .SetServers("localhost");
                    Cache = factory.Create();
                }
                catch (Exception excp)
                {
                    _loggerRef.Target.Log(excp);
                }
            }
        }

        /// <summary>
        /// Publishes a curve.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <returns></returns>
        [WebMethod]
        public string PublishCurve(object[][] structurePropertiesRange, object[][] publishPropertiesRange, object[][] valuesRange)
        {
            // Create curve
            IPricingStructure pricingStructure = PricingStructureFactory.CreatePricingStructure(_loggerRef.Target, Cache, structurePropertiesRange, valuesRange);
            // Get properties needed
            Market market = GetMarket(pricingStructure);
            string uniqueIdentifier = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            NamedValueSet properties = pricingStructure.GetPricingStructureId().Properties;
            // Save
            Publish(market, uniqueIdentifier, properties, publishPropertiesRange);
            return uniqueIdentifier;
        }

        /// <summary>
        /// Publishes a curve.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="additionalRange">Extra data range</param>
        /// <returns></returns>
        [WebMethod]
        public string PublishCurveAdditional(object[][] structurePropertiesRange, object[][] publishPropertiesRange, 
            object[][] valuesRange, object[][] additionalRange)
        {
            // Create curve
            IPricingStructure pricingStructure
                = PricingStructureFactory.CreatePricingStructure(_loggerRef.Target, Cache, structurePropertiesRange, valuesRange, additionalRange);
            // Get properties needed
            Market market = GetMarket(pricingStructure);
            string uniqueIdentifier = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            NamedValueSet properties = pricingStructure.GetPricingStructureId().Properties;
            // Save
            Publish(market, uniqueIdentifier, properties, publishPropertiesRange);
            return uniqueIdentifier;
        }

        /// <summary>
        /// Publishes the volatility cube.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="strikesOrTenorsRange">The strikes or tenors range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <returns></returns>
        [WebMethod]
        public string PublishVolatilityCube(object[][] structurePropertiesRange, object[][] publishPropertiesRange, object[][] strikesOrTenorsRange, object[][] valuesRange)
        {
            // Translate into useful objects
            NamedValueSet structureProperties = structurePropertiesRange.ToNamedValueSet();
            var data = (object[,])PricingStructureFactory.TrimNulls(valuesRange.ConvertArrayToMatrix());
            decimal[] strikes = strikesOrTenorsRange.ConvertToOneDimensionalArray<decimal>();
            string[] expiries = PricingStructureFactory.ExtractExpiries(data).Distinct().ToArray();
            string[] tenors = PricingStructureFactory.ExtractTenors(data).Distinct().ToArray();
            decimal[,] vols = PricingStructureFactory.ConvertToDecimalArray(data, 2);
            // Create curve
            var volatilityCube = new VolatilityCube(structureProperties, expiries, tenors, vols, strikes);
            // Get properties needed
            Market market = GetMarket(volatilityCube);
            string uniqueIdentifier = volatilityCube.GetPricingStructureId().UniqueIdentifier;
            NamedValueSet properties = volatilityCube.GetPricingStructureId().Properties;
            // Save
            Publish(market, uniqueIdentifier, properties, publishPropertiesRange);
            return uniqueIdentifier;
        }

        /// <summary>
        /// Publishes the LPM cap floor vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        [WebMethod]
        public string PublishLpmCapFloorVolMatrix(object[][] structurePropertiesRange, object[][] publishPropertiesRange, object[][] valuesRange, object[][] rateCurveFiltersRange)
        {
            // Translate into useful objects
            NamedValueSet structureProperties = structurePropertiesRange.ToNamedValueSet();
            object[,] values = valuesRange.ConvertArrayToMatrix();
            string[] columnNames = Array.ConvertAll(values.GetRow(0), Convert.ToString);
            object[] ppd = values.GetColumn(1);
            for (int index = 0; index < ppd.Length; index++)
            {
                ppd[index] = ppd[index] is Double ? Convert.ToDouble(ppd[index])/100 : ppd[index];
            }
            values.SetColumn(1, ppd);
            object[][] data = values.GetRows(1, values.RowCount());
            // Create matrix
            var baseDate = structureProperties.GetValue<DateTime>("BaseDate", false);
            if (baseDate == DateTime.MinValue)
            {
                baseDate = structureProperties.GetValue<DateTime>("BuildDateTime", false).Date;
                if (baseDate == DateTime.MinValue)
                {
                    baseDate = DateTime.Today;
                }
            }
            string sourceName = structureProperties.GetString("Source", true);
            string currency = structureProperties.GetString("Currency", true);
            string marketName = structureProperties.GetString("MarketName", true);
            string indexName = structureProperties.GetString("IndexName", true);
            int year = baseDate.Year;
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(baseDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string id = string.Format("LPMCapFloorCurve.{0}.{1:yyyy-MM-dd}", currency, baseDate);
            var matrix = new CapFloorATMMatrix(columnNames, data, structurePropertiesRange, baseDate, id);
            // Load underlying curve
            Market market = GetCurve(_loggerRef.Target, Cache, rateCurveFiltersRange);
            // Create the capFloors and properties
            Market capFloors = LPMCapFloorCurve.ProcessCapFloor(_loggerRef.Target, Cache, market, matrix);
            string newId = string.Format("{0}.{1}.CapFloor.{2}.{3}.{4}.Week{5}", marketName, indexName, currency, sourceName, year, weekOfYear);
            string name = string.Format("CapFloor-{0}-{1}-{2:dd/MM/yyyy}", currency, sourceName, baseDate);
            structureProperties.Set("Identifier", newId);
            structureProperties.Set("Name", name);
            // Save
            Publish(capFloors, "Orion.Market." + newId, structureProperties, publishPropertiesRange);
            return newId;
        }

        private static Market GetCurve(ILogger logger, ICoreCache cache, object[][] rateCurveFiltersRange)
        {
            NamedValueSet rateCurveFilters = rateCurveFiltersRange.ToNamedValueSet();
            IExpression queryExpr = Expr.BoolAND(rateCurveFilters);
            List<ICoreItem> loadedCurves = cache.LoadItems<Market>(queryExpr);
            if (loadedCurves.Count == 0)
            {
                throw new InvalidOperationException("Requested curve is not available.");
            }
            if (loadedCurves.Count > 1)
            {
                throw new InvalidOperationException("More than 1 curve found for supplied filters.");
            }
            var loadedCurve = loadedCurves.Single();
            var market = (Market)loadedCurve.Data;
            var yieldCurveValuation = (YieldCurveValuation)market.Items1[0];
            DateTime baseDate = yieldCurveValuation.baseDate.Value;
            string currency = PropertyHelper.ExtractCurrency(loadedCurve.AppProps);
            yieldCurveValuation.discountFactorCurve = ConstructDiscountFactors(logger, cache, yieldCurveValuation.discountFactorCurve, baseDate, currency);
            return market;
        }

        /// <summary>
        /// Publishes the LPM swaption vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        [WebMethod]
        public string PublishLpmSwaptionVolMatrix(object[][] structurePropertiesRange, object[][] publishPropertiesRange, object[][] valuesRange, object[][] rateCurveFiltersRange)
        {
            NamedValueSet structureProperties = structurePropertiesRange.ToNamedValueSet();
            object[,] values = valuesRange.ConvertArrayToMatrix();
            string[] tenors = Array.ConvertAll(values.GetRow(0).Skip(1).ToArray(), Convert.ToString);
            string[] expiries = Array.ConvertAll(values.GetColumn(0).Skip(1).ToArray(), Convert.ToString);
            object[][] dataObjects = values.GetRows(1, values.RowCount(), 1, values.ColumnCount());
            object[][] data = dataObjects.Select(a => a.Select(b => b).ToArray()).ToArray();
            // Setup base values needed to build
            var baseDate = structureProperties.GetValue<DateTime>("BaseDate", false);
            if (baseDate == DateTime.MinValue)
            {
                baseDate = structureProperties.GetValue<DateTime>("BuildDateTime", false).Date;
                if (baseDate == DateTime.MinValue)
                {
                    baseDate = DateTime.Today;
                }
            }
            string sourceName = structureProperties.GetString("Source", true);
            string currency = structureProperties.GetString("Currency", true);
            string marketName = structureProperties.GetString("MarketName", true);
            string indexName = structureProperties.GetString("IndexName", true);
            int year = baseDate.Year;
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(baseDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string id = string.Format("LPMSwaptionCurve.{0}.{1:yyyy-MM-dd}", currency, baseDate);
            var ppdGrid = new SwaptionPPDGrid(expiries, tenors, data);
            // Load underlying curve
            Market market = GetCurve(_loggerRef.Target, Cache, rateCurveFiltersRange);
            Market matrix = LPMSwaptionCurve.ProcessSwaption(_loggerRef.Target, Cache, market, ppdGrid, id);
            string newId = string.Format("{0}.{1}.Swaption.{2}.{3}.{4}.Week{5}", marketName, indexName, currency, sourceName, year, weekOfYear);
            string name = string.Format("Swaption-{0}-{1}-{2:dd/MM/yyyy}", currency, sourceName, baseDate);
            structureProperties.Set("Identifier", newId);
            structureProperties.Set("Name", name);
            // Save
            Publish(matrix, "Orion.Market." + newId, structureProperties, publishPropertiesRange);
            return newId;
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
            var curveId = new RateCurveIdentifier(properties);
            var algorithmHolder = new PricingStructureAlgorithmsHolder(logger, cache, curveId.PricingStructureType, curveId.Algorithm);
            var curve = new RateCurve(properties, algorithmHolder, dates, values);
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

        public void Publish(Market market, string uniqueIdentifier, NamedValueSet curveProperties, object[][] publishPropertiesRange)
        {
            NamedValueSet publishProperties = publishPropertiesRange.ToNamedValueSet();
            TimeSpan lifetime = GetLifetime(publishProperties);
            //if (publishProperties.GetValue("LPMCopy", false))
            //{
            //    var lpmPublisher = new LpmPublisher(_loggerRef, Cache);
            //    lpmPublisher.Publish(market, curveProperties, lifetime);
            //    //Publish(market, uniqueIdentifier, curveProperties);
            //}
            Cache.SaveObject(market, uniqueIdentifier, curveProperties, lifetime);
            Global.LoggerRef.Target.LogInfo("Published '{0}'.", uniqueIdentifier);
        }

        private static TimeSpan GetLifetime(NamedValueSet publishProperties)
        {
            int expiryInterval = publishProperties.GetValue("ExpiryIntervalInMins", 60);
            var lifetime = new TimeSpan(0, expiryInterval, 0);
            return lifetime;
        }

        private static Market GetMarket(IPricingStructure pricingStructure)
        {
            string uniqueIdentifier = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            var fpml = pricingStructure.GetFpMLData();
            Market market = PricingStructureHelper.CreateMarketFromFpML(uniqueIdentifier, fpml);
            return market;
        }

        /// <summary>
        /// Gets the curve.
        /// </summary>
        /// <param name="uniqueName">The uniqueName.</param>
        /// <param name="forceBootstrap">The force a bootstrap flag.</param>
        /// <returns></returns>
        public IPricingStructure GetCurve(String uniqueName, Boolean forceBootstrap)
        {
            var item = Cache.LoadItem<Market>(uniqueName);
            if (item == null)
                return null;
            var deserializedMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;
            if (forceBootstrap)
            {
                properties.Set("Bootstrap", true);
            }
            //Handle ratebasiscurves that are dependent on another ratecurve.
            //TODO This functionality needs to be extended for calibrations (bootstrapping),
            //TODO where there is AccountReference dependency on one or more pricing structures.
            var pst = PropertyHelper.ExtractPricingStructureType(properties);
            if (pst == PricingStructureTypeEnum.RateBasisCurve)
            {
                //Get the referrence curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = Cache.LoadItem<Market>(refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricingstructure
                var psBasis = CurveEngine.Factory.PricingStructureFactory.Create(_loggerRef.Target, Cache, null, null, refCurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            if (pst == PricingStructureTypeEnum.RateXccyCurve)
            {
                //Get the referrence curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = Cache.LoadItem<Market>(refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Get the referrence curve identifier.
                var refFxCurveId = properties.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, true);
                //Load the data.
                var refFxItem = Cache.LoadItem<Market>(refFxCurveId);
                var deserializedRefFxCurveMarket = (Market)refFxItem.Data;
                var refFxCurveProperties = refFxItem.AppProps;
                //Get the currency2 curve identifier.
                var currency2CurveId = properties.GetValue<string>(CurveProp.ReferenceCurrency2CurveId, true);
                //Load the data.
                var currrecny2Item = Cache.LoadItem<Market>(currency2CurveId);
                var deserializedCurrecny2CurveMarket = (Market)currrecny2Item.Data;
                var refCurrecnyCurveProperties = currrecny2Item.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var refFxCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefFxCurveMarket.Items[0],
                        deserializedRefFxCurveMarket.Items1[0], refFxCurveProperties);
                var currency2CurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedCurrecny2CurveMarket.Items[0],
                        deserializedCurrecny2CurveMarket.Items1[0], refCurrecnyCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricingstructure
                var psBasis = CurveEngine.Factory.PricingStructureFactory.Create(_loggerRef.Target, Cache, null, null, refCurveFpmlTriplet, refFxCurveFpmlTriplet,
                                                                    currency2CurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            // Create FpML pair from Market
            //
            var fpmlPair = new Pair<PricingStructure, PricingStructureValuation>(deserializedMarket.Items[0],
                                                                                 deserializedMarket.Items1[0]);
            //create and set the pricingstructure
            var ps = CurveEngine.Factory.PricingStructureFactory.Create(_loggerRef.Target, Cache, null, null, fpmlPair, properties);
            return ps;
        }
    }
}