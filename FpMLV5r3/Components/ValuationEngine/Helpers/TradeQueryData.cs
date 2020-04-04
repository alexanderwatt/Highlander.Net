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

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    [Serializable]
    public class TradeQueryData
    {
        public string UniqueIdentifier;
        public string ProductType;
        public string TradeId;
        public DateTime TradeDate;
        public DateTime MaturityDate;
        public DateTime EffectiveDate;
        public string TradeState;
        public string RequiredCurrencies;
        public string RequiredPricingStructures;
        public string ProductTaxonomy;
        public DateTime AsAtDate;
        public string SourceSystem;
        public string TradingBookId;
        public string TradingBookName;
        public string BaseParty;
        public string Party1;
        public string Party2;
        public string CounterPartyName;
        public string OriginatingPartyName;
        public int NumberOfColumns = 19;
    }

    public class TradeQueryHeader
    {
        public string UniqueIdentifier = "UniqueIdentifier";
        public string ProductType = "ProductType";
        public string TradeId = "TradeId";
        public string TradeDate = "TradeDate";
        public string MaturityDate = "MaturityDate";
        public string EffectiveDate = "EffectiveDate";
        public string TradeState = "TradeState";
        public string RequiredCurrencies = "RequiredCurrencies";
        public string RequiredPricingStructures = "RequiredPricingStructures";
        public string ProductTaxonomy = "ProductTaxonomy";
        public string AsAtDate = "AsAtDate";
        public string SourceSystem = "SourceSystem";
        public string TradingBookId = "TradingBookId";
        public string TradingBookName = "TradingBookName";
        public string BaseParty = "BaseParty";
        public string Party1 = "Party1";
        public string Party2 = "Party2";
        public string CounterPartyName = "CounterPartyName";
        public string OriginatingPartyName = "OriginatingPartyName";
    }
}
