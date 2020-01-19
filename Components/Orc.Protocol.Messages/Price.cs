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

using System.Xml.Serialization;


namespace Highlander.Orc.Messages
{
    #region PriceFeedBroadcastMessage

    [XmlRootAttribute(ElementName = "orc_message")]
    public class PriceFeedBroadcastMessage : SendMessage
    {
        public PriceFeedBroadcastMessage()
            : base(MessageTypes.PRICEFEED_BROADCAST)
        {
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "bid")]
        public string Bid { get; set; }

        [XmlElement(ElementName = "bid_volume")]
        public string BidVolume { get; set; }

        [XmlElement(ElementName = "ask")]
        public string Ask { get; set; }

        [XmlElement(ElementName = "ask_volume")]
        public string AskVolume { get; set; }

        [XmlElement(ElementName = "last")]
        public string Last { get; set; }

        [XmlElement(ElementName = "high")]
        public string High { get; set; }

        [XmlElement(ElementName = "low")]
        public string Low { get; set; }

        [XmlElement(ElementName = "close")]
        public string Close { get; set; }

        [XmlElement(ElementName = "open")]
        public string Open { get; set; }

        [XmlElement(ElementName = "turnover_volume")]
        public string TurnoverVolume { get; set; }

        [XmlElement(ElementName = "turnover")]
        public string Turnover { get; set; }
    }

    #endregion

    #region PriceGetMessage

    [XmlRootAttribute(ElementName = "orc_message")]
    public class PriceGetMessage : SendMessage
    {
        public PriceGetMessage()
            : base(MessageTypes.PRICE_GET)
        {
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }
    }

    #endregion

    #region Price

    [XmlRootAttribute(ElementName = "price")]
    public class Price : MessageBase
    {
        [XmlElement(ElementName = "bid")]
        public string Bid { get; set; }

        [XmlElement(ElementName = "bid_volume")]
        public string BidVolume { get; set; }

        [XmlElement(ElementName = "ask")]
        public string Ask { get; set; }

        [XmlElement(ElementName = "ask_volume")]
        public string AskVolume { get; set; }

        [XmlElement(ElementName = "last")]
        public string Last { get; set; }

        [XmlElement(ElementName = "high")]
        public string High { get; set; }

        [XmlElement(ElementName = "low")]
        public string Low { get; set; }

        [XmlElement(ElementName = "close")]
        public string Close { get; set; }

        [XmlElement(ElementName = "open")]
        public string Open { get; set; }

        [XmlElement(ElementName = "turnover_volume")]
        public string TurnoverVolume { get; set; }

        [XmlElement(ElementName = "turnover")]
        public string Turnover { get; set; }
    }

    #endregion

    #region PricefeedToggle

    [XmlRootAttribute(ElementName = "orc_message")]
    public class PriceFeedToggle : SendMessage
    {
        public PriceFeedToggle()
            : base(MessageTypes.PRICEFEED_TOGGLE)
        {
        }

        public string SubsID
        {
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "toggle")]
        public bool Toggle { get; set; }

        [XmlElement(ElementName = "separate_feed")]
        public bool SeparateFeed { get; set; }
    }

    #endregion

    #region Pricefeed
    [XmlRootAttribute(ElementName = "orc_message")]
    public class PriceFeed : ReplyMessage
    {
        public PriceFeed()
            : base(MessageTypes.PRICE_FEED)
        {
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "bid")]
        public string Bid { get; set; }

        [XmlElement(ElementName = "bid_volume")]
        public string BidVolume { get; set; }

        [XmlElement(ElementName = "ask")]
        public string Ask { get; set; }

        [XmlElement(ElementName = "ask_volume")]
        public string AskVolume { get; set; }

        [XmlElement(ElementName = "last")]
        public string Last { get; set; }

        [XmlElement(ElementName = "high")]
        public string High { get; set; }

        [XmlElement(ElementName = "low")]
        public string Low { get; set; }

        [XmlElement(ElementName = "close")]
        public string Close { get; set; }

        [XmlElement(ElementName = "open")]
        public string Open { get; set; }

        [XmlElement(ElementName = "open_balance")]
        public string OpenBalance { get; set; }

        [XmlElement(ElementName = "settlement_price")]
        public string SettlementPrice { get; set; }

        [XmlElement(ElementName = "standard_trading_status")]
        public string StandardTradingStatus { get; set; }

        [XmlElement(ElementName = "turnover_volume")]
        public string TurnoverVolume { get; set; }

        [XmlElement(ElementName = "turnover")]
        public string Turnover { get; set; }
    }

    #endregion
}
