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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;
using Highlander.Build;

namespace Highlander.Metadata.Common
{
    public enum EnvId
    {
        Undefined,
        Utt_UnitTest,
        Dev_Development,
        Sit_SystemTest,
        Stg_StagingLive,
        Prd_Production
    }

    public enum SvcId
    {
        Undefined,
        CoreServer,
        MarketData,
        GridSwitch,
        CoreWProxy
    }

    internal static class NameConst
    {
        public const char EscChar = '%';
        public const char SepList = '|';
        public const char SepType = '/';
        public const char SepPair = '=';
        public const char SepElem = ':'; // array element
    }

    internal class NetSubnet
    {
        public readonly EnvId Env;
        public readonly string Name;
        public NetServer PrimaryNode;
        //public NetSubnet FallbackSite;
        public NetSubnet(EnvId env, string name) { Env = env; Name = name; }
    }

    internal class NetServer
    {
        //public readonly EnvId Env;
        public readonly string Name;
        //public NetSite Site;
        //public NetNode(EnvId env, string name) { Env = env; Name = name; }
        public NetServer(string name) { Name = name; }
    }

    public class NetTopology
    {
        //public const string SiteAuSyd = "Sydney";
        //public const string SiteAuMel = "Melbourne";
        //public const string SiteUkLon = "London";

        private readonly Dictionary<string, NetSubnet> _sites = new Dictionary<string, NetSubnet>();
        private readonly Dictionary<string, NetServer> _nodes = new Dictionary<string, NetServer>();
        private static string SiteKey(EnvId env, string siteName) { return EnvHelper.EnvName(env) + siteName; }
        
        private NetSubnet CreateSite(EnvId env, string siteName, string nodeName)
        {
            var site = new NetSubnet(env, siteName);
            var node = new NetServer(nodeName);
            //node.Site = site;
            site.PrimaryNode = node;
            _sites.Add(SiteKey(env, siteName), site);
            _nodes.Add(nodeName, node);
            return site;
        }

        //public NetTopology()
        //{
        //    // DEV (development)
        //    NetSubnet SydneyDev = CreateSite(EnvId.Dev_Development, Site_AU_SYD, Server_DEV_Sydney);
        //    NetSubnet LondonDev = CreateSite(EnvId.DEV_Development, Site_UK_LON, Server_DEV_London);
        //    LondonDev.FallbackSite = SydneyDev;
        //    // SIT (system test)
        //    ////NetSubnet SydneySIT = CreateSite(EnvId.SIT_SystemTest, Site_AU_SYD, Server_SIT_Sydney);
        //    ////SydneySIT.FallbackSite = null;
        //    // STG (staging or live)
        //    NetSubnet SydneyLive = CreateSite(EnvId.STG_StagingLive, Site_AU_SYD, Server_STG_Sydney);
        //    NetSubnet MelbourneLive = CreateSite(EnvId.STG_StagingLive, Site_AU_MEL, Server_STG_Melbourne);
        //    SydneyLive.FallbackSite = MelbourneLive;
        //    MelbourneLive.FallbackSite = SydneyLive;
        //    // PRD (true production) - not defined yet
        //}

        //public string GetPrimaryServerNames(EnvId env, string siteName, bool includeDevLocalHost)
        //{
        //    if (env == EnvId.Utt_UnitTest)
        //        return "localhost";
        //    List<string> results = new List<string>();
        //    if (includeDevLocalHost && env == EnvId.Dev_Development)
        //    {
        //        // prefix with localhost
        //        results.Add("localhost");
        //    }
        //    foreach (NetSubnet site in _sites.Values)
        //    {
        //        if (site.Env == env
        //            && (null == siteName || (site.Name == siteName))
        //            && (site.PrimaryNode != null))
        //            results.Add(site.PrimaryNode.Name);
        //    }
        //    return String.Join(";", results.ToArray());
        //}

        //public string GetFallbackServerNames(EnvId env, string siteName)
        //{
        //    List<string> results = new List<string>();
        //    foreach (NetSubnet site in _sites.Values)
        //    {
        //        if ((site.Env == env)
        //            && (null == siteName || (site.Name == siteName))
        //            && site.FallbackSite != null
        //            && site.FallbackSite.PrimaryNode != null)
        //        {
        //            results.Add(site.FallbackSite.PrimaryNode.Name);
        //        }
        //    }
        //    return results.Count > 0 ? String.Join(";", results.ToArray()) : null;
        //}
    }

    public static class EnvHelper
    {
        private static readonly EnvId BuildEnv = ParseEnvName(BuildConst.BuildEnv);

        public static EnvId ParseEnvName(string input)
        {
            return Enum.GetValues(typeof (EnvId)).Cast<EnvId>().FirstOrDefault(env => string.Compare(input, EnvName(env), StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static EnvId CheckEnv(EnvId env)
        {
            if (env > BuildEnv)
                throw new ArgumentException("Environment must be <= " + BuildConst.BuildEnv, nameof(env));
            if (env == EnvId.Undefined)
                return BuildEnv;
            return env;
        }

        public static string EnvName(EnvId env)
        {
            if (env != EnvId.Undefined)
                return env.ToString().Split('_')[0];
            return null;
        }

        public static string SvcPrefix(SvcId svc)
        {
            switch (svc)
            {
                case SvcId.CoreServer: return "QDS_Core";
                case SvcId.MarketData: return "QDS_MDAG";
                case SvcId.GridSwitch: return "QDS_Grid";
                case SvcId.CoreWProxy: return "QDS_Prxy";
                default:
                    throw new NotSupportedException("SvcId: " + svc);
            }
        }

        private static int SvcPortOffset(SvcId svc)
        {
            switch (svc)
            {
                case SvcId.CoreServer: return 13;
                case SvcId.MarketData: return 18;
                case SvcId.GridSwitch: return 21;
                case SvcId.CoreWProxy: return 22;
                default:
                    throw new NotSupportedException("SvcId: " + svc);
            }
        }

        private static int SvcPortBase(EnvId env)
        {
            switch (env)
            {
                case EnvId.Utt_UnitTest: return 8100;
                case EnvId.Dev_Development: return 8200;
                case EnvId.Sit_SystemTest: return 8300;
                case EnvId.Stg_StagingLive: return 8400;
                case EnvId.Prd_Production: return 8500;
                default:
                    throw new NotSupportedException("EnvId: " + env);
            }
        }

        public static int SvcPort(EnvId env, SvcId svc)
        {
            return SvcPortBase(env) + SvcPortOffset(svc);
        }

        public static string[] GetServiceAddrs(EnvId env, SvcId svc, bool includeFallbackServers)
        {
            int port = SvcPort(env, svc);
            var settings = GetAppSettings(env, SvcPrefix(svc), false);
            string hostList = GetAppSettingsValueAsString(settings, "PrimaryServers");
            if (includeFallbackServers)
                hostList += ";" + GetAppSettingsValueAsString(settings, "FallbackServers");
            return hostList.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(host =>
                $"{host}:{port}").ToArray();
        }

        public static string FormatDbCfgStr(EnvId env, string dbServer, string dbPrefix)
        {
            return String.Format("Data Source={1};Initial Catalog={2}_{0};Integrated Security=True", EnvName(env), dbServer, dbPrefix);
        }

        private static string DecodeText(string valueText)
        {
            // converts %xx to char
            var sb = new StringBuilder();
            for (int i = 0; i < valueText.Length; i++)
            {
                char ch = valueText[i];
                if (ch == NameConst.EscChar)
                {
                    // found escaped char
                    // - next 2 chars are hex digits
                    sb.Append(Convert.ToChar(Byte.Parse(valueText.Substring(i + 1, 2), NumberStyles.HexNumber)));
                    i += 2;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static Dictionary<string, object> DeserialiseNamedValueSet(string text)
        {
            var result = new Dictionary<string, object>();
            // extract dictionary from serialised named value set
            if (text != null)
            {
                string delims = NameConst.SepList + Environment.NewLine;
                string[] setParts = text.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string setPart in setParts)
                {
                    if (!string.IsNullOrEmpty(setPart.Trim()))
                    {
                        // construct by deserialising text in the format: name/type=value
                        string[] nameValueParts = setPart.Trim().Split(NameConst.SepPair);
                        if (nameValueParts.Length != 2)
                            throw new ArgumentException("Text ('" + text + "') is not in name/type=value format");
                        string[] nameParts = nameValueParts[0].Split(NameConst.SepType);
                        if (nameParts.Length != 2)
                            throw new ArgumentException("Text ('" + text + "') is not in name/type=value format");
                        string name = nameParts[0];
                        //Type valueType = typeof(string); // default type
                        string valueString = DecodeText(nameValueParts[1]);
                        object value = valueString;
                        string baseTypeName = nameParts[1].Trim();
                        switch (baseTypeName.ToLower())
                        {
                            case "int":
                            case "int32":
                                value = int.Parse(valueString); break;
                            case "int64":
                                value = long.Parse(valueString); break;
                            case "bool":
                            case "boolean":
                                value = bool.Parse(valueString); break;
                            case "guid":
                                value = new Guid(valueString); break;
                            case "double":
                                value = double.Parse(valueString); break;
                            case "decimal":
                                value = decimal.Parse(valueString); break;
                            case "datetime":
                                value = DateTime.Parse(valueString); break;
                            case "datetimeoffset":
                                value = DateTimeOffset.Parse(valueString); break;
                            case "timespan":
                                value = TimeSpan.Parse(valueString); break;
                            case "string":
                                break; // conversion not required
                            default:
                                throw new NotSupportedException("Type '" + baseTypeName + "' not supported!");
                        }
                        result[name.ToLower()] = value;
                    }
                }
            }
            return result;
        }

        private static bool CandidateMatchesArgument(string candidate, string argument)
        {
            if (candidate == null)
                return true;
            if (candidate == "*")
                return true;
            if (argument == null)
                return false;
            if (argument == "*")
                return false;
            return string.Compare(candidate, argument, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static string GetResource(Assembly assembly, string resourceName)
        {
            using (TextReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        private static string GetResourceWithPartialName(Assembly assembly, string partialResourceName)
        {
            return (from resourceName in assembly.GetManifestResourceNames() where resourceName.Contains(partialResourceName) select GetResource(assembly, resourceName)).FirstOrDefault();
        }

        private static T DeserializeFromString<T>(Type asRootType, string serializedObject)
        {
            using (var stringReader = new StringReader(serializedObject))
            {
                var serializer = new XmlSerializer(asRootType);
                var deserializedObject = (T)serializer.Deserialize(stringReader);
                return deserializedObject;
            }
        }

        private static T DeserializeFromFile<T>(Type asRootType, string filename)
        {
            using (var stringReader = new StreamReader(filename))
            {
                var serializer = new XmlSerializer(asRootType);
                var deserializedObject = (T)serializer.Deserialize(stringReader);
                return deserializedObject;
            }
        }

        private static List<EnvConfigRuleSet> _ruleSets;
        public static Dictionary<string, object> GetAppSettings(EnvId env, string applName, bool reload)
        {
            var result = new Dictionary<string, object>();

            // load settings if required
            if (reload || (_ruleSets == null))
            {
                var ruleSets = new List<EnvConfigRuleSet>();
                // get default settings
                const string resourceName = "Default.cfg.xml";
                Assembly assembly = Assembly.GetExecutingAssembly();
                string xmlText = GetResourceWithPartialName(assembly, resourceName);
                if (xmlText == null)
                    throw new FileNotFoundException(
                        $"Embedded resource '{resourceName}' not found in {assembly.FullName}");
                ruleSets.Add(DeserializeFromString<EnvConfigRuleSet>(typeof(EnvConfigRuleSet), xmlText));

                // load all custom xml files
                string assemblyPath = Path.GetDirectoryName(assembly.CodeBase.Replace("file:///", ""));
                if (assemblyPath != null)
                {
                    string[] xmlFileNames = Directory.GetFiles(assemblyPath, "QDS.Core.*.cfg.xml", SearchOption.TopDirectoryOnly);
                    ruleSets.AddRange(xmlFileNames.Select(xmlFileName => DeserializeFromFile<EnvConfigRuleSet>(typeof (EnvConfigRuleSet), xmlFileName)));
                }
                _ruleSets = ruleSets;
            }

            // filter
            string hostName = Dns.GetHostName();
            WindowsIdentity winIdent = WindowsIdentity.GetCurrent();
            {
                string userName = winIdent.Name.Split('\\')[1];
                string envName = EnvName(env);
                var selectedRules = new List<EnvConfigRule>();
                foreach (EnvConfigRuleSet ruleSet in _ruleSets)
                {
                    if (ruleSet.v2Rules != null)
                    {
                        foreach (EnvConfigRule rule in ruleSet.v2Rules)
                        {
                            if (!rule.Disabled
                                && CandidateMatchesArgument(rule.Env, envName)
                                && CandidateMatchesArgument(rule.HostName, hostName)
                                && CandidateMatchesArgument(rule.ApplName, applName)
                                && CandidateMatchesArgument(rule.UserName, userName))
                                selectedRules.Add(rule);
                        }
                    }
                }

                // sort and accumulate
                selectedRules.Sort();
                foreach (EnvConfigRule rule in selectedRules)
                {
                    // config values are cumulative
                    if (!rule.Disabled)
                    {
                        var ruleNamedValues = DeserialiseNamedValueSet(rule.Settings);
                        foreach (var item in ruleNamedValues)
                            result[item.Key.ToLower()] = item.Value;
                    }
                }
            }
            return result;
        }

        public static object GetAppSettingsValue(Dictionary<string, object> settings, string key)
        {
            settings.TryGetValue(key.ToLower(), out var result);
            return result;
        }

        public static string GetAppSettingsValueAsString(Dictionary<string, object> settings, string key)
        {
            object result = GetAppSettingsValue(settings, key);
            return result?.ToString();
        }
    }
}
