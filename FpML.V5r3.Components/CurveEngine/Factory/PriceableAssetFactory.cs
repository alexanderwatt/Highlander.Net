/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Assets.Equity;
using Orion.Models.Rates.Futures;
using Orion.Identifiers;
using Orion.Analytics.Interpolations;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.Assets.Inflation.Index;
using Orion.CurveEngine.Assets.Inflation.Swaps;
using Orion.CurveEngine.Assets.Options;
using Orion.CurveEngine.Assets.Rates.CapFloorLet;
using Orion.CurveEngine.Assets.Rates.CapsFloors;
using Orion.CurveEngine.Assets.Rates.Futures;
using Orion.Util.Serialisation;
using MarketQuoteHelper = Orion.CurveEngine.Helpers.MarketQuoteHelper;

#endregion

namespace Orion.CurveEngine.Factory
{
    ///<summary>
    /// A helper class to create term points.
    ///</summary>
    public class PriceableAssetFactory
    {
        #region Parse PriceableAssets

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static FxRateSet Parse(IEnumerable<IPriceableFxAssetController> priceableAssets)
        {
            FxRateSet fxRateSet = null;
            if (priceableAssets != null)
            {
                var quotedAssetSetFactory = new QuotedAssetSetFactory();
                foreach (var priceableAsset in priceableAssets)
                {
                    string instrumentId = priceableAsset.Id;
                    Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                    quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
                }
                fxRateSet = quotedAssetSetFactory.CreateFxRateSet();
            }
            return fxRateSet;
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static FxRateSet Parse(IEnumerable<IPriceableCommodityAssetController> priceableAssets)
        {
            FxRateSet fxRateSet = null;
            if (priceableAssets != null)
            {
                var quotedAssetSetFactory = new QuotedAssetSetFactory();
                foreach (var priceableAsset in priceableAssets)
                {
                    string instrumentId = priceableAsset.Id;
                    Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                    quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
                }
                fxRateSet = quotedAssetSetFactory.CreateFxRateSet();
            }
            return fxRateSet;
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static FxRateSet Parse(IEnumerable<IPriceableEquityAssetController> priceableAssets)
        {
            FxRateSet fxRateSet = null;
            if (priceableAssets != null)
            {
                var quotedAssetSetFactory = new QuotedAssetSetFactory();
                foreach (var priceableAsset in priceableAssets)
                {
                    string instrumentId = priceableAsset.Id;
                    Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                    quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
                }
                fxRateSet = quotedAssetSetFactory.CreateFxRateSet();
            }
            return fxRateSet;
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="priceableSpreadAssets"> </param>
        /// <returns></returns>
        public static FxRateSet Parse(IEnumerable<IPriceableCommodityAssetController> priceableAssets, IEnumerable<IPriceableCommodityAssetController> priceableSpreadAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            if (priceableSpreadAssets != null)
            {
                foreach (var priceableAsset in priceableSpreadAssets)
                {
                    string instrumentId = priceableAsset.Id;
                    Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                    quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
                }
            }
            return quotedAssetSetFactory.CreateFxRateSet();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="priceableSpreadAssets"> </param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableRateAssetController> priceableAssets, IEnumerable<IPriceableRateAssetController> priceableSpreadAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            if (priceableSpreadAssets!=null)
            {
                foreach (var priceableAsset in priceableSpreadAssets)
                {
                    string instrumentId = priceableAsset.Id;
                    Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                    quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
                }
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableRateAssetController> priceableAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableOptionAssetController> priceableAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableRateSpreadAssetController> priceableAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableBondAssetController> priceableAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(IEnumerable<IPriceableFuturesAssetController> priceableAssets)
        {
            if (priceableAssets == null) return null;
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (var priceableAsset in priceableAssets)
            {
                string instrumentId = priceableAsset.Id;
                Asset underlyingAsset = AssetHelper.Parse(instrumentId);
                quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, priceableAsset.BasicAssetValuation);
            }
            return quotedAssetSetFactory.Create();
        }

        #endregion

        #region Build the Asset Properties

        /// <summary>
        ///  converts data to properties.
        /// </summary>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="assetIdentifier">The valid asset identifier4.</param>
        /// <param name="baseDate">The Base date.</param>
        /// <param name="amount">The amount/notional or position.</param>
        /// <returns></returns>
        public static NamedValueSet BuildPropertiesForAssets(String nameSpace, String assetIdentifier, DateTime baseDate, Decimal? amount)
        {
            var namedValueSet = new NamedValueSet();
            string[] results = assetIdentifier.Split('-');
            string ccy = results[0];
            string type = results[1];
            namedValueSet.Set(CurveProp.AssetType, type);
            namedValueSet.Set(CurveProp.Currency1, ccy);
            namedValueSet.Set(CurveProp.AssetId, assetIdentifier);
            namedValueSet.Set(CurveProp.BaseDate, baseDate);
            namedValueSet.Set(EnvironmentProp.NameSpace, nameSpace);
            var assetType = (AssetTypesEnum)Enum.Parse(typeof(AssetTypesEnum), type, true);
            string expiryTerm = results[2];
            if (amount.HasValue)
            {
                namedValueSet.Set("Notional", amount);
            }
            switch (assetType)
            {
                case AssetTypesEnum.SimpleFra:
                case AssetTypesEnum.Fra:
                case AssetTypesEnum.BillFra:
                case AssetTypesEnum.SpreadFra:
                {
                    namedValueSet.Set("StartTerm", expiryTerm);
                    if (results.Length > 3)
                    {
                        string indexTerm = results[3];
                        namedValueSet.Set("ExtraItem", indexTerm);
                    }
                    break;
                }
                case AssetTypesEnum.Caplet:
                case AssetTypesEnum.Floorlet:
                case AssetTypesEnum.BillCaplet:
                case AssetTypesEnum.BillFloorlet:
                {
                    namedValueSet.Set("StartTerm", expiryTerm);
                    if (results.Length > 3)
                    {
                        string indexTerm = results[3];
                        namedValueSet.Set("ExtraItem", indexTerm);
                    }
                    if (results.Length > 4)
                    {
                        string strike = results[4];
                        namedValueSet.Set("Strike", strike);
                    }
                    break;
                }
                case AssetTypesEnum.BondForward:
                {
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExtraItem", term);
                    }
                    break;
                }
                case AssetTypesEnum.Bond:
                {
                    namedValueSet.Set("StartTerm", expiryTerm);
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    var bondElements = results[2].Split('.');
                    if (bondElements.Length > 4)
                    {
                        var temp = bondElements[3];
                        var coupon = Decimal.Parse(temp.Replace(',', '.')) / 100;
                        var maturity = bondElements[4];
                        namedValueSet.Set("Maturity", maturity);
                        namedValueSet.Set("Coupon", coupon);
                    }
                    break;
                }
                case AssetTypesEnum.CPISwap:
                case AssetTypesEnum.XccySwap:
                case AssetTypesEnum.ZeroRate:
                case AssetTypesEnum.Xibor:
                case AssetTypesEnum.OIS:
                case AssetTypesEnum.SpreadDeposit:
                case AssetTypesEnum.Deposit:
                case AssetTypesEnum.XccyDepo:
                case AssetTypesEnum.BankBill:
                case AssetTypesEnum.FxSpot:
                case AssetTypesEnum.FxForward:
                case AssetTypesEnum.CommoditySpot:
                case AssetTypesEnum.Repo:
                case AssetTypesEnum.RepoSpread:
                case AssetTypesEnum.Equity:
                case AssetTypesEnum.BondSpot:
                {
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    namedValueSet.Set("Term", expiryTerm);
                    break;
                }
                case AssetTypesEnum.IRCap:
                case AssetTypesEnum.IRFloor:
                {
                    namedValueSet.Set("Term", expiryTerm);
                    if (results.Length > 3)
                    {
                        string indexTerm = results[3];
                        namedValueSet.Set("ExtraItem", indexTerm);
                    }
                    if (results.Length > 4)
                    {
                        string strike = results[4];
                        namedValueSet.Set("Strike", strike);
                    }
                    break;
                }
                case AssetTypesEnum.BasisSwap:
                case AssetTypesEnum.XccyBasisSwap:
                case AssetTypesEnum.ResettableXccyBasisSwap:
                case AssetTypesEnum.IRSwap:
                case AssetTypesEnum.OISSwap:
                case AssetTypesEnum.ClearedIRSwap:
                {
                    namedValueSet.Set("Term", expiryTerm);
                    if (results.Length > 3)
                    {
                        namedValueSet.Set("IndexTerm", results[3]);
                        expiryTerm = expiryTerm + "." + results[3];
                    }
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    break;
                }
                case AssetTypesEnum.ZCCPISwap:
                case AssetTypesEnum.CPIndex:
                {
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    namedValueSet.Set("Term", expiryTerm);
                    break;
                }
                case AssetTypesEnum.EquityForward:
                case AssetTypesEnum.CommodityForward:
                case AssetTypesEnum.CommoditySpread:
                {
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryTerm", term);
                    }
                    break;
                }
                case AssetTypesEnum.CommodityAverageForward:
                {
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryTerm", term);
                        var underlyingTenor = results[4];
                        namedValueSet.Set("UnderlyingTenor", underlyingTenor);
                    }
                    break;
                }
                case AssetTypesEnum.CommodityFuture:
                case AssetTypesEnum.CommodityFutureSpread:
                case AssetTypesEnum.IRFuture:
                {
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    if (amount.HasValue)
                    {
                        namedValueSet.Set("Position", amount);
                    }
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryCode", term);
                    }
                    break;
                }
                case AssetTypesEnum.IRFutureOption:
                {
                    namedValueSet.Set("OptionType", "Put");
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    if (amount.HasValue)
                    {
                        namedValueSet.Set("Position", amount);
                    }
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryCode", term);
                    }
                    if (results.Length > 4)
                    {
                        string strike = results[4];
                        namedValueSet.Set("Strike", strike);
                    }
                    break;
                }
                case AssetTypesEnum.IRCallFutureOption:
                {
                    namedValueSet.Set("OptionType", "Call");
                    namedValueSet.Set("AssetType", AssetTypesEnum.IRFutureOption.ToString());
                    namedValueSet.Set("ExtraItem", expiryTerm);
                    if (amount.HasValue)
                    {
                        namedValueSet.Set("Position", amount);
                    }
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryCode", term);
                    }
                    if (results.Length > 4)
                    {
                        string strike = results[4];
                        namedValueSet.Set("Strike", strike);
                    }
                    break;
                }
                case AssetTypesEnum.IRPutFutureOption:
                {
                    namedValueSet.Set("OptionType", "Put");
                    namedValueSet.Set("AssetType", AssetTypesEnum.IRFutureOption.ToString());
                        namedValueSet.Set("ExtraItem", expiryTerm);
                    if (amount.HasValue)
                    {
                        namedValueSet.Set("Position", amount);
                    }
                    if (results.Length > 3)
                    {
                        string term = results[3];
                        namedValueSet.Set("ExpiryCode", term);                    
                    }
                    if(results.Length > 4)
                    {
                        string strike = results[4];
                        namedValueSet.Set("Strike", strike);
                    }
                    break;
                }
                default:
                    throw new NotSupportedException($"Asset type {assetType} is not supported");

            }
            return namedValueSet;
        }


        /// <summary>
        ///  converts data to properties.
        /// </summary>
        /// <param name="assetIdentifier">The valid asset identifier4.</param>
        /// <param name="nameSpace">THe namespace</param>
        /// <param name="baseDate">The Base date.</param>
        /// <returns></returns>
        public static NamedValueSet BuildPropertiesForAssets(string nameSpace, String assetIdentifier, DateTime baseDate)
        {
            return BuildPropertiesForAssets(nameSpace, assetIdentifier, baseDate, null);
        }

        /// <summary>
        ///  converts data to properties.
        /// </summary>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="assetIdentifier">The valid asset identifier4.</param>
        /// <param name="baseDate">The Base date.</param>
        /// <param name="coupon">The bond coupon.</param>
        /// <param name="maturity">The bond maturity.</param>
        /// <returns></returns>
        public static NamedValueSet BuildPropertiesForBondAssets(String nameSpace, String assetIdentifier, DateTime baseDate, Decimal coupon, DateTime maturity)
        {
            var namedValueSet = new NamedValueSet();
            string[] results = assetIdentifier.Split('-');
            string ccy = results[0];
            string type = results[1];
            string bondType = results[2];
            namedValueSet.Set(CurveProp.AssetType, type);
            namedValueSet.Set(CurveProp.Currency1, ccy);
            namedValueSet.Set(CurveProp.AssetId, assetIdentifier);
            namedValueSet.Set(CurveProp.BaseDate, baseDate);
            namedValueSet.Set("ExtraItem", bondType);
            namedValueSet.Set("Maturity", maturity);
            namedValueSet.Set("Coupon", coupon);
            namedValueSet.Set("BondType", bondType);
            namedValueSet.Set(EnvironmentProp.NameSpace, nameSpace);
            return namedValueSet;
        }

        #endregion

        #region Create Priceable Assets

        /// <summary>
        /// General create function.
        /// Extract the AssetId property. eg AUD-IRSwap-3Y.
        /// Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        /// Query the cache with the above properties and return the Instrument set.
        /// If this set is null, return an error message, asking to create a default instrument.
        /// Can use the CreateInstrument function in Excel.
        /// Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        /// Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        /// the asset is a rate index.
        /// Find the marketQuote and the volatility? in the BAV.
        /// Extract the base date from the properties.
        /// Call the PriceableAsset create functions to create an IPriceableAssetController.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="bav">THe basic asset valuation.</param>
        /// <param name="properties">The asset properties.</param>
        /// <param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <returns></returns>
        public static IPriceableAssetController Create(ILogger logger, ICoreCache cache, string nameSpace, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            try
            {
                var assetId = properties.GetValue<string>("AssetId", true);
                var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetId);
                return Create(logger, cache, nameSpace, instrument, bav, properties, fixingcalendar, paymentCalendar);
            }
            catch (System.Exception ex)
            {              
                logger.Log(ex);
            }
            return null;
        }

        ///  <summary>
        ///  Creates the specified asset.
        ///  </summary>
        ///  <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="assetIdentifier">The asset identifier.</param>
        ///  <param name="baseDate">The base date.</param>
        ///  <param name="basicAssetValuation">The BasicAssetValuation.</param>
        ///  <param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        ///  <returns>The string id.</returns>
        public static IPriceableAssetController Create(ILogger logger, ICoreCache cache, string nameSpace,
            string assetIdentifier, DateTime baseDate, BasicAssetValuation basicAssetValuation,
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            try
            {
                NamedValueSet assetProperties = BuildPropertiesForAssets(nameSpace, assetIdentifier, baseDate);
                Instrument instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, instrument, basicAssetValuation, assetProperties, fixingcalendar, paymentCalendar);
                return priceableAsset;
            }
            catch (System.Exception ex)
            {
                logger.Log(ex);
            }
            return null;
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureType">The additional.</param>
        /// <param name="priceQuoteUnits">The units of measure.</param>
        /// <param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <returns>The string id.</returns>
        internal static IPriceableAssetController Create(ILogger logger, ICoreCache cache, string nameSpace,
            string assetIdentifier, DateTime baseDate, Decimal[] values, string[] measureType, string[] priceQuoteUnits,
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            try
            {
                var properties = BuildPropertiesForAssets(nameSpace, assetIdentifier, baseDate);
                var priceableAsset = BuildPriceableAsset(logger, cache, nameSpace, values, measureType, priceQuoteUnits, properties, fixingcalendar, paymentCalendar, out string _);
                return priceableAsset;
            }
            catch (System.Exception ex)
            {
                logger.Log(ex);
            }
            return null;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureType">The additional.</param>
        /// <param name="priceQuoteUnits">The units of measure.</param>
        /// <param name="properties">the properties.</param>
        /// <param name="message">A debug message for information purposes.</param>
        /// <param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <returns>The string id.</returns>
        public static IPriceableAssetController BuildPriceableAsset(ILogger logger, ICoreCache cache, 
            string nameSpace, Decimal[] values, string[] measureType, string[] priceQuoteUnits, 
            NamedValueSet properties, IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar, out string message)
        {
            //sets the default.
            message = "Asset not built-";
            try
            {
                string bondId = "";
                //AssetType property - make sure it exists.
                var assetType = properties.GetValue<string>("AssetType", true);
                if (assetType == null)
                {
                    message = message + "because of a non-existent AssetType property.";
                    return null;
                }
                Boolean isBondType = assetType.Equals("BOND");
                //make sure there is an AssetId.
                var assetIdentifier = properties.GetValue<string>("AssetId", true);
                if (assetIdentifier == null)
                {
                    message = message + "because of a non-existent AssetId property.";
                    return null;
                }
                //check there is a maturity property if it is a bond type.
                if (isBondType)
                {
                    message = "This is a bond type.";
                    //make sure there is a maturity for the bond.
                    var bondMaturity = properties.GetValue<DateTime>("Maturity", true);
                    //make sure there is a coupon for the bond.
                    var coupon = properties.GetValue<Decimal>("Coupon", true);
                    bondId = assetIdentifier + '-' + coupon + '-' + bondMaturity.ToShortDateString();
                }
                //make sure there is a base date.
                var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
                //create the asset-basic asset valuation pair.
                var asset = AssetHelper.CreateAssetPair(assetIdentifier, values, measureType, priceQuoteUnits);
                //gets the data group.
                var datagroup = properties.GetValue<string>(CurveProp.DataGroup, false) ?? "Local.PriceableAsset";
                //sets up the unique identifier.
                var uniqueIdentifier = properties.GetValue<string>(CurveProp.UniqueIdentifier, false);
                if (uniqueIdentifier == null)
                {
                    var uniqueId = isBondType
                                                    ? datagroup + "." + bondId
                                                    : datagroup + "." + assetIdentifier + '-' + values[0] + '-' +
                                                      baseDate.ToShortDateString();
                    properties.Set(CurveProp.UniqueIdentifier, uniqueId);
                }
                //create the priceable asset.
                var priceableAsset = Create(logger, cache, nameSpace, asset.Second, properties, fixingcalendar, paymentCalendar);
                //return a message
                message = "Asset built successfully.";
                //return the cache id.
                return priceableAsset;
            }
            catch (System.Exception ex)
            {
                logger.Log(ex);
            }
            return null;
        }

        /// <summary>
        /// General create function.
        /// Extract the AssetId property. eg AUD-IRSwap-3Y.
        /// Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        /// Query the cache with the above properties and return the Instrument set.
        /// If this set is null, return an error message, asking to create a default instrument.
        /// Can use the CreateInstrument function in Excel.
        /// Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        /// Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        /// the asset is a rate index.
        /// Find the marketQuote and the volatility? in the BAV.
        /// Extract the base date from the properties.
        /// Call the PriceableAsset create functions to create an IPriceableAssetController.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache that must contain all required calendar reference data.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instrument as previously retrieved from the cache. </param>
        /// <param name="bav">THe basic asset valuation with the market data.</param>
        /// <param name="properties">A bag of properties.</param>
        /// <param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <returns></returns>
        public static IPriceableAssetController Create(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties, 
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            //Check the asset type and get the appropriate instrument details from the cache..
            //
            var type = properties.GetValue<string>("AssetType", true);
            var assetType = (AssetTypesEnum)Enum.Parse(typeof(AssetTypesEnum), type, true);
            //Create the appropriate priceable asset.
            IPriceableAssetController priceableAsset;
            //Clone the instrument to ensure immutability
            var cloneInstrument = XmlSerializerHelper.Clone(instrument);
            switch (assetType)
            {
                case AssetTypesEnum.SimpleFra:
                case AssetTypesEnum.Fra:
                    {
                        priceableAsset = CreateSimpleFra(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BillFra:
                    {
                        priceableAsset = CreateBillFra(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Caplet:
                    {
                        priceableAsset = CreateCaplet(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Floorlet:
                    {
                        priceableAsset = CreateFloorlet(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BillCaplet:
                    {
                        priceableAsset = CreateBillCaplet(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BillFloorlet:
                    {
                        priceableAsset = CreateBillFloorlet(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.SpreadFra:
                    {
                        priceableAsset = CreateSpreadFra(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.SpreadDeposit:
                    {
                        priceableAsset = CreateSpreadDeposit(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BasisSwap:
                    {
                        priceableAsset = CreateSimpleBasisSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.IRSwap:
                    {
                        priceableAsset = CreateSimpleIrSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.XccySwap:
                    {
                        priceableAsset = CreateSimpleIrSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.XccyBasisSwap:
                    {
                        priceableAsset = CreateXccyBasisSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.ResettableXccyBasisSwap:
                    {
                        priceableAsset = CreateResettableXccyBasisSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CPISwap:
                    {
                        priceableAsset = CreateRevenueInflationSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.ZCCPISwap:
                    {
                        priceableAsset = CreateZcInflationSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.IRFloor:
                {
                    priceableAsset = CreateIRFloor(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                    break;
                }
                case AssetTypesEnum.IRCap:
                    {
                        priceableAsset = CreateIRCap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.ZeroRate:
                    {
                        priceableAsset = CreateZeroRate(logger, cache, nameSpace, cloneInstrument, bav, properties, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Xibor:
                    {
                        priceableAsset = CreateXibor(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.OIS:
                    {
                        priceableAsset = CreateOis(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.OISSwap:
                    {
                        priceableAsset = CreateOisSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CPIndex:
                    {
                        priceableAsset = CreateInflationXibor(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Deposit:
                    {
                        priceableAsset = CreateDeposit(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.XccyDepo:
                    {
                        priceableAsset = CreateDeposit(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BankBill:
                    {
                        priceableAsset = CreateBankBill(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.IRFuture:
                {
                    priceableAsset = CreateIRFuture(logger, cache, nameSpace, cloneInstrument, bav, properties, paymentCalendar);
                    break;
                }
                case AssetTypesEnum.IRFutureOption:
                    {
                        priceableAsset = CreateIRFutureOption(logger, cache, nameSpace, cloneInstrument, true, bav, properties, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.IRPutFutureOption:
                {
                    priceableAsset = CreateIRFutureOption(logger, cache, nameSpace, cloneInstrument, true, bav, properties, paymentCalendar);
                    break;
                }
                case AssetTypesEnum.IRCallFutureOption:
                {
                    priceableAsset = CreateIRFutureOption(logger, cache, nameSpace, cloneInstrument, false, bav, properties, paymentCalendar);
                    break;
                }
                case AssetTypesEnum.FxSpot:
                    {
                        priceableAsset = CreateFxSpotRate(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.FxForward:
                    {
                        priceableAsset = CreateFxForwardRate(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommoditySpot:
                    {
                        priceableAsset = CreateCommoditySpot(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommodityForward:
                    {
                        priceableAsset = CreateCommodityForward(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommodityAverageForward:
                    {
                        priceableAsset = CreateCommodityAverageForward(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommodityFuture:
                    {
                        priceableAsset = CreateCommodityFuture(logger, cache, nameSpace, cloneInstrument, bav, properties, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommodityFutureSpread:
                    {
                        priceableAsset = CreateCommodityFutureSpread(logger, cache, nameSpace, cloneInstrument, bav, properties, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.CommoditySpread:
                    {
                        priceableAsset = CreateCommoditySpread(logger, cache, nameSpace, cloneInstrument, bav, properties, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.BondSpot:
                    {
                        priceableAsset = CreateBondSpot(logger, cache, nameSpace, cloneInstrument, bav, BondPriceEnum.YieldToMaturity, properties, fixingcalendar);
                        break;
                    }
                case AssetTypesEnum.BondForward:
                    {
                        priceableAsset = CreateBondForward(logger, cache, nameSpace, cloneInstrument, bav, BondPriceEnum.YieldToMaturity, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Bond:
                    {
                        priceableAsset = CreateBond(logger, cache, nameSpace, cloneInstrument, bav, BondPriceEnum.YieldToMaturity, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.Equity:
                    {
                        priceableAsset = CreateEquitySpot(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar);
                        break;
                    }
                case AssetTypesEnum.EquityForward:
                    {
                        priceableAsset = CreateEquityForward(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar);
                        break;
                    }
                case AssetTypesEnum.Repo:
                    {
                        priceableAsset = CreateRepo(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.RepoSpread:
                    {
                        priceableAsset = CreateSpreadRepo(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                case AssetTypesEnum.ClearedIRSwap:
                    {
                        priceableAsset = CreateClearedIRSwap(logger, cache, nameSpace, cloneInstrument, bav, properties, fixingcalendar, paymentCalendar);
                        break;
                    }
                default:
                    throw new NotSupportedException($"Asset type {assetType} is not supported");
            }
            priceableAsset.BasicAssetValuation = bav;
            return priceableAsset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommodityFuture"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="rollCalendar"></param>
        private static IPriceableAssetController CreateCommodityFuture(ILogger logger, ICoreCache cache, string nameSpace, 
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties, IBusinessCalendar rollCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var code = properties.GetValue<string>("ExtraItem", true);
                    var expiryCode = properties.GetValue<string>("ExpiryCode", false);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommodityFutureNodeStruct nodeStruct && expiryCode != "Unknown Property" && nodeStruct.Future != null)
                    {
                        nodeStruct.Future.id = assetId;
                        if (rollCalendar == null)
                        {
                            rollCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        var codes = code.Split('.');
                        if (codes.Length < 2) return null;
                        var exchangeCode = EnumHelper.Parse<ExchangeIdentifierEnum>(nodeStruct.Future.exchangeId.Value);//codes[0]
                        var contractCode = codes[1];
                        if (exchangeCode != ExchangeIdentifierEnum.XLME)//Temporary until the other futures classes are changed to be configurable.
                        {
                            var codeEnum = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(contractCode);
                            switch (codeEnum)
                            {
                                case RateFutureAssetAnalyticModelIdentifier.W:
                                    return new PriceableWheatFuture(baseDate, nodeStruct, rollCalendar, normalisedRate);
                                case RateFutureAssetAnalyticModelIdentifier.CER:
                                    return new PriceableCER(baseDate, nodeStruct, rollCalendar, normalisedRate);
                                case RateFutureAssetAnalyticModelIdentifier.B:
                                    return new PriceableIceBrentFuture(baseDate, nodeStruct, rollCalendar,
                                                                       normalisedRate);
                                default:
                                    throw new NotSupportedException($"Futures code {code} is not supported");
                            }
                        }
                        switch (exchangeCode)
                        {
                            case ExchangeIdentifierEnum.XLME:
                                return new PriceableLMEFuture(baseDate, nodeStruct, rollCalendar, normalisedRate);
                            default:
                                throw new NotSupportedException($"Futures code {code} is not supported");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommodityFutureSpread"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="calendar"></param>
        private static IPriceableAssetController CreateCommodityFutureSpread(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties, IBusinessCalendar calendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var code = properties.GetValue<string>("ExtraItem", true);
                    var expiryCode = properties.GetValue<string>("ExpiryCode", false);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommodityFutureNodeStruct nodeStruct && expiryCode != "Unknown Property")
                    {
                        nodeStruct.Future.id = assetId;
                        DateTime riskDate;
                        if (calendar == null)
                        {
                            calendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        var codes = code.Split('.');
                        if (codes.Length < 2) return null;
                        var exchangeCode = EnumHelper.Parse<ExchangeIdentifierEnum>(nodeStruct.Future.exchangeId.Value);//codes[0]
                        var contractCode = codes[1];
                        if (exchangeCode != ExchangeIdentifierEnum.XLME)//Temporary until the other futures classes are changed to be configurable.
                        {
                            var codeEnum = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(contractCode);
                            switch (codeEnum)
                            {
                                case RateFutureAssetAnalyticModelIdentifier.W:
                                    riskDate = new PriceableWheatFuture(baseDate, nodeStruct, calendar, normalisedRate).RiskMaturityDate;
                                    break;
                                case RateFutureAssetAnalyticModelIdentifier.CER:
                                    riskDate = new PriceableCER(baseDate, nodeStruct, calendar, normalisedRate).RiskMaturityDate;
                                    break;
                                case RateFutureAssetAnalyticModelIdentifier.B:
                                    riskDate = new PriceableIceBrentFuture(baseDate, nodeStruct, calendar,
                                                                       normalisedRate).RiskMaturityDate;
                                    break;
                                default:
                                    throw new NotSupportedException($"Futures code {code} is not supported");
                            }
                            return new PriceableCommoditySpread(assetId, baseDate, riskDate, normalisedRate);
                        }
                        switch (exchangeCode)
                        {
                            case ExchangeIdentifierEnum.XLME:
                                riskDate = new PriceableLMEFuture(baseDate, nodeStruct, calendar, normalisedRate).RiskMaturityDate;
                                break;
                            default:
                                throw new NotSupportedException($"Futures code {code} is not supported");
                        }
                        return new PriceableCommoditySpread(assetId, baseDate, riskDate, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommoditySpread"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="calendar"></param>
        private static IPriceableAssetController CreateCommoditySpread(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties, IBusinessCalendar calendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExpiryTerm", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    //var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommoditySpotNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (calendar == null)
                        {
                            calendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (nodeStruct.Commodity != null)
                        {
                            nodeStruct.Commodity.id = assetId;
                        }
                        else
                        {
                            nodeStruct.Commodity = new Commodity { id = assetId };
                        }
                        return new PriceableCommoditySpread(assetId, baseDate, term, nodeStruct, calendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIRFuture"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="rollCalendar"></param>
        public static IPriceableAssetController CreateIRFuture(ILogger logger, ICoreCache cache, string nameSpace, 
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties, IBusinessCalendar rollCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    int? position = properties.GetValue("Position", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(bav.quote);
                    BasicQuotation futuresVol = MarketQuoteHelper.FindQuotationByMeasureType("Volatility", bav.quote);
                    decimal futuresVolValue = futuresVol?.value ?? 0;
                    int positionValue = position.Value;
                    if (instrument.InstrumentNodeItem is IRFutureNodeStruct nodeStruct)
                    {
                        nodeStruct.Future.id = assetId;
                        if (rollCalendar == null)
                        {
                            rollCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        //var modelIdentifier = EnumHelper.Parse<FutureAssetAnalyticModelIdentifier>(code);
                        return new PriceableRateFuturesAsset(baseDate, positionValue, nodeStruct, rollCalendar, normalisedRate, futuresVolValue);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIRFutureOption"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="isPut">Is it a put?</param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="rollCalendar"></param>
        private static IPriceableAssetController CreateIRFutureOption(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, bool isPut, BasicAssetValuation bav, NamedValueSet properties, IBusinessCalendar rollCalendar)
        {
            if (instrument == null) return null;
            try
            {
                var assetId = properties.GetValue<string>("AssetId", true);
                int? position = properties.GetValue("Position", 1);
                var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                //Get the market quote, add the spread and normalise i.e. the fixed rate.
                BasicQuotation quotedVol = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(bav.quote);
                BasicQuotation strike = MarketQuoteHelper.FindQuotationByMeasureType("Strike", bav.quote);
                int positionValue = position.Value;
                if (instrument.InstrumentNodeItem is IRFutureOptionNodeStruct nodeStruct)
                {
                    nodeStruct.Future.id = assetId;
                    if (rollCalendar == null)
                    {
                        rollCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                    }
                    return new PriceableRateFuturesOptionAsset(baseDate, positionValue, isPut, nodeStruct, rollCalendar, quotedVol, strike.value);
                }
            }
            catch (System.Exception ex)
            {
                logger.Log(ex);
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBond"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instrument</param>
        /// <param name="bav">The basic asset valution</param>
        /// <param name="quoteType">The quote type</param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBond(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, BondPriceEnum quoteType, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var bondMaturity = properties.GetValue<DateTime>("Maturity", true);
                    var coupon = properties.GetValue<Decimal>("Coupon", true);
                    var notional = properties.GetValue<decimal>("Notional", 0);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BondNodeStruct nodeStruct)
                    {
                        nodeStruct.Bond.id = assetId;
                        nodeStruct.Bond.couponRate = coupon;
                        nodeStruct.Bond.couponRateSpecified = true;
                        nodeStruct.Bond.maturity = bondMaturity;
                        nodeStruct.Bond.maturitySpecified = true;
                        if (notional != 0)
                        {
                            nodeStruct.Bond.faceAmount = notional;
                            nodeStruct.Bond.faceAmountSpecified = true;
                        }
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SettlementDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleBond(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate, quoteType);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBondSpot"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="quoteType">THe qote type</param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        private static IPriceableAssetController CreateBondSpot(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, BondPriceEnum quoteType, NamedValueSet properties,
            IBusinessCalendar fixingCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    //var bondMaturity = properties.GetValue<DateTime>("Maturity", true);
                    //var coupon = properties.GetValue<Decimal>("Coupon", true);
                    //var notional = properties.GetValue<decimal>("Notional", 0);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BondNodeStruct nodeStruct)
                    {
                        nodeStruct.Bond.id = assetId;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SettlementDate.businessCenters, nameSpace);
                        }
                        return new PriceableBondSpot(baseDate, nodeStruct, fixingCalendar, normalisedRate, quoteType);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBondForward"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="quoteType">THe qote type</param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBondForward(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, BondPriceEnum quoteType, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var tenor = properties.GetValue<String>("ExtraItem", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BondNodeStruct nodeStruct)
                    {
                        nodeStruct.Bond.id = assetId;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SettlementDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        var period = PeriodHelper.Parse(tenor);
                        return new PriceableBondForward(baseDate, nodeStruct, period, fixingCalendar, paymentCalendar, normalisedRate, quoteType);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEquitySpot"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        private static IPriceableAssetController CreateEquitySpot(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar settlementCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var position = properties.GetValue("Position", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuotePriceAddSpreadAndNormalise(quotes);
                    var nodeStruct = instrument.InstrumentNodeItem as EquityNodeStruct;
                    //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                    //Otherwise a calendar is created.
                    if (settlementCalendar == null)
                    {
                        if (nodeStruct != null)
                            settlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SettlementDate.businessCenters, nameSpace);
                    }
                    return new PriceableEquitySpot(baseDate, position, nodeStruct, settlementCalendar, normalisedRate);
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEquityForward"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        private static IPriceableAssetController CreateEquityForward(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar settlementCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExpiryTerm", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var position = properties.GetValue("Position", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuotePriceAddSpreadAndNormalise(quotes);
                    var nodeStruct = instrument.InstrumentNodeItem as EquityNodeStruct;
                    if (nodeStruct != null)
                    {
                        var forwardTerm = PeriodHelper.Parse(term);
                        nodeStruct.SettlementDate.period = forwardTerm.period;
                        nodeStruct.SettlementDate.periodMultiplier = forwardTerm.periodMultiplier;
                        nodeStruct.SettlementDate.periodSpecified = true;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (settlementCalendar == null)
                        {
                            settlementCalendar = BusinessCenterHelper.ToBusinessCalendar(cache,
                                                                                             nodeStruct.SettlementDate
                                                                                                       .businessCenters,
                                                                                             nameSpace);
                        }
                    }
                    return new PriceableEquityForward(baseDate, position, nodeStruct, settlementCalendar, normalisedRate);
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBankBill"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBankBill(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BankBillNodeStruct nodeStruct)
                    {
                        nodeStruct.Deposit.id = assetId;
                        //this handles term types like: SP (Spot) and TN (Tom next).
                        nodeStruct.Deposit.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableBankBill(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar,
                                                                        nodeStruct.BusinessDayAdjustments, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableOis"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateOis(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is XiborNodeStruct nodeStruct)
                    {
                        nodeStruct.RateIndex.id = assetId;
                        nodeStruct.RateIndex.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableOis(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableOis"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateOisSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("Term", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                        nodeStruct.SimpleIRSwap.id = assetId;
                        //Set the notional.
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.Calculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleIRSwap(baseDate, nodeStruct.SimpleIRSwap, nodeStruct.SpotDate, nodeStruct.Calculation,
                                                         nodeStruct.DateAdjustments, nodeStruct.UnderlyingRateIndex, fixingCalendar,
                                                         paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateXibor(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is XiborNodeStruct nodeStruct)
                    {
                        nodeStruct.RateIndex.id = assetId;
                        nodeStruct.RateIndex.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableXibor(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInflationXibor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateInflationXibor(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is XiborNodeStruct nodeStruct)
                    {
                        nodeStruct.RateIndex.id = assetId;
                        nodeStruct.RateIndex.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableInflationXibor(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxForwardRate"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateFxForwardRate(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is FxSpotNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        var fxRateAsset = new FxRateAsset { id = assetId, quotedCurrencyPair = nodeStruct.QuotedCurrencyPair };
                        if (term == "ON")
                        {
                            return new PriceableFxONRate(baseDate, notional, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, normalisedRate);
                        }
                        if (term == "TN")
                        {
                            return new PriceableFxTNRate(baseDate, notional, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, normalisedRate);
                        }
                        return new PriceableFxForwardRate(baseDate, term, notional, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxSpotRate"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateFxSpotRate(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    //var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is FxSpotNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        var fxRateAsset = new FxRateAsset { id = assetId, quotedCurrencyPair = nodeStruct.QuotedCurrencyPair };
                        return new PriceableFxSpotRate(baseDate, notional, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommoditySpot"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateCommoditySpot(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    //string code = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommoditySpotNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (nodeStruct.Commodity != null)
                        {
                            nodeStruct.Commodity.id = assetId;
                        }
                        else
                        {
                            nodeStruct.Commodity = new Commodity { id = assetId };
                        }
                        return new PriceableCommoditySpot(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommodityForward"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateCommodityAverageForward(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExpiryTerm", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var underlyingTenor = properties.GetValue<string>("UnderlyingTenor", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommodityAverageSwapNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (nodeStruct.Commodity != null)
                        {
                            nodeStruct.Commodity.id = assetId;
                        }
                        else
                        {
                            nodeStruct.Commodity = new Commodity { id = assetId };
                        }
                        return new PriceableCommodityAverageForward(baseDate, notional, term, underlyingTenor, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommodityForward"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateCommodityForward(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExpiryTerm", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is CommoditySpotNodeStruct nodeStruct)
                    {
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (nodeStruct.Commodity != null)
                        {
                            nodeStruct.Commodity.id = assetId;
                        }
                        else
                        {
                            nodeStruct.Commodity = new Commodity { id = assetId };
                        }
                        return new PriceableCommodityForward(baseDate, notional, term, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleDiscountFra"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBillFra(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                var startTerm = properties.GetValue<string>("StartTerm", false);
                var indexTerm = properties.GetValue<string>("ExtraItem", false);
                if (startTerm != null && indexTerm != null)
                {
                    try
                    {
                        var assetId = properties.GetValue<string>("AssetId", true);
                        var notional = properties.GetValue<decimal>("Notional", 1);
                        var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                        var quotes = new List<BasicQuotation>(bav.quote);
                        //Get the market quote, add the spread and normalise i.e. the fixed rate.
                        BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                        var nodeStruct = (SimpleFraNodeStruct)instrument.InstrumentNodeItem;
                        nodeStruct.SimpleFra.id = assetId;
                        nodeStruct.SimpleFra.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleFra.endTerm = nodeStruct.SimpleFra.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleDiscountFra(baseDate, nodeStruct, fixingCalendar, paymentCalendar, notional, normalisedRate);
                    }
                    catch (System.Exception ex)
                    {
                        logger.Log(ex);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleFra"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        public static IPriceableAssetController CreateSimpleFra(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleFraNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleFra.id = assetId;
                        nodeStruct.SimpleFra.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleFra.endTerm = nodeStruct.SimpleFra.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleFra(baseDate, nodeStruct, fixingCalendar, paymentCalendar, notional, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCaplet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateCaplet(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue("Notional", 1.0m);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is RateOptionNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleRateOption.id = assetId;
                        nodeStruct.SimpleRateOption.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleRateOption.endTerm
                            = nodeStruct.SimpleRateOption.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableCaplet(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, notional, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFloorlet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache"></param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument"></param>
        /// <param name="bav"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateFloorlet(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is RateOptionNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleRateOption.id = assetId;
                        nodeStruct.SimpleRateOption.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleRateOption.endTerm
                            = nodeStruct.SimpleRateOption.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableFloorlet(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar,
                            notional, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableZeroRate"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instrument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateZeroRate(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is ZeroRateNodeStruct nodeStruct)
                    {
                        Period tenor = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableZeroRate(assetId, baseDate, notional, tenor, nodeStruct, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDeposit"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateRepo(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is RepoNodeStruct nodeStruct)
                    {
                        nodeStruct.Deposit.id = assetId;
                        //this handles term types like: SP (Spot) and TN (Tom next).
                        nodeStruct.Deposit.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableRepo(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar,
                                                                        nodeStruct.BusinessDayAdjustments, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSpreadDeposit"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateSpreadRepo(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is RepoNodeStruct nodeStruct)
                    {
                        nodeStruct.Deposit.id = assetId;
                        nodeStruct.Deposit.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSpreadRepo(baseDate, nodeStruct, fixingCalendar,
                                                  paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDeposit"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateDeposit(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is DepositNodeStruct nodeStruct)
                    {
                        nodeStruct.Deposit.id = assetId;
                        //this handles term types like: SP (Spot) and TN (Tom next).
                        nodeStruct.Deposit.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableDeposit(baseDate, notional, nodeStruct, fixingCalendar, paymentCalendar,
                                                                        nodeStruct.BusinessDayAdjustments, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSpreadDeposit"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateSpreadDeposit(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is DepositNodeStruct nodeStruct)
                    {
                        nodeStruct.Deposit.id = assetId;
                        nodeStruct.Deposit.term = PeriodHelper.Parse(term);
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSpreadDeposit(baseDate, nodeStruct, fixingCalendar,
                                                  paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSpreadFra"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateSpreadFra(ILogger logger, ICoreCache cache, string nameSpace, 
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleFraNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleFra.id = assetId;
                        nodeStruct.SimpleFra.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleFra.endTerm = nodeStruct.SimpleFra.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSpreadFra(baseDate, nodeStruct, fixingCalendar,
                                                        paymentCalendar, nodeStruct.RateIndex,
                                                        normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDiscountCaplet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBillCaplet(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is RateOptionNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleRateOption.id = assetId;
                        nodeStruct.SimpleRateOption.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleRateOption.endTerm
                            = nodeStruct.SimpleRateOption.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableDiscountCaplet(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar,
                            notional, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDiscountFloorlet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateBillFloorlet(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var startTerm = properties.GetValue<string>("StartTerm", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is RateOptionNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleRateOption.id = assetId;
                        nodeStruct.SimpleRateOption.startTerm = PeriodHelper.Parse(startTerm);
                        nodeStruct.SimpleRateOption.endTerm
                            = nodeStruct.SimpleRateOption.startTerm.Sum(PeriodHelper.Parse(indexTerm));
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableDiscountFloorlet(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar,
                            notional, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateIRCap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("Term", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", false);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is SimpleIRCapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRCap.term = PeriodHelper.Parse(term);
                        if (indexTerm != null)
                        {
                            nodeStruct.SimpleIRCap.paymentFrequency = PeriodHelper.Parse(indexTerm);
                        }
                        nodeStruct.SimpleIRCap.id = assetId;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleIRCap(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateIRFloor(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("Term", true);
                    var indexTerm = properties.GetValue<string>("ExtraItem", false);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    if (instrument.InstrumentNodeItem is SimpleIRCapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRCap.term = PeriodHelper.Parse(term);
                        if (indexTerm != null)
                        {
                            nodeStruct.SimpleIRCap.paymentFrequency = PeriodHelper.Parse(indexTerm);
                        }
                        nodeStruct.SimpleIRCap.id = assetId;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleIRFloor(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, bav);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBasisSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instrument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateSimpleBasisSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var indexTerm = properties.GetValue<string>("IndexTerm", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    if (instrument.InstrumentNodeItem is BasisSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.MarginLeg.term = PeriodHelper.Parse(term);
                        //Invert the quote if the margi leg is not the index tenor.
                        BasicQuotation normalisedSpread = nodeStruct.MarginLeg.paymentFrequency.Equals(PeriodHelper.Parse(indexTerm))
                            ? MarketQuoteHelper.GetInverseMarketQuoteAddSpreadAndNormalise(quotes)
                            : MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);

                        nodeStruct.MarginLeg.id = assetId;
                        //Set the notional.
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.MarginLegCalculation.Item = amount;
                        //Set the notional - nodeStruct.MarginLegCalculation.Item
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.MarginLegDateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableBasisSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedSpread);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXccyBasisSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateXccyBasisSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedSpread = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BasisSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.MarginLeg.term = PeriodHelper.Parse(term);
                        nodeStruct.MarginLeg.id = assetId;
                        //Set the notional.
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.MarginLegCalculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.MarginLegDateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableXccyBasisSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedSpread);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableResettableXccyBasisSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instrument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateResettableXccyBasisSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedSpread = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is BasisSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.MarginLeg.term = PeriodHelper.Parse(term);
                        nodeStruct.MarginLeg.id = assetId;
                        //Set the notional.
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.MarginLegCalculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.MarginLegDateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableResettableXccyBasisSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedSpread);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateSimpleIrSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument == null) return null;
            try
            {
                var term = properties.GetValue<string>("ExtraItem", true);
                var assetId = properties.GetValue<string>("AssetId", true);
                var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                var notional = properties.GetValue<decimal>("Notional", 1);
                var quotes = new List<BasicQuotation>(bav.quote);
                //Get the market quote, add the spread and normalise i.e. the fixed rate.
                BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
                {
                    nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                    nodeStruct.SimpleIRSwap.id = assetId;
                    //Set the notional.;
                    Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                    nodeStruct.Calculation.Item = amount;
                    //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                    //Otherwise a calendar is created.
                    if (fixingCalendar == null)
                    {
                        fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                    }
                    if (paymentCalendar == null)
                    {
                        paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                    }
                    return new PriceableSimpleIRSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                }
            }
            catch (System.Exception ex)
            {
                logger.Log(ex);
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateClearedIRSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateClearedIRSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                        nodeStruct.SimpleIRSwap.id = assetId;
                        //Set the notional.;
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.Calculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableIRSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleZeroCouponInflationSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateZcInflationSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                        nodeStruct.SimpleIRSwap.id = assetId;
                        //Set the notional.
                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.Calculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleZeroCouponInflationSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar,
                                                                          normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleRevenueInflationSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="instrument">The instument class.</param>
        /// <param name="bav">A basic asset valuation.</param>
        /// <param name="properties">A property bag.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        private static IPriceableAssetController CreateRevenueInflationSwap(ILogger logger, ICoreCache cache, string nameSpace,
            Instrument instrument, BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (instrument != null)
            {
                try
                {
                    var term = properties.GetValue<string>("ExtraItem", true);
                    var assetId = properties.GetValue<string>("AssetId", true);
                    var baseDate = properties.GetValue<DateTime>("BaseDate", true);
                    var notional = properties.GetValue<decimal>("Notional", 1);
                    var quotes = new List<BasicQuotation>(bav.quote);
                    //Get the market quote, add the spread and normalise i.e. the fixed rate.
                    BasicQuotation normalisedRate = MarketQuoteHelper.GetMarketQuoteAddSpreadAndNormalise(quotes);
                    if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
                    {
                        nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                        nodeStruct.SimpleIRSwap.id = assetId;
                        //Set the notional.

                        Notional amount = NotionalFactory.Create(MoneyHelper.GetAmount(notional));
                        nodeStruct.Calculation.Item = amount;
                        //This allows optimisation around calendar creation, so that one calendar can be instantiated for many assets.
                        //Otherwise a calendar is created.
                        if (fixingCalendar == null)
                        {
                            fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                        }
                        if (paymentCalendar == null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                        }
                        return new PriceableSimpleRevenueInflationSwap(baseDate, nodeStruct,
                                                                       fixingCalendar, paymentCalendar,
                                                                       normalisedRate);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Log(ex);
                }
            }
            return null;
        }

        #endregion

        #region Surfaces

        /// <summary>
        /// Creates an asset dependent on two curves.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="asset">The asset.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotation">The quotation.</param>
        /// <returns>A priceable asset controller.</returns>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        public static IPriceableAssetController CreateSurface(ILogger logger, ICoreCache cache, string nameSpace,
            Asset asset, Decimal strike, DateTime baseDate, BasicQuotation quotation,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            return CreateSurface(logger, cache, nameSpace, asset, null, strike, baseDate, quotation, fixingCalendar, paymentCalendar);
        }

        /// <summary>
        /// Creates an asset dependent on two curves.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="asset">The asset.</param>
        /// <param name="amount">The notional amount.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotation">The quotation.</param>
        /// <returns>A priceable asset controller.</returns>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        public static IPriceableAssetController CreateSurface(ILogger logger, ICoreCache cache, string nameSpace,
            Asset asset, Decimal? amount, Decimal strike, DateTime baseDate, BasicQuotation quotation,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var bqs = new[] { quotation };
            BasicAssetValuation bav = BasicAssetValuationHelper.Create(bqs);
            try
            {
                var namedValueSet = new NamedValueSet();
                string[] results = asset.id.Split('-');
                string ccy = results[0];
                string type = results[1];
                string startTerm = results[2];
                string indexTerm = results[3];
                namedValueSet.Set("AssetType", type);
                namedValueSet.Set(CurveProp.Currency1, ccy);
                namedValueSet.Set("AssetId", asset.id);
                namedValueSet.Set(CurveProp.BaseDate, baseDate);
                namedValueSet.Set("StartTerm", startTerm);
                namedValueSet.Set("ExtraItem", indexTerm);
                namedValueSet.Set("Strike", strike.ToString(CultureInfo.InvariantCulture));
                if (amount.HasValue)
                {
                    namedValueSet.Set("Notional", amount.Value);
                }
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, bav, namedValueSet, fixingCalendar, paymentCalendar);
                return priceableAsset;
            }
            catch
            {
                throw new NotSupportedException($"Asset Type {asset.GetType().Name} is not supported");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates the priceable asset data.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="marketEnvironment">The market.</param>
        /// <returns></returns>
        public static IAssetControllerData CreateAssetControllerData(string[] metrics, DateTime baseDate,
                                                                     IMarketEnvironment marketEnvironment)
        {
            var basicAssetValuation = new BasicAssetValuation
            {
                quote =
                    metrics.Select(
                        metricName => BasicQuotationHelper.Create(0.0m, metricName)).
                    ToArray()
            };
            return new AssetControllerData(basicAssetValuation, baseDate, marketEnvironment);
        }


        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="additionals">Additional data.</param>
        /// <returns></returns>
        public static List<IPriceableRateAssetController> CreatePriceableRateAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            string[] assetIdentifiers, decimal[] values, decimal[] additionals, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (assetIdentifiers.Length != values.Length ||
                additionals != null && assetIdentifiers.Length != additionals.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string assetIdentifier in assetIdentifiers)
            {
                if (string.IsNullOrEmpty(assetIdentifier)) break;
                var properties = new PriceableAssetProperties(assetIdentifier);
                decimal additional = additionals?[index] ?? 0;
                int arraySize = (properties.AssetType != AssetTypesEnum.IRFuture) ? 1 : 2;
                var vals = new Decimal[arraySize];
                var measures = new string[arraySize];
                var quotes = new string[arraySize];

                if (properties.AssetType == AssetTypesEnum.IRFuture)
                {
                    vals[0] = values[index];
                    measures[0] = "MarketQuote";
                    quotes[0] = "DecimalRate";
                    vals[1] = additional;
                    measures[1] = "Volatility";
                    quotes[1] = "LognormalVolatility";
                }
                else
                {
                    vals[0] = values[index] + additional;
                    measures[0] = "MarketQuote";
                    quotes[0] = "DecimalRate";
                }

                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, assetIdentifier, baseDate, vals, measures, quotes, fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableRateAssetController priceableRateAssetController)
                {
                    priceableAssets.Add(priceableRateAssetController);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="forecastRateTenor">The forecastRateTenor. The index itself is ignored.</param>
        /// <param name="instrumentData">The instrument Data.</param>
        /// <param name="baseDate">The base Date.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableRateAssetController> CreatePriceableRateAssetsWithBasisSwaps(ILogger logger, ICoreCache cache, string nameSpace,
            Period forecastRateTenor, QuotedAssetSet instrumentData, DateTime baseDate, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (forecastRateTenor == null)
            {
                return CreatePriceableRateAssets(logger, cache, nameSpace, baseDate, instrumentData, fixingCalendar, rollCalendar);
            }
            var xArray = new List<double>();
            var values = new List<double>();
            var forecastIndexTenor = forecastRateTenor.ToString();
            int j = 0;
            //The interpolator that may be required.
            IInterpolation spreadInterpolator = null;
            foreach (var asset in instrumentData.instrumentSet.Items)
            {
                var assetProperties = new PriceableAssetProperties(asset.id);
                //Ensure only the relevant basis swaps are included. They should be all of one currency and index.
                //
                if (assetProperties.AssetType == AssetTypesEnum.BasisSwap)
                {
                    var basisInstrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, asset.id);
                    var basisSwap = (BasisSwapNodeStruct)basisInstrument.InstrumentNodeItem;
                    //The x array of the interpolator.
                    //
                    var time = PeriodHelper.Parse(assetProperties.Term).ToYearFraction();
                    xArray.Add(time);
                    //The value array. We assume all the basis swaps supplied are on the same underlying.
                    //
                    var bqs = new List<BasicQuotation>(instrumentData.assetQuote[j].quote);
                    var val = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", bqs);
                    //Need to make sure the correct margin is added depending on the underlying index.
                    //
                    var marginIndex = basisSwap.MarginLegRateIndex.term.ToString();
                    if (marginIndex == forecastIndexTenor)
                    {
                        values.Add((double)-val.value);
                    }
                    var baseIndex = basisSwap.BaseLegRateIndex.term.ToString();
                    if (baseIndex == forecastIndexTenor)
                    {
                        values.Add((double)val.value);
                    }
                }
                j++;
            }
            //Instantiate a spread interpolator. This is linear only. TODO have other interpolators.
            //
            if (xArray.Count > 0)
            {
                spreadInterpolator = new LinearInterpolation();
                spreadInterpolator.Initialize(xArray.ToArray(), values.ToArray());
            }
            //Create the priceable assets.
            //
            var priceableAssets = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (var asset in instrumentData.instrumentSet.Items)
            {
                var inst = new PriceableAssetProperties(asset.id);
                if (inst.AssetType != AssetTypesEnum.BasisSwap)
                {
                    var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, asset.id);
                    if (inst.AssetType == AssetTypesEnum.IRSwap
                        || inst.AssetType == AssetTypesEnum.SimpleIRSwap)
                    {
                        var swap = (SimpleIRSwapNodeStruct)instrument.InstrumentNodeItem;
                        //Check the underlying index.
                        //
                        if (swap.UnderlyingRateIndex.term.ToString() != forecastIndexTenor &&
                            spreadInterpolator != null)
                        {
                            var bqs = new List<BasicQuotation>(instrumentData.assetQuote[index].quote);
                            var time = PeriodHelper.Parse(inst.Term).ToYearFraction();
                            var val = (decimal)spreadInterpolator.ValueAt(time, true);
                            //Careful here, as any spread will be overwritten.
                            //
                            MarketQuoteHelper.ReplaceQuotationByMeasureType("Spread", bqs, val);
                            instrumentData.assetQuote[index].quote = bqs.ToArray();
                        }
                    }
                    //Can't handle basis swaps yet as priceable assets.
                    //
                    var assetIdentifier = BuildPropertiesForAssets(nameSpace, asset.id, baseDate);
                    var priceableAsset = Create(logger, cache, nameSpace, instrument, instrumentData.assetQuote[index], assetIdentifier, fixingCalendar, rollCalendar);
                    if (priceableAsset is PriceableRateAssetController priceableRateAssetController)
                    {
                        priceableAssets.Add(priceableRateAssetController);
                    }
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableRateAssetController> CreatePriceableRateAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, QuotedAssetSet quotedAssetSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            int index = 0;
            var priceableAssets = new List<IPriceableRateAssetController>();
            foreach (Asset asset in quotedAssetSet.instrumentSet.Items)
            {
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableRateAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="assetIdentifier">The underlying instrument of the options.</param>
        /// <param name="strike">The strike, if any?</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        /// <returns></returns>     
        public static List<IPriceableOptionAssetController> CreatePriceableOptionAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, string assetIdentifier, 
            IRateCurve discountCurve, IRateCurve forecastCurve, Decimal? strike, QuotedAssetSet quotedAssetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var priceableAssets = new List<IPriceableOptionAssetController>();
            foreach (var quote in quotedAssetSet.assetQuote)
            {
                var term = quote.objectReference?.href;
                if (term != null)
                {
                    var expiryTenor = PeriodHelper.Parse(term);
                    var priceableOptionASset = new PriceableSimpleOptionAsset(logger, cache, nameSpace, assetIdentifier, baseDate, expiryTenor, strike, 
                        quote.quote[0].value, discountCurve, forecastCurve, fixingCalendar, rollCalendar);
                    priceableAssets.Add(priceableOptionASset);
                }
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableOptionAssetController> CreatePriceableRateOptionAssets(ILogger logger, ICoreCache cache, string nameSpace,
            DateTime baseDate, QuotedAssetSet quotedAssetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            int index = 0;
            var priceableAssets = new List<IPriceableOptionAssetController>();
            foreach (Asset asset in quotedAssetSet.instrumentSet.Items)
            {
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is IPriceableRateOptionAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableClearedRateAssetController> CreatePriceableClearedRateAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, 
            QuotedAssetSet quotedAssetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            int index = 0;
            var priceableAssets = new List<IPriceableClearedRateAssetController>();
            foreach (Asset asset in quotedAssetSet.instrumentSet.Items)
            {
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is IPriceableClearedRateAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="forecastRateTenor">The forecastRateTenor. The index itself is ignored.</param>
        /// <param name="instrumentData">The instrument Data.</param>
        /// <param name="baseDate">The base Date.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableClearedRateAssetController> CreatePriceableClearedRateAssetsWithBasisSwaps(ILogger logger, ICoreCache cache, string nameSpace,
            Period forecastRateTenor, QuotedAssetSet instrumentData, DateTime baseDate, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (forecastRateTenor == null)
            {
                return CreatePriceableClearedRateAssets(logger, cache, nameSpace, baseDate, instrumentData, fixingCalendar, rollCalendar);
            }
            var xArray = new List<double>();
            var values = new List<double>();
            var forecastIndexTenor = forecastRateTenor.ToString();
            int j = 0;
            //The interpolator that may be required.
            IInterpolation spreadInterpolator = null;
            foreach (var asset in instrumentData.instrumentSet.Items)
            {
                var assetProperties = new PriceableAssetProperties(asset.id);
                //Ensure only the relevant basis swaps are included. They should be all of one currency and index.
                //
                if (assetProperties.AssetType == AssetTypesEnum.BasisSwap)
                {
                    var basisInstrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, asset.id);
                    var basisSwap = (BasisSwapNodeStruct)basisInstrument.InstrumentNodeItem;
                    //The x array of the interpolator.
                    //
                    var time = PeriodHelper.Parse(assetProperties.Term).ToYearFraction();
                    xArray.Add(time);
                    //The value array. We assume all the basis swaps supplied are on the same underlying.
                    //
                    var bqs = new List<BasicQuotation>(instrumentData.assetQuote[j].quote);
                    var val = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", bqs);
                    //Need to make sure the correct margin is added depending on the underlying index.
                    //
                    var marginIndex = basisSwap.MarginLegRateIndex.term.ToString();
                    if (marginIndex == forecastIndexTenor)
                    {
                        values.Add((double)-val.value);
                    }
                    var baseIndex = basisSwap.BaseLegRateIndex.term.ToString();
                    if (baseIndex == forecastIndexTenor)
                    {
                        values.Add((double)val.value);
                    }
                }
                j++;
            }
            //Instantiate a spread interpolator. This is linear only. TODO have other interpolators.
            //
            if (xArray.Count > 0)
            {
                spreadInterpolator = new LinearInterpolation();
                spreadInterpolator.Initialize(xArray.ToArray(), values.ToArray());
            }
            //Create the priceable assets.
            //
            var priceableAssets = new List<IPriceableClearedRateAssetController>();
            int index = 0;
            foreach (var asset in instrumentData.instrumentSet.Items)
            {
                var inst = new PriceableAssetProperties(asset.id);
                if (inst.AssetType != AssetTypesEnum.BasisSwap)
                {
                    var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, asset.id);
                    if (inst.AssetType == AssetTypesEnum.IRSwap
                        || inst.AssetType == AssetTypesEnum.SimpleIRSwap
                        || inst.AssetType == AssetTypesEnum.ClearedIRSwap)
                    {
                        var swap = (SimpleIRSwapNodeStruct)instrument.InstrumentNodeItem;
                        //Check the underlying index.
                        //
                        if (swap.UnderlyingRateIndex.term.ToString() != forecastIndexTenor &&
                            spreadInterpolator != null)
                        {
                            var bqs = new List<BasicQuotation>(instrumentData.assetQuote[index].quote);
                            var time = PeriodHelper.Parse(inst.Term).ToYearFraction();
                            var val = (decimal)spreadInterpolator.ValueAt(time, true);
                            //Careful here, as any spread will be overwritten.
                            //
                            MarketQuoteHelper.ReplaceQuotationByMeasureType("Spread", bqs, val);
                            instrumentData.assetQuote[index].quote = bqs.ToArray();
                        }
                    }
                    //Can't handle basis swaps yet as priceable assets.
                    //
                    var assetIdentifier = BuildPropertiesForAssets(nameSpace, asset.id, baseDate);
                    var priceableAsset = Create(logger, cache, nameSpace, instrument, instrumentData.assetQuote[index], assetIdentifier, fixingCalendar, rollCalendar);
                    if (priceableAsset is IPriceableClearedRateAssetController priceableRateAssetController)
                    {
                        priceableAssets.Add(priceableRateAssetController);
                    }
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableRateSpreadAssetController> CreatePriceableRateSpreadAssets(ILogger logger, ICoreCache cache, string nameSpace,
            DateTime baseDate, QuotedAssetSet quotedAssetSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var index = 0;
            var priceableAssets = new List<IPriceableRateSpreadAssetController>();
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableRateSpreadAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Cuurently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <returns></returns>
        public static List<IPriceableRateAssetController> CreatePriceableRateAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate,
            string[] assetIdentifiers, decimal[] values, string[] measureTypes, string[] priceQuoteUnits,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (assetIdentifiers.Length != values.Length && assetIdentifiers.Length != priceQuoteUnits.Length && assetIdentifiers.Length != measureTypes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableRateAssetController>();
            var uniqueIds = (IEnumerable<string>)assetIdentifiers;
            foreach (var assetIdentifier in uniqueIds.Distinct())
            {
                var id = new PriceableAssetProperties(assetIdentifier);
                var index = 0;
                var vals = new List<Decimal>();
                var measures = new List<String>();
                var quotes = new List<String>();
                foreach (var ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    vals.Add(values[index - 1]);
                    measures.Add(measureTypes[index - 1]);
                    quotes.Add(priceQuoteUnits[index - 1]);
                }
                var priceableAsset = Create(logger, cache, nameSpace, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), fixingCalendar, rollCalendar);
                //Not efficient, but the spreadfras need to be excluded now they implement the IPriceableRateAssetController interface.
                if (priceableAsset is PriceableRateAssetController item && id.AssetType != AssetTypesEnum.SpreadFra)
                {
                    priceableAssets.Add(item);
                }
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableFuturesAssetController> CreatePriceableFuturesAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, QuotedAssetSet quotedAssetSet,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
        {
            int index = 0;
            var priceableAssets = new List<IPriceableFuturesAssetController>();
            foreach (Asset asset in quotedAssetSet.instrumentSet.Items)
            {
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], settlementCalendar, rollCalendar);
                if (priceableAsset is IPriceableFuturesAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableBondAssetController> CreatePriceableBondAssets(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, QuotedAssetSet quotedAssetSet,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
        {
            int index = 0;
            var priceableAssets = new List<IPriceableBondAssetController>();
            foreach (Asset asset in quotedAssetSet.instrumentSet.Items)
            {
                IPriceableAssetController priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], settlementCalendar, rollCalendar);
                if (priceableAsset is PriceableBondAssetController rateAsset)
                {
                    priceableAssets.Add(rateAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ThenBy(a => a.Id).ToList();
            return priceableAssets;
        }

        ///// <summary>
        ///// Creates the specified asset identifiers.
        ///// </summary>
        ///// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        ///// <param name="assetIdentifiers">The asset identifiers.</param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="values">The adjusted rates.</param>
        ///// <returns></returns>
        ///// <param name="additional">Additional data.</param>
        //public List<IPriceableCreditAssetController> CreatePriceableCreditAssets(ICoreCache cache, string[] assetIdentifiers, DateTime baseDate, Decimal[] values,
        //                                                                            Decimal[] additional)
        //{
        //    if ((assetIdentifiers.Length != values.Length) && (assetIdentifiers.Length != additional.Length))
        //    {
        //        throw new ArgumentOutOfRangeException("values", "the rates do not match the number of assets");
        //    }
        //    var priceableAssets = new List<IPriceableCreditAssetController>();
        //    var uniqueIds = (IEnumerable<string>)assetIdentifiers;
        //    foreach (var assetIdentifier in uniqueIds.Distinct())
        //    {
        //        var index = 0;
        //        var vals = new List<Decimal>();
        //        var measures = new List<String>();
        //        var quotes = new List<String>();
        //        foreach (var ids in assetIdentifiers)
        //        {
        //            index++;
        //            if (ids != assetIdentifier) continue;
        //            vals.Add(values[index - 1]);
        //            measures.Add("MarketQuote");
        //            quotes.Add("DecimalRate");
        //            vals.Add(additional[index - 1]);
        //            measures.Add("Volatility");
        //            quotes.Add("LognormalVolatility");
        //        }

        //        var priceableAsset = Create(cache, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), null, null);

        //        if (priceableAsset is PriceableCreditAssetController)
        //        {
        //            priceableAssets.Add((IPriceableCreditAssetController)priceableAsset);
        //        }
        //    }
        //    priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
        //    return priceableAssets;
        //}

        ///// <summary>
        ///// Creates the specified asset identifiers.
        ///// </summary>
        ///// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        ///// <param name="assetIdentifiers">The asset identifiers.</param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="values">The adjusted rates.</param>
        ///// <param name="measureTypes">The measure types. Cuurently supports MarketQuote and Volatility.</param>
        ///// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        ///// <returns></returns>
        //public List<IPriceableCreditAssetController> CreatePriceableCreditAssets(ICoreCache cache, string[] assetIdentifiers, DateTime baseDate, Decimal[] values,
        //                                                                            String[] measureTypes, String[] priceQuoteUnits)
        //{
        //    if ((assetIdentifiers.Length != values.Length) && (assetIdentifiers.Length != priceQuoteUnits.Length) && (assetIdentifiers.Length != measureTypes.Length))
        //    {
        //        throw new ArgumentOutOfRangeException("values", "the rates do not match the number of assets");
        //    }
        //    var priceableAssets = new List<IPriceableCreditAssetController>();
        //    var uniqueIds = (IEnumerable<string>)assetIdentifiers;
        //    foreach (var assetIdentifier in uniqueIds.Distinct())
        //    {
        //        var index = 0;
        //        var vals = new List<Decimal>();
        //        var measures = new List<String>();
        //        var quotes = new List<String>();
        //        foreach (var ids in assetIdentifiers)
        //        {
        //            index++;
        //            if (ids != assetIdentifier) continue;
        //            vals.Add(values[index - 1]);
        //            measures.Add(measureTypes[index - 1]);
        //            quotes.Add(priceQuoteUnits[index - 1]);
        //        }
        //        var priceableAsset = Create(cache, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), null, null);
        //        if (priceableAsset is PriceableCreditAssetController)
        //        {
        //            priceableAssets.Add((IPriceableCreditAssetController)priceableAsset);
        //        }
        //    }
        //    priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
        //    return priceableAssets;
        //}

        ///// <summary>
        ///// Builds the priceable asset set.
        ///// </summary>
        ///// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        ///// <param name="baseDate">The base date for assert creation.</param>
        ///// <param name="quotedAssetSet">The quotedAssetSet.</param>
        ///// <returns></returns>     
        //public List<IPriceableCreditAssetController> CreatePriceableCreditAssets(ICoreCache cache, DateTime baseDate, QuotedAssetSet quotedAssetSet)
        //{
        //    var index = 0;
        //    var priceableAssets = new List<IPriceableCreditAssetController>();
        //    foreach (var asset in quotedAssetSet.instrumentSet)
        //    {
        //        var priceableAsset = Create(cache, asset.id, baseDate, quotedAssetSet.assetQuote[index], null, null);
        //        var priceableCreditAsset = priceableAsset as PriceableCreditAssetController;
        //        if (priceableCreditAsset != null)
        //        {
        //            priceableAssets.Add(priceableCreditAsset);
        //        }
        //        index++;
        //    }
        //    priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
        //    return priceableAssets;
        //}

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableFxAssetController> CreatePriceableFxAssets(ILogger logger, ICoreCache cache, string nameSpace,
            string[] assetIdentifiers, DateTime baseDate, Decimal[] values,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (assetIdentifiers.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableFxAssetController>();
            var uniqueIds = (IEnumerable<string>)assetIdentifiers;
            foreach (var assetIdentifier in uniqueIds.Distinct())
            {
                var properties = new PriceableAssetProperties(assetIdentifier);
                var index = 0;
                var vals = new List<Decimal>();
                var measures = new List<String>();
                var quotes = new List<String>();
                foreach (var ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    if (properties.AssetType == AssetTypesEnum.IRFuture) continue;
                    vals.Add(values[index - 1]); //TODO if future measure is volatility.
                    measures.Add("MarketQuote");
                    quotes.Add("DecimalRate");
                }
                var priceableAsset = Create(logger, cache, nameSpace, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableFxAssetController priceableFxAssetController)
                {
                    priceableAssets.Add(priceableFxAssetController);
                }
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        ///// <summary>
        ///// Creates the specified asset identifiers.
        ///// </summary>
        ///// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        ///// <param name="assetIdentifiers">The asset identifiers.</param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="values">The adjusted rates.</param>
        ///// <param name="measureTypes">The measure types. Cuurently supports MarketQuote and Volatility.</param>
        ///// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        ///// <returns></returns>
        //public List<IPriceableFxAssetController> CreatePriceableFxAssets(ICoreCache cache, string[] assetIdentifiers, DateTime baseDate, Decimal[] values,
        //                                                                            String[] measureTypes, String[] priceQuoteUnits)
        //{
        //    if ((assetIdentifiers.Length != values.Length) && (assetIdentifiers.Length != priceQuoteUnits.Length) && (assetIdentifiers.Length != measureTypes.Length))
        //    {
        //        throw new ArgumentOutOfRangeException("values", "the rates do not match the number of assets");
        //    }
        //    var priceableAssets = new List<IPriceableFxAssetController>();
        //    var uniqueIds = (IEnumerable<string>)assetIdentifiers;
        //    foreach (var assetIdentifier in uniqueIds.Distinct())
        //    {
        //        var index = 0;
        //        var vals = new List<Decimal>();
        //        var measures = new List<String>();
        //        var quotes = new List<String>();
        //        foreach (var ids in assetIdentifiers)
        //        {
        //            index++;
        //            if (ids != assetIdentifier) continue;
        //            vals.Add(values[index - 1]);
        //            measures.Add(measureTypes[index - 1]);
        //            quotes.Add(priceQuoteUnits[index - 1]);
        //        }
        //        var priceableAsset = Create(cache, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), null, null);
        //        if (priceableAsset is PriceableFxAssetController)
        //        {
        //            priceableAssets.Add((PriceableFxAssetController)priceableAsset);
        //        }
        //    }
        //    priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
        //    return priceableAssets;
        //}

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableCommodityAssetController> CreatePriceableCommodityAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, 
            string[] assetIdentifiers, decimal[] values, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (assetIdentifiers.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableCommodityAssetController>();
            var uniqueIds = (IEnumerable<string>)assetIdentifiers;
            foreach (var assetIdentifier in uniqueIds.Distinct())
            {
                var properties = new PriceableAssetProperties(assetIdentifier);
                var index = 0;
                var vals = new List<Decimal>();
                var measures = new List<String>();
                var quotes = new List<String>();
                foreach (var ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    if (properties.AssetType == AssetTypesEnum.IRFuture) continue;
                    vals.Add(values[index - 1]);
                    measures.Add("MarketQuote");
                    quotes.Add("DecimalRate");
                }
                var priceableAsset = Create(logger, cache, nameSpace, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(), quotes.ToArray(), fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableCommodityAssetController priceableCommodityAssetController)
                {
                    priceableAssets.Add(priceableCommodityAssetController);
                }
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Cuurently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <returns></returns>
        public static List<IPriceableRateSpreadAssetController> CreatePriceableSpreadFraAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, string[] assetIdentifiers, DateTime baseDate, Decimal[] values,
            String[] measureTypes, String[] priceQuoteUnits, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if ((assetIdentifiers.Length != values.Length) && (assetIdentifiers.Length != priceQuoteUnits.Length) && (assetIdentifiers.Length != measureTypes.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableRateSpreadAssetController>();
            var uniqueIds = (IEnumerable<string>)assetIdentifiers;
            foreach (var assetIdentifier in uniqueIds.Distinct())
            {
                var properties = new PriceableAssetProperties(assetIdentifier);
                if (properties.AssetType != AssetTypesEnum.SpreadFra) continue;
                var index = 0;
                var vals = new List<Decimal>();
                var measures = new List<String>();
                var quotes = new List<String>();
                foreach (var ids in assetIdentifiers)
                {
                    index++;
                    if (ids.Split('-')[1] != "SpreadFra") continue;
                    if (ids != assetIdentifier) continue;
                    vals.Add(values[index - 1]);
                    measures.Add(measureTypes[index - 1]);
                    quotes.Add(priceQuoteUnits[index - 1]);
                }
                var priceableAsset = Create(logger, cache, nameSpace, assetIdentifier, baseDate, vals.ToArray(), measures.ToArray(),
                                            quotes.ToArray(), fixingCalendar, rollCalendar);
                priceableAssets.Add((PriceableRateSpreadAssetController)priceableAsset);
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param> 
        /// <param name="quotedAssetSet">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <returns></returns>
        public static List<IPriceableRateSpreadAssetController> CreatePriceableSpreadFraAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, QuotedAssetSet quotedAssetSet, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (quotedAssetSet.assetQuote.Length != quotedAssetSet.instrumentSet.Items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(quotedAssetSet), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableRateSpreadAssetController>();
            var index = 0;
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                index++;
                var properties = new PriceableAssetProperties(asset.id);
                if (properties.AssetType != AssetTypesEnum.SpreadFra) continue;
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index - 1], fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableRateSpreadAssetController spreadAsset)
                {
                    priceableAssets.Add(spreadAsset);
                }
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableEquityAssetController> CreatePriceableEquityAssets(ILogger logger, ICoreCache cache,
            string nameSpace, DateTime baseDate, FxRateSet quotedAssetSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var index = 0;
            var priceableAssets = new List<IPriceableEquityAssetController>();
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableEquityAssetController priceableCommodityAsset)
                {
                    priceableAssets.Add(priceableCommodityAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        ///  <param name="fixingCalendar">The fixingCalendar.</param>
        ///   <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableFxAssetController> CreatePriceableFxAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, FxRateSet quotedAssetSet, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var index = 0;
            var priceableAssets = new List<IPriceableFxAssetController>();
            //1.    Check if there are Fx forward points in the basic quotations. Cache the spot rate. i.e. find -FxSpot-SP.
            BasicAssetValuation spotRateAsset = (from spotRateAssets in quotedAssetSet.assetQuote
                                                 where spotRateAssets.objectReference.href.EndsWith("-FxSpot-SP", StringComparison.InvariantCultureIgnoreCase)
                                                 select spotRateAssets).Single();//TODO This blows up if there is no spot rate!
            BasicAssetValuation tNRateAsset = null;
            var exists = (from spotRateAssets in quotedAssetSet.assetQuote
                          where spotRateAssets.objectReference.href.EndsWith("TN", StringComparison.InvariantCultureIgnoreCase)
                          select spotRateAssets).Any();
            if (exists)
            {
                tNRateAsset =
                (from spotRateAssets in quotedAssetSet.assetQuote
                 where spotRateAssets.objectReference.href.EndsWith("TN", StringComparison.InvariantCultureIgnoreCase)
                 select spotRateAssets).Single();
            }
            decimal spotRate = spotRateAsset.quote[0].value;
            //var forwards = PriceQuoteUnitsEnum.RateSpread
            //2.    If there are, derive the outrights from the cached spot rate and the forward points.
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                var isONAsset = asset.id.EndsWith("ON", StringComparison.InvariantCultureIgnoreCase);
                var isTNAsset = asset.id.EndsWith("TN", StringComparison.InvariantCultureIgnoreCase);
                var bav = quotedAssetSet.assetQuote[index];
                var quote = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", bav.quote);
                if (quote.quoteUnits.Value == "DecimalSpread")
                {
                    quote.quoteUnits.Value = "DecimalRate";
                    if (isTNAsset)
                    {
                        quote.value = spotRate - quote.value;
                    }
                    if (isONAsset && tNRateAsset != null)
                    {
                        var onquote = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", tNRateAsset.quote);
                        if (onquote.quoteUnits.Value == "DecimalSpread")
                        {
                            quote.value = spotRate - quote.value - onquote.value;
                        }
                        if (onquote.quoteUnits.Value == "DecimalRate")
                        {
                            quote.value = onquote.value - quote.value;
                        }
                    }
                    else if (!isTNAsset)
                    {
                        quote.value = spotRate + quote.value;
                    }
                }
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, bav, fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableFxAssetController fxAsset)
                {
                    priceableAssets.Add(fxAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache: needed for instrument configuration and calendar functions.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static List<IPriceableCommodityAssetController> CreatePriceableCommodityAssets(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, FxRateSet quotedAssetSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var index = 0;
            var priceableAssets = new List<IPriceableCommodityAssetController>();
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], fixingCalendar, rollCalendar);
                if (priceableAsset is PriceableCommodityAssetController priceableCommodityAsset)
                {
                    priceableAssets.Add(priceableCommodityAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        /// <summary>
        /// Builds the priceable asset set.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="calendar">The calendar.</param>
        /// <param name="baseDate">The base date for assert creation.</param>
        /// <param name="quotedAssetSet">The quotedAssetSet.</param>
        /// <returns></returns>     
        public static List<IPriceableCommoditySpreadAssetController> CreatePriceableCommoditySpreadAssets(ILogger logger, ICoreCache cache, string nameSpace,
            DateTime baseDate, QuotedAssetSet quotedAssetSet, IBusinessCalendar calendar)
        {
            var index = 0;
            var priceableAssets = new List<IPriceableCommoditySpreadAssetController>();
            foreach (var asset in quotedAssetSet.instrumentSet.Items)
            {
                var priceableAsset = Create(logger, cache, nameSpace, asset.id, baseDate, quotedAssetSet.assetQuote[index], null, null);
                if (priceableAsset is PriceableCommoditySpreadAssetController commodityAsset)
                {
                    priceableAssets.Add(commodityAsset);
                }
                index++;
            }
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            return priceableAssets;
        }

        #endregion

        #region Custom Priceable Asset Creation

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="amount">The amount.</param>
        /// <param name="discountingType">The discounting type.</param>
        /// <param name="effectiveDate">The base date.</param>
        /// <param name="tenor">The maturity tenor.</param>
        /// <param name="fxdDayFraction">The fixed leg day fraction.</param>
        /// <param name="businessCenters">The payment business centers.</param>
        /// <param name="businessDayConvention">The payment business day convention.</param>
        /// <param name="fxdFrequency">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">THe currency.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public static PriceableSimpleIRSwap CreateSimpleIRSwap(ILogger logger, ICoreCache cache, 
            string nameSpace, string id, DateTime baseDate, string currency,
            decimal amount, DiscountingTypeEnum? discountingType,
            DateTime effectiveDate, string tenor, string fxdDayFraction,
            string businessCenters, string businessDayConvention, string fxdFrequency,
            RateIndex underlyingRateIndex, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
        {
                if (fixingCalendar == null)
                {
                    fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, BusinessCentersHelper.Parse(businessCenters), nameSpace);
                }
                if (paymentCalendar == null)
                {
                    paymentCalendar = fixingCalendar;
                }
                var instrument = new PriceableSimpleIRSwap(id, baseDate, currency, amount, discountingType, effectiveDate, tenor, fxdDayFraction, businessCenters,
                    businessDayConvention, fxdFrequency, underlyingRateIndex, fixingCalendar, paymentCalendar, fixedRate);
                return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixedLegDayCount">The fixed leg day count basis.</param>
        /// <param name="term">The term of the swap.</param>
        /// <param name="paymentFrequency">The payment frequency.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="spotDate">The spot date.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        public static IPriceableAssetController CreateInterestRateSwap(ILogger logger, ICoreCache cache, 
            string nameSpace, String currency, DateTime baseDate, DateTime spotDate, String fixedLegDayCount,
            string term, string paymentFrequency, string rollConvention, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicAssetValuation bav)
        {
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, "AUD", "IRSwap");
            var fixedRate = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", new List<BasicQuotation>(bav.quote));
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            if (instrument != null)
            {
                var nodeStruct = (SimpleIRSwapNodeStruct)instrument.InstrumentNodeItem;
                nodeStruct.SimpleIRSwap.term = PeriodHelper.Parse(term);
                nodeStruct.SimpleIRSwap.paymentFrequency = PeriodHelper.Parse(paymentFrequency);
                nodeStruct.SimpleIRSwap.dayCountFraction.Value = fixedLegDayCount;
                nodeStruct.SimpleIRSwap.id = "IRSwap-" + currency;
                if (fixingCalendar == null)
                {
                    fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                }
                if (paymentCalendar == null)
                {
                    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                }
                return new PriceableSimpleIRSwap(baseDate, nodeStruct, fixingCalendar, paymentCalendar, normalisedRate);
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="notionalWeights">THe notional weights.</param>
        /// <param name="fixedLegDayCount">The fixed leg day count basis.</param>
        /// <param name="fixingDateAdjustments">The fixingDateAdjustments.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="notional">The actual first notional. THis must be consistent with the weights.</param>
        /// <param name="dates">The dates.</param>
        public static IPriceableAssetController CreateInterestRateSwap(String identifier, DateTime baseDate,
            Money notional, DateTime[] dates, Decimal[] notionalWeights, String fixedLegDayCount,
            BusinessDayAdjustments fixingDateAdjustments, BasicAssetValuation bav)
        {
            var fixedRate = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", new List<BasicQuotation>(bav.quote));
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            var calculation = CalculationFactory.CreateFixed(normalisedRate.value, notional,
                                                             DayCountFractionHelper.Parse(fixedLegDayCount), DiscountingTypeEnum.Standard);
            return new PriceableSimpleIRSwap(baseDate, identifier, dates, notionalWeights, calculation,
                                                 fixingDateAdjustments, null, normalisedRate);
        }

        /// <summary>
        /// Gets the bond asset swap spread.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="maturityDate">THe maturity Date.</param>
        /// <param name="dayCountFraction">The fixed leg daycount basis.</param>
        /// <param name="couponFrequency">The coupon frequency.</param>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="rollConvention">The roll convention e.g. FOLLOWING.</param>
        /// <param name="businessCenters">The business centers e.g. AUSY.</param>
        /// <param name="ytm">The ytm.</param>
        /// <param name="curve">The curve to use for calculations.</param>
        /// <param name="identifier">The identifier of the bond.</param>
        /// <param name="valuationDate">The base date and valuation Date.</param>
        /// <param name="settlementDate">The settlement date.</param>
        /// <param name="exDivDate">The next ex div date.</param>
        /// <param name="notional">The actual first notional. This must be consistent with the weights.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        public static Decimal GetBondAssetSwapSpread(ILogger logger, ICoreCache cache, string nameSpace, 
            string identifier, DateTime valuationDate, DateTime settlementDate, DateTime exDivDate,
            Money notional, Decimal coupon, DateTime maturityDate, String dayCountFraction, string couponFrequency, String issuerName,
            string rollConvention, string businessCenters, IRateCurve curve, Decimal ytm, IBusinessCalendar paymentCalendar)
        {
            var fixedRate = BasicQuotationHelper.Create(ytm, "MarketQuote", "DecimalRate");
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(rollConvention, businessCenters);
            var bond = new Bond
            {
                id = identifier,
                couponRate = coupon,
                couponRateSpecified = true,
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                faceAmount = notional.amount,
                faceAmountSpecified = true,
                maturity = maturityDate,
                maturitySpecified = true,
                currency = new IdentifiedCurrency { Value = notional.currency.Value }
            };
            bond.faceAmountSpecified = true;
            bond.parValue = notional.amount;
            bond.parValueSpecified = true;
            bond.paymentFrequency = PeriodHelper.Parse(couponFrequency);
            bond.Item = issuerName;
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, BusinessCentersHelper.Parse(businessCenters), nameSpace);
            }
            var priceableBond = new PriceableSimpleBond(valuationDate, bond, settlementDate, exDivDate, businessDayAdjustments, paymentCalendar, fixedRate, BondPriceEnum.YieldToMaturity);
            var asw = priceableBond.CalculateAssetSwap(curve, valuationDate);
            return asw;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets.PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="spotDate">the spot date.</param>
        /// <param name="notional">The notional amount.</param>
        /// <param name="paymentBusinessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixingDateOffset">The fixing date business day adjustments.</param>
        /// <param name="floatingResetRates">The reset rates.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="spread">The spread on the floating leg.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRSwap CreateIRSwap(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime spotDate, Double notional, BusinessDayAdjustments paymentBusinessDayAdjustments, 
            RelativeDateOffset fixingDateOffset, List<Decimal> floatingResetRates, IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar, BasicQuotation fixedRate, BasicQuotation spread, NamedValueSet properties)
        {
            var term = properties.GetString("Term", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var currency = properties.GetString("Currency", true);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var fixedLegDayCount = properties.GetString("FixedLegDayCount", true);
            var floatingLegDayCount = properties.GetString("FloatingLegDayCount", true);
            var floatingIndex = properties.GetString("FloatingRateIndex", true);
            var floatingFrequency = properties.GetString("FloatingFrequency", true);
            var fixedFrequency = properties.GetString("FixedFrequency", true);
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            var normalisedSpread = MarketQuoteHelper.NormalisePriceUnits(spread, "DecimalRate");
            var notional1 = MoneyHelper.GetAmount(notional, currency);
            var fixedLegSwap = new SimpleIRSwap
            {
                term = PeriodHelper.Parse(term),
                paymentFrequency = PeriodHelper.Parse(fixedFrequency),
                dayCountFraction = DayCountFractionHelper.Parse(fixedLegDayCount)
            };
            var floatingLegSwap = new SimpleIRSwap
            {
                term = PeriodHelper.Parse(term),
                paymentFrequency = PeriodHelper.Parse(floatingFrequency),
                dayCountFraction = DayCountFractionHelper.Parse(floatingLegDayCount)
            };
            //Create the calculations.
            var floatingRateIndex = FloatingRateIndexHelper.Parse(floatingIndex);
            var floatingCalculation = CalculationFactory.CreateFloating(normalisedSpread.value, notional1, floatingRateIndex,
                floatingLegSwap.paymentFrequency, DayCountFractionHelper.Parse(floatingLegDayCount),
                discountingType);
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingDateOffset.businessCenters, nameSpace);
            }
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentBusinessDayAdjustments.businessCenters, nameSpace);
            }
            return new PriceableIRSwap(baseDate, fixedLegSwap, spotDate, notional1,
                paymentBusinessDayAdjustments, floatingLegSwap, floatingCalculation, fixingDateOffset,
                floatingResetRates, fixingCalendar, paymentCalendar, normalisedRate, normalisedSpread);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets.PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="fixedNotionals">The fixed notional. This must match the relevant date array i.e. count - 1</param>
        /// <param name="floatingResetRates">An array of override rest rates for the floating leg.</param>
        /// <param name="dateAdjustments">The fixingDateAdjustments.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="fixedRollDates">The fixed roll dates, including the final maturity.</param>
        /// <param name="floatingRollDates">The floating roll dates, including the final maturity.</param>
        /// <param name="floatingNotionals">The floating leg notional. This must match the relevant date array i.e. count - 1.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRSwap CreateIRSwap( List<DateTime> fixedRollDates, List<Decimal> fixedNotionals,
            List<DateTime> floatingRollDates, List<Decimal> floatingNotionals, List<Decimal> floatingResetRates,
            BusinessDayAdjustments dateAdjustments, BasicAssetValuation bav, NamedValueSet properties)
        {
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var ccy = properties.GetString("Currency", true);
            var currency = CurrencyHelper.Parse(ccy);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var fixedLegDayCount = properties.GetString("FixedLegDayCount", true);
            var floatingLegDayCount = properties.GetString("FloatingLegDayCount", true);
            var fixedRate = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", new List<BasicQuotation>(bav.quote));
            var floatingSpread = MarketQuoteHelper.FindQuotationByMeasureType("Spread", new List<BasicQuotation>(bav.quote));
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            var normalisedSpread = MarketQuoteHelper.NormalisePriceUnits(floatingSpread, "DecimalRate");
            //Create the notional schedule.
            var nonNegativeSchedule1 = NonNegativeScheduleHelper.Create(fixedRollDates, fixedNotionals);
            var notional1 = NonNegativeAmountScheduleHelper.Create(nonNegativeSchedule1, currency);
            var fixedCalculation = CalculationFactory.CreateFixed(normalisedRate.value, notional1,
                                   DayCountFractionHelper.Parse(fixedLegDayCount), discountingType);
            var nonNegativeSchedule2 = NonNegativeScheduleHelper.Create(floatingRollDates, floatingNotionals);
            var notional2 = NonNegativeAmountScheduleHelper.Create(nonNegativeSchedule2, currency);
            var floatingCalculation = CalculationFactory.CreateFixed(normalisedSpread.value, notional2,
                                      DayCountFractionHelper.Parse(floatingLegDayCount), discountingType);
            return new PriceableIRSwap(baseDate, fixedRollDates, floatingRollDates,
                floatingResetRates, fixedCalculation, dateAdjustments, floatingCalculation, null, normalisedRate, normalisedSpread);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="includeStubFlag">A flag. If true: include the first caplet.</param>
        /// <param name="notional">The notional.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="rollBackwardFlag">A flag, if true, rolls the dates backward from the maturity date. 
        /// Currently, rollForward is not implemented.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="term">THe term of the cap.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="effectiveDate">The efective Date.</param>
        /// <param name="paymentFrequency">The cap roll frequency.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRFloor CreateIRFloor(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            DateTime effectiveDate, string term, string currency, string paymentFrequency, Boolean includeStubFlag, 
            double notional, double strike, Boolean rollBackwardFlag, List<double> lastResets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, NamedValueSet properties)
        {
            var amount = MoneyHelper.GetAmount(notional, currency);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var capCalculation = CalculationFactory.CreateFloating(amount, floatingIndex.floatingRateIndex,
                              floatingIndex.indexTenor, DayCountFractionHelper.Parse(dayCount), discountingType);
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, resetConvention.businessCenters, nameSpace);
            }
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentDateAdjustments.businessCenters, nameSpace);
            }
            return new PriceableIRFloor(baseDate,
                effectiveDate, term, strike, lastResets,
                includeStubFlag, paymentFrequency, true,
                resetConvention, paymentDateAdjustments, capCalculation, fixingCalendar, paymentCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="includeStubFlag">A flag. If true: include the first caplet.</param>
        /// <param name="notional">The notional.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="rollBackwardFlag">A flag, if true, rolls the dates backward from the maturity date. 
        /// Currently, rollForward is not implemented.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg day count.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="term">THe term of the cap.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="effectiveDate">The effective Date.</param>
        /// <param name="paymentFrequency">The cap roll frequency.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRCap CreateIRCap(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate,
            DateTime effectiveDate, string term, string currency, string paymentFrequency,
            Boolean includeStubFlag, double notional, double strike, Boolean rollBackwardFlag,
            List<double> lastResets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, NamedValueSet properties)
        {
            var amount = MoneyHelper.GetAmount(notional, currency);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var capCalculation = CalculationFactory.CreateFloating(amount, floatingIndex.floatingRateIndex,
                floatingIndex.indexTenor, DayCountFractionHelper.Parse(dayCount), discountingType);
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, resetConvention.businessCenters, nameSpace);
            }
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentDateAdjustments.businessCenters, nameSpace);
            }
            return new PriceableIRCap(baseDate,
                effectiveDate, term, strike, lastResets,
                includeStubFlag, paymentFrequency, true,
                resetConvention, paymentDateAdjustments, 
                capCalculation, fixingCalendar, paymentCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="notionals">The notionals.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="rolldates">The roll dates.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRCap CreateIRCap(ILogger logger, ICoreCache cache, string nameSpace, String identifier, DateTime baseDate, string currency,
            List<DateTime> rolldates, List<double> notionals, List<double> strikes, List<double> resets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex, IBusinessCalendar fixingCalendar, NamedValueSet properties)
        {
            var notional = MoneyHelper.GetAmount(notionals[0], currency);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var capCalculation = CalculationFactory.CreateFloating(notional, floatingIndex.floatingRateIndex,
                              floatingIndex.indexTenor, DayCountFractionHelper.Parse(dayCount), discountingType);
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, resetConvention.businessCenters, nameSpace);
            }
            return new PriceableIRCap(baseDate, identifier, rolldates, notionals, strikes, resets,
                resetConvention, paymentDateAdjustments, capCalculation, fixingCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The local cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="notionals">The notionals.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="rolldates">The roll dates.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public static PriceableIRFloor CreateIRFloor(ILogger logger, ICoreCache cache, string nameSpace, String identifier, DateTime baseDate, string currency,
            List<DateTime> rolldates, List<double> notionals, List<double> strikes, List<double> resets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex, IBusinessCalendar fixingCalendar, NamedValueSet properties)
        {
            var notional = MoneyHelper.GetAmount(notionals[0], currency);
            var isDiscounted = properties.GetValue<bool>("IsDiscounted", true);
            DiscountingTypeEnum? discountingType = null;
            if (isDiscounted)
            {
                discountingType = DiscountingTypeEnum.Standard;
            }
            var capCalculation = CalculationFactory.CreateFloating(notional, floatingIndex.floatingRateIndex,
                              floatingIndex.indexTenor, DayCountFractionHelper.Parse(dayCount), discountingType);
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, resetConvention.businessCenters, nameSpace);
            }
            return new PriceableIRFloor(baseDate, identifier, rolldates, notionals, strikes, resets,
                resetConvention, paymentDateAdjustments, capCalculation, fixingCalendar);
        }

        #endregion
    }
}