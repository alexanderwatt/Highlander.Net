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
using System.ServiceModel;
using Highlander.Metadata.Common;

#endregion

namespace Highlander.Core.Common
{
    [ServiceContract]
    public interface ISessCtrlV131
    {
        // session control (2-way over TCP only)
        [OperationContract]
        V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo);

        [OperationContract]
        void CloseSessionV131(V131SessionHeader header);
    }

    /// <summary>
    /// 
    /// </summary>
    public class SessCtrlSenderV131 : CustomClientBase<ISessCtrlV131>, ISessCtrlV131
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressBinding"></param>
        public SessCtrlSenderV131(AddressBinding addressBinding)
            : base(addressBinding)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo)
        {
            return Channel.BeginSessionV131(header, clientInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        public void CloseSessionV131(V131SessionHeader header)
        {
            Channel.CloseSessionV131(header);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SessCtrlRecverV131 : ISessCtrlV131
    {
        private readonly ISessCtrlV131 _channel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public SessCtrlRecverV131(ISessCtrlV131 channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo)
        {
            return _channel.BeginSessionV131(header, clientInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        public void CloseSessionV131(V131SessionHeader header)
        {
            _channel.CloseSessionV131(header);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class V131ModuleInfo : IModuleInfo
    {
        private readonly V131ClientInfo _clientInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientInfo"></param>
        public V131ModuleInfo(V131ClientInfo clientInfo) { _clientInfo = clientInfo; }

        #region IModuleInfo Members

        public Guid NodeGuid => _clientInfo.NodeGuid;
        public EnvId BuildEnv => CoreHelper.ToEnvId(_clientInfo.BuildEnv);
        public EnvId ConfigEnv => CoreHelper.ToEnvId(_clientInfo.ConfigEnv);
        public string HostName => _clientInfo.HostName;
        public string HostIpV4 => _clientInfo.HostIpV4;
        public string[] NetAddrs => _clientInfo.NetAddrs;
        public string UserName => _clientInfo.UserInfo.UserIdentityName.Split('\\')[1];
        public string UserWDom => _clientInfo.UserInfo.UserIdentityName.Split('\\')[0];
        public string ApplName => _clientInfo.ApplInfo.AssmName;
        public string ApplNVer => _clientInfo.ApplInfo.AssmNVer;
        public string ApplFVer => _clientInfo.ApplInfo.AssmFVer;
        public string ApplPTok => _clientInfo.ApplInfo.AssmPTok;
        public int ApplHash => _clientInfo.ApplInfo.AssmHash;
        public string CoreName => _clientInfo.CompInfo.AssmName;
        public string CoreNVer => _clientInfo.CompInfo.AssmNVer;
        public string CoreFVer => _clientInfo.CompInfo.AssmFVer;
        public string CorePTok => _clientInfo.CompInfo.AssmPTok;
        public int CoreHash => _clientInfo.CompInfo.AssmHash;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public string UserFullName => null;

        // IIdentity methods
        /// <summary>
        /// 
        /// </summary>
        public string Name => _clientInfo.UserInfo.UserIdentityName;

        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationType => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        public bool IsAuthenticated => throw new NotImplementedException();
    }
}
