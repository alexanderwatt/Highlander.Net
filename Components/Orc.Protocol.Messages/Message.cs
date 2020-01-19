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
using System.Xml.Serialization;

namespace Highlander.Orc.Messages
{
    public enum MessageTypes
    {
        Unknown = 0, 
        login,
        logout,
        LOGIN_REPLY,
        REPLY,
        TRADE_FEED_TOGGLE,
        TRADE_FEED,
        TRADE_INSERT,
        TRADE_MODIFY,
        TRADE_DELETE,
        TRADE_GET,
        TRADE_RANGE_GET,
        INSTRUMENT_FEED_TOGGLE,
        INSTRUMENT_FEED,
        INSTRUMENT_DOWNLOAD,
        INSTRUMENT_GET,
        INSTRUMENT_UPDATE,
        INSTRUMENT_CREATE,
        INSTRUMENT_DELETE,
        INSTRUMENT_ATTRIBUTES_SET,
        INSTRUMENT_PARAMETERS_SET,
        VOLATILITY_RAW_SURFACE_GET,
        VOLATILITY_RAW_SURFACE_INSERT,
        VOLATILITY_SURFACE_GET,
        VOLATILITY_SURFACE_INSERT,
        VOLATILITY_SURFACE_DELETE,
        VOLATILITY_SURFACE_DOWNLOAD,
        VOLATILITY_MODEL_DOWNLOAD,
        UNDERLYING_DOWNLOAD,
        UNDERLYING_UPDATE,
        REFERENCE_PRICE_SET,
        REFERENCE_PRICE_GET,
        PORTFOLIO_POSITION_FEED_TOGGLE,
        PORTFOLIO_POSITION_FEED,
        PORTFOLIO_DOWNLOAD,
        PORTFOLIO_GET,
        PORTFOLIO_CREATE,
        PORTFOLIO_DELETE,
        PORTFOLIO_UPDATE,
        PORTFOLIO_POSITION_UPDATE,
        YIELD_CURVE_GET,
        YIELD_CURVE_INSERT,
        YIELD_CURVE_UPDATE,
        YIELD_CURVE_DELETE,
        YIELD_CURVE_DOWNLOAD,
        DIVIDEND_DELETE,
        DIVIDEND_GET,
        DIVIDEND_INSERT,
        DIVIDEND_UPDATE,
        THEORETICAL_CALCULATION,
        THEORETICAL_CALCULATION_FEED_STOP,
        THEORETICAL_CALCULATION_GROUP,
        PRICEFEED_BROADCAST,
        CURRENCY_CREATE,
        TICK_RULE_DELETE,
        TICK_RULE_DOWNLOAD,
        TICK_RULE_UPDATE,
        CALENDAR_DELETE,
        CALENDAR_DOWNLOAD,
        CALENDAR_ENTRIES_DELETE,
        CALENDAR_ENTRIES_INSERT,
        CALENDAR_GET,
        CALENDAR_INSERT,
        CALENDAR_SET_DEFAULT,
        CALENDAR_UPDATE,
        MONEY_FEED_TOGGLE,
        MONEY_FEED,
        MONEY_INSERT,
        MONEY_DELETE,
        MONEY_RANGE_GET,
        STRESSTEST,
        VOLATILITY_GET,
        PORTFOLIO_EMPTY,
        PRICE_GET,
        PRICEFEED_TOGGLE,
        PRICE_FEED,
        ORDER_INSERT,
        ORDER_REPLY

    }

    [Serializable]
    public abstract class MessageBase
    {
        public bool HasDifferentInformationThan(object netsObjectToCompare, out CompareEventArgs outcome)
        {
            CompareHelper helper = new CompareHelper();
            helper.CompareObjects(this, netsObjectToCompare, out outcome);
            if (outcome.CompareResult == 1)
                return true;
            return false;
        }
    }

    public class MessageHeader : MessageBase
    {
        public MessageHeader(MessageTypes messageType)
        {
            MessageType = messageType.ToString();         
        }

        public MessageHeader()
        {
        }

        [XmlElement(ElementName = "message_type")]
        public string MessageType { get; set; }

        [XmlElement(ElementName = "private")]
        public string PrivateKey { get; set; }
    }

    [XmlRootAttribute(ElementName = "orc_message")]
    public abstract class SendMessage : MessageBase
    {
        protected SendMessage(MessageTypes messageType)
        {
            Header = new MessageHeader(messageType);
        }

        [XmlElement(ElementName = "message_info")]
        public MessageHeader Header { get; set; }
    }

    [XmlRootAttribute(ElementName = "orc_message")]
    public abstract class ReplyMessage : MessageBase
    {
        protected ReplyMessage(MessageTypes messageType)
        {
            Header = new MessageHeader(messageType);
        }

        [XmlElement(ElementName = "reply_to")]
        public MessageHeader Header { get; set; }

        [XmlElement(ElementName = "error")]
        public int ErrorCode { get; set; }

        [XmlElement(ElementName = "error_description")]
        public string ErrorDescription { get; set; }
    }
}
