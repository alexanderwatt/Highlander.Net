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
using System.Runtime.Serialization;
using System.ServiceModel;
using Highlander.Metadata.Common;
using Highlander.Utilities;
using Highlander.Utilities.Logging;

namespace Highlander.Core.Common
{
    [DataContract]
    public class V111DiscoverReply
    {
        [DataMember]
        public string[] SupportedContracts;
    }

    [ServiceContract]
    public interface IDiscoverV111
    {
        // service discovery (2-way over TCP only)
        [OperationContract]
        V111DiscoverReply DiscoverServiceV111();
    }

    public class DiscoverSenderV111 : CustomClientBase<IDiscoverV111>, IDiscoverV111
    {
        public DiscoverSenderV111(AddressBinding addressBinding)
            : base(addressBinding)
        { }

        public V111DiscoverReply DiscoverServiceV111()
        {
            return Channel.DiscoverServiceV111();
        }
    }

    public class DiscoverRecverV111 : IDiscoverV111
    {
        private readonly IDiscoverV111 _channel;
        public DiscoverRecverV111(IDiscoverV111 channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public V111DiscoverReply DiscoverServiceV111()
        {
            return _channel.DiscoverServiceV111();
        }
    }

    public partial class V111Helpers
    {
        private static bool QueryService(ILogger logger, string host, int port, string serviceName,
            string[] requiredContracts, int maxRoundTrips, out Int64 bestRoundTrip)
        {
            bestRoundTrip = TimeSpan.FromSeconds(30).Ticks;
            AddressBinding addressBinding = WcfHelper.CreateAddressBinding(
                    WcfConst.NetTcp, host, port, serviceName, typeof(IDiscoverV111).Name);
            try
            {
                using (var client = new DiscoverSenderV111(addressBinding))
                {
                    // query service n times to find best round trip
                    int loop = 0;
                    while (loop < maxRoundTrips)
                    {
                        DateTimeOffset clientTime1 = DateTimeOffset.Now;
                        V111DiscoverReply reply = client.DiscoverServiceV111();
                        // fail if server does not support required contracts
                        if (requiredContracts != null)
                        {
                            foreach (string requiredContract in requiredContracts)
                            {
                                if (reply.SupportedContracts == null)
                                {
                                    logger.LogDebug("Server ({0}) does not support contract: '{1}'", addressBinding.Address.Uri.AbsoluteUri, requiredContract);
                                    return false;
                                }
                                bool supportsContract = false;
                                foreach (string supportedContract in reply.SupportedContracts)
                                {
                                    if (supportedContract == requiredContract)
                                        supportsContract = true;
                                }
                                if (!supportsContract)
                                {
                                    logger.LogDebug("Server ({0}) does not support contract: '{1}'", addressBinding.Address.Uri.AbsoluteUri, requiredContract);
                                    return false;
                                }
                            }
                        }
                        // all required contracts supported
                        DateTimeOffset clientTime2 = DateTimeOffset.Now;
                        Int64 roundTrip = clientTime2.Ticks - clientTime1.Ticks;
                        if ((roundTrip >= 0) && (roundTrip < bestRoundTrip))
                            bestRoundTrip = roundTrip;
                        // loop
                        loop++;
                    }
                    //logger.LogDebug("Server ({0}) answered ({1} ticks)", addressBinding.Address.Uri.AbsoluteUri, bestRoundTrip);
                    return true;
                }
            }
            catch (EndpointNotFoundException)
            {
                //logger.LogDebug("Server ({0}) not answering.", addressBinding.Address.Uri.AbsoluteUri);
                return false;
            }
            catch (CommunicationObjectFaultedException)
            {
                //logger.LogDebug("Server ({0}) not answering.", addressBinding.Address.Uri.AbsoluteUri);
                return false;
            }
            //catch (Exception excp)
            //{
            //    logger.LogError("Server ({0}) threw exception: {1}", addressBinding.Address.Uri.AbsoluteUri, excp);
            //    return false;
            //}
        }
        internal class RoundTripData
        {
            public readonly ServiceAddress Addr;
            public readonly Int64 Ticks;
            public RoundTripData(ServiceAddress addr, Int64 ticks) { Addr = addr; Ticks = ticks; }
        }

        public static ServiceAddress ResolveServer(ILogger logger, string serviceName, ServiceAddresses serviceAddrs, string[] requiredContracts)
        {
            var serverList = new List<RoundTripData>();
            foreach (ServiceAddress serviceAddr in serviceAddrs.List)
            {
                Int64 roundTrip; // ticks
                int maxRoundTrips = 1;
                if (serviceAddrs.List.Count > 1)
                    maxRoundTrips = 3;
                if (QueryService(logger, serviceAddr.Host, serviceAddr.Port, serviceName, requiredContracts, maxRoundTrips, out roundTrip))
                {
                    // add addr to list
                    serverList.Add(new RoundTripData(serviceAddr, roundTrip));
                }
            } // foreach server address
            // find 'nearest' server
            if (serverList.Count > 0)
            {
                ServiceAddress address = serverList[0].Addr;
                Int64 bestRoundTrip = serverList[0].Ticks;
                foreach (RoundTripData item in serverList)
                {
                    if (item.Ticks < bestRoundTrip)
                    {
                        address = item.Addr;
                        bestRoundTrip = item.Ticks;
                    }
                }
                return address;
            }
            throw new EndpointNotFoundException(
                $"No server in list '{serviceAddrs}' found!");
        }
    }
}
