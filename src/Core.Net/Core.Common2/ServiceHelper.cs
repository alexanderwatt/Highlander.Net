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

namespace Highlander.Core.Common
{
    /// <summary>
    /// The node type
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// undefined (not initialised) mode
        /// </summary>
        Undefined,
        /// <summary>
        /// server or root node
        /// eg. persistent store manager
        /// </summary>
        Server,
        /// <summary>
        /// router or intermediary mode
        /// eg. non-persistent server providing caching and routing services
        /// </summary>
        Router,
        /// <summary>
        /// client or leaf node
        /// eg. a real-time end-user application, or spreadsheet
        /// </summary>
        Client,
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResolvedService
    {
        public string Host;
        public int Port;
        public Guid Token;
    }

    public class ServiceAddress
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string Protocol;

        /// <summary>
        /// 
        /// </summary>
        public readonly string Host;

        /// <summary>
        /// 
        /// </summary>
        public readonly int Port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="address"></param>
        /// <param name="defaultPort"></param>
        public ServiceAddress(string protocol, string address, int defaultPort)
        {
            Protocol = protocol;
            string[] parts = address.Split(':');
            Host = parts[0];
            Port = defaultPort;
            if (parts.Length > 1)
                Port = int.Parse(parts[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ServiceAddresses
    {
        private readonly ServiceAddress[] _addrs;

        /// <summary>
        /// 
        /// </summary>
        public IList<ServiceAddress> List => _addrs;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addrs"></param>
        public ServiceAddresses(ServiceAddress[] addrs)
        {
            _addrs = addrs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="addresses"></param>
        /// <param name="defaultPort"></param>
        public ServiceAddresses(string[] protocols, IEnumerable<string> addresses, int defaultPort)
        {
            var result = new List<ServiceAddress>();
            foreach (string t1 in addresses)
            {
                result.AddRange(protocols.Select(t => new ServiceAddress(t, t1, defaultPort)));
            }
            _addrs = result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="addr"></param>
        /// <param name="defaultPort"></param>
        public ServiceAddresses(string scheme, string addr, int defaultPort)
        {
            _addrs = new[] { new ServiceAddress(scheme, addr, defaultPort) };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _addrs.Length; i++)
            {
                if (i > 0) sb.Append(";");
                sb.Append(_addrs[i]);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ServiceHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string FormatAddress(string host, int port)
        {
            return $"{host}:{port}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string FormatEndpoint(string scheme, int port)
        {
            return $"{scheme}:{port}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileVersion"></param>
        /// <param name="majorVersion"></param>
        /// <param name="minorVersion"></param>
        /// <param name="buildDate"></param>
        public static void ParseBuildLabel(string fileVersion, out int majorVersion, out int minorVersion, out DateTime buildDate)
        {
            // parses a build label in the "a.b.mmdd.bbnn" format used by TFS
            string[] fileVersionParts = fileVersion.Split('.');
            if (fileVersionParts.Length != 4)
                throw new FormatException("File version must be in 'a.b.mmdd.bbnn' format!");
            bool parsed = true;
            majorVersion = 0;
            //parsed = parsed && Int32.TryParse(fileVersionParts[0], out majorVersion);
            parsed = Int32.TryParse(fileVersionParts[0], out majorVersion);
            minorVersion = 0;
            parsed = parsed && Int32.TryParse(fileVersionParts[1], out minorVersion);
            //int yy = 0;
            //parsed = parsed && Int32.TryParse(fileVersionParts[2].Substring(0, 2), out yy);
            int monthNum = 0;
            parsed = parsed && Int32.TryParse(fileVersionParts[2].Substring(0, 2), out monthNum);
            int dd = 0;
            parsed = parsed && Int32.TryParse(fileVersionParts[2].Substring(2, 2), out dd);
            //int nn = 0;
            //parsed = parsed && Int32.TryParse(fileVersionParts[3].Substring(2, 2), out nn);
            if (!parsed)
                throw new FormatException("File version must be in 'a.b.mmdd.bbnn' format!");
            // derived year and month from monthNum
            // e.g. 1301 = 1st Jan 2019
            int yyyy = (monthNum - 1) / 12 + 2018;
            int mm = (monthNum - 1) % 12 + 1;
            buildDate = new DateTime(yyyy, mm, dd);
        }
    }
}
