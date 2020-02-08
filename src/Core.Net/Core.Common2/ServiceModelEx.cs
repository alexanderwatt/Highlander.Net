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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Highlander.Core.Common
{
    public class AddressBinding
    {
        public readonly EndpointAddress Address;

        public readonly Binding Binding;

        public AddressBinding(EndpointAddress address, Binding binding)
        {
            Address = address;
            Binding = binding;
        }

        //public AddressBinding(string uri, int limitMultiplier, TimeSpan msgLifetime)
        //{
        //    Address = new EndpointAddress(uri);
        //    Binding = WcfHelper.CreateBinding(WcfHelper.GetSchemeFromAddress(Address), limitMultiplier, msgLifetime);
        //}
    }

    internal class EndpointData
    {
        public readonly string Scheme;

        public readonly int Port;

        public Binding Binding;

        public string Address;

        public ServiceEndpoint Endpoint;

        public string QueueName;

        public EndpointData(string schemePort)
        {
            string[] schemePortParts = schemePort.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Scheme = schemePortParts[0].ToLower();
            Port = int.Parse(schemePortParts[1]);
        }
    }

    public static class WcfHelper
    {
        // static members
        internal static string GetSchemeFromAddress(EndpointAddress address)
        {
            return address.Uri.Scheme.ToLower();
        }

        internal static Binding CreateBinding(string scheme, int limitMultiplier, TimeSpan msgLifetime)
        {
            // returns a wcf binding with send/recv limits to n times default size
            const int poolMultiplier = 8;
            switch (scheme)
            {
                //case WcfConst.NetMsmq:
                //    {
                //        var msmqBinding = new NetMsmqBinding(NetMsmqSecurityMode.None)
                //        {
                //            ExactlyOnce = false,
                //            Durable = false,
                //            TimeToLive = msgLifetime,
                //            MaxReceivedMessageSize = limitMultiplier * 64 * 1024,
                //            ReaderQuotas =
                //            {
                //                MaxStringContentLength = limitMultiplier * 8 * 1024,
                //                MaxArrayLength = limitMultiplier * 16384,
                //                MaxBytesPerRead = limitMultiplier * 4096
                //            },
                //            MaxBufferPoolSize = poolMultiplier * limitMultiplier * 512 * 1024
                //        };
                //        return msmqBinding;
                //    }
                case WcfConst.NetPipe:
                    {
                        var pipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
                        {
                            MaxReceivedMessageSize = limitMultiplier * 64 * 1024,
                            ReaderQuotas =
                            {
                                MaxStringContentLength = limitMultiplier * 8 * 1024,
                                MaxArrayLength = limitMultiplier * 16384,
                                MaxBytesPerRead = limitMultiplier * 4096
                            },
                            MaxBufferPoolSize = poolMultiplier * limitMultiplier * 512 * 1024
                        };
                        return pipeBinding;
                    }
                case WcfConst.NetTcp:
                    {
                        var tcpBinding = new NetTcpBinding(SecurityMode.None)
                        {
                            MaxReceivedMessageSize = limitMultiplier * 64 * 1024,
                            ReaderQuotas =
                            {
                                MaxStringContentLength = limitMultiplier * 8 * 1024,
                                MaxArrayLength = limitMultiplier * 16384,
                                MaxBytesPerRead = limitMultiplier * 4096
                            },
                            MaxBufferPoolSize = poolMultiplier * limitMultiplier * 512 * 1024//,
                            //PortSharingEnabled = true
                        };
                        return tcpBinding;
                    }
                case WcfConst.Http:
                    {
                        var httpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None)
                                              {
                                                  MaxReceivedMessageSize = limitMultiplier*64*1024,
                                                  ReaderQuotas =
                                                      {
                                                          MaxStringContentLength = limitMultiplier*8*1024,
                                                          MaxArrayLength = limitMultiplier*16384,
                                                          MaxBytesPerRead = limitMultiplier*4096
                                                      },
                                                  MaxBufferPoolSize = poolMultiplier*limitMultiplier*512*1024
                        };
                        return httpBinding;
                    }
                case WcfConst.NetHttp://TODO Currently in development
                {
                    var httpBinding = new NetHttpBinding(BasicHttpSecurityMode.None)
                    {
                        MaxReceivedMessageSize = limitMultiplier * 64 * 1024,
                        ReaderQuotas =
                        {
                            MaxStringContentLength = limitMultiplier*8*1024,
                            MaxArrayLength = limitMultiplier*16384,
                            MaxBytesPerRead = limitMultiplier*4096
                        },
                        MaxBufferPoolSize = poolMultiplier * limitMultiplier * 512 * 1024
                    };
                    return httpBinding;
                }
                case WcfConst.WebHttp://TODO Currently in development
                {
                    var httpBinding = new WSHttpBinding(SecurityMode.None)
                    {
                        MaxReceivedMessageSize = limitMultiplier * 64 * 1024,
                        ReaderQuotas =
                        {
                            MaxStringContentLength = limitMultiplier*8*1024,
                            MaxArrayLength = limitMultiplier*16384,
                            MaxBytesPerRead = limitMultiplier*4096
                        },
                        MaxBufferPoolSize = poolMultiplier * limitMultiplier * 512 * 1024
                    };
                    return httpBinding;
                }
                default:
                    throw new NotSupportedException($"scheme: '{scheme}'");
            }
        }

        //internal static string FormatHttpBaseAddress(string host, int port)
        //{
        //    return String.Format("{0}://{1}:{2}", Constants.WcfSchemeHttp, host ?? "localhost", port);
        //}
        internal static string FormatAddress(string scheme, string host, int port, string applName, string intfName)
        {
            switch (scheme)
            {
                case WcfConst.NetMsmq:
                    return $"{WcfConst.NetMsmq}://{host ?? "localhost"}/private/{applName}_{port}_{intfName}";
                case WcfConst.NetPipe:
                    return $"{WcfConst.NetPipe}://{host ?? "localhost"}/{applName}_{port}_{intfName}";
                case WcfConst.NetTcp:
                    return $"{WcfConst.NetTcp}://{host ?? "localhost"}:{port}/{applName}_{intfName}";
                case WcfConst.Http:
                    return $"{WcfConst.Http}://{host ?? "localhost"}:{port}/{applName}_{intfName}";
                case WcfConst.NetHttp:
                    return $"{WcfConst.NetHttp}://{host ?? "localhost"}:{port}/{applName}_{intfName}";
                case WcfConst.WebHttp:
                    return $"{WcfConst.WebHttp}://{host ?? "localhost"}:{port}/{applName}_{intfName}";
                default:
                    throw new NotSupportedException($"scheme: '{scheme}'");
            }
        }

        internal static string FormatQueueName(string applName, int port, string intfName)
        {
            return $@".\private$\{applName}_{port}_{intfName}";
        }

        public static AddressBinding CreateAddressBinding(
            string scheme, string host, int port, string applName, string intfName, int limitMultiplier, TimeSpan msgLifetime)
        {
            return new AddressBinding(
                new EndpointAddress(FormatAddress(scheme, host, port, applName, intfName)),
                CreateBinding(scheme, limitMultiplier, msgLifetime));
        }

        public static AddressBinding CreateAddressBinding(
            string scheme, string host, int port, string applName, string intfName)
        {
            return new AddressBinding(
                new EndpointAddress(FormatAddress(scheme, host, port, applName, intfName)),
                CreateBinding(scheme, WcfConst.LimitMultiplier, WcfConst.ExpressMsgLifetime));
        }

        public static AddressBinding CreateAddressBinding(
            string uri, int limitMultiplier, TimeSpan msgLifetime)
        {
            var address = new EndpointAddress(uri);
            Binding binding = CreateBinding(GetSchemeFromAddress(address), limitMultiplier, msgLifetime);
            return new AddressBinding(address, binding);
        }

        public static AddressBinding CreateAddressBinding(string uri)
        {
            var address = new EndpointAddress(uri);
            Binding binding = CreateBinding(GetSchemeFromAddress(address), WcfConst.LimitMultiplier, WcfConst.ExpressMsgLifetime);
            return new AddressBinding(address, binding);
        }
    }
}