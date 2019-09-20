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

#region Usings

using System;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using FpML.V5r3.Reporting;
using Orion.Util.Serialisation;

#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// InstrumentDataHelper
    /// </summary>
    public class InstrumentDataHelper
    {
        private static string CreateKey(string nameSpace, string assetType, string currency)
        {
            var formatString = nameSpace + "." + InstrumentConfigData.GenericName + ".{0}.{1}";//".{0}.{1}-{0}"
            return string.Format(formatString, assetType, currency);
        }

        private static string CreateKey(string nameSpace, string assetType, string currency, string extra)
        {
            var formatString = nameSpace + "." + InstrumentConfigData.GenericName + ".{0}.{1}.{2}"; //".{0}.{1}-{0}.{2}";
            return string.Format(formatString, assetType, currency, extra);
        }

        private static string CreateKey(string nameSpace, string assetType, string currency, string extra, string extra2)
        {
            var formatString = nameSpace + "." + InstrumentConfigData.GenericName + ".{0}.{1}.{2}.{3}";//".{0}.{1}-{0}.{2}.{3}"
            return string.Format(formatString, assetType, currency, extra, extra2);
        }

        private static string CreateBondKey(string nameSpace, string bond)
        {
            var formatString = nameSpace + "." + FixedIncomeData.GenericName + ".{0}";
            return string.Format(formatString, bond);
        }

        public static string CreateEquityExchangeKey(string nameSpace, string exchange)
        {
            var formatString = nameSpace + "." + EquityExchangeData.GenericName + ".{0}";
            return string.Format(formatString, exchange);
        }

        private static string CreateEquityAssetKey(string nameSpace, string equityId)
        {
            var formatString = nameSpace + "." + EquityAssetData.GenericName + ".{0}";
            return string.Format(formatString, equityId);
        }

        /// <summary>
        /// Gets the instrument config data.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="parts">The AssetId or the parts of the AssetId.</param>
        /// <returns></returns>
        public static Instrument GetInstrumentConfigurationData(ICoreCache cache, string nameSpace, params string[] parts)
        {
            bool simplifiable = true;
            if (parts.Length == 1)
            {
                parts = parts[0].Split('-');
            }
            string uniqueName;
            var assetType = (AssetTypesEnum)Enum.Parse(typeof(AssetTypesEnum), parts[1], true);
            var swapTypes = new[] { AssetTypesEnum.IRSwap, AssetTypesEnum.BasisSwap, AssetTypesEnum.XccyBasisSwap, AssetTypesEnum.XccySwap };
            var fxTypes = new[] { AssetTypesEnum.FxSpot, AssetTypesEnum.FxForward };
            var bondTypes = new[] { AssetTypesEnum.Bond};
            var commodityTypes = new[] { AssetTypesEnum.CommoditySpread, AssetTypesEnum.CommodityForward};
            var equityTypes = new[] { AssetTypesEnum.Equity, AssetTypesEnum.EquityForward, AssetTypesEnum.EquitySpread };
            var irfuturesOptionTypes = new[] { AssetTypesEnum.IRFutureOption, AssetTypesEnum.IRCallFutureOption, AssetTypesEnum.IRPutFutureOption };
            var otherTypes = new[] { AssetTypesEnum.BondForward, AssetTypesEnum.BondSpot };
            ICoreItem loadedItem;
            Instrument instrument;
            string exchangeMIC = null;
            if (swapTypes.Contains(assetType) && parts.Length > 3)
            {
                uniqueName = CreateKey(nameSpace, parts[1], parts[0], parts[2], parts[3]);
            }
            else if (irfuturesOptionTypes.Contains(assetType) && parts.Length > 2)
            {
                uniqueName = CreateKey(nameSpace, AssetTypesEnum.IRFutureOption.ToString(), parts[0], parts[2]);
            }
            else if (commodityTypes.Contains(assetType) && parts.Length > 3)
            {
                uniqueName = CreateKey(nameSpace, parts[1], parts[0], parts[2], parts[3]);
            }
            else if (fxTypes.Contains(assetType))
            {
                return GetFxInstrumentConfigurationData(cache, nameSpace, parts);
            }
            else if (bondTypes.Contains(assetType) && parts[2].Split('.').Length > 4)
            {
                uniqueName = CreateBondKey(nameSpace, parts[2]);
                var extraData = cache.LoadItem<Bond>(uniqueName);
                var bondType = extraData.AppProps.GetValue<string>(BondProp.BondType, true);
                uniqueName = CreateKey(nameSpace, parts[1], parts[0], bondType);
                loadedItem = cache.LoadItem<Instrument>(uniqueName);
                instrument = loadedItem.Data as Instrument;
                if (instrument != null)
                {
                    var newInstrument = XmlSerializerHelper.Clone(instrument);
                    ((BondNodeStruct)newInstrument.InstrumentNodeItem).Bond = extraData.Data as Bond;
                    return newInstrument;
                }
            }
            else if (equityTypes.Contains(assetType))
            {
                var assetCode = parts[2].Split('.');
                EquityAsset equityAsset = null;
                if (assetCode.Length == 2)
                {
                    uniqueName = CreateEquityAssetKey(nameSpace, parts[2]);
                    var equity = cache.LoadItem<EquityAsset>(uniqueName);
                    exchangeMIC = equity.AppProps.GetValue<string>(EquityProp.ExchangeMIC, true);
                    equityAsset = equity.Data as EquityAsset;
                }
                if (assetCode.Length == 1)
                {
                    exchangeMIC = assetCode[0];
                }
                uniqueName = CreateEquityExchangeKey(nameSpace, exchangeMIC);
                loadedItem = cache.LoadItem<ExchangeConfigData>(uniqueName);
                if (assetType == AssetTypesEnum.Equity && loadedItem.Data is ExchangeConfigData exchange)
                {
                    instrument = new Instrument();
                    var equityNode = new EquityNodeStruct
                    {
                        Equity = equityAsset,
                        Exchange = exchange.ExchangeData,
                        SettlementDate = exchange.SettlementDate
                    };
                    instrument.AssetType = parts[1];
                    instrument.InstrumentNodeItem = equityNode;
                    instrument.Currency = CurrencyHelper.Parse(parts[0]);
                    instrument.ExtraItem = equityNode.Exchange.MIC;
                    return instrument;
                }
                if (parts.Length > 3)
                {
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0], exchangeMIC, parts[3]);
                }
                else
                {
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0], exchangeMIC);
                    simplifiable = false;
                }
            }
            else if (otherTypes.Contains(assetType) && parts.Length > 3)
            {
                //This is to simplify the configuration data and no bond forward is required
                uniqueName = CreateKey(nameSpace, parts[1], parts[0], parts[2], parts[3]);
            }
            //Default Area that sweeps all the standard keys structures.
            else if (parts.Length > 2)
            {
                uniqueName = CreateKey(nameSpace, parts[1], parts[0], parts[2]);
            }
            else
            {
                uniqueName = CreateKey(nameSpace, parts[1], parts[0]);
                simplifiable = false;
            }
            loadedItem = cache.LoadItem<Instrument>(uniqueName);
            if (loadedItem == null && simplifiable)
            {
                if (swapTypes.Contains(assetType) && parts.Length > 3)
                {
                    //This handles the special case of IRSwaps, where the underlying maturity should use the default.
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0], "Default", parts[3]);
                }
                else if (commodityTypes.Contains(assetType) && parts.Length > 3)
                {
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0], parts[2]);
                }
                else if (equityTypes.Contains(assetType) && parts.Length > 3)
                {
                    //Orion.V5r3.Configuration.Instrument.EquityForward.AUD-EquityForward.AU.0D
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0], exchangeMIC);
                }
                else if (otherTypes.Contains(assetType))
                {
                    //This is to simplify the configuration data and no bond forward is required
                    uniqueName = CreateKey(nameSpace, AssetTypesEnum.Bond.ToString(), parts[0], parts[2]);
                }
                else
                {
                    uniqueName = CreateKey(nameSpace, parts[1], parts[0]);
                }
                loadedItem = cache.LoadItem<Instrument>(uniqueName);
            }
            instrument = loadedItem?.Data as Instrument;
            return instrument;
        }

        private static Instrument GetFxInstrumentConfigurationData(ICoreCache cache, string nameSpace, string[] parts)
        {
            string currency1 = parts[0].Substring(0, 3);
            string currency2 = parts[0].Substring(3, 3);
            if (currency1 == "USD")
            {
                currency1 = currency2;
                currency2 = "USD";
            }
            string uniqueName;
            bool isCross = currency2 != "USD";
            ICoreItem loadedItem = null;
            if (parts.Length > 2)
            {
                uniqueName = CreateKey(nameSpace, parts[1], currency1 + currency2, parts[2]);
                loadedItem = cache.LoadItem<Instrument>(uniqueName);
                if (loadedItem == null && isCross) // try again the other way round
                {
                    uniqueName = CreateKey(nameSpace, parts[1], currency2 + currency1, parts[2]);
                    loadedItem = cache.LoadItem<Instrument>(uniqueName);
                }
            }
            if (loadedItem == null)
            {
                uniqueName = CreateKey(nameSpace, parts[1], currency1 + currency2);
                loadedItem = cache.LoadItem<Instrument>(uniqueName);
                if (loadedItem == null && isCross) // try again the other way round
                {
                    uniqueName = CreateKey(nameSpace, parts[1], currency2 + currency1);
                    loadedItem = cache.LoadItem<Instrument>(uniqueName);
                }
            }
            var instrument = loadedItem?.Data as Instrument;
            return instrument;
        }

        private static ApplicationException MissingKey(string key)
        {
            return new ApplicationException($"The config '{key}' is not available.");
        }

    }
}
