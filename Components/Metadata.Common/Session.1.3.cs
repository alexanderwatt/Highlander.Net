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
using Highlander.Utilities.Logging;
using Orion.Build;

#endregion

namespace Highlander.Metadata.Common
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
        [field: DataMember]
        public string AssmName { get; }

        [field: DataMember]
        public string AssmNVer { get; }

        [field: DataMember]
        public string AssmFVer { get; }

        [field: DataMember]
        public string AssmIVer { get; }

        [field: DataMember]
        public string AssmPTok { get; }

        [field: DataMember]
        public int AssmHash { get; }

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
            AssmName = parts[0];
            AssmNVer = parts[1].Split('=')[1];
            //AssmCult = parts[2].Split('=')[1];
            AssmPTok = parts[3].Split('=')[1];
            AssmHash = assembly.GetHashCode();
            foreach (object attr in assembly.GetCustomAttributes(true))
            {
                if (attr is AssemblyFileVersionAttribute attribute)
                    AssmFVer = attribute.Version;
                if (attr is AssemblyInformationalVersionAttribute versionAttribute)
                    AssmIVer = versionAttribute.InformationalVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        public V131AssmInfo(ProcessModule module)
        {
            AssmName = module.ModuleName.Split('.')[0];
            AssmNVer = "1.0.0.0";
            AssmPTok = "null";
            AssmFVer = module.FileVersionInfo.FileVersion;
            AssmIVer = module.FileVersionInfo.ProductVersion;
        }
    }

    [DataContract]
    public class V131UserInfo : IIdentity
    {
        [field: DataMember]
        public string UserIdentityName { get; }

        [field: DataMember]
        public string UserFullName { get; }

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

            UserIdentityName = userIdentityName;
            UserFullName = userFullName;
        }

        // IIdentity methods
        /// <summary>
        /// 
        /// </summary>
        public string Name => UserIdentityName;

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
        [field: DataMember]
        public Guid NodeGuid { get; }

        [field: DataMember]
        public V131EnvId BuildEnv { get; }

        [field: DataMember]
        public V131EnvId ConfigEnv { get; }

        [field: DataMember]
        public string HostName { get; }

        [field: DataMember]
        public string HostIpV4 { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [field: DataMember]
        public string[] NetAddrs { get; }

        [field: DataMember]
        public V131UserInfo UserInfo { get; } = new V131UserInfo();

        [field: DataMember]
        public V131AssmInfo CompInfo { get; } = new V131AssmInfo();

        [field: DataMember]
        public V131AssmInfo ApplInfo { get; } = new V131AssmInfo();

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
            NodeGuid = nodeGuid;
            BuildEnv = CoreHelper.ToV131EnvId(EnvHelper.ParseEnvName(BuildConst.BuildEnv));
            ConfigEnv = configEnv;
            // get remaining data members from system
            HostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(HostName);
            IPAddress[] hostIPs = hostEntry.AddressList;
            NetAddrs = new string[hostIPs.Length];
            for (int i = 0; i < hostIPs.Length; i++)
            {
                NetAddrs[i] = hostIPs[i].ToString();
                if (hostIPs[i].AddressFamily == AddressFamily.InterNetwork)
                    HostIpV4 = hostIPs[i].ToString();
            }
            UserInfo = new V131UserInfo(userIdentityName, userFullName);
            // get calling application details
            // if unmanaged - get Win32 details
            Assembly application = applAssembly ?? Assembly.GetEntryAssembly();
            ApplInfo = application != null ? new V131AssmInfo(application) : new V131AssmInfo(Process.GetCurrentProcess().MainModule);
            // get calling component details
            CompInfo = new V131AssmInfo(coreAssembly);
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
