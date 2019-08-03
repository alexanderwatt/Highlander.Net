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

#region Usings

using System;
using Core.Common;

#endregion

namespace Core.Alert.Server
{
    public class InternalSignal
    {
        public string RuleName { get; set; }
        public string AlertServer { get; set; }
        public string SignalMessage { get; set; }
        public AlertStatus Status { get; set; }
        public int ReminderCount { get; set; }
        public DateTimeOffset LastMonitored { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AlertSignal AsAlertSignal()
        {
            AlertSignal result = new AlertSignal
                                     {
                                         AlertServer = AlertServer,
                                         RuleUniqueId = RuleName,
                                         LastMonitored = LastMonitored.ToString(),
                                         SignalMessage = SignalMessage,
                                         Status = Status.ToString()
                                     };
            return result;
        }
    }
}
