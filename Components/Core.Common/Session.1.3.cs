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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.ServiceModel;
using Highlander.Build;
using Highlander.Utilities.Logging;

#endregion

namespace Highlander.Core.Common
{
    public enum V131EnvId
    {
        Undefined,
        UTT_UnitTest,
        DEV_Development,
        SIT_SystemTest,
        STG_StagingLive,
        PRD_Production
    }

    public partial class V131Helpers
    {
        /// <summary>
        /// Checks the required file version of a candidate against required. Version numbers must be in the 4-part
        /// dotted "Major.Minor.BuildDate.Revision" format. The "Major" parts must be equal. For the "Minor" and "BuildDate"
        /// parts, the candidate must be greater than or equal to the required. The "Revision" parts are ignored.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="requiredVersion">The required version.</param>
        /// <param name="candidateVersion">The candidate version.</param>
        /// <returns></returns>
        public static bool CheckRequiredFileVersion(ILogger logger, string requiredVersion, string candidateVersion)
        {
            bool result = false;
            try
            {
                ServiceHelper.ParseBuildLabel(requiredVersion, out var requiredMajorVersion, out var requiredMinorVersion, out var requiredBuildDate);
                ServiceHelper.ParseBuildLabel(candidateVersion, out var candidateMajorVersion, out var candidateMinorVersion, out var candidateBuildDate);
                result =
                    candidateMajorVersion == requiredMajorVersion &&
                    candidateMinorVersion >= requiredMinorVersion &&
                    candidateBuildDate >= requiredBuildDate;
            }
            catch (FormatException e)
            {
                logger.Log(e);
            }
            return result;
        }
    }

    [DataContract]
    public class V131AssmInfo
    {
        [DataMember]
        private readonly string _assmName;
        public string AssmName => _assmName;

        [DataMember]
        private readonly string _assmNVer;
        public string AssmNVer => _assmNVer;

        [DataMember]
        private readonly string _assmFVer;
        public string AssmFVer => _assmFVer;

        [DataMember]
        private readonly string _assmIVer;
        public string AssmIVer => _assmIVer;

        [DataMember]
        private readonly string _assmPTok;
        public string AssmPTok => _assmPTok;

        [DataMember]
        private readonly int _assmHash;
        public int AssmHash => _assmHash;

        /// <summary>
        /// 
        /// </summary>
        public V131AssmInfo() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public V131AssmInfo(Assembly assembly)
        {
            string[] parts = assembly.FullName.Split(',');
            _assmName = parts[0];
            _assmNVer = parts[1].Split('=')[1];
            //AssmCult = parts[2].Split('=')[1];
            _assmPTok = parts[3].Split('=')[1];
            _assmHash = assembly.GetHashCode();
            foreach (object attr in assembly.GetCustomAttributes(true))
            {
                if (attr is AssemblyFileVersionAttribute attribute)
                    _assmFVer = attribute.Version;
                if (attr is AssemblyInformationalVersionAttribute versionAttribute)
                    _assmIVer = versionAttribute.InformationalVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        public V131AssmInfo(ProcessModule module)
        {
            _assmName = module.ModuleName.Split('.')[0];
            _assmNVer = "1.0.0.0";
            _assmPTok = "null";
            _assmFVer = module.FileVersionInfo.FileVersion;
            _assmIVer = module.FileVersionInfo.ProductVersion;
        }
    }

    [DataContract]
    public class V131UserInfo : IIdentity
    {
        [DataMember]
        private readonly string _userIdentityName;
        public string UserIdentityName => _userIdentityName;

        [DataMember]
        private readonly string _userFullName;
        public string UserFullName => _userFullName;

        /// <summary>
        /// 
        /// </summary>
        public V131UserInfo() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdentityName"></param>
        /// <param name="userFullName"></param>
        public V131UserInfo(string userIdentityName, string userFullName)
        {
            if (userIdentityName == null)
                throw new ArgumentNullException(nameof(userIdentityName));
            if (userIdentityName.Split('\\').Length != 2)
                throw new ArgumentException("userIdentityName not in domain\\loginid format!");

            _userIdentityName = userIdentityName;
            _userFullName = userFullName;
        }

        // IIdentity methods
        /// <summary>
        /// 
        /// </summary>
        public string Name => _userIdentityName;

        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationType => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        public bool IsAuthenticated => throw new NotImplementedException();
    }

    [DataContract]
    public class V131ClientInfo
    {
        [DataMember]
        private readonly Guid _nodeGuid;
        public Guid NodeGuid => _nodeGuid;

        [DataMember]
        private readonly V131EnvId _buildEnv;
        public V131EnvId BuildEnv => _buildEnv;

        [DataMember]
        private readonly V131EnvId _configEnv;
        public V131EnvId ConfigEnv => _configEnv;

        [DataMember]
        private readonly string _hostName;
        public string HostName => _hostName;

        [DataMember]
        private readonly string _hostIpV4;
        public string HostIpV4 => _hostIpV4;

        [DataMember]
        private readonly string[] _netAddrs;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] NetAddrs => _netAddrs;

        [DataMember]
        private readonly V131UserInfo _userInfo = new V131UserInfo();
        public V131UserInfo UserInfo => _userInfo;

        [DataMember]
        private readonly V131AssmInfo _compInfo = new V131AssmInfo();
        public V131AssmInfo CompInfo => _compInfo;

        [DataMember]
        private readonly V131AssmInfo _applInfo = new V131AssmInfo();
        public V131AssmInfo ApplInfo => _applInfo;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public V131ClientInfo() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="configEnv"></param>
        /// <param name="coreAssembly"></param>
        /// <param name="applAssembly"></param>
        /// <param name="userIdentityName"></param>
        /// <param name="userFullName"></param>
        public V131ClientInfo(
            Guid nodeGuid, V131EnvId configEnv, 
            Assembly coreAssembly, Assembly applAssembly, 
            string userIdentityName, string userFullName)
        {
            _nodeGuid = nodeGuid;
            _buildEnv = CoreHelper.ToV131EnvId(EnvHelper.ParseEnvName(BuildConst.BuildEnv));
            _configEnv = configEnv;
            // get remaining data members from system
            _hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(HostName);
            IPAddress[] hostIPs = hostEntry.AddressList;
            _netAddrs = new string[hostIPs.Length];
            for (int i = 0; i < hostIPs.Length; i++)
            {
                _netAddrs[i] = hostIPs[i].ToString();
                if (hostIPs[i].AddressFamily == AddressFamily.InterNetwork)
                    _hostIpV4 = hostIPs[i].ToString();
            }
            _userInfo = new V131UserInfo(userIdentityName, userFullName);
            // get calling application details
            // if unmanaged - get Win32 details
            Assembly application = applAssembly ?? Assembly.GetEntryAssembly();
            _applInfo = application != null ? new V131AssmInfo(application) : new V131AssmInfo(Process.GetCurrentProcess().MainModule);
            // get calling component details
            _compInfo = new V131AssmInfo(coreAssembly);
        }
    }

    [DataContract]
    public class V131ErrorDetail
    {
        [DataMember]
        public string FullName;

        [DataMember]
        public string Message;

        [DataMember]
        public string Source;

        [DataMember]
        public string StackTrace;

        [DataMember]
        public V131ErrorDetail InnerError;

        // constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public V131ErrorDetail(Exception e)
        {
            FullName = e.GetType().FullName;
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            if (e.InnerException != null)
                InnerError = new V131ErrorDetail(e.InnerException);
        }
    }

    [DataContract]
    public class V131SessionHeader
    {
        [DataMember]
        public readonly Guid SessionId; // client id

        [DataMember]
        public readonly Guid RequestId;

        [DataMember]
        public readonly string ReplyAddress;

        [DataMember]
        public readonly string ReplyContract;

        [DataMember]
        public readonly bool MoreFollowing;

        [DataMember]
        public readonly bool ReplyRequired;

        [DataMember]
        public readonly bool DebugRequest;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public V131SessionHeader() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        public V131SessionHeader(Guid clientId)
        {
            SessionId = clientId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="requestId"></param>
        /// <param name="moreFollowing"></param>
        /// <param name="replyRequired"></param>
        /// <param name="replyAddress"></param>
        /// <param name="replyContract"></param>
        /// <param name="debugRequest"></param>
        public V131SessionHeader(Guid clientId, Guid requestId, bool moreFollowing, bool replyRequired, string replyAddress, string replyContract, bool debugRequest)
        {
            SessionId = clientId;
            RequestId = requestId;
            MoreFollowing = moreFollowing;
            ReplyRequired = replyRequired;
            ReplyAddress = replyAddress;
            ReplyContract = replyContract;
            DebugRequest = debugRequest;
        }
    }

    [DataContract]
    public class V131SessionReply
    {
        [DataMember]
        public readonly bool Success;

        [DataMember]
        public readonly Guid SessionId; // client id

        [DataMember]
        public readonly string Message;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="message"></param>
        public V131SessionReply(Guid clientId, string message)
        {
            // access granted
            Success = true;
            SessionId = clientId;
            Message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public V131SessionReply(string message)
        {
            // access denied
            Message = message;
        }
    }

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
