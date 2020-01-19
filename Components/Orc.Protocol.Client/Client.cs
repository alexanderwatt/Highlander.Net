/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Text;
using System.Threading;
using System.Xml.XPath;
using Highlander.Orc.AsyncSockets;
using Highlander.Orc.Messages;
using Highlander.Utilities.Logging;

namespace Highlander.Orc.Client
{

    public class Client
    {
 

        private readonly ISequencer _sequencer;
        private readonly IDictionary<Guid, SubscriptionDetail> _subsDetailIndex;
        private ILogger _logger;

        private readonly bool _debugMessages;

        public Client(ILogger logger)
        {
            _subsDetailIndex = new Dictionary<Guid, SubscriptionDetail>();
            _sequencer = new Sequencer();
            _logger = logger;
        }

        public OPAdapter OpAdaptor { get; private set; }


        public void Start(string host, int port) 
        {           
            OpAdaptor = new OPAdapter(OnMessageReceivedEvent);          
            OpAdaptor.Startup(host,port);
        }

        public void Send(MessageBase msg, Guid subscriptionId, WaitCallback callback)
        {
            // check if subscription is old or expired - todo
            SubscriptionDetail subsDetail = new SubscriptionDetail(subscriptionId, callback);
            lock (_subsDetailIndex)
            {
                _subsDetailIndex.Add(subscriptionId, subsDetail);
            }
            OPParser xParse = new OPParser {DebugMessages = _debugMessages};
            string sOp = xParse.XMLtoOP(msg);
            byte[] buffer = Encoding.ASCII.GetBytes(sOp);
            OpAdaptor.SendServer.Send(buffer);
        }

        /// <summary>
        /// Despatcher.
        /// </summary>
        /// <param name="reply">The reply.</param>
        private void OnMessageReceivedEvent(string reply)
        {
            //receive message;
            try
            {
                var array = reply.ToCharArray();
                int openBrackets = array.Count(eachchar => (eachchar == '}')
                );
                int closeBrackets = array.Count(eachchar => (eachchar == '{')
                );
                if (openBrackets != closeBrackets)
                    throw new Exception("Message is incomplete");
                OPParser xParse = new OPParser();
                String strippedReply = reply.Substring(10);
                //System.Diagnostics.Debug.Print(reply);
                XPathNavigator xNav = xParse.OPtoXML(strippedReply);
                var guid = xNav.SelectSingleNode("//private")?.ToString();
                Guid subscriptionId = new Guid(guid);
                lock (_subsDetailIndex)
                {
                    if (_subsDetailIndex.TryGetValue(subscriptionId, out _))
                    {
                        // Don't want to parse here (later on want to parallelise this) but just to test       
                        const string key1 = "feed";
                        const string key2 = "resp";
                        try
                        {
                            string messageType = xNav.SelectSingleNode("//message_type")?.ToString();
                            if (messageType.ToUpper() == MessageTypes.INSTRUMENT_FEED.ToString())
                                _sequencer.SequenceCallbackWithKey(key1, InstrumentFeedHandler, xNav);
                            else if (messageType.ToUpper() == MessageTypes.PRICE_FEED.ToString())
                                _sequencer.SequenceCallbackWithKey(key1, PriceFeedHandler, xNav);
                            else if (messageType.ToUpper() == MessageTypes.THEORETICAL_CALCULATION_GROUP.ToString())
                                _sequencer.SequenceCallbackWithKey(key2, TheoreticalCalculationHandler, xNav);
                            else if (messageType.ToUpper() == MessageTypes.TRADE_FEED.ToString())
                                _sequencer.SequenceCallbackWithKey(key1, TradeFeedHandler, xNav);
                            else if (messageType.ToUpper() == MessageTypes.ORDER_INSERT.ToString())
                                _sequencer.SequenceCallbackWithKey(key2, OrderHandler, xNav);
                        }
                        catch (Exception ex)
                        {
                            throw new System.MissingFieldException("No message type in header " + ex.Message);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw new System.FormatException("Problem receiving message :" + ex.Message);
            }
        }


        /// <summary>
        /// Handle the instrument feed reply
        /// </summary>
        /// <param name="state">The state.</param>
        private void InstrumentFeedHandler(object state)
        {
            try
            {

                XPathNavigator xNav = (XPathNavigator)state;
                var guid = xNav.SelectSingleNode("//private")?.ToString();
                InstrumentFeed feed = new InstrumentFeed();
                Guid subscriptionId = new Guid(guid ?? throw new InvalidOperationException());
                string sMessage = xNav.OuterXml;
                string underlying = "";
                if (sMessage != "")
                {
                    //Extract the Orc XML into the business object.
                    feed = (InstrumentFeed)XmlHelper.Deserialize(sMessage, feed.GetType());
                    if (feed.InstrumentID != null)
                    {
                        underlying = feed.InstrumentID.Underlying;                                                
                    }
                }
                lock (_subsDetailIndex)
                {
                    if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsStatus))
                    {
                        _sequencer.SequenceCallbackWithKey(underlying, subsStatus.ClientCallback, xNav.InnerXml);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException("Problem receiving message :" + ex.Message);
            }
        }
        
        /// <summary>
        /// Handle price feed reply
        /// </summary>
        /// <param name="state">The state.</param>
        private void PriceFeedHandler(object state)
        {
           
            try
            {
                XPathNavigator xNav = (XPathNavigator)state;
                string guid = xNav.SelectSingleNode("//private")?.ToString();
                Guid subscriptionId = new Guid(guid);
                var feed = new PriceFeed();
                string sMessage = xNav.OuterXml;
                string underlying = "";
                if (sMessage != "")
                {
                    //Extract the Orc XML into the business object.
                    feed = (PriceFeed)XmlHelper.Deserialize(sMessage, feed.GetType());
                    if (feed.InstrumentId != null)
                        underlying = feed.InstrumentId.Underlying;
                }
                lock (_subsDetailIndex)
                {
                    if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsStatus))
                    {
                        _sequencer.SequenceCallbackWithKey(underlying, subsStatus.ClientCallback, (object)xNav.InnerXml.ToString());
                    }
                }
            }            
            catch (Exception ex)
            {
                throw new FormatException("Problem receiving price feed message:" + ex.Message);
            }
        }

        /// <summary>
        /// Handle the theoretical calc reply
        /// </summary>
        /// <param name="state">The state.</param>
        private void TheoreticalCalculationHandler(object state)
        {
            XPathNavigator xNav = (XPathNavigator)state;
            string guid = xNav.SelectSingleNode("//private")?.ToString();
            Guid subscriptionId = new Guid(guid);
            TheoreticalCalculationGroupReply feed = new TheoreticalCalculationGroupReply();
            string sMessage = xNav.OuterXml;
            string key2 = "resp";
            if (sMessage != "")
            {
                //Extract the Orc XML into the business object.
                feed = (TheoreticalCalculationGroupReply)XmlHelper.Deserialize(sMessage, feed.GetType());    
            }
            lock (_subsDetailIndex)
            {
                if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsStatus))
                {
                    _sequencer.SequenceCallbackWithKey(key2, subsStatus.ClientCallback, xNav.InnerXml);
                }
            }
        }

        /// <summary>
        /// Handle the theoretical calc reply
        /// </summary>
        /// <param name="state">The state.</param>
        private void TradeFeedHandler(object state)
        {
            XPathNavigator xNav = (XPathNavigator)state;
            string guid = xNav.SelectSingleNode("//private")?.ToString();
            Guid subscriptionId = new Guid(guid);
            TradeFeed feed = new TradeFeed();
            string sMessage = xNav.OuterXml;
            string key2 = "resp";
            if (sMessage != "")
            {
                //Extract the Orc XML into the business object.
                feed = (TradeFeed)XmlHelper.Deserialize(sMessage, feed.GetType());              
            }
            lock (_subsDetailIndex)
            {
                if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsStatus))
                {
                    _sequencer.SequenceCallbackWithKey(key2, subsStatus.ClientCallback, xNav.InnerXml);
                }
            }
        }

        /// <summary>
        /// Handle the order reply
        /// </summary>
        /// <param name="state">The state.</param>
        private void OrderHandler(object state)
        {
            XPathNavigator xNav = (XPathNavigator)state;
            string guid = xNav.SelectSingleNode("//private")?.ToString();
            Guid subscriptionId = new Guid(guid);
            OrderReply feed = new OrderReply();
            string sMessage = xNav.OuterXml;
            string key2 = "resp";
            if (sMessage != "")
            {
                //Extract the Orc XML into the business object.
                feed = (OrderReply)XmlHelper.Deserialize(sMessage, feed.GetType());
            }
            lock (_subsDetailIndex)
            {
                if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsStatus))
                {
                    _sequencer.SequenceCallbackWithKey(key2, subsStatus.ClientCallback, xNav.InnerXml);
                }
            }
        }
    }

    internal class SubscriptionDetail
    {
        public Guid SubsId { get; }

        public WaitCallback ClientCallback { get; }

        public SubscriptionDetail(
            Guid subscriptionId,
            WaitCallback clientCallback)
        {
            SubsId = subscriptionId;
            ClientCallback = clientCallback;
        }
    }
}
