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

    #region Order Insert

    [XmlRoot(ElementName = "orc_message")]
    public class OrderInsert : SendMessage
    {
        public OrderInsert()
            : base(MessageTypes.ORDER_INSERT)
        {
        }

        public String SubsID
        {
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "activate")]
        public string Activate { get; set; }

        [XmlElement(ElementName = "order")]
        public Order Order { get; set; }
    }

    #endregion

    #region Order

    [XmlRoot(ElementName = "order")]
    public class Order : MessageBase
    {
        [XmlElement(ElementName = "buy_or_sell")]
        public string BuyOrSell { get; set; }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "volume")]
        public double Volume { get; set; }

        [XmlArray(ElementName = "additional_data"), XmlArrayItem(ElementName = "additional_data_entry")]
        public List<AdditionalDataEntry> AdditionalData { get; set; }

        //private string _buyer;

        //[XmlElement(ElementName = "buyer")]
        //public string Buyer
        //{
        //   get { return _buyer; }
        //   set { _buyer = value; }
        //}

        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }

        //private double _commission;

        //[XmlElement(ElementName = "commission")]
        //public double Commission
        //{
        //   get { return _commission; }
        //   set { _commission = value; }
        //}


        //private string _currency;

        //[XmlElement(ElementName = "currency")]
        //public string Currency
        //{
        //   get { return _currency; }
        //   set { _currency = value; }
        //}

        //private string _customerID;

        //[XmlElement(ElementName = "customer_id")]
        //public string CustomerID
        //{
        //   get { return _customerID; }
        //   set { _customerID = value; }
        //}

        //private string _customerReference;

        //[XmlElement(ElementName = "customer_reference")]
        //public string CustomerReference
        //{
        //   get { return _customerReference; }
        //   set { _customerReference = value; }
        //}

        //private double _fee;

        //[XmlElement(ElementName = "fee")]
        //public double Fee
        //{
        //   get { return _fee; }
        //   set { _fee = value; }
        //}

        //private string _market;

        //[XmlElement(ElementName = "market")]
        //public string Market
        //{
        //   get { return _market; }
        //   set { _market = value; }
        //}

        //private string _order_exec_style;

        //[XmlElement(ElementName = "order_exec_style")]
        //public string Order_exec_style
        //{
        //   get { return _order_exec_style; }
        //   set { _order_exec_style = value; }
        //}

        [XmlElement(ElementName = "portfolio")]
        public string Portfolio { get; set; }

        [XmlElement(ElementName = "price")]
        public double Price { get; set; }

        //private int _priority;

        //[XmlElement(ElementName = "priority")]
        //public int Priority
        //{
        //   get { return _priority; }
        //   set { _priority = value; }
        //}

        [XmlElement(ElementName = "price_condition")]
        public string PriceCondition { get; set; }


        //private string _settlementDate;

        //[XmlElement(ElementName = "settlement_date")]
        //public string SettlementDate
        //{
        //   get { return _settlementDate; }
        //   set { _settlementDate = value; }
        //}

        //private string _submarket;

        //[XmlElement(ElementName = "submarket")]
        //public string Submarket
        //{
        //   get { return _submarket; }
        //   set { _submarket = value; }
        //}

        [XmlElement(ElementName = "validity")]
        public string Validity { get; set; }

        //private double _ytm;

        //[XmlElement(ElementName = "ytm")]
        //public double Ytm
        //{
        //   get { return _ytm; }
        //   set { _ytm = value; }
        //}

    }

    #endregion

    #region Order Reply

    [XmlRoot(ElementName = "orc_message")]
    public class OrderReply : ReplyMessage
    {
        public OrderReply()
            : base(MessageTypes.ORDER_REPLY)
        {
        }

        [XmlElement(ElementName = "activate")]
        public string Activate { get; set; }

        [XmlElement(ElementName = "order_tag")]
        public int OrderTag { get; set; }

        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }
    }

    #endregion
}
