﻿//------------------------------------------------------------------------------
// <copyright project="Examples" file="HistoricalDataRequest.cs" company="Jordan Robinson">
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

    public static class HistoricalDataRequest
    {
        public static void RunExample()
        {
            var sessionOptions = new SessionOptions {ServerHost = "127.0.0.1", ServerPort = 8194};

            var session = new Session(sessionOptions);
            session.Start();
            session.OpenService("//blp/refdata");

            Service service = session.GetService("//blp/refdata");

            Request request = service.CreateRequest("HistoricalDataRequest");

            //request information for the following securities
            request.Append("securities", "IBM US EQUITY");
            request.Append("securities", "SPY US EQUITY");
            request.Append("securities", "C A COMDTY");
            request.Append("securities", "AAPL 150117C00600000 EQUITY"); //this is a stock option: TICKER yyMMdd[C/P]\d{8} EQUITY

            //include the following simple fields in the result
            request.Append("fields", "BID");
            request.Append("fields", "ASK");

            //Historical requests allow a few overrides.  See the developer's guide A.2.4 for more information.

            request.Set("startDate", DateTime.Today.AddMonths(-1).ToString("yyyyMMdd")); //Request that the information start three months ago from today.  This override is required.
            request.Set("endDate", DateTime.Today.AddDays(10).ToString("yyyyMMdd")); //Request that the information end three days before today.  This is an optional override.  The default is today.
            
            //Determine the frequency and calendar type of the output. To be used in conjunction with Period Selection.
            request.Set("periodicityAdjustment", "CALENDAR"); //Optional string.  Valid values are ACTUAL (default), CALENDAR, and FISCAL.

            //Determine the frequency of the output. To be used in conjunction with Period Adjustment.
            request.Set("periodicitySelection", "DAILY"); //Optional string.  Valid values are DAILY (default), WEEKLY, MONTHLY, QUARTERLY, SEMI_ANNUALLY, and YEARLY

            //Sets quote to Price or Yield for a debt instrument whose default value is quoted in yield (depending on pricing source).
            request.Set("pricingOption", "PRICING_OPTION_PRICE"); //Optional string.  Valid values are PRICING_OPTION_PRICE (default) and PRICING_OPTION_YIELD

            //Adjust for "change on day"
            request.Set("adjustmentNormal", true); //Optional bool. Valid values are true and false (default = false)

            //Adjusts for Anormal Cash Dividends
            request.Set("adjustmentAbnormal", false); //Optional bool. Valid values are true and false (default = false)

            //Capital Changes Defaults
            request.Set("adjustmentSplit", true); //Optional bool. Valid values are true and false (default = false)

            //The maximum number of data points to return, starting from the startDate
            //request.Set("maxDataPoints", 5); //Optional integer.  Valid values are positive integers.  The default is unspecified in which case the response will have all data points between startDate and endDate

            //Indicates whether to use the average or the closing price in quote calculation.
            request.Set("overrideOption", "OVERRIDE_OPTION_CLOSE"); //Optional string.  Valid values are OVERRIDE_OPTION_GPA for an average and OVERRIDE_OPTION_CLOSE (default) for the closing price

            var requestID = new CorrelationID(1);
            session.SendRequest(request, requestID);

            bool continueToLoop = true;
            while (continueToLoop)
            {
                Event eventObj = session.NextEvent();
                switch (eventObj.Type)
                {
                    case Event.EventType.RESPONSE: // final event
                        continueToLoop = false;
                        HandleResponseEvent(eventObj);
                        break;
                    case Event.EventType.PARTIAL_RESPONSE:
                        HandleResponseEvent(eventObj);
                        break;
                    default:
                        HandleOtherEvent(eventObj);
                        break;
                }
            }
        }

        private static void HandleResponseEvent(Event eventObj)
        {
            Console.WriteLine("EventType = " + eventObj.Type);
            foreach (Message message in eventObj.GetMessages())
            {
                Console.WriteLine();
                Console.WriteLine("correlationID= " + message.CorrelationID);
                Console.WriteLine("messageType = " + message.MessageType);
                
                Element elmSecurityData = message["securityData"];

                Element elmSecurity = elmSecurityData["security"];
                string security = elmSecurity.GetValueAsString();
                Console.WriteLine(security);

                Element elmFieldData = elmSecurityData["fieldData"];
                for (int valueIndex = 0; valueIndex < elmFieldData.NumValues; valueIndex++)
                {
                    Element elmValues = elmFieldData.GetValueAsElement(valueIndex);
                    DateTime date = elmValues.GetElementAsDate("date").ToSystemDateTime();
                    double bid = elmValues.GetElementAsFloat64("BID");
                    double ask = elmValues.GetElementAsFloat64("ASK");

                    Console.WriteLine(string.Format("{0:yyyy-MM-dd}: BID = {1}, ASK = {2}", date, bid, ask));
                }
            }
        }

        private static void HandleOtherEvent(Event eventObj)
        {
            Console.WriteLine("EventType=" + eventObj.Type);
            foreach (Message message in eventObj.GetMessages())
            {
                Console.WriteLine("correlationID=" + message.CorrelationID);
                Console.WriteLine("messageType=" + message.MessageType);
                Console.WriteLine(message.ToString());
                if (Event.EventType.SESSION_STATUS == eventObj.Type && message.MessageType.Equals("SessionTerminated"))
                {
                    Console.WriteLine("Terminating: " + message.MessageType);
                }
            }
        }
    }
}
