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
using System.Net;
using System.Net.Sockets;
using Highlander.Utilities;
using Highlander.Utilities.Logging;
using CommunicationState = System.ServiceModel.CommunicationState;

namespace Highlander.Core.Common
{
    public class CustomServiceHost<I, S> : ServiceHostBase where S : I
    {
        // a service host configured for a multi-threaded singleton service instance

        // instance members
        private readonly ILogger _logger;
        private readonly EndpointData[] _endpoints;
        private readonly string _applName;
        private readonly string _intfName;
        public CustomServiceHost(
            ILogger logger, S serviceImpl, string endpoints, string applName, string intfName,
            bool multiThreaded)
            //: base(serviceImpl)
        {
            int limitMultiplier = WcfConst.LimitMultiplier;
            TimeSpan msgLifetime = WcfConst.ExpressMsgLifetime;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (endpoints == null)
                throw new ArgumentNullException(nameof(endpoints));
            string[] endpointsParts = endpoints.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (endpointsParts.Length == 0)
                throw new ArgumentNullException(nameof(endpoints));
            _endpoints = new EndpointData[endpointsParts.Length];
            for (int i = 0; i < _endpoints.Length; i++)
                _endpoints[i] = new EndpointData(endpointsParts[i]);
            _applName = applName ?? throw new ArgumentNullException(nameof(applName));
            _intfName = intfName ?? throw new ArgumentNullException(nameof(intfName));
            // add standard behaviour
            ServiceBehaviorAttribute serviceBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (serviceBehavior == null)
            {
                serviceBehavior = new ServiceBehaviorAttribute();
                Description.Behaviors.Add(serviceBehavior);
            }
            // exception handling
            //serviceBehavior.IncludeExceptionDetailInFaults = true;
            // multi-threading singleton mode
            //serviceBehavior.UseSynchronizationContext = false;
            serviceBehavior.InstanceContextMode = InstanceContextMode.Single;
            serviceBehavior.ConcurrencyMode = multiThreaded ? ConcurrencyMode.Multiple : ConcurrencyMode.Single;
            // add endpoints
            string httpAddress = null;
            foreach (var endpoint in _endpoints)
            {
                endpoint.Binding = WcfHelper.CreateBinding(endpoint.Scheme, limitMultiplier, msgLifetime);
                endpoint.Address = WcfHelper.FormatAddress(endpoint.Scheme, null, endpoint.Port, _applName, _intfName);
                // save http base address for metadata behaviour setup (below)
                if (endpoint.Scheme == WcfConst.Http)
                {
                    httpAddress = endpoint.Address;
                }
                endpoint.Endpoint = AddServiceEndpoint(typeof(I), endpoint.Binding, endpoint.Address);
                if (endpoint.Scheme == WcfConst.NetMsmq)
                    endpoint.QueueName = WcfHelper.FormatQueueName(_applName, endpoint.Port, _intfName);
                //_Logger.LogDebug("{1}: Added service endpoint: {0}", _Endpoints[i].Endpoint.Address, this.GetType().Name);
            }
            // add metadata behaviour (if HTTP endpoint exists)
            ServiceMetadataBehavior metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
            if ((metadataBehavior == null) && (httpAddress != null))
            {
                metadataBehavior = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = new Uri(httpAddress)
                };
                Description.Behaviors.Add(metadataBehavior);
                _logger.LogDebug("Added MEX for endpoint: {0}", httpAddress);
            }
            // ready to open
            Open();
            if (State != CommunicationState.Opened)
            {
                throw new Exception($"ServiceHost State ({State}) <> Opened!");
            }
            //_Logger.LogDebug("ServiceHost State: {0}", this.State);
        }

        //public CustomServiceHost(
        //    ILogger logger, S serviceImpl, string endpoints, string applName, string intfName)
        //    : this(logger, serviceImpl, endpoints, applName, intfName, 
        //        false)
        //{ }

        protected override void OnOpening()
        {
            //_Logger.LogDebug("{0}: Opening", this.GetType().Name);
            // ensure queues exist before service host opens
            foreach (EndpointData epData in _endpoints)
            {
                if (epData.Scheme != WcfConst.NetMsmq) continue;
                if (!MessageQueue.Exists(epData.QueueName))
                    MessageQueue.Create(epData.QueueName, false);
            }
            base.OnOpening();
            //_Logger.LogDebug("{0}: Opened", this.GetType().Name);
        }

        public List<string> GetIpV4Addresses(string scheme)
        {
            List<string> results = new List<string>();
            // get IP address instead of hostname for addresses
            string hostIpv4 = null;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    hostIpv4 = ipAddress.ToString();
            }
            if (hostIpv4 == null)
                throw new Exception("Unable to resolve host name to IPV4 address!");
            foreach (EndpointData ep in _endpoints)
            {
                if (scheme == null || ep.Scheme.ToLower() == scheme.ToLower())
                    results.Add(WcfHelper.FormatAddress(ep.Scheme, hostIpv4, ep.Port, _applName, _intfName));
            }
            return results;
        }

        protected override ServiceDescription CreateDescription(out IDictionary<string, CoreWCF.Description.ContractDescription> implementedContracts)
        {
            throw new NotImplementedException();
        }

        protected override void OnClosed()
        {
            //_Logger.LogDebug("{0}: Closing", this.GetType().Name);
            // remove queues after service host closed
            foreach (var endpoint in _endpoints)
            {
                if (endpoint.Scheme != WcfConst.NetMsmq) continue;
                try
                {
                    if (MessageQueue.Exists(endpoint.QueueName))
                        MessageQueue.Delete(endpoint.QueueName);
                }
                catch (MessageQueueException mqExcp)
                {
                    _logger.Log(mqExcp);
                }
            }

            base.OnClosed();
            //_Logger.LogDebug("{0}: Closed", this.GetType().Name);
        }
    }
}
