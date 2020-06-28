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

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Assets.Commodities.Cash;
using Highlander.CurveEngine.V5r3.Assets.Rates;
using Highlander.CurveEngine.V5r3.Assets.Rates.CapsFloors;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Helpers
{
    /// <summary>
    /// A useful asset helper class.
    /// </summary>
    public static class AssetHelper
    {
        #region Parsing Functions

        /// <summary>
        /// Creates the specified assets in a quoted asset set.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Currently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <param name="includeMarketQuoteValues">An include flag. If false, then the market quotes are set as null.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(string[] assetIdentifiers, Decimal[] values,
                                           String[] measureTypes, String[] priceQuoteUnits, bool includeMarketQuoteValues)
        {
            if (assetIdentifiers.Length != values.Length && assetIdentifiers.Length != priceQuoteUnits.Length && (assetIdentifiers.Length != measureTypes.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(values), "The rates do not match the number of assets");
            }
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (string assetIdentifier in assetIdentifiers.Distinct())
            {
                int index = 0;
                var bav
                    = new BasicAssetValuation
                    {
                        objectReference = new AnyAssetReference { href = assetIdentifier }
                    };
                var bqs = new List<BasicQuotation>();
                foreach (string ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    BasicQuotation bq;
                    if (measureTypes[index - 1] == AssetMeasureEnum.MarketQuote.ToString() && !includeMarketQuoteValues)
                    {
                        bq = BasicQuotationHelper.Create(measureTypes[index - 1],
                                                             priceQuoteUnits[index - 1]);
                        bqs.Add(bq);
                    }
                    else
                    {
                        bq = BasicQuotationHelper.Create(values[index - 1], measureTypes[index - 1],
                                                             priceQuoteUnits[index - 1]);
                        bqs.Add(bq);
                    }

                }
                bav.quote = bqs.ToArray();
                quotedAssetSetFactory.AddAssetAndQuotes(Parse(assetIdentifier), bav);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the assetIdentifiers.
        /// </summary>
        /// <param name="assetIdentifiers"></param>
        /// <param name="values"></param>
        /// <param name="adjustments"></param>
        /// <returns></returns>
        public static FxRateSet ParseToFxRateSet(string[] assetIdentifiers, decimal[] values, decimal[] adjustments)
        {
            if (assetIdentifiers.Length != values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            if (adjustments != null && assetIdentifiers.Length != adjustments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            const string rateQuotationType = PriceableCommoditySpot.RateQuotationType;
            int index = 0;
            foreach (string assetIdentifier in assetIdentifiers)
            {
                var bav
                    = new BasicAssetValuation
                    {
                        objectReference = new AnyAssetReference { href = assetIdentifier }
                    };
                var addOn = adjustments?[index] ?? 0.0m;
                var bqs = new List<BasicQuotation>
                              {BasicQuotationHelper.Create(values[index] + addOn, rateQuotationType, "DecimalRate")};
                bav.quote = bqs.ToArray();
                quotedAssetSetFactory.AddAssetAndQuotes(Parse(assetIdentifier), bav);
                index++;
            }
            return quotedAssetSetFactory.CreateFxRateSet();
        }

        /// <summary>
        /// Creates the specified assets in a quoted asset set.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Currently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <param name="includeMarketQuoteValues">An include flag. If false, then the market quotes are set as null.</param>
        /// <returns></returns>
        public static FxRateSet ParseToFxRateSet(string[] assetIdentifiers, Decimal[] values,
                                           String[] measureTypes, String[] priceQuoteUnits, bool includeMarketQuoteValues)
        {
            if (assetIdentifiers.Length != values.Length && assetIdentifiers.Length != priceQuoteUnits.Length && assetIdentifiers.Length != measureTypes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (string assetIdentifier in assetIdentifiers.Distinct())
            {
                int index = 0;
                var bav
                    = new BasicAssetValuation
                    {
                        objectReference = new AnyAssetReference { href = assetIdentifier }
                    };
                var bqs = new List<BasicQuotation>();
                foreach (string ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    BasicQuotation bq;
                    if (measureTypes[index - 1] == AssetMeasureEnum.MarketQuote.ToString() && !includeMarketQuoteValues)
                    {
                        bq = BasicQuotationHelper.Create(measureTypes[index - 1],
                                                             priceQuoteUnits[index - 1]);
                        bqs.Add(bq);
                    }
                    else
                    {
                        bq = BasicQuotationHelper.Create(values[index - 1], measureTypes[index - 1],
                                                             priceQuoteUnits[index - 1]);
                        bqs.Add(bq);
                    }

                }
                bav.quote = bqs.ToArray();
                quotedAssetSetFactory.AddAssetAndQuotes(Parse(assetIdentifier), bav);
            }
            return quotedAssetSetFactory.CreateFxRateSet();
        }

        /// <summary>
        /// Creates the specified assets in a quoted asset set.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Currently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(string[] assetIdentifiers, Decimal[] values,
                                           String[] measureTypes, String[] priceQuoteUnits)
        {
            if (assetIdentifiers.Length != values.Length && assetIdentifiers.Length != priceQuoteUnits.Length && assetIdentifiers.Length != measureTypes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            foreach (string assetIdentifier in assetIdentifiers.Distinct())
            {
                int index = 0;
                var bav
                    = new BasicAssetValuation
                    {
                        objectReference = new AnyAssetReference { href = assetIdentifier }
                    };
                var bqs = new List<BasicQuotation>();
                foreach (string ids in assetIdentifiers)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    BasicQuotation bq = BasicQuotationHelper.Create(values[index - 1], measureTypes[index - 1],
                                                             priceQuoteUnits[index - 1]);
                    bqs.Add(bq);
                }
                bav.quote = bqs.ToArray();
                quotedAssetSetFactory.AddAssetAndQuotes(Parse(assetIdentifier), bav);
            }
            return quotedAssetSetFactory.Create();
        }

        #endregion

        #region QuotedAssetSet functions

        /// <summary>
        /// Creates a quoted asset set without the asset type specified.
        /// </summary>
        /// <param name="assetType">The asset Type to remove.</param>
        /// <param name="assetSet">The original assetSet</param>
        /// <returns></returns>
        public static FxRateSet RemoveAssetsFromQuotedAssetSet(AssetTypesEnum assetType, FxRateSet assetSet)
        {
            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType19>();
            var index = 0;
            foreach (var asset in assetSet.instrumentSet.Items)
            {
                var tempAssetType = new PriceableAssetProperties(asset.id).AssetType;
                if (tempAssetType != assetType)
                {
                    assets.Add(asset);
                    quotes.Add(assetSet.assetQuote[index]);
                    if (assetSet.instrumentSet.ItemsElementName != null)
                    {
                        types.Add(assetSet.instrumentSet.ItemsElementName[index]);
                    }
                    else
                    {
                        var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(tempAssetType.ToString());
                        types.Add(assetTypeFpML);
                    }
                }
                index++;
            }
            var instrumentSet = new InstrumentSet { Items = assets.ToArray(), ItemsElementName = types.ToArray() };
            return new FxRateSet { instrumentSet = instrumentSet, assetQuote = quotes.ToArray() };
        }

        /// <summary>
        /// Creates a quoted asset set without the asset type specified.
        /// </summary>
        /// <param name="assetType">The asset Type to remove.</param>
        /// <param name="assetSet">The original assetSet</param>
        /// <returns></returns>
        public static QuotedAssetSet RemoveAssetsFromQuotedAssetSet(AssetTypesEnum assetType, QuotedAssetSet assetSet)
        {
            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType19>();
            var index = 0;
            foreach (var asset in assetSet.instrumentSet.Items)
            {
                var tempAssetType = new PriceableAssetProperties(asset.id).AssetType;
                if (tempAssetType != assetType)
                {
                    assets.Add(asset);
                    quotes.Add(assetSet.assetQuote[index]);
                    if (assetSet.instrumentSet.ItemsElementName != null)
                    {
                        types.Add(assetSet.instrumentSet.ItemsElementName[index]);
                    }
                    else
                    {
                        var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(tempAssetType.ToString());
                        types.Add(assetTypeFpML);
                    }               
                }
                index++;
            }
            var instrumentSet = new InstrumentSet { Items = assets.ToArray(), ItemsElementName = types.ToArray() };
            return new QuotedAssetSet { instrumentSet = instrumentSet, assetQuote = quotes.ToArray() };
        }

        /// <summary>
        /// Creates a quoted asset set.
        /// </summary>
        /// <param name="instrumentIds">The list of instrument ids.</param>
        /// <param name="values">The list of values.</param>
        /// <param name="measureType">The measure type.</param>
        /// <param name="priceQuoteUnit">The price quote unit.</param>
        /// <returns></returns>
        public static QuotedAssetSet CreateQuotedAssetSet(string[] instrumentIds, decimal[] values, string measureType, string priceQuoteUnit)
        {
            return CreateQuotedAssetSet(instrumentIds, values, GenerateValues(measureType,values.Length), GenerateValues(priceQuoteUnit,values.Length));
        }

        private static String[] GenerateValues(String value, int number)
        {
            var result = new List<String>();
            for(int i = 0; i < number; i++)
            {
                result.Add(value);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Creates a quoted asset set.
        /// </summary>
        /// <param name="instrumentIds">The list of instrument ids.</param>
        /// <param name="values">The list of values.</param>
        /// <param name="measureTypes">The list of measure types.</param>
        /// <param name="priceQuoteUnits">The list of price quote units.</param>
        /// <returns></returns>
        public static QuotedAssetSet CreateQuotedAssetSet(string[] instrumentIds, decimal[] values, string[] measureTypes, string[] priceQuoteUnits)
        {
            if ((instrumentIds.Length != values.Length) && (instrumentIds.Length != priceQuoteUnits.Length) && (instrumentIds.Length != measureTypes.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var assetPairs = new List<Pair<Asset, BasicAssetValuation>>(); 
            var uniqueIds = (IEnumerable<string>)instrumentIds;
            // Loop through each distinct instrumentId
            foreach (var assetIdentifier in uniqueIds.Distinct())
            {
                if (string.IsNullOrEmpty(assetIdentifier)) break;
                var index = 0;
                var vals = new List<Decimal>();
                var measures = new List<String>();
                var quotes = new List<String>();
                // Loop through each item with that id
                foreach (var ids in instrumentIds)
                {
                    index++;
                    if (ids != assetIdentifier) continue;
                    vals.Add(values[index - 1]);
                    measures.Add(measureTypes[index - 1]);
                    quotes.Add(priceQuoteUnits[index - 1]);
                }
                var assetPair = CreateAssetPair(assetIdentifier, vals.ToArray(), measures.ToArray(), quotes.ToArray());
                assetPairs.Add(assetPair);
            }
            return MapFromAssetPairs(assetPairs);
        }

        #endregion

        #region More Parsing

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="value"></param>
        /// <param name="measureType"></param>
        /// <param name="priceQuoteUnits"></param>
        /// <returns></returns>
        public static Pair<Asset, BasicAssetValuation> CreateAssetPair(string instrumentId, decimal[] value, string[] measureType, string[] priceQuoteUnits)
        {
            if (value.Length == priceQuoteUnits.Length && value.Length == measureType.Length)
            {
                var  underlyingAsset = Parse(instrumentId);
                var listBasicQuotations = new List<BasicQuotation>();
                var counter = 0;
                foreach (var val in value)
                {
                    listBasicQuotations.Add(BasicQuotationHelper.Create(val, measureType[counter],
                                                                        priceQuoteUnits[counter]));
                    counter++;
                }              
                return new Pair<Asset, BasicAssetValuation>(underlyingAsset,
                                                            BasicAssetValuationHelper.Create(underlyingAsset.id,
                                                                                             listBasicQuotations.ToArray
                                                                                                 ()));
            }
            throw new System.Exception("Unequal number of values and priceQuoteUnits.");
        }

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Pair<Asset, BasicAssetValuation> ParseSurface(string instrumentId, decimal value)
        {
            const string rateQuotationType = PriceableCapRateAsset.VolatilityQuotationType;
            SimpleFra underlyingAsset;
            var results = instrumentId.Split('-');
            var instrument = results[1];
            var listBasicQuotations = new List<BasicQuotation>();
            var asset = EnumHelper.Parse<AssetTypesEnum>(instrument);
            switch (asset)
            {
                case AssetTypesEnum.BillCaplet:
                case AssetTypesEnum.BillFloorlet:
                case AssetTypesEnum.Floorlet:
                case AssetTypesEnum.Caplet:
                    {
                        var index = results[3];
                        underlyingAsset = new SimpleFra { id = instrumentId, startTerm = PeriodHelper.Parse(results[2]) };
                        underlyingAsset.endTerm 
                            = underlyingAsset.startTerm.Sum(PeriodHelper.Parse(index));
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalVolatility"));
                        break;
                    }
                default:
                    throw new NotSupportedException($"Asset type {instrument} is not supported");
            }
            return new Pair<Asset, BasicAssetValuation>(underlyingAsset, BasicAssetValuationHelper.Create(underlyingAsset.id, listBasicQuotations.ToArray()));
        }

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="value"></param>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public static Pair<string, BasicAssetValuation> ParseSimplified(string instrumentId, decimal value, decimal adjustment)
        {
            var result = Parse(instrumentId, value, adjustment);
            var simpleResult = new Pair<string, BasicAssetValuation>(result.First.id, result.Second);
            return simpleResult;
        }

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <returns></returns>
        public static Asset Parse(string instrumentId)
        {
            Asset underlyingAsset;
            var properties = new PriceableAssetProperties(instrumentId);
            switch (properties.AssetType)
            {
                case AssetTypesEnum.ZeroRate:
                    {
                        var zeroRate = new Cash { id = instrumentId };
                        underlyingAsset = zeroRate;
                        break;
                    }
                case AssetTypesEnum.Xibor:
                case AssetTypesEnum.OIS:
                    {
                        var rateIndex = new RateIndex { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = rateIndex;
                        break;
                    }
                case AssetTypesEnum.IRSwap:
                case AssetTypesEnum.ClearedIRSwap:
                case AssetTypesEnum.OISSwap:
                case AssetTypesEnum.XccySwap:
                case AssetTypesEnum.SimpleIRSwap:
                case AssetTypesEnum.XccyBasisSwap:
                case AssetTypesEnum.BasisSwap:
                case AssetTypesEnum.ResettableXccyBasisSwap:
                    {
                        var simpleIRSwap = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = simpleIRSwap;
                        break;
                    }
                case AssetTypesEnum.Deposit:
                case AssetTypesEnum.SpreadDeposit:
                case AssetTypesEnum.XccyDepo:
                case AssetTypesEnum.BankBill:
                case AssetTypesEnum.Repo:
                case AssetTypesEnum.RepoSpread:
                    {
                        var deposit = new Deposit { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = deposit;
                        break;
                    }
                case AssetTypesEnum.SimpleFra:
                case AssetTypesEnum.Fra:
                case AssetTypesEnum.BillFra:
                case AssetTypesEnum.SpreadFra:
                    {
                        var simpleFra = new SimpleFra { id = instrumentId, startTerm = properties.TermTenor };
                        if (properties.ForwardIndex == null)
                        {
                            throw new ArgumentException("ForwardIndex must be set in the instrumentId " + instrumentId,
                                nameof(instrumentId));
                        }
                        simpleFra.endTerm = simpleFra.startTerm.Sum(properties.ForwardIndex);
                        underlyingAsset = simpleFra;
                        break;
                    }
                case AssetTypesEnum.IRFloor:
                case AssetTypesEnum.IRCap:
                    {
                        var simpleIRCap = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = simpleIRCap;
                        break;
                    }
                case AssetTypesEnum.IRFutureOption:
                case AssetTypesEnum.IRFuture:
                    {
                        var future = new Future { id = instrumentId };
                        underlyingAsset = future;
                        break;
                    }
                case AssetTypesEnum.CommodityFuture:
                case AssetTypesEnum.CommodityFutureSpread:
                    {
                        var future = new Future { id = instrumentId };
                        underlyingAsset = future;
                        break;
                    }
                case AssetTypesEnum.CPIndex:
                    {
                        var rateIndex = new RateIndex { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = rateIndex;
                        break;
                    }
                case AssetTypesEnum.SimpleCPISwap:
                case AssetTypesEnum.CPISwap:
                case AssetTypesEnum.ZCCPISwap:
                    {
                        var simpleIRSwap = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        underlyingAsset = simpleIRSwap;
                        break;
                    }
                case AssetTypesEnum.Equity:
                case AssetTypesEnum.EquityForward:
                    {
                        //  var tenor = results[2];
                        var equityAsset = new EquityAsset { id = instrumentId };
                        underlyingAsset = equityAsset;
                        break;
                    }
                case AssetTypesEnum.FxSpot:
                case AssetTypesEnum.FxForward:
                    {
                        //  var tenor = results[2];
                        var fxRateAsset = new FxRateAsset { id = instrumentId };
                        underlyingAsset = fxRateAsset;
                        break;
                    }
                case AssetTypesEnum.CommoditySpot:
                case AssetTypesEnum.CommodityForward:
                case AssetTypesEnum.CommodityAverageForward:
                case AssetTypesEnum.CommoditySpread:
                    {
                        var commodityAsset = new Commodity { id = instrumentId };
                        underlyingAsset = commodityAsset;
                        break;
                    }
                case AssetTypesEnum.Bond:
                case AssetTypesEnum.BondSpot:
                case AssetTypesEnum.BondForward:
                    {
                        var bond = new Bond { id = instrumentId };
                        underlyingAsset = bond;
                        break;
                    }
                default:
                    throw new NotSupportedException($"Asset type {properties.AssetType} is not supported");
            }
            return underlyingAsset;
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="instrumentIds"></param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(string[] instrumentIds)
        {
            var values = new decimal[instrumentIds.Length];
            return Parse(instrumentIds, values, values);
        }


        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="instrumentIds"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(string[] instrumentIds, decimal[] values)
        {
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            for (int i = 0; i < instrumentIds.Length; i++)
            {
                Pair<Asset, BasicAssetValuation> assetPair = Parse(instrumentIds[i], values[i], null);
                quotedAssetSetFactory.AddAssetAndQuotes(assetPair.First, assetPair.Second);
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="instrumentIds"></param>
        /// <param name="values"></param>
        /// <param name="adjustments"></param>
        /// <returns></returns>
        public static QuotedAssetSet ParseTenorSet(string[] instrumentIds, decimal[] values, decimal[] adjustments)
        {
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            if (adjustments != null)
            {
                for (int i = 0; i < instrumentIds.Length; i++)
                {
                    Pair<Asset, BasicAssetValuation> assetPair = Parse(instrumentIds[i], values[i], adjustments[i]);
                    quotedAssetSetFactory.AddAssetAndQuotes(assetPair.First, assetPair.Second);
                }
            }
            else
            {
                for (int i = 0; i < instrumentIds.Length; i++)
                {
                    Pair<Asset, BasicAssetValuation> assetPair = Parse(instrumentIds[i], values[i], null);
                    quotedAssetSetFactory.AddAssetAndQuotes(assetPair.First, assetPair.Second);
                }
            }
            return quotedAssetSetFactory.CreateTenorSet();
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="instrumentIds"></param>
        /// <param name="values"></param>
        /// <param name="adjustments"></param>
        /// <returns></returns>
        public static QuotedAssetSet Parse(string[] instrumentIds, decimal[] values, decimal[] adjustments)
        {
            var quotedAssetSetFactory = new QuotedAssetSetFactory();
            if(adjustments!=null)
            {
                for (int i = 0; i < instrumentIds.Length; i++)
                {
                    Pair<Asset, BasicAssetValuation> assetPair = Parse(instrumentIds[i], values[i], adjustments[i]);
                    quotedAssetSetFactory.AddAssetAndQuotes(assetPair.First, assetPair.Second);
                }
            }
            else
            {
                for (int i = 0; i < instrumentIds.Length; i++)
                {
                    Pair<Asset, BasicAssetValuation> assetPair = Parse(instrumentIds[i], values[i], null);
                    quotedAssetSetFactory.AddAssetAndQuotes(assetPair.First, assetPair.Second);
                }
            }
            return quotedAssetSetFactory.Create();
        }

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="value"></param>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        public static Pair<Asset, BasicAssetValuation> Parse(string instrumentId, decimal value, decimal? adjustment)
        {
            const string rateQuotationType = PriceableSimpleRateAsset.RateQuotationType;
            const string volatilityQuotationType = PriceableCapRateAsset.VolatilityQuotationType;
            Asset underlyingAsset;
            decimal additional = 0.0m;
            if (adjustment != null)
            {
                additional = (decimal)adjustment;
            }
            var listBasicQuotations = new List<BasicQuotation>();
            var properties = new PriceableAssetProperties(instrumentId);
            switch (properties.AssetType)
            {
                //This is in place to handle volatility curves where the tenor is the expiry.
                case AssetTypesEnum.Period:
                {
                        //There is no underlying asset.
                        underlyingAsset = null;
                        listBasicQuotations.Add(BasicQuotationHelper.Create(instrumentId, value, volatilityQuotationType, "LognormalVolatility"));
                        break;
                }
                case AssetTypesEnum.ZeroRate:
                    {
                        underlyingAsset = new Cash { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.Xibor:
                case AssetTypesEnum.OIS:
                    {
                        underlyingAsset = new RateIndex { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.IRSwap:
                case AssetTypesEnum.ClearedIRSwap:
                case AssetTypesEnum.OISSwap:
                case AssetTypesEnum.XccySwap:
                case AssetTypesEnum.SimpleIRSwap:
                case AssetTypesEnum.XccyBasisSwap:
                case AssetTypesEnum.BasisSwap:
                case AssetTypesEnum.ResettableXccyBasisSwap:
                    {
                        underlyingAsset = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.Caplet:
                case AssetTypesEnum.Floorlet:
                case AssetTypesEnum.BillCaplet:
                case AssetTypesEnum.BillFloorlet:
                    {
                        underlyingAsset = new SimpleFra
                        {
                            id = instrumentId,
                            startTerm = properties.TermTenor,
                            endTerm = properties.TermTenor.Sum(properties.ForwardIndex)
                        };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, volatilityQuotationType, "LognormalVolatility"));
                        if (adjustment != null)
                        {
                            listBasicQuotations.Add(BasicQuotationHelper.Create(additional, "Strike", "DecimalRate"));
                        }
                        break;
                    }
                case AssetTypesEnum.Deposit:
                case AssetTypesEnum.SpreadDeposit:
                case AssetTypesEnum.XccyDepo:
                case AssetTypesEnum.BankBill:
                case AssetTypesEnum.Repo:
                case AssetTypesEnum.RepoSpread:
                    {
                        underlyingAsset = new Deposit { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.SimpleFra:
                case AssetTypesEnum.Fra:
                case AssetTypesEnum.BillFra:
                case AssetTypesEnum.SpreadFra:
                    {
                        underlyingAsset = new SimpleFra
                        {
                            id = instrumentId,
                            startTerm = properties.TermTenor,
                            endTerm = properties.TermTenor.Sum(properties.ForwardIndex)//TODO this restricts the perios to be the same!!!
                        };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.Swaption:
                {
                    underlyingAsset = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                    listBasicQuotations.Add(BasicQuotationHelper.Create(value, volatilityQuotationType, "LognormalVolatility"));
                    if (adjustment != null)
                    {
                        listBasicQuotations.Add(BasicQuotationHelper.Create(additional, "Strike", "DecimalRate"));
                    }
                    break;
                }
                case AssetTypesEnum.IRFloor:
                case AssetTypesEnum.IRCap:
                    {
                        underlyingAsset = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, volatilityQuotationType, "LognormalVolatility"));
                        if (adjustment != null)
                        {
                            listBasicQuotations.Add(BasicQuotationHelper.Create(additional, "Strike", "DecimalRate"));
                        }
                        break;
                    }
                case AssetTypesEnum.IRFutureOption:
                case AssetTypesEnum.IRCallFutureOption:
                case AssetTypesEnum.IRPutFutureOption:
                {
                        underlyingAsset = new Future { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, volatilityQuotationType, "LognormalVolatility"));
                    if (adjustment != null)
                    {
                        listBasicQuotations.Add(BasicQuotationHelper.Create(additional, "Strike", "DecimalRate"));
                    }
                        break;
                }
                case AssetTypesEnum.IRFuture:
                    {
                        underlyingAsset = new Future { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
                        if (adjustment != null)
                        {
                            listBasicQuotations.Add(BasicQuotationHelper.Create(additional, "Volatility", "LognormalVolatility"));
                        }
                        break;
                    }
                case AssetTypesEnum.CommodityFuture:
                case AssetTypesEnum.CommodityFutureSpread:
                    {
                        underlyingAsset = new Future { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.CPIndex:
                    {
                        underlyingAsset = new RateIndex { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.SimpleCPISwap:
                case AssetTypesEnum.CPISwap:
                case AssetTypesEnum.ZCCPISwap:
                    {
                        underlyingAsset = new SimpleIRSwap { id = instrumentId, term = properties.TermTenor };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "DecimalRate"));
                        break;
                    }
                case AssetTypesEnum.FxSpot:
                case AssetTypesEnum.FxForward:
                    {
                        underlyingAsset = new FxRateAsset { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "FxRate"));
                        break;
                    }
                case AssetTypesEnum.Equity:
                case AssetTypesEnum.EquityForward:
                    {
                        underlyingAsset = new EquityAsset { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "Price"));
                        break;
                    }
                case AssetTypesEnum.CommoditySpot:
                case AssetTypesEnum.CommodityForward:
                case AssetTypesEnum.CommodityAverageForward:
                case AssetTypesEnum.CommoditySpread:
                    {
                        underlyingAsset = new Commodity { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "Price"));
                        break;
                    }
                case AssetTypesEnum.Bond:
                case AssetTypesEnum.BondSpot:
                case AssetTypesEnum.BondForward:
                    {
                        underlyingAsset = new Bond { id = instrumentId };
                        listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));//Changed from DirtyPrice.
                        break;
                    }
                case AssetTypesEnum.Lease:
                {
                    underlyingAsset = new Lease { id = instrumentId };
                    listBasicQuotations.Add(BasicQuotationHelper.Create(value + additional, rateQuotationType, "Price"));
                    break;
                }
                default:
                    throw new NotSupportedException($"Asset type {properties.AssetType} is not supported");
            }
            var id = underlyingAsset?.id;
            if (underlyingAsset == null)
            {
                id = listBasicQuotations[0].id;
            }
            return new Pair<Asset, BasicAssetValuation>(underlyingAsset, BasicAssetValuationHelper.Create(id, listBasicQuotations.ToArray()));
        }

        /// <summary>
        /// Maps from a list of asset pairs to a quoted asset set.
        /// </summary>
        /// <param name="assetPairs"></param>
        /// <returns></returns>
        internal static QuotedAssetSet MapFromAssetPairs(ICollection<Pair<Asset, BasicAssetValuation>> assetPairs)
        {
            var quotedAssetSet = new QuotedAssetSet();
            var assets = new Asset[assetPairs.Count];
            var basicAssetValuations = new BasicAssetValuation[assetPairs.Count];
            var types = new List<ItemsChoiceType19>();
            var index = 0;
            foreach(var pair in assetPairs)
            {
                assets[index] = pair.First;
                basicAssetValuations[index] = pair.Second;
                var properties = new PriceableAssetProperties(assets[index].id);
                var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(properties.AssetType.ToString());
                types.Add(assetTypeFpML);
                index++;
            }
            quotedAssetSet.assetQuote = basicAssetValuations;
            var instrumentSet = new InstrumentSet { Items = assets.ToArray(), ItemsElementName = types.ToArray() };
            quotedAssetSet.instrumentSet = instrumentSet;
            return quotedAssetSet;      
        }

        /// <summary>
        /// Parses the string info into an asset.
        /// </summary>
        /// <param name="bondTypeId">The type of bond.</param>
        /// <param name="coupon">The coupon rate</param>
        /// <param name="daycount">The daycount.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="ytm">The ytm.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <returns></returns>
        public static Pair<Asset, BasicAssetValuation> ParseBond(string bondTypeId, DateTime maturityDate, decimal coupon, string daycount, string frequency, decimal ytm)
        {
            const string rateQuotationType = "MarketQuote";
            var bondId = bondTypeId +'-' + coupon + '-' + maturityDate.ToShortDateString();
            var underlyingAsset = new Bond
            {
                id = bondId,
                maturity = maturityDate,
                maturitySpecified = true,
                couponRate = coupon,
                couponRateSpecified = true,
                dayCountFraction = DayCountFractionHelper.Parse(daycount),
                paymentFrequency = PeriodHelper.Parse(frequency)
            };
            var listBasicQuotations = new List<BasicQuotation>
                                          {BasicQuotationHelper.Create(ytm, rateQuotationType, "DecimalRate")};
            return new Pair<Asset, BasicAssetValuation>(underlyingAsset, BasicAssetValuationHelper.Create(underlyingAsset.id, listBasicQuotations.ToArray()));
        }
    }

    public class AssetTypeConvertor
    {
        //BasisSwap,1
        //Bond,2
        //Deposit,3
        //XccyDepo,4
        //IRFuture,5
        //IRFutureOption,6
        //Caplet,7
        //Floorlet,8
        //BillCaplet,9
        //BillFloorlet,10
        //FxSpot,11
        //FxForward,12
        //FxFuture,13
        //CommoditySpot,14
        //CommodityForward,15
        //CommodityFuture,16
        //CommodityAverageForward,17
        //CommoditySpread,18
        //CommodityFutureSpread,19
        //BankBill,20
        //SimpleFra,21
        //Fra,22
        //SpreadDeposit,23
        //SpreadFra,24
        //BillFra,25
        //SimpleIRSwap,26
        //IRSwap,27
        //XccySwap,28
        //XccyBasisSwap,29
        //IRCap,30
        //Xibor,31
        //OIS,32
        //OISSwap,33
        //CPIndex,34
        //CPISwap,35
        //SimpleCPISwap,36
        //ZCCPISwap,37
        //ZeroRate,38
        //Equity,39
        //EquityForward,40
        //EquitySpread,41
        //BondSpot,42
        //BondForward,43
        //Repo,44
        //RepoSpread,45
        //ClearedIRSwap,46
        //ResettableXccyBasisSwap, 47
        //IRFloor,48
        //IRPutFutureOption,49
        //IRCallFutureOption,50
        //Swaption, 51
        //Period, 52
        public static readonly ItemsChoiceType19[] EnumConversions =
            {
                ItemsChoiceType19.simpleIrSwap,//BasisSwap,1
                ItemsChoiceType19.bond,//Bond,2
                ItemsChoiceType19.deposit,//Deposit,3
                ItemsChoiceType19.deposit,//XccyDepo,4
                ItemsChoiceType19.future,//IRFuture,5
                ItemsChoiceType19.future,//IRFutureOption,6
                ItemsChoiceType19.simpleFra,//Caplet,7
                ItemsChoiceType19.simpleFra,//Floorlet,8
                ItemsChoiceType19.simpleFra,//BillCaplet,9
                ItemsChoiceType19.simpleFra,//BillFloorlet,10
                ItemsChoiceType19.fx,//FxSpot,11
                ItemsChoiceType19.fx,//FxForward,12
                ItemsChoiceType19.bond,//FxFuture,13
                ItemsChoiceType19.commodity,//CommoditySpot,14
                ItemsChoiceType19.commodity,//CommodityForward,15
                ItemsChoiceType19.future,//CommodityFuture,16
                ItemsChoiceType19.commodity,//CommodityAverageForward,17
                ItemsChoiceType19.commodity,//CommoditySpread,18
                ItemsChoiceType19.future,//CommodityFutureSpread,19
                ItemsChoiceType19.deposit,//BankBill,20
                ItemsChoiceType19.simpleFra,//SimpleFra,21
                ItemsChoiceType19.simpleFra,//Fra,22
                ItemsChoiceType19.deposit,//SpreadDeposit,23
                ItemsChoiceType19.simpleFra,//SpreadFra,24
                ItemsChoiceType19.simpleFra,//BillFra,25
                ItemsChoiceType19.simpleIrSwap,//SimpleIRSwap,26
                ItemsChoiceType19.simpleIrSwap,//IRSwap,27
                ItemsChoiceType19.simpleIrSwap,//XccySwap,28
                ItemsChoiceType19.simpleIrSwap,//XccyBasisSwap,29
                ItemsChoiceType19.simpleIrSwap,//IRCap,30
                ItemsChoiceType19.rateIndex,//Xibor,31
                ItemsChoiceType19.rateIndex,//OIS,32
                ItemsChoiceType19.simpleIrSwap,//OISSwap,33
                ItemsChoiceType19.rateIndex,//CPIndex,34
                ItemsChoiceType19.simpleIrSwap,//CPISwap,35
                ItemsChoiceType19.simpleIrSwap,//SimpleCPISwap,36
                ItemsChoiceType19.simpleIrSwap,//ZCCPISwap,37
                ItemsChoiceType19.deposit,//ZeroRate,38
                ItemsChoiceType19.equity,//EquitySpot,39
                ItemsChoiceType19.equity,//EquityForward,40
                ItemsChoiceType19.equity,//EquitySpread,41
                ItemsChoiceType19.bond,//BondSpot,42
                ItemsChoiceType19.bond,//BondForward,43
                ItemsChoiceType19.deposit,//Repo,44
                ItemsChoiceType19.deposit,//RepoSpread,45
                ItemsChoiceType19.simpleIrSwap,//ClearedIRSwap,46
                ItemsChoiceType19.simpleIrSwap,//ResettableXccyBasisSwap,47
                ItemsChoiceType19.simpleIrSwap,//IRFloor, 48
                ItemsChoiceType19.future,//IRPutFutureOption,49
                ItemsChoiceType19.future,//IRCallFutureOption,50
                ItemsChoiceType19.simpleIrSwap,//Swaption,51
                ItemsChoiceType19.index//Period,52
            };

        public static ItemsChoiceType19 GetEnumConversions(AssetTypesEnum id)
        {
            var num = (int) id;
            return EnumConversions[num];
        }

        public static ItemsChoiceType19 ParseEnumStringToFpML(string assetIdString)
        {
            if (!AssetTypesValue.TryParseEnumString(assetIdString, out AssetTypesEnum result))
                throw new ArgumentException($"Cannot convert '{assetIdString}' to AssetTypesEnum");
            return GetEnumConversions(result);
        }

        #endregion
    }
}