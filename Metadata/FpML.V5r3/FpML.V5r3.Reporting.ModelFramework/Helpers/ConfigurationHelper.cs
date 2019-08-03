/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;
using System.Diagnostics;
using Orion.Util.Helpers;

namespace Orion.ModelFramework.Helpers
{
    /// <summary>
    /// Helper for configuration
    /// </summary>
    public static class ConfigurationHelper
    {
        private static readonly IDictionary<string, System.Configuration.Configuration> Configurations = new Dictionary<string, System.Configuration.Configuration>();

        private static string _defaultCallingAssemblyConfigFilename = GetReferencingAssemblyConfigFilename();
        private static string _defaultCallingAssemblyConfigName = GetReferencingAssemblyName();

        /// <summary>
        /// Gets the name of the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyName()
        {
            StackTrace st = new StackTrace();
            var declaringType = st.GetFrame(3).GetMethod().DeclaringType;
            if (declaringType != null)
                return declaringType.Assembly.GetName().Name;
            return null;
        }

        /// <summary>
        /// Gets the referencing assembly.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyConfigFilename()
        {
            StackTrace st = new StackTrace();
            var declaringType = st.GetFrame(3).GetMethod().DeclaringType;
            if (declaringType != null)
                return $"{declaringType.Assembly.GetName().Name}.dll.config";
            return null;
        }

        /// <summary>
        /// Gets the referencing assembly location.
        /// </summary>
        /// <returns></returns>
        static string GetReferencingAssemblyLocation()
        {
            StackTrace st = new StackTrace();
            var declaringType = st.GetFrame(3).GetMethod().DeclaringType;
            if (declaringType != null)
            {
                var newLoc = ReflectionHelper.GetAssemblyCodeBaseLocation(declaringType.Assembly);
                return newLoc;
            }
            return null;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="fullConfigurationFilePath">The full configuration file path.</param>
        /// <returns></returns>
        public static System.Configuration.Configuration GetConfiguration(string fullConfigurationFilePath)
        {
            System.Configuration.Configuration configuration;
            lock (Configurations)
            {
                if (Configurations.ContainsKey(fullConfigurationFilePath))
                {
                    configuration = Configurations[fullConfigurationFilePath];
                }
                else
                {
                    lock (Configurations)
                    {
                        System.Configuration.ExeConfigurationFileMap map =
                            new System.Configuration.ExeConfigurationFileMap
                            {
                                ExeConfigFilename = fullConfigurationFilePath
                            };
                        configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, System.Configuration.ConfigurationUserLevel.None);
                        Configurations.Add(fullConfigurationFilePath, configuration);
                    }
                }
            }
            return configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static System.Configuration.Configuration GetConfiguration()
        {
            _defaultCallingAssemblyConfigName = GetReferencingAssemblyName();
            _defaultCallingAssemblyConfigFilename = GetReferencingAssemblyConfigFilename();
            string defaultCallingAssemblyLocation = GetReferencingAssemblyLocation();

            string keyName = $"{_defaultCallingAssemblyConfigName}Location";
            string overrideLocation = GetConfigurationKeyVal(keyName);
            string targetLocation = overrideLocation.Length > 0 ? overrideLocation : defaultCallingAssemblyLocation;
            string fullConfigurationFilePath =
                $@"{targetLocation}\{_defaultCallingAssemblyConfigFilename}";

            System.Configuration.Configuration configuration;
            lock (Configurations)
            {
                if (Configurations.ContainsKey(fullConfigurationFilePath))
                {
                    configuration = Configurations[fullConfigurationFilePath];
                }
                else
                {
                    lock (Configurations)
                    {
                        System.Configuration.ExeConfigurationFileMap map =
                            new System.Configuration.ExeConfigurationFileMap
                            {
                                ExeConfigFilename = fullConfigurationFilePath
                            };
                        configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, System.Configuration.ConfigurationUserLevel.None);
                        Configurations.Add(fullConfigurationFilePath, configuration);
                    }
                }
            }
            return configuration;
        }

        /// <summary>
        /// Gets the app settings section entry.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public static string GetAppSettingsSectionEntry(System.Configuration.Configuration configuration, string sectionName, string keyName)
        {
            string result = string.Empty;
            System.Configuration.ConfigurationSection configSection = configuration.GetSection(sectionName);
            if (configSection != null)
            {
                System.Configuration.AppSettingsSection section = (System.Configuration.AppSettingsSection)configuration.GetSection(sectionName);
                if (section?.Settings?[keyName] != null)
                {
                    var item = section.Settings[keyName];
                    result = item.Value;
                }
            }
            return result;
        }

        internal static string GetConfigurationKeyVal(string key)
        {
            string retval = string.Empty;
            string keyVal = key;
            if (System.Configuration.ConfigurationManager.AppSettings[keyVal] != null)
                retval = System.Configuration.ConfigurationManager.AppSettings[keyVal];
            return retval;
        }
    }
}
