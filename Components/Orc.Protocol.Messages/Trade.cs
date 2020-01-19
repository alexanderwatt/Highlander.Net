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
using System.Xml.Serialization;


namespace Highlander.Orc.Messages
{

    #region Trade Feed Subscription Toggle
    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeFeedSubscriptionToggle : SendMessage
    {

        public TradeFeedSubscriptionToggle()
            : base(MessageTypes.TRADE_FEED_TOGGLE)
        {
        }

        public string SubsID
        {
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "toggle")]
        public bool Toggle { get; set; }

        [XmlElement(ElementName = "market")]
        public string Market { get; set; }

        [XmlElement(ElementName = "originator")]
        public string Originator { get; set; }

        [XmlElement(ElementName = "portfolio")]
        public string Portfolio { get; set; }

        [XmlElement(ElementName = "customer_id")]
        public string CustomerID { get; set; }

        [XmlElement(ElementName = "origin")]
        public string Origin { get; set; }
    }

    #endregion

    #region Trade Feed

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeFeed : ReplyMessage
    {
        public TradeFeed()
            : base(MessageTypes.TRADE_FEED)
        {
        }
        public string SubsID
        {
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "trade_reply")]
        public TradeReply TradeReply { get; set; }
    }

    #endregion

    #region Trade Reply

    /// <summary>
    /// NOTE: This is a sequentially numbered dictionary in the OP.
    /// </summary>
    [XmlRootAttribute(ElementName = "trade_reply")]
    public class TradeReply : MessageBase
    {
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        [XmlElement(ElementName = "counterpart_trade")]
        public bool CounterpartTrade { get; set; }

        [XmlElement(ElementName = "deleted")]
        public bool Deleted { get; set; }

        [XmlElement(ElementName = "trade")]
        public Trade Trade { get; set; }

        [XmlElement(ElementName = "trade_tag")]
        public int TradeTag { get; set; }

        [XmlElement(ElementName = "verified")]
        public bool Verified { get; set; }

        [XmlElement(ElementName = "basket_order_tag")]
        public int BasketOrderTag { get; set; }

        [XmlElement(ElementName = "date_changed")]
        public string DateChanged { get; set; }

        [XmlElement(ElementName = "date_created")]
        public string DateCreated { get; set; }

        [XmlElement(ElementName = "order_tag")]
        public int OrderTag { get; set; }

        [XmlElement(ElementName = "parent_order_tag")]
        public int ParentOrderTag { get; set; }

        [XmlElement(ElementName = "time_changed")]
        public string TimeChanged { get; set; }

        [XmlElement(ElementName = "time_created")]
        public string TimeCreated { get; set; }
    }

    #endregion

    #region Trade Insert

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeInsert : SendMessage
    {
        public TradeInsert()
            : base(MessageTypes.TRADE_INSERT)
        {
        }

        [XmlElement(ElementName = "trade")]
        public Trade Trade { get; set; }

        [XmlElement(ElementName = "update_portfolio")]
        public bool UpdatePortfolio { get; set; }

        [XmlElement(ElementName = "update_ticker")]
        public bool UpdateTicker { get; set; }

        [XmlElement(ElementName = "order_tag")]
        public int OrderTag { get; set; }
    }

    #endregion

    #region Trade Modify

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeModify : SendMessage
    {
        public TradeModify()
            : base(MessageTypes.TRADE_MODIFY)
        {
        }

        [XmlElement(ElementName = "trade_tag")]
        public int TradeTag { get; set; }

        [XmlElement(ElementName = "trade")]
        public Trade Trade { get; set; }

        [XmlElement(ElementName = "update_portfolio")]
        public bool UpdatePortfolio { get; set; }

        [XmlElement(ElementName = "verified")]
        public bool Verified { get; set; }
    }

    #endregion

    #region Trade Delete

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeDelete : SendMessage
    {
        public TradeDelete()
            : base(MessageTypes.TRADE_DELETE)
        {
        }

        [XmlElement(ElementName = "trade_tag")]
        public int TradeTag { get; set; }
    }

    #endregion

    #region Trade

    [XmlRootAttribute(ElementName = "trade")]
    public class Trade : MessageBase
    {
        [XmlElement(ElementName = "buy_or_sell")]
        public string BuyOrSell { get; set; }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "volume")]
        public double Volume { get; set; }

        [XmlArrayAttribute(ElementName = "additional_data"), XmlArrayItem(ElementName = "additional_data_entry")]
        public List<AdditionalDataEntry> AdditionalData { get; set; }

        [XmlElement(ElementName = "buyer")]
        public string Buyer { get; set; }

        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }

        [XmlElement(ElementName = "commission")]
        public double Commission { get; set; }

        [XmlElement(ElementName = "counterpart")]
        public string Counterparty { get; set; }

        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        [XmlElement(ElementName = "customer_id")]
        public string CustomerID { get; set; }

        [XmlElement(ElementName = "customer_reference")]
        public string CustomerReference { get; set; }

        [XmlElement(ElementName = "date_created")]
        public string DateCreated { get; set; }

        [XmlElement(ElementName = "exchange_order_id")]
        public string ExchangeOrderID { get; set; }

        [XmlElement(ElementName = "exchange_timestamp")]
        public string ExchangeTimestamp { get; set; }

        [XmlElement(ElementName = "exchange_trade_id")]
        public string ExchangeTradeID { get; set; }

        [XmlElement(ElementName = "fee")]
        public double Fee { get; set; }

        [XmlElement(ElementName = "fx_rate")]
        public double FxRate { get; set; }

        [XmlElement(ElementName = "invested")]
        public double Invested { get; set; }

        [XmlElement(ElementName = "market")]
        public string Market { get; set; }

        [XmlElement(ElementName = "origin")]
        public string Origin { get; set; }

        [XmlElement(ElementName = "originator")]
        public string Originator { get; set; }

        [XmlElement(ElementName = "owner")]
        public string Owner { get; set; }

        [XmlElement(ElementName = "portfolio")]
        public string Portfolio { get; set; }

        [XmlElement(ElementName = "price")]
        public double Price { get; set; }

        [XmlElement(ElementName = "priority")]
        public int Priority { get; set; }

        [XmlElement(ElementName = "seller")]
        public string Seller { get; set; }

        [XmlElement(ElementName = "settlement_date")]
        public string SettlementDate { get; set; }

        [XmlElement(ElementName = "submarket")]
        public string Submarket { get; set; }

        [XmlElement(ElementName = "trade_date")]
        public string TradeDate { get; set; }

        [XmlElement(ElementName = "trade_time")]
        public string TradeTime { get; set; }

        [XmlElement(ElementName = "unique_trade_id")]
        public string UniqueTradeID { get; set; }

        [XmlElement(ElementName = "ytm")]
        public double Ytm { get; set; }
    }

    #endregion

    #region Trade Get

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeGet : SendMessage
    {
        public TradeGet()
            : base(MessageTypes.TRADE_GET)
        {
        }

        [XmlElement(ElementName = "trade_tag")]
        public int TradeTag { get; set; }
    }

    #endregion

    #region Trade Get Reply

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeGetReply : ReplyMessage
    {
        public TradeGetReply()
            : base(MessageTypes.TRADE_GET)
        {
        }

        [XmlElement(ElementName = "trade_reply")]
        public TradeReply TradeReply { get; set; }
    }

    #endregion

    #region Trade Range Get

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeRangeGet : SendMessage
    {
        public TradeRangeGet()
            : base(MessageTypes.TRADE_RANGE_GET)
        {
        }

        [XmlElement(ElementName = "startdate")]
        public string StartDate { get; set; }

        [XmlElement(ElementName = "starttime")]
        public string StartTime { get; set; }

        [XmlElement(ElementName = "enddate")]
        public string EndDate { get; set; }

        [XmlElement(ElementName = "endtime")]
        public string EndTime { get; set; }

        [XmlElement(ElementName = "date_change_from")]
        public string DateChangedFrom { get; set; }

        [XmlElement(ElementName = "time_change_from")]
        public string TimeChangedFrom { get; set; }

        [XmlElement(ElementName = "date_change_to")]
        public string DateChangedTo { get; set; }

        [XmlElement(ElementName = "time_change_to")]
        public string TimeChangedTo { get; set; }
    }

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TradeRangeGetReply : ReplyMessage
    {
        public TradeRangeGetReply()
            : base(MessageTypes.TRADE_RANGE_GET)
        {
        }

        [XmlArrayAttribute(ElementName = "trade_replies"), XmlArrayItem(ElementName = "trade_reply")]
        public List<TradeReply> TradeReplies { get; set; }
    }

    #endregion

    #region Trade Additional Data

    [XmlRoot(ElementName = "additional_data_entry")]
    public class AdditionalDataEntry : MessageBase
    {
        [XmlElement(ElementName = "key", IsNullable = false)]
        public string Key { get; set; }

        [XmlElement(ElementName = "value", IsNullable = false)]
        public string Value { get; set; }
    }

    #endregion
}
