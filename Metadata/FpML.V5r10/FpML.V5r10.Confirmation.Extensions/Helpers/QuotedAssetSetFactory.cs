
using System;
using System.Collections.Generic;
using Orion.Util.Helpers;

namespace nab.QDS.FpML.V47
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

            foreach (Pair<Asset, BasicAssetValuation> assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
            }

            result.instrumentSet = assets.ToArray();
            result.assetQuote = quotes.ToArray();
            
            return result;
        }

        public FxRateSet CreateFxRateSet()
        {
            var result = new FxRateSet();

            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();

            foreach (Pair<Asset, BasicAssetValuation> assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
            }

            result.instrumentSet = assets.ToArray();
            result.assetQuote = quotes.ToArray();

            return result;
        }

        ///// <summary>
        ///// Creates a quoted asset set.
        ///// </summary>
        ///// <param name="instrumentIds">The list of instrument ids.</param>
        ///// <param name="values">The list of values.</param>
        ///// <param name="additional">The list of additional.</param>
        ///// <returns></returns>
        //public static QuotedAssetSet Parse(string[] instrumentIds, decimal[] values, decimal[] additional)
        //{
        //    if ((instrumentIds.Length != values.Length) && (instrumentIds.Length != additional.Length))
        //    {
        //        throw new ArgumentOutOfRangeException("values", "the rates do not match the number of assets");
        //    }
        //    var assetPairs = new List<Pair<Asset, BasicAssetValuation>>();

        //    var index = 0;
        //    foreach (var assetIdentifier in instrumentIds)
        //    {
        //        var assetPair = Parse(assetIdentifier, values[index], additional[index]);
        //        assetPairs.Add(assetPair);
        //        index++;
        //    }

        //    return MapFromAssetPairs(assetPairs);
        //}

        ///// <summary>
        ///// Maps from a list of asset pairs to a quoted asset set.
        ///// </summary>
        ///// <param name="assetPairs"></param>
        ///// <returns></returns>
        //internal static QuotedAssetSet MapFromAssetPairs(List<Pair<Asset, BasicAssetValuation>> assetPairs)
        //{
        //    var quotedAssetSet = new QuotedAssetSet();
        //    var assets = new Asset[assetPairs.Count];
        //    var bavs = new BasicAssetValuation[assetPairs.Count];
        //    var index = 0;
        //    foreach (var pair in assetPairs)
        //    {
        //        assets[index] = pair.First;
        //        bavs[index] = pair.Second;
        //        index++;
        //    }
        //    quotedAssetSet.assetQuote = bavs;
        //    quotedAssetSet.instrumentSet = assets;

        //    return quotedAssetSet;
        //}

        ///// <summary>
        ///// Parses the string info into an asset.
        ///// </summary>
        ///// <param name="instrumentId"></param>
        ///// <param name="value"></param>
        ///// <param name="adjustment"></param>
        ///// <returns></returns>
        //public static Pair<Asset, BasicAssetValuation> Parse(string instrumentId, decimal value, decimal adjustment)
        //{
        //    const string rateQuotationType = "MarketQuote";

        //    Asset underlyingAsset;

        //    var results = instrumentId.Split('-');
        //    var instrument = results[1];

        //    var listBasicQuotations = new List<BasicQuotation>();

        //    switch (instrument)
        //    {
        //        case "ZeroRate":
        //            {
        //                var zeroRate = new Cash { id = instrumentId };
        //                underlyingAsset = zeroRate;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "Xibor":
        //        case "OIS":
        //            {
        //                var tenor = results[2];
        //                var rateIndex = new RateIndex { id = instrumentId, term = PeriodHelper.Parse(tenor) };
        //                underlyingAsset = rateIndex;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "IRSwap":
        //        case "XccySwap":
        //        case "SimpleIRSwap":
        //            {
        //                var simpleIRSwap = new SimpleIRSwap { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                underlyingAsset = simpleIRSwap;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "Deposit":
        //        case "XccyDepo":
        //        case "BankBill":
        //            {
        //                var deposit = new Deposit { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                underlyingAsset = deposit;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "SimpleFra":
        //        case "Fra":
        //        case "BillFra":
        //        case "SpreadFra":
        //            {
        //                var index = results[3];
        //                var asset = new SimpleFra { id = instrumentId, startTerm = PeriodHelper.Parse(results[2]) };

        //                asset.endTerm = asset.startTerm.Sum(PeriodHelper.Parse(index));
        //                underlyingAsset = asset;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "IRCap":
        //            {
        //                var simpleIRCap = new SimpleIRSwap { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                underlyingAsset = simpleIRCap;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, "Premium", "Amount"));
        //                break;
        //            }
        //        case "IRFuture":
        //            {
        //                var future = new Future { id = instrumentId };

        //                underlyingAsset = future;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(adjustment, "Volatility", "LognormalVolatility"));
        //                break;
        //            }
        //        case "CommodityFuture":
        //            {
        //                var future = new Future { id = instrumentId };

        //                underlyingAsset = future;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "CPIndex":
        //            {
        //                var tenor = results[2];
        //                var rateIndex = new RateIndex { id = instrumentId, term = PeriodHelper.Parse(tenor) };
        //                underlyingAsset = rateIndex;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "SimpleCPISwap":
        //        case "CPISwap":
        //        case "ZCCPISwap":
        //            {
        //                var simpleIRSwap = new SimpleIRSwap { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                underlyingAsset = simpleIRSwap;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "DecimalRate"));
        //                break;
        //            }
        //        case "FxSpot":
        //        case "FxForward":
        //            {
        //                //  var tenor = results[2];
        //                var fxRateAsset = new FxRateAsset { id = instrumentId };
        //                underlyingAsset = fxRateAsset;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value + adjustment, rateQuotationType, "FxRate"));
        //                break;
        //            }
        //        case "CommoditySpot":
        //        case "CommodityForward":
        //            {
        //                var commodityAsset = new FxRateAsset { id = instrumentId };
        //                underlyingAsset = commodityAsset;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "Price"));
        //                break;
        //            }
        //        case "Bond":
        //            {
        //                var asset = new Bond { id = instrumentId };
        //                underlyingAsset = asset;
        //                listBasicQuotations.Add(BasicQuotationHelper.Create(value, rateQuotationType, "DirtyPrice"));
        //                break;
        //            }
        //        default:
        //            throw new NotSupportedException(string.Format("Asset type {0} is not supported", instrument));
        //    }

        //    return new Pair<Asset, BasicAssetValuation>(underlyingAsset, BasicAssetValuationHelper.Create(underlyingAsset.id, listBasicQuotations.ToArray()));

        //}

        ///// <summary>
        ///// Parses the data.
        ///// </summary>
        ///// <param name="instrumentIds"></param>
        ///// <returns></returns>
        //public static QuotedAssetSet Parse(string[] instrumentIds)
        //{
        //    var quotedAssetSetFactory = new QuotedAssetSetFactory();

        //    const string rateQuotationType = "MarketQuote";

        //    for (var i = 0; i < instrumentIds.Length; i++)
        //    {
        //        Asset underlyingAsset;

        //        var instrumentId = instrumentIds[i];
        //        var results = instrumentIds[i].Split('-');
        //        var instrument = results[1];

        //        var listBasicQuotations = new List<BasicQuotation>();

        //        const string priceUnitDecimalRate = "DecimalRate";

        //        switch (instrument)
        //        {
        //            case "ZeroRate":
        //                {
        //                    underlyingAsset = new Cash { id = instrumentId };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "Xibor":
        //            case "OIS":
        //                {
        //                    var tenor = results[2];
        //                    underlyingAsset = new RateIndex { id = instrumentId, term = PeriodHelper.Parse(tenor) };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "IRSwap":
        //            case "XccySwap":
        //            case "SimpleIRSwap":
        //                {
        //                    underlyingAsset = new SimpleIRSwap { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "Deposit":
        //            case "XccyDepo":
        //            case "BankBill":
        //                {
        //                    underlyingAsset = new Deposit { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "SimpleFra":
        //            case "Fra":
        //            case "BillFra":
        //                {
        //                    var index = results[3];
        //                    var asset = new SimpleFra { id = instrumentId, startTerm = PeriodHelper.Parse(results[2]) };

        //                    asset.endTerm = asset.startTerm.Sum(PeriodHelper.Parse(index));
        //                    underlyingAsset = asset;
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "IRFuture":
        //                {
        //                    underlyingAsset = new Future { id = instrumentId };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create("Volatility", "LognormalVolatility"));
        //                    break;
        //                }
        //            case "CPIndex":
        //                {
        //                    var tenor = results[2];
        //                    underlyingAsset = new RateIndex { id = instrumentId, term = PeriodHelper.Parse(tenor) };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            case "SimpleCPISwap":
        //            case "CPISwap":
        //            case "ZCCPISwap":
        //                {
        //                    underlyingAsset = new SimpleIRSwap { id = instrumentId, term = PeriodHelper.Parse(results[2]) };
        //                    listBasicQuotations.Add(BasicQuotationHelper.Create(rateQuotationType, priceUnitDecimalRate));
        //                    break;
        //                }
        //            default:
        //                throw new NotSupportedException(string.Format("Asset type {0} is not supported", instrument));
        //        }

        //        quotedAssetSetFactory.AddAssetAndQuotes(underlyingAsset, BasicAssetValuationHelper.Create(underlyingAsset.id, listBasicQuotations.ToArray()));
        //    }
        //    return quotedAssetSetFactory.Create();
        //}

        //public static List<BasicAssetValuation> GetAssetQuote(QuotedAssetSet quotedAssetSet, string instrumentId)
        //{
        //    return quotedAssetSet.assetQuote.Where(basicAssetValuation => basicAssetValuation.objectReference.href == instrumentId).ToList();
        //}

        public static Asset CreateAsset(string instrumentId)
        {
            // todo - maybe alex has a better asset helper somewhere?
            // assumes instrument id is in format: ccy-assettype-...
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
            foreach (string instrId in instrIds)
            {
                Asset asset = CreateAsset(instrId);
                assetList.Add(asset);
                foreach (string side in sides)
                {
                    BasicQuotation quote = BasicQuotationHelper.Create("MarketQuote", "DecimalRate", side);
                    quoteList.Add(new BasicAssetValuation
                                      {
                                          objectReference = new AnyAssetReference { href = asset.id },
                                          quote = new[] { quote }
                                      });
                }
            }
            return new QuotedAssetSet
                       {
                           instrumentSet = assetList.ToArray(),
                           assetQuote = quoteList.ToArray()
                       };
        }
    }
}