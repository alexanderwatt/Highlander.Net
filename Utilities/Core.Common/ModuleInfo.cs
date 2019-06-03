/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using Orion.Build;

namespace Core.Common
{
    /// <summary>
    /// Interface providing module information
    /// </summary>
    public interface IModuleInfo : IIdentity
    {
        Guid NodeGuid { get; }
        EnvId BuildEnv { get; }
        EnvId ConfigEnv { get; }
        string HostName { get; }
        string HostIpV4 { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] NetAddrs { get; }
        string UserName { get; }
        string UserWDom { get; }
        string UserFullName { get; }
        string ApplName { get; }
        string ApplNVer { get; }
        string ApplFVer { get; }
        string ApplPTok { get; }
        int ApplHash { get; }
        string CoreName { get; }
        string CoreNVer { get; }
        string CoreFVer { get; }
        string CorePTok { get; }
        int CoreHash { get; }
    }

    public class ModuleInfo : IModuleInfo
    {
        // NodeGuid
        public Guid NodeGuid { get; }

        // Env
        public EnvId ConfigEnv { get; set; }

        private readonly EnvId _buildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);
        public EnvId BuildEnv { get => _buildEnv;
            set => throw new NotSupportedException();
        }

        // HostName
        private string _hostName;
        public string HostName { get => _hostName;
            set => _hostName = value;
        }

        // HostIpV4
        public string HostIpV4 { get; set; }

        // NetAddrs
        public string[] NetAddrs { get; }

        // UserIdentity
        private readonly string _userIdentityName;
        public string UserIdentityName => _userIdentityName;

        public string UserName
        {
            get
            {
                if (_userIdentityName == null)
                    return null;
                string[] parts = _userIdentityName.Split('\\');
                if (parts.Length > 1)
                    return parts[1];
                return _userIdentityName;
            }
        }
        public string UserWDom
        {
            get
            {
                if (_userIdentityName == null)
                    return null;
                string[] parts = _userIdentityName.Split('\\');
                if (parts.Length > 1)
                    return parts[0];
                return _userIdentityName;
            }
        }

        // UserFullName
        public string UserFullName { get; }

        // ApplName
        public string ApplName { get; set; }

        // ApplNVer
        public string ApplNVer { get; set; }

        // ApplFVer
        public string ApplFVer { get; set; }

        // ApplPTok
        public string ApplPTok { get; set; }

        // ApplHash
        public int ApplHash { get; set; }

        // CoreName
        public string CoreName { get; set; }

        // CoreNVer
        public string CoreNVer { get; set; }

        // CoreFVer
        public string CoreFVer { get; set; }

        // CorePTok
        public string CorePTok { get; set; }

        // CoreHash
        public int CoreHash { get; set; }

        // IIdentity methods
        public string Name => _userIdentityName;

        public string AuthenticationType => throw new NotImplementedException();
        public bool IsAuthenticated => throw new NotImplementedException();

        public ModuleInfo(string env, Guid nodeGuid, string userIdentityName, string userFullName, Assembly applAssembly, Assembly coreAssembly)
        {
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            if (userIdentityName == null)
                throw new ArgumentNullException(nameof(userIdentityName));
            if (userIdentityName.Split('\\').Length != 2)
                throw new ArgumentException("userIdentityName not in domain\\loginid format!");

            NodeGuid = nodeGuid; // Guid.NewGuid();
            ConfigEnv = EnvHelper.ParseEnvName(env);
            _hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);
            IPAddress[] hostIPs = hostEntry.AddressList;
            NetAddrs = new string[hostIPs.Length];
            for (int i = 0; i < hostIPs.Length; i++)
            {
                NetAddrs[i] = hostIPs[i].ToString();
                if (hostIPs[i].AddressFamily == AddressFamily.InterNetwork)
                    HostIpV4 = hostIPs[i].ToString();
            }
            _userIdentityName = userIdentityName;
            UserFullName = userFullName;
            // get calling application details
            {
                Assembly assembly = applAssembly ?? Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    string[] parts = assembly.FullName.Split(',');
                    ApplName = parts[0].Replace('.', '_');
                    ApplNVer = parts[1].Split('=')[1];
                    //_ApplCult = parts[2].Split('=')[1];
                    ApplPTok = parts[3].Split('=')[1];
                    ApplHash = assembly.GetHashCode();
                    foreach (object attr in assembly.GetCustomAttributes(true))
                        if (attr is AssemblyFileVersionAttribute)
                            ApplFVer = ((AssemblyFileVersionAttribute)attr).Version;
                }
                else
                {
                    // entry assembly is unmanaged - get Win32 details
                    ProcessModule pm = Process.GetCurrentProcess().MainModule;
                    ApplName = pm.ModuleName.Split('.')[0];
                    ApplNVer = "1.0.0.0";
                    ApplPTok = "null";
                    ApplFVer = pm.FileVersionInfo.FileVersion; // "1.0.0.0";
                }
            }
            // get calling component details
            {
                Assembly assembly = coreAssembly ?? Assembly.GetExecutingAssembly();
                string[] parts = assembly.FullName.Split(',');
                CoreName = parts[0];
                CoreNVer = parts[1].Split('=')[1];
                //_CoreCult = parts[2].Split('=')[1];
                CorePTok = parts[3].Split('=')[1];
                CoreHash = assembly.GetHashCode();
                foreach (object attr in assembly.GetCustomAttributes(true))
                    if (attr is AssemblyFileVersionAttribute)
                        CoreFVer = ((AssemblyFileVersionAttribute)attr).Version;
            }
        }
    }

}
