﻿//------------------------------------------------------------------------------
// <copyright project="Examples" file="IntradayBarDataRequest.cs" company="Jordan Robinson">
//     Copyright (c) 2013 Jordan Robinson. All rights reserved.
//
//     The use of this software is governed by the Microsoft Public License
//     which is included with this distribution.
// </copyright>
//------------------------------------------------------------------------------

namespace Examples
{
    using System;
    using BEmu; //un-comment this line to use the Bloomberg API Emulator
    //using Bloomberglp.Blpapi; //un-comment this line to use the actual Bloomberg API

    public static class IntradayBarDataRequest
    {
        public static void RunExample()
        {
            var soptions = new SessionOptions {ServerHost = "127.0.0.1", ServerPort = 8194};

            var session = new Session(soptions);
            if (session.Start() && session.OpenService("//blp/refdata"))
            {
                Service service = session.GetService("//blp/refdata");
                Request request = service.CreateRequest("IntradayBarRequest");

                const string security = "SPY US EQUITY";
                request.Set("security", security); //required

                request.Set("eventType", "TRADE"); //optional: TRADE(default), BID, ASK, BID_BEST, ASK_BEST, BEST_BID, BEST_ASK, BID_YIELD, ASK_YIELD, MID_PRICE, AT_TRADE, SETTLE
                //note that BID_YIELD, ASK_YIELD, MID_PRICE, AT_TRADE, and SETTLE don't appear in the API documentation, but you will see them if you call "service.ToString() using the actual Bloomberg API"
                request.Set("eventTypes", "BID"); //A request can have multiple eventTypes

                //data goes back no farther than 140 days (7.2.4)
                DateTime dtStart = DateTime.Today.AddDays(-1); //yesterday
                request.Set("startDateTime", new Datetime(dtStart.AddHours(9.5).ToUniversalTime())); //Required Datetime, UTC time
                request.Set("endDateTime", new Datetime(dtStart.AddHours(16).ToUniversalTime())); //Required Datetime, UTC time

                //(Required) Sets the length of each time bar in the response. Entered as a whole number, between 1 and 1440 in minutes.
                //  One minute is the lowest possible granularity. (despite A.2.8, the interval setting cannot be omitted)
                request.Set("interval", 60);

                //When set to true, a bar contains the previous bar values if there was no tick during this time interval.
                request.Set("gapFillInitialBar", false); //Optional bool. Valid values are true and false (default = false)

                //Option on whether to return EIDs for the security.
                request.Set("returnEids", false); //Optional bool. Valid values are true and false (default = false)

                //Setting this to true will populate fieldData with an extra element containing a name and value for the relative date. For example RELATIVE_DATE = 2002 Q2
                request.Set("returnRelativeDate", false); //Optional bool. Valid values are true and false (default = false)

                //Adjust historical pricing to reflect: Regular Cash, Interim, 1st Interim, 2nd Interim, 3rd Interim, 4th Interim, 5th Interim, Income,
                //  Estimated, Partnership Distribution, Final, Interest on Capital, Distribution, Prorated.
                request.Set("adjustmentNormal", false); //Optional bool. Valid values are true and false (default = false)

                //Adjust historical pricing to reflect: Special Cash, Liquidation, Capital Gains, Long-Term Capital Gains, Short-Term Capital Gains, Memorial,
                //  Return of Capital, Rights Redemption, Miscellaneous, Return Premium, Preferred Rights Redemption, Proceeds/Rights, Proceeds/Shares, Proceeds/Warrants.
                request.Set("adjustmentAbnormal", false); //Optional bool. Valid values are true and false (default = false)

                //Adjust historical pricing and/or volume to reflect: Spin-Offs, Stock Splits/Consolidations, Stock Dividend/Bonus, Rights Offerings/Entitlement.
                request.Set("adjustmentSplit", false); //Optional bool. Valid values are true and false (default = false)

                //Setting to true will follow the DPDF<GO> BLOOMBERG PROFESSIONAL service function. True is the default setting for this option..
                request.Set("adjustmentFollowDPDF", false); //Optional bool. Valid values are true and false (default = false)


                session.SendRequest(request, new CorrelationID(-999));

                bool continueLoop = true;
                while (continueLoop)
                {
                    Event evt = session.NextEvent();
                    switch (evt.Type)
                    {
                        case Event.EventType.RESPONSE:
                            ProcessResponse(evt, security);
                            continueLoop = false;
                            break;
                        case Event.EventType.PARTIAL_RESPONSE:
                            ProcessResponse(evt, security);
                            break;
                    }
                }

            }
        }

        private static void ProcessResponse(Event evt, string security)
        {
            //Note that the IntradayBarResponse does not include the name of the requested security anywhere
            Console.WriteLine(security);

            foreach (var msg in evt.GetMessages())
            {
                Element elmBarTickDataArray = msg["barData"]["barTickData"];
                for (int valueIndex = 0; valueIndex < elmBarTickDataArray.NumValues; valueIndex++)
                {
                    Element elmBarTickData = elmBarTickDataArray.GetValueAsElement(valueIndex);

                    DateTime dtTick = elmBarTickData.GetElementAsDatetime("time").ToSystemDateTime();

                    double open = elmBarTickData.GetElementAsFloat64("open");
                    double high = elmBarTickData.GetElementAsFloat64("high");
                    double low = elmBarTickData.GetElementAsFloat64("low");
                    double close = elmBarTickData.GetElementAsFloat64("close");

                    int numEvents = elmBarTickData.GetElementAsInt32("numEvents");
                    long volume = elmBarTickData.GetElementAsInt64("volume");
                    double value = elmBarTickData.GetElementAsFloat64("value");

                    Console.WriteLine(dtTick.ToString("HH:mm:ss"));
                    Console.WriteLine(string.Format("\t open = {0:c2}", open));
                    Console.WriteLine(string.Format("\t high = {0:c2}", high));
                    Console.WriteLine(string.Format("\t low = {0:c2}", low));
                    Console.WriteLine(string.Format("\t close = {0:c2}", close));

                    Console.WriteLine(string.Format("\t numEvents = {0:n0}", numEvents));
                    Console.WriteLine(string.Format("\t volume = {0:n0}", volume));
                    Console.WriteLine(string.Format("\t value = {0:n0}", value));

                    Console.WriteLine();
                }
            }
        }

    }
}
