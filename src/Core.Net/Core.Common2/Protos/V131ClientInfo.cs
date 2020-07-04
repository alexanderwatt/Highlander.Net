using Highlander.Core.Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Highlander.Build;

namespace Highlander.Grpc.Session
{
    public partial class V131ClientInfo
    {
        public Guid NodeGuidAsGuid => Guid.Parse(nodeGuid_);

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
            NodeGuid = nodeGuid.ToString();
            BuildEnv = CoreHelper.ToV131EnvId(EnvHelper.ParseEnvName(BuildConst.BuildEnv));
            ConfigEnv = configEnv;
            // get remaining data members from system
            HostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(HostName);
            IPAddress[] hostIPs = hostEntry.AddressList;
            var items = new Google.Protobuf.Collections.RepeatedField<string>();
            foreach (var host in hostIPs)
            {
                netAddrs_.Add(host.ToString());
                if (host.AddressFamily == AddressFamily.InterNetwork)
                    HostIpV4 = host.ToString();
            }
            netAddrs_ = items;
            UserInfo = new V131UserInfo(userIdentityName, userFullName);
            // get calling application details
            // if unmanaged - get Win32 details
            Assembly application = applAssembly ?? Assembly.GetEntryAssembly();
            ApplInfo = application != null ? new V131AssmInfo(application) : new V131AssmInfo(Process.GetCurrentProcess().MainModule);
            // get calling component details
            CompInfo = new V131AssmInfo(coreAssembly);
        }
    }
}
