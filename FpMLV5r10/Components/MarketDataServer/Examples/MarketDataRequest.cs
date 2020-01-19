﻿//------------------------------------------------------------------------------
// <copyright project="Examples" file="MarketDataRequest.cs" company="Jordan Robinson">
//     Copyright (c) 2013 Jordan Robinson. All rights reserved.
//
//     The use of this software is governed by the Microsoft Public License
//     which is included with this distribution.
// </copyright>
//------------------------------------------------------------------------------

namespace Examples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
        //un-comment the following two lines to use the Bloomberg API Emulator
    using BEmu;

    //this declaration specifies that EventHandler refers to BEmu.EventHandler and not System.EventHandler.  The Bloomberg API named this ambiguously.

    //un-comment the following two lines to use the actual Bloomberg API
    //using Bloomberglp.Blpapi;
    //using EventHandler = Bloomberglp.Blpapi.EventHandler; //this declaration specifies that EventHandler refers to Bloomberglp.Blpapi.EventHandler and not System.EventHandler.  The Bloomberg API named this ambiguously.

    public static class MarketDataRequest
    {
        public static void RunExample()
        {
            var sessionOptions = new SessionOptions {ServerHost = "localhost", ServerPort = 8194};

            var session = new Session(sessionOptions, ProcessEvent);
            session.StartAsync();
        }

        private static readonly List<string> Fields = new[] { "BID", "ASK", "LAST" }.ToList();

        private static void ProcessEvent(Event evt, Session session)
        {
            switch (evt.Type)
            {
                case Event.EventType.SESSION_STATUS: //use this to open the service
                    foreach (Message message in evt.GetMessages())
                    {
                        if (message.MessageType.Equals("SessionStarted"))
                        {
                            try
                            {
                                session.OpenServiceAsync("//blp/mktdata", new CorrelationID(-9999));
                            }
                            catch (Exception)
                            {
                                Console.Error.WriteLine("Could not open //blp/mktdata for async");
                            }
                        }
                    }
                    break;

                case Event.EventType.SERVICE_STATUS: //use this to subscribe to ticker feeds
                    var slist = new List<Subscription>();
                    
                    //Conflate the data to show every two seconds.
                    //  Please note that the Bloomberg API Emulator code does not treat this exactly correct: individual subscriptions should each have their own interval setting.
                    //  I have not coded that in the emulator.
                    List<string> options = new[] { "interval=2" }.ToList(); //2 seconds.  //Comment this line to receive a subscription data event whenever it happens in the market.

                    slist.Add(new Subscription("IBM US EQUITY", Fields, options));
                    slist.Add(new Subscription("SPY US EQUITY", Fields, options));
                    slist.Add(new Subscription("AAPL 150117C00600000 EQUITY", Fields, options));

                    session.Subscribe(slist);
                    break;

                case Event.EventType.SUBSCRIPTION_DATA:
                case Event.EventType.RESPONSE:
                case Event.EventType.PARTIAL_RESPONSE:
                    ProcessEvent(evt);
                    break;
            }
        }

        private static void ProcessEvent(Event evt)
        {
            const bool excludeNullElements = true;
            foreach (Message message in evt.GetMessages())
            {
                string security = message.TopicName;
                foreach (var field in Fields)
                {
                    //This ignores the extraneous fields in the response
                    if (message.HasElement(field, excludeNullElements)) //be careful, excludeNullElements is false by default
                    {
                        Element elmField = message[field];

                        Console.WriteLine(string.Format("{0:HH:mm:ss}: {1}, {2}", 
                            DateTime.Now,
                            security, 
                            elmField.ToString().Trim()));
                    }
                }
            }
            Console.WriteLine();
        }

    }
}