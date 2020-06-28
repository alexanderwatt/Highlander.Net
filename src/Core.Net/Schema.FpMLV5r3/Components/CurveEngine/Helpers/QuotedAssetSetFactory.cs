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

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;

namespace Highlander.CurveEngine.V5r3.Helpers
{
    public class QuotedAssetSetFactory
    {
        private readonly List<Pair<Asset, BasicAssetValuation>> _assetAndQuotes = new List<Pair<Asset, BasicAssetValuation>>();

        public void AddAssetAndQuotes(Asset underlyingAsset, BasicAssetValuation quotes)
        {
            _assetAndQuotes.Add(new Pair<Asset, BasicAssetValuation>(underlyingAsset, quotes));       
        }

        public QuotedAssetSet Create()
        {
            var result = new QuotedAssetSet();
            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType19>();
            foreach (Pair<Asset, BasicAssetValuation> assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
                //Handles the case of tenor curves
                var id = assetAndQuote.First?.id ?? assetAndQuote.Second?.objectReference?.href;
                var properties = new PriceableAssetProperties(id);
                var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(properties.AssetType.ToString());
                types.Add(assetTypeFpML);
            }
            var instrumentSet = new InstrumentSet { Items = assets.ToArray(), ItemsElementName = types.ToArray() };
            result.instrumentSet = instrumentSet;
            result.assetQuote = quotes.ToArray();  
            return result;
        }

        /// <summary>
        /// Handles the case of tenor curves
        /// </summary>
        /// <returns></returns>
        public QuotedAssetSet CreateTenorSet()
        {
            var result = new QuotedAssetSet();
            var quotes = new List<BasicAssetValuation>();
            foreach (Pair<Asset, BasicAssetValuation> assetAndQuote in _assetAndQuotes)
            {
                quotes.Add(assetAndQuote.Second);
            }
            result.instrumentSet = null;
            result.assetQuote = quotes.ToArray();
            return result;
        }

        public FxRateSet CreateFxRateSet()
        {
            var result = new FxRateSet();
            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType19>();
            foreach (Pair<Asset, BasicAssetValuation> assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
                var properties = new PriceableAssetProperties(assetAndQuote.First.id);
                var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(properties.AssetType.ToString());
                types.Add(assetTypeFpML);
            }
            var instrumentSet = new InstrumentSet { Items = assets.ToArray(), ItemsElementName = types.ToArray() };
            result.instrumentSet = instrumentSet;
            result.assetQuote = quotes.ToArray();
            return result;
        }

        public static Asset CreateAsset(string instrumentId)
        {
            // assumes instrument id is in format: ccy-assetType-...
            string[] instrIdParts = instrumentId.Split('-');
            string currency = instrIdParts[0];
            string assetType = instrIdParts[1];
            switch (assetType.ToLower())
            {
                case "deposit":
                    return new Deposit
                               {
                                   id = instrumentId,
                                   instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                   currency = new IdentifiedCurrency { Value = currency }
                               };
                case "fxspot":
                    return new FxRateAsset
                               {
                                   id = instrumentId,
                                   instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                   currency = new IdentifiedCurrency { Value = currency }
                               };
                case "irfuture":
                    return new Future
                               {
                                   id = instrumentId,
                                   instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                   currency = new IdentifiedCurrency { Value = currency }
                               };
                case "irfutureoption":
                    return new Future
                    {
                        id = instrumentId,
                        instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                        currency = new IdentifiedCurrency { Value = currency }
                    };
                case "irswap":
                    return new SimpleIRSwap
                               {
                                   id = instrumentId,
                                   instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                   currency = new IdentifiedCurrency { Value = currency }
                               };
                default:
                    throw new ArgumentException("Unknown Asset type", instrumentId);
            }
        }

        /// <summary>
        /// Creates a QuotedAssetSet from a set of instrument ids and sides, all with measureType set to MarketQuote
        /// and QuoteUnits set to DecimalRate (useful for building market data requests).
        /// </summary>
        /// <param name="instrIds">The (m) instrument ids.</param>
        /// <param name="sides">The (n) sides.</param>
        /// <returns>An M * N matrix of assets/quotes in a QuotedAssetSet</returns>
        public static QuotedAssetSet Parse(string[] instrIds, string[] sides)
        {
            var assetList = new List<Asset>();
            var quoteList = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType19>();
            foreach (string instrId in instrIds)
            {
                Asset asset = CreateAsset(instrId);
                assetList.Add(asset);
                quoteList.AddRange(sides.Select(side => BasicQuotationHelper.Create("MarketQuote", "DecimalRate", side)).Select(quote => new BasicAssetValuation
                    {
                        objectReference = new AnyAssetReference {href = asset.id}, quote = new[] {quote}
                    }));
                var properties = new PriceableAssetProperties(asset.id);
                var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(properties.AssetType.ToString());
                types.Add(assetTypeFpML);
            }
            var instrumentSet = new InstrumentSet { Items = assetList.ToArray(), ItemsElementName = types.ToArray() };
            return new QuotedAssetSet
                       {
                           instrumentSet = instrumentSet,
                           assetQuote = quoteList.ToArray()
                       };
        }
    }
}