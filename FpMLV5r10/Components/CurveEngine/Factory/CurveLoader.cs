#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Constants;
using FxCurve = Orion.CurveEngine.PricingStructures.Curves.FxCurve;

#endregion

namespace Orion.CurveEngine.Factory
{
    /// <summary>
    /// The curve factory class.
    /// </summary>
    public static class CurveLoader
    {
        #region Fields

        private const string CurrencyUsd = "USD";
        private const string DefaultTenor = "3M";

        #endregion

        #region Creators withour the cache.

        /// <summary>
        /// This method does not rebootstrap the priceing structure and thus does not need a cache or business calendars.
        /// Thids loads all current curve, surface and cubes.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache. This is required for the algorithm used in the ointerpolator.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        public static IPricingStructure LoadPricingStructure(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            return PricingStructureFactory.Create(logger, cache, nameSpace, null, null, fpmlData, properties);
        }


        /// <summary>
        /// Loads a pre-built fx curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache. This is required for the algorithm used in the ointerpolator.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="marketName"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IFxCurve LoadFxCurve(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2, string stress)
        {
            return LoadFxCurve(logger, cache, nameSpace, null, null, marketName, currency1, currency2, stress);
        }

        /// <summary>
        /// Loads an interest raet curve that does not need to be bootstrapped.
        /// </summary>
        /// <param name="cache">The cache. This is required for the algorithm used in the ointerpolator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IRateCurve LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace, string name)
        {
            return LoadInterestRateCurve(logger, cache, nameSpace, null, null, name);
        }

        /// <summary>
        /// Creeates all valid pricingstructures.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        public static IPricingStructure LoadCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            return PricingStructureFactory.Create(logger, cache, nameSpace, null, null, fpmlData, properties);
        }

        /// <summary>
        /// Loads a ratespread curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="referenceCurveData">The reference curve data.</param>
        /// <param name="spreadCurveData">The spread curve data.</param>
        public static IPricingStructure LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData)
        {
            return PricingStructureFactory.Create(logger, cache, nameSpace, null, null, referenceCurveData, spreadCurveData);
        }

        /// <summary>
        /// Returns an pricingstructure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="referenceCurveData">The reference curve data.</param>
        /// <param name="referenceFxCurveData">The Fx ewfwewncw curve.</param>
        /// <param name="currency2CurveData">The currency2 data.</param>
        /// <param name="spreadCurveData">The spread curve data.</param>
        public static IPricingStructure LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceFxCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> currency2CurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData)
        {
            return PricingStructureFactory.Create(logger, cache, nameSpace, null, null, referenceCurveData, referenceFxCurveData, currency2CurveData, spreadCurveData);
        }

        #endregion

        #region Load any curve but FX

        /// <summary>
        /// Loads a curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static ICurve LoadCurve(ILogger logger, ICoreCache cache,
                                           string nameSpace, IBusinessCalendar fixingCalendar,
                                           IBusinessCalendar rollCalendar, string marketName, string curveName,
                                           string stress)
        {
            //  Pricing structure type
            var pricingStructure = curveName.Split('.')[0];
            ICurve structure;
            var pricingStructureType = EnumHelper.Parse<PricingStructureTypeEnum>(pricingStructure);
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                case PricingStructureTypeEnum.InflationCurve:
                case PricingStructureTypeEnum.BondFinancingCurve:
                case PricingStructureTypeEnum.BondFinancingBasisCurve:               
                case PricingStructureTypeEnum.RateSpreadCurve:
                case PricingStructureTypeEnum.RateBasisCurve:
                case PricingStructureTypeEnum.RateXccyCurve:
                    structure = LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, curveName, stress);
                    break;
                case PricingStructureTypeEnum.BondDiscountCurve:
                case PricingStructureTypeEnum.BondCurve:
                    structure = LoadBondCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, curveName, stress);
                    break;
                case PricingStructureTypeEnum.CommodityCurve:
                case PricingStructureTypeEnum.CommoditySpreadCurve:
                    structure = LoadCommodityCurve(logger, cache, nameSpace, curveName);
                    break;
                case PricingStructureTypeEnum.EquityCurve:
                case PricingStructureTypeEnum.EquitySpreadCurve:
                    structure = LoadEquityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, curveName, stress);
                    break;
                default:
                    var message =
                        $"Specified pricing structure type : '{pricingStructureType}' has not been recognised.";
                    throw new ApplicationException(message);
            }
            return structure;
        }

        #endregion

        #region Loads a Bond or Discount Curve

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="creditInstrumentId"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IPricingStructure LoadDiscountCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string creditInstrumentId, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.DiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondDiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondFinancingBasisCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondFinancingCurve.ToString()));
            // Filter by supplied currency
            return LoadDiscountCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, creditInstrumentId, stress, curveTypeFilters);
        }

        internal static IPricingStructure LoadDiscountCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string creditInstrumentId, string stress, IExpression curveTypeFilters)
        {
            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);
            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(creditInstrumentId) ? Expr.IsNull(CurveProp.CreditInstrumentId) : Expr.IsEQU(CurveProp.CreditInstrumentId, creditInstrumentId);
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                ? Expr.IsNull(CurveProp.StressName)
                : Expr.IsEQU(CurveProp.StressName, stress);
            IExpression filter = Expr.BoolAND(stressFilter,
                Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                curveTypeFilters,
                currencyFilter,
                tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            int count = results.Count;
            if (count > 1)
            {
                return null;
                //throw new ArgumentException(string.Format(TooManyCurvesFound, marketName, currency, tenor));
            }
            ICoreItem result = results.Single();
            var market = (Market)result.Data;
            var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
            var properties = result.AppProps;
            var rateCurve = PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
            return rateCurve;
        }

        #endregion

        #region Load an IR curve.

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IRateCurve LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.DiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateSpreadCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondDiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondFinancingBasisCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondFinancingCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateBasisCurve.ToString()));
            // Filter by supplied currency
            //This wont work when a tenor is provided for a discount curve.
            //Use the LoadDiscountCurve.
            return LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress, curveTypeFilters);
        }

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IRateCurve LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, string name)
        {
            var id = nameSpace + "." + name;
            var result = cache.LoadItem<Market>(id);
            if (result != null)
            {
                var market = (Market)result.Data;
                var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                    market.Items1[0]);
                var properties = result.AppProps;
                var rateCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                return rateCurve;
            }
            return null;
        }

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IRateCurve LoadInterestRateCurve(ILogger logger, ICoreCache cache, 
            string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string curveName, string stress)
        {
            // Filter by supplied tenor
            if (curveName != null && marketName != null)
            {
                var identifier = PricingStructureIdentifier.ValidRateCurveIdentifier(marketName, curveName, stress);
                //int count = results.Count;
                if (identifier == null)
                {
                    MarketEnvironmentHelper.ResolveRateCurveName(curveName, out string currency, out string tenor);
                    var curve = LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress);
                    return curve;
                }
                var result = cache.LoadItem<Market>(nameSpace + "." + identifier);
                if (result != null)
                {
                    var market = (Market)result.Data;
                    var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                       market.Items1[0]);
                    var properties = result.AppProps;
                    var rateCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                    return rateCurve;
                }
            }
            return null;
        }

        internal static IRateCurve LoadInterestRateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress, IExpression curveTypeFilters)
        {
            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);
            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(tenor) ? Expr.IsNull(CurveProp.IndexTenor) : Expr.IsEQU(CurveProp.IndexTenor, tenor);
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                                           ? Expr.IsNull(CurveProp.StressName)
                                           : Expr.IsEQU(CurveProp.StressName, stress);
            IExpression filter = Expr.BoolAND(stressFilter,
                                              Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                                              Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                                              curveTypeFilters,
                                              currencyFilter,
                                              tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            int count = results.Count;
            if (count == 0)
            {
                if (tenor != DefaultTenor)
                {
                    // try again for 3M tenor
                    return LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, curveTypeFilters);
                }
                if (!curveTypeFilters.DisplayString().Contains(PricingStructureTypeEnum.XccySpreadCurve.ToString()))
                {
                    // try again for XCcySpreadCurve
                    var newCurveTypeFilters = Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.XccySpreadCurve.ToString());
                    return LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, newCurveTypeFilters);
                }
                return null;
                //throw new ArgumentException(string.Format(NoCurvesFound, marketName, currency, tenor));
            }
            if (count > 1)
            {
                return null;
                //throw new ArgumentException(string.Format(TooManyCurvesFound, marketName, currency, tenor));
            }
            ICoreItem result = results.Single();
            var market = (Market)result.Data;
            var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
            var properties = result.AppProps;
            var rateCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
            return rateCurve;
        }

        #endregion

        #region Load a volaility curve.

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IVolatilitySurface LoadVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.CapVolatilityCurve.ToString()));
            // Filter by supplied currency
            //This wont work when a tenor is provided for a discount curve.
            //Use the LoadDiscountCurve.
            return LoadVolatilityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress, curveTypeFilters);
        }

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IVolatilitySurface LoadVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, string name)
        {
            var id = nameSpace + "." + name;
            var result = cache.LoadItem<Market>(id);
            if (result != null)
            {
                var market = (Market)result.Data;
                var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                    market.Items1[0]);
                var properties = result.AppProps;
                var volCurve = (IVolatilitySurface)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                return volCurve;
            }
            return null;
        }

        /// <summary>
        /// Loads a volatility curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IVolatilitySurface LoadVolatilityCurve(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string curveName, string stress)
        {
            // Filter by supplied tenor
            if (curveName != null && marketName != null)
            {
                var identifier = PricingStructureIdentifier.ValidVolatilityCurveIdentifier(marketName, curveName, stress);
                //int count = results.Count;
                if (identifier == null)
                {
                    MarketEnvironmentHelper.ResolveVolatilityCurveName(curveName, out string currency, out string tenor);
                    var curve = LoadVolatilityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress);
                    return curve;
                }
                var result = cache.LoadItem<Market>(nameSpace + "." + identifier);
                if (result != null)
                {
                    var market = (Market)result.Data;
                    var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                        market.Items1[0]);
                    var properties = result.AppProps;
                    var volCurve = (IVolatilitySurface)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                    return volCurve;
                }
            }
            return null;
        }

        internal static IVolatilitySurface LoadVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress, IExpression curveTypeFilters)
        {
            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);
            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(tenor) ? Expr.IsNull(CurveProp.IndexTenor) : Expr.IsEQU(CurveProp.IndexTenor, tenor);
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                ? Expr.IsNull(CurveProp.StressName)
                : Expr.IsEQU(CurveProp.StressName, stress);
            IExpression filter = Expr.BoolAND(stressFilter,
                Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                curveTypeFilters,
                currencyFilter,
                tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            int count = results.Count;
            if (count == 0)
            {
                if (tenor != DefaultTenor)
                {
                    // try again for 3M tenor
                    return LoadVolatilityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, curveTypeFilters);
                }
                return null;
            }
            if (count > 1)
            {
                return null;
            }
            ICoreItem result = results.Single();
            var market = (Market)result.Data;
            var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
            var properties = result.AppProps;
            var volCurve = (IVolatilitySurface)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
            return volCurve;
        }

        #endregion

        #region Equity Curve loading

        /// <summary>
        /// Loads an equity curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IEquityCurve LoadEquityCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.EquityCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.EquitySpreadCurve.ToString()));

            // Filter by supplied currency
            return LoadEquityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress, curveTypeFilters);
        }

        /// <summary>
        /// Loads an equity curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IEquityCurve LoadEquityCurve(ILogger logger, ICoreCache cache, 
            string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string curveName, string stress)
        {
            // Filter by supplied tenor
            if (curveName != null && marketName != null)
            {
                var identifier = PricingStructureIdentifier.ValidRateCurveIdentifier(marketName, curveName, stress);
                //int count = results.Count;
                if (identifier == null)
                {
                    MarketEnvironmentHelper.ResolveRateCurveName(curveName, out string currency, out string tenor);
                    var curve = LoadEquityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress);
                    return curve;
                }
                var result = cache.LoadItem<Market>(nameSpace + "." + identifier);
                if (result != null)
                {
                    var market = (Market)result.Data;
                    var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                       market.Items1[0]);
                    var properties = result.AppProps;
                    var rateCurve = (IEquityCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                    return rateCurve;
                }
            }
            return null;
        }

        internal static IEquityCurve LoadEquityCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress, IExpression curveTypeFilters)
        {
            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);
            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(tenor) ? Expr.IsNull(CurveProp.IndexTenor) : Expr.IsEQU(CurveProp.IndexTenor, tenor);
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                                           ? Expr.IsNull(CurveProp.StressName)
                                           : Expr.IsEQU(CurveProp.StressName, stress);

            IExpression filter = Expr.BoolAND(stressFilter,
                                              Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                                              Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                                              curveTypeFilters,
                                              currencyFilter,
                                              tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            int count = results.Count;
            if (count == 0)
            {
                if (tenor != DefaultTenor)
                {
                    // try again for 3M tenor
                    return LoadEquityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, curveTypeFilters);
                }
                if (!curveTypeFilters.DisplayString().Contains(PricingStructureTypeEnum.XccySpreadCurve.ToString()))
                {
                    // try again for XCcySpreadCurve
                    var newCurveTypeFilters = Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.XccySpreadCurve.ToString());
                    return LoadEquityCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, newCurveTypeFilters);
                }
                return null;
                //throw new ArgumentException(string.Format(NoCurvesFound, marketName, currency, tenor));
            }
            if (count > 1)
            {
                return null;
                //throw new ArgumentException(string.Format(TooManyCurvesFound, marketName, currency, tenor));
            }
            ICoreItem result = results.Single();
            var market = (Market)result.Data;
            var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
            var properties = result.AppProps;
            var rateCurve = (IEquityCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
            return rateCurve;
        }

        #endregion

        #region Bond Curve loading

        /// <summary>
        /// Loads an bond curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IBondCurve LoadBondCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
               Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondCurve.ToString()),
               Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondDiscountCurve.ToString()));
            // Filter by supplied currency
            return LoadBondCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress, curveTypeFilters);
        }

        /// <summary>
        /// Loads an equity curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IBondCurve LoadBondCurve(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string curveName, string stress)
        {
            // Filter by supplied tenor
            if (curveName != null && marketName != null)
            {
                var identifier = PricingStructureIdentifier.ValidRateCurveIdentifier(marketName, curveName, stress);
                //int count = results.Count;
                if (identifier == null)
                {
                    MarketEnvironmentHelper.ResolveRateCurveName(curveName, out string currency, out string tenor);
                    var curve = LoadBondCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, tenor, stress);
                    return curve;
                }
                var result = cache.LoadItem<Market>(nameSpace + "." + identifier);
                if (result != null)
                {
                    var market = (Market)result.Data;
                    var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                       market.Items1[0]);
                    var properties = result.AppProps;
                    var rateCurve = (IBondCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
                    return rateCurve;
                }
            }
            return null;
        }

        internal static IBondCurve LoadBondCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, string marketName, string currency, string tenor, string stress, IExpression curveTypeFilters)
        {
            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);
            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(tenor) ? Expr.IsNull(CurveProp.IndexTenor) : Expr.IsEQU(CurveProp.IndexTenor, tenor);
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                                           ? Expr.IsNull(CurveProp.StressName)
                                           : Expr.IsEQU(CurveProp.StressName, stress);

            IExpression filter = Expr.BoolAND(stressFilter,
                                              Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                                              Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                                              curveTypeFilters,
                                              currencyFilter,
                                              tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            int count = results.Count;
            if (count == 0)
            {
                if (tenor != DefaultTenor)
                {
                    // try again for 3M tenor
                    return LoadBondCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, marketName, currency, DefaultTenor, stress, curveTypeFilters);
                }
                return null;
                //throw new ArgumentException(string.Format(NoCurvesFound, marketName, currency, tenor));
            }
            if (count > 1)
            {
                return null;
                //throw new ArgumentException(string.Format(TooManyCurvesFound, marketName, currency, tenor));
            }
            ICoreItem result = results.Single();
            var market = (Market)result.Data;
            var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
            var properties = result.AppProps;
            var rateCurve = (IBondCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, ps, properties);
            return rateCurve;
        }

        #endregion

        #region FxCurve Loading

        /// <summary>
        /// Loads an interest raet curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFxCurve LoadFxCurve(ILogger logger, ICoreCache cache, string nameSpace, string name)
        {
            var id = nameSpace + "." + name;
            var result = cache.LoadItem<Market>(id);
            if (result != null)
            {
                var market = (Market)result.Data;
                var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                    market.Items1[0]);
                var properties = result.AppProps;
                var fxCurve = (IFxCurve)PricingStructureFactory.Create(logger, cache, nameSpace, null, null, ps, properties);
                return fxCurve;
            }
            return null;
        }

        /// <summary>
        /// Loads an fx curve.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="marketName"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static IFxCurve LoadFxCurve(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            string marketName, string currency1, string currency2, string stress)
        {
            if (currency1.Equals(CurrencyUsd, StringComparison.OrdinalIgnoreCase)
             || currency2.Equals(CurrencyUsd, StringComparison.OrdinalIgnoreCase))
            {
                ICoreItem result = LoadSingleFxCurve(cache, nameSpace, marketName, currency1, currency2, stress);
                if (result != null)
                {
                    var market = (Market)result.Data;
                    var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                       market.Items1[0]);
                    var properties = result.AppProps;
                    bool invert = properties.GetString(CurveProp.Currency1, true) != currency1;
                    var fxCurve = new FxCurve(logger, cache, nameSpace, ps, properties, fixingCalendar, rollCalendar, invert);
                    return fxCurve;
                }
            }
            else
            {
                LoadTwoFxCurves(cache, nameSpace, marketName, currency1, currency2, stress, out ICoreItem result1, out ICoreItem result2);
                if (result1 != null && result2 != null)
                {
                    var market1 = (Market)result1.Data;
                    var ps1 = new Pair<PricingStructure, PricingStructureValuation>(market1.Items[0],
                                                                                        market1.Items1[0]);
                    var market2 = (Market)result2.Data;
                    var ps2 = new Pair<PricingStructure, PricingStructureValuation>(market2.Items[0],
                                                                                        market2.Items1[0]);
                    var newProperties = new NamedValueSet();
                    newProperties.Set(CurveProp.PricingStructureType, PricingStructureTypeEnum.FxCurve.ToString());
                    newProperties.Set(CurveProp.Currency1, currency1);
                    newProperties.Set(CurveProp.Currency2, currency2);
                    newProperties.Set(CurveProp.MarketAndDate, marketName);
                    newProperties.Set(CurveProp.BaseDate, result1.AppProps.GetValue<DateTime>(CurveProp.BaseDate));
                    bool invert1 = result1.AppProps.GetString(CurveProp.Currency1, true) != currency1;
                    bool invert2 = result2.AppProps.GetString(CurveProp.Currency1, true) != currency2;
                    var fxCurve1 = new FxCurve(logger, cache, nameSpace, ps1, result1.AppProps, fixingCalendar, rollCalendar, invert1);
                    var fxCurve2 = new FxCurve(logger, cache, nameSpace, ps2, result2.AppProps, fixingCalendar, rollCalendar, invert2);
                    var fxCurve = new FxDerivedCurve(logger, cache, nameSpace, fxCurve1, fxCurve2, newProperties);
                    return fxCurve;
                }
            }
            return null;
        }

        private static void LoadTwoFxCurves(ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2, string stress,
                                          out ICoreItem result1, out ICoreItem result2)
        {
            // Try currency1 per USD
            var currencyId1 = nameSpace + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, stress, false);
            var currencyCurve1 = cache.LoadItem<Market>(currencyId1);
            // Try USD per currency1
            if (currencyCurve1 == null)
            {
                currencyId1 = nameSpace + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, stress, true);
                currencyCurve1 = cache.LoadItem<Market>(currencyId1);
            }          
            // Try currency2 per USD
            var currencyId2 = nameSpace + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, CurrencyUsd, currency2, stress, false);
            var currencyCurve2 = cache.LoadItem<Market>(currencyId2);
            //Try USD per currency2
            if (currencyCurve2==null)
            {
                currencyId2 = nameSpace + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, CurrencyUsd, currency2, stress, true);
                currencyCurve2 = cache.LoadItem<Market>(currencyId2);
            }
            result1 = LoadAndCheckMarketItem(currencyCurve1, false);
            result2 = LoadAndCheckMarketItem(currencyCurve2, false);

        }

        //private static void GetTwoFxCurvesProperties(ICoreCache cache, string nameSapce, string marketName, string currency1, string currency2,
        //                          out NamedValueSet result1, out NamedValueSet result2)
        //{
        //    // Try currency1 per USD
        //    var currencyId1 = nameSapce + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, null, false);
        //    var currencyCurve1 = cache.LoadItem<Market>(currencyId1).AppProps;
        //    // Try USD per currency1
        //    if (currencyCurve1 == null)
        //    {
        //        currencyId1 = nameSapce + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, null, true);
        //        result1 = cache.LoadItem<Market>(currencyId1).AppProps;
        //    }
        //    // Try currency2 per USD
        //    var currencyId2 = nameSapce + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, CurrencyUsd, currency2, null, false);
        //    var currencyCurve2 = cache.LoadItem<Market>(currencyId2).AppProps;
        //    //Try USD per currency2
        //    if (currencyCurve2 == null)
        //    {
        //        currencyId2 = nameSapce + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, CurrencyUsd, currency2, null, true);
        //        result2 = cache.LoadItem<Market>(currencyId2).AppProps;
        //    }
        //    result1 = currencyCurve1;
        //    result2 = currencyCurve2;
        //}

        private static ICoreItem LoadSingleFxCurve(ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2, string stress)
        {
            //Use the unique name for all markets.
            var curveId = nameSpace + "." + PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, currency2, stress, false);
            var curve = cache.LoadItem<Market>(curveId);
            if (curve == null)
            {
                curveId = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, currency2, stress, true);
                curve = cache.LoadItem<Market>(nameSpace + "." + curveId);
            }
            return LoadAndCheckMarketItem(curve, false);
        }

        #endregion

        #region CommodityCurve Loading

        /// <summary>
        /// Loads a commodity curve from supplied parameters.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ICommodityCurve LoadCommodityCurve(ILogger logger, ICoreCache cache, string nameSpace, string name)
        {
            var id = nameSpace + "." + name;
            var result = cache.LoadItem<Market>(id);
            if (result != null)
            {
                var market = (Market)result.Data;
                var ps = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0],
                                                                                    market.Items1[0]);
                var properties = result.AppProps;
                var commodityCurve = (ICommodityCurve)PricingStructureFactory.Create(logger, cache, nameSpace, null, null, ps, properties);
                return commodityCurve;
            }
            return null;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// The fx curve signature for stressing.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static string FxCurveSignature(string marketName, string currency1, string currency2, string stress)
        {
            return $"FX_{marketName}_{currency1}_{currency2}_{stress ?? "None"}".ToLower();
        }

        /// <summary>
        /// The ir curve signature for stressing.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static string IrCurveSignature(string marketName, string curveName, string stress)
        {
            return $"IR_{marketName}_{curveName}_{stress ?? "None"}".ToLower();
        }


        private static ICoreItem LoadAndCheckMarketItem(ICoreItem marketItem, bool throwError)//Pair<NamedValueSet, Market> marketPair
        {
            if (throwError)
            {
                if (marketItem == null)
                    throw new ApplicationException("Curve not found!");
                var market = (Market)marketItem.Data;
                if (market == null)
                    throw new ApplicationException("Curve not found!");
                if ((market.Items == null) || (market.Items.Length < 1))
                    throw new ApplicationException("Curve '" + market.id + "' contains no PricingStructure!");
                if ((market.Items1 == null) || (market.Items1.Length < 1))
                    throw new ApplicationException("Curve '" + market.id + "' contains no PricingStructureValuation!");
            }
            else
            {
                var market = marketItem?.Data as Market;
                if (market?.Items == null || (market.Items.Length < 1))
                    return null;
                if ((market.Items1 == null) || (market.Items1.Length < 1))
                    return null;
            }
            return marketItem;
        }

        #endregion

    }
}