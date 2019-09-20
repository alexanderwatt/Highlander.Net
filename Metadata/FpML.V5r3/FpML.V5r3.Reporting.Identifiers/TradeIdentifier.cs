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
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.ModelFramework.Trades;

namespace Orion.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class TradeIdentifier : Identifier, ITradeIdentifier
    {
        /// <summary>
        /// TradeDate
        /// </summary>
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// DataType
        /// </summary>
        public string DataType { get; private set; }

        /// <summary>
        /// TradeType
        /// </summary>
        public ItemChoiceType15 TradeType { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        public ProductTypeSimpleEnum? ProductType { get; set; }

        ///<summary>
        /// The Source System.
        ///</summary>
        public string SourceSystem { get; set; }

        ///<summary>
        /// The party1.
        ///</summary>
        public string Party1 { get; set; }

        ///<summary>
        /// The party2.
        ///</summary>
        public string Party2 { get; set; }

        ///<summary>
        /// The base party.
        ///</summary>
        public string BaseParty { get; set; }

        ///<summary>
        /// The counter party.
        ///</summary>
        public string CounterParty { get; set; }

        ///<summary>
        /// An id for a trade.
        ///</summary>
        ///<param name="properties">The properties. These need to include:
        /// SourceSystem, Id and Trade date.</param>
        public TradeIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties(properties);
        }

        ///<summary>
        /// An id for a trade.
        ///</summary>
        ///<param name="tradeType">The trade type.</param>
        ///<param name="productType">The product type.</param>
        ///<param name="tradeId">The trade Id.</param>
        public TradeIdentifier(ItemChoiceType15 tradeType, ProductTypeSimpleEnum? productType, string tradeId)
        {
            SetProperties(tradeType, productType, tradeId);
        }

        /// <summary>
        ///  An id for a ratecurve.
        /// </summary>
        /// <param name="tradeType">The trade type.</param>
        /// <param name="productType">The product type.</param>
        /// <param name="tradeId">The trade Id.</param>
        /// <param name="tradeDate">The trade Date</param>
        /// <param name="sourceSystem">The source system</param>
        public TradeIdentifier(ItemChoiceType15 tradeType, ProductTypeSimpleEnum? productType, string tradeId, DateTime tradeDate, string sourceSystem)
        {
            SetProperties(tradeType, productType, tradeId, tradeDate, sourceSystem);
        }

        private void SetProperties(NamedValueSet properties)
        {
            try
            {
                DataType = FunctionProp.Trade.ToString();
                SourceSystem = PropertyHelper.ExtractSourceSystem(properties);
                var tradeId = properties.GetValue<string>(TradeProp.TradeId);
                Id = tradeId;
                Domain = SourceSystem + '.' + DataType;
                TradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate);
                TradeType = EnumHelper.Parse<ItemChoiceType15>(properties.GetValue<string>(TradeProp.TradeType), true);
                ProductType = ProductTypeSimpleScheme.ParseEnumString(properties.GetValue<string>(TradeProp.ProductType, false));
                Party1 = properties.GetValue<string>(TradeProp.Party1, false);
                Party2 = properties.GetValue<string>(TradeProp.Party2, false);
                BaseParty = properties.GetValue<string>(TradeProp.BaseParty);
                CounterParty = properties.GetValue<string>(TradeProp.CounterPartyId);
                UniqueIdentifier = BuildUniqueId(tradeId);
                if (properties.GetValue<string>(CurveProp.UniqueIdentifier)!=null)
                {
                    UniqueIdentifier = properties.GetValue<string>(CurveProp.UniqueIdentifier);
                }
                PropertyHelper.Update(Properties, CurveProp.UniqueIdentifier, UniqueIdentifier);
                PropertyHelper.Update(Properties, EnvironmentProp.Domain, Domain);
            }
            catch
            {
                throw new System.Exception("Invalid tradeId.");
            }
        }

        private void SetProperties(ItemChoiceType15 tradeType, ProductTypeSimpleEnum? productType, string tradeId, DateTime tradeDate, string sourceSystem)
        {
            try
            {
                DataType = TradeProp.Trade;
                SourceSystem = sourceSystem;
                Domain = SourceSystem + '.' + DataType;
                TradeDate = tradeDate;
                TradeType = tradeType;
                ProductType = productType;
                Id = tradeId;
                UniqueIdentifier = BuildUniqueId(tradeId);
                Properties = new NamedValueSet();
                Properties.Set(EnvironmentProp.DataType, FunctionProp.Trade.ToString());
                Properties.Set(EnvironmentProp.SourceSystem, "Orion");
                Properties.Set(EnvironmentProp.Domain, SourceSystem + '.' + DataType);
                Properties.Set(TradeProp.Identifier, Id);
                Properties.Set(CurveProp.UniqueIdentifier, UniqueIdentifier);
                Properties.Set(TradeProp.TradeDate, tradeDate);
            }
            catch
            {
                throw new System.Exception("Invalid tradeId.");
            }
        }

        private void SetProperties(ItemChoiceType15 tradeType, ProductTypeSimpleEnum? productType, string tradeId)
        {
            try
            {
                DataType = FunctionProp.Trade.ToString();
                SourceSystem = "Orion";
                Domain = SourceSystem + '.' + DataType;
                TradeType = tradeType;
                ProductType = productType;
                Id = tradeId;
                UniqueIdentifier = BuildUniqueId(tradeId);
                Properties = new NamedValueSet();
                Properties.Set(EnvironmentProp.DataType, FunctionProp.Trade.ToString());
                Properties.Set(EnvironmentProp.SourceSystem, "Orion");
                Properties.Set(EnvironmentProp.Domain, SourceSystem + '.' + DataType);
                Properties.Set(TradeProp.Identifier, Id);
                Properties.Set(CurveProp.UniqueIdentifier, UniqueIdentifier);
                //Properties.Set(TradeProp.TradeDate, tradeDate);
            }
            catch
            {
                throw new System.Exception("Invalid tradeId.");
            }
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string BuildUniqueId(string tradeId)
        {
            return $"{DataType}.{SourceSystem}.{TradeType}.{tradeId}";
        }
    }
}