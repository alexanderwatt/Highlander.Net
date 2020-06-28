/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Constants
{
    public class WFPropName
    {
        public const string EnvId = "EnvId";
        public const string Port = "Port";
        public const string Hosts = "Hosts";
        public const string NodeId = "NodeId";
        public const string ExcpName = "Excp.Name";
        public const string ExcpText = "Excp.Text";
    }

    public static class WFHelper
    {
        public static string GetExcpName(Exception e)
        {
            if (e.InnerException != null)
                return $"{e.GetType().Name}[{GetExcpName(e.InnerException)}]";
            return e.GetType().Name;
        }
        public static string GetExcpText(Exception e)
        {
            if (e.InnerException != null)
                return $"{e.Message}[{GetExcpText(e.InnerException)}]";
            return e.Message;
        }
    }
}
