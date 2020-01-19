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
using System.Windows.Forms;
using System.Threading;
using Highlander.Orc.Messages;
using Highlander.Utilities.Logging;

namespace Highlander.Orc.Message.Test
{
    public partial class OrcTestForm : Form
    {

        private readonly SynchronizationContext _syncContext;

        // IAsyncLogger methods
        public void AsyncLogMessage(string msg) { _syncContext.Post(SyncLogMessage, msg); }
        public void SyncLogMessage(object state) { LogMessage((string)state); }
        public void AsyncLogException(Exception e) { _syncContext.Post(SyncLogException, e); }
        public void SyncLogException(object state) { LogException((Exception)state); }

        Client.Client _client;


        public OrcTestForm()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
        }

        public void LogMessage(string msg)
        {
            txtLog.AppendText(Environment.NewLine + DateTime.Now.ToLongTimeString() + ": ");
            txtLog.AppendText(msg);
        }
        public void LogException(Exception e)
        {
            txtLog.AppendText(Environment.NewLine + DateTime.Now.ToLongTimeString() + "!!!!!EXCEPTION!!!!!" + Environment.NewLine);
            txtLog.AppendText(e.ToString());
        }
        public void LogClear()
        {
            txtLog.Clear();
            txtLog.AppendText(Environment.NewLine + DateTime.Now.ToLongTimeString() + ": Log cleared.");
        }

        private void Form1Load(object sender, EventArgs e)
        {                 
     
        }

        private void Button1Click(object sender, EventArgs e)
        {
            //ILogger logger = Logger.CreateMultiLogger(new ILogger[] { Logger.CreateTextBoxLogger(txtLog), Logger.CreateDebugLogger() });
            ILogger logger = new TraceLogger(false);
            _client = new Client.Client(logger);
            Guid subsId1 = Guid.NewGuid();
            var loginMessage = new Login("NAB04", "test") { SubsID = subsId1.ToString() };
            var priceFeedToggle = new PriceFeedToggle();
            var instrumentID = new InstrumentID();
            priceFeedToggle.Toggle = true;
            instrumentID.InstrumentTag = 64155;
            priceFeedToggle.InstrumentId = instrumentID;
            var actions = new List<string> {"Theoretical price", "Base price", "Skew delta"};
            Guid subsId2 = Guid.NewGuid();
            Guid subsId3 = Guid.NewGuid();
            Guid subsId4 = Guid.NewGuid();
            Guid subsId5 = Guid.NewGuid();
            var insFeedMessage = new InstrumentFeedSubscriptionToggle { Market = "ASX" , Kind = "Call", Toggle = true , SubsID=subsId2.ToString() };                        
            var priceFeedMessage = new PriceFeedToggle { InstrumentId = instrumentID,  Toggle = true, SubsID = subsId3.ToString() };
            var theoCalcMessage = new TheoreticalCalculationGroupMessage { InstrumentId = instrumentID, Actions = actions, SubsID = subsId4.ToString() };
            var tradeFeedMessage = new TradeFeedSubscriptionToggle { Originator = "NAB04",  Toggle = true, SubsID = subsId5.ToString() };
            const string host = "10.16.177.53";
            const int port = 7980;
            _client.Start(host, port);
            _client.Send(loginMessage, subsId1, GenericResponseCatcher);
            //_Client.Send(priceFeedToggle);
            _client.Send(insFeedMessage, subsId2, GenericResponseCatcher);
            _client.Send(priceFeedMessage, subsId3, GenericResponseCatcher);
            _client.Send(theoCalcMessage, subsId4, GenericResponseCatcher);
            _client.Send(tradeFeedMessage, subsId5, GenericResponseCatcher);
        }

        public void GenericResponseCatcher(object state)
        {
            AsyncLogMessage((string)state);
        }
    }
}
